using System;
using System.Collections.Generic;
using System.IO;

namespace ExportExcel
{
    /*
     * 结构
     * Sign
     * StrTable
     *   BodyLen,Count
     *   Repeated String  (Len:Bytes)
     *  TableIndex
     *    BodyLen,Count
     *    Repeated StringIndex(int) Offset(FromHeader),BodyLen
     *   Repeated Table      
     *      Headers 
     *          Count
     *          Repated StringIndex(int)  两行
     *      Repeated Row 
     *          Row Count
     *          BodyLen Data,  (如果Cell是List/List_Tuple, 第一个int描述数量, 如果cell 是 Tuple, 第一个int 0或者1)
     *      
     */
    public class Exporter_BinData : IProcessNode
    {
        public const string C_SIGN = "ABAB";
        public const string C_FILE_NAME = "data.bin";
        public const string C_FILE_LANG_NAME = "data_{0}.bin";
        public static uint S_SIGN;
        public EExportFlag _flag;
        public ExeConfig.BinConfig _config;

        public Exporter_BinData(EExportFlag flag, ExeConfig.BinConfig config)
        {
            _flag = flag;
            _config = config;
            S_SIGN = 0;
            foreach (var p in C_SIGN)
            {
                S_SIGN = S_SIGN << 8;
                S_SIGN |= (uint)(p);
            }            
        }
        public string GetName()
        {
            return "Export Bin Data";
        }

        public void Process(DataBase data_base)
        {
            if (_config == null || !_config.enable)
                return;            

            string path = Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(path);

            _WriteBin(path, data_base, null);

            foreach (var p in data_base.LangList)
            {
                path = Path.Combine(_config.dir, string.Format(C_FILE_LANG_NAME, p));
                _WriteBin(path, data_base, p);
            }
        }

        public void _WriteBin(string file_path, DataBase data_base, string lang)
        {
            StrDict str_dict = new StrDict();
            List<BinTable> bin_table_list = new List<BinTable>();
            data_base.ForeachTable((table) =>
            {
                if ((table.TableExportFlag & _flag) == EExportFlag.none)
                    return;

                if (table.MultiLangBody == null && lang == null)
                {
                    BinTable bin_table = new BinTable(str_dict, table, table.Body, _flag);
                    bin_table.GenBuff();
                    bin_table_list.Add(bin_table);
                }
                else if (lang != null && table.MultiLangBody != null && table.MultiLangBody.ContainsKey(lang))
                {
                    BinTable bin_table = new BinTable(str_dict, table, table.MultiLangBody[lang], _flag);
                    bin_table.GenBuff();
                    bin_table_list.Add(bin_table);
                }
            });

            if (bin_table_list.Count == 0)
                return;

            TableIndex table_index = new TableIndex(bin_table_list, str_dict);

            //计算所有table的offset
            {
                table_index.GenBuff();
                str_dict.GenBuff();
                int len = 4; //header
                len += str_dict.Buffer.Length;
                len += table_index.Buffer.Length;
                foreach (var p in bin_table_list)
                {
                    p.Offset = len;
                    len += p.Buffer.Length;
                }
                table_index.GenBuff();
            }

            if (File.Exists(file_path))
                File.Delete(file_path);

            using FileStream fs = new FileStream(file_path, FileMode.CreateNew, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(S_SIGN);
            bw.Write(str_dict.Buffer);
            bw.Write(table_index.Buffer);
            foreach (var p in bin_table_list)
                bw.Write(p.Buffer);

            fs.Close();
        }

        public static byte[] AppendSize(BinaryWriter bw, MemoryStream ms)
        {
            ms.TryGetBuffer(out ArraySegment<byte> body_buff);

            bw.Write7BitEncodedInt(body_buff.Count);
            ms.TryGetBuffer(out ArraySegment<byte> header_buff);
            header_buff = header_buff.Slice(body_buff.Count);


            int total_len = body_buff.Count + header_buff.Count;
            var buff = new byte[total_len];

            //复制BodyLen
            header_buff.CopyTo(buff);
            body_buff.CopyTo(buff, header_buff.Count);
            return buff;
        }

        public class TableIndex
        {
            public List<BinTable> _list;
            public StrDict _str_dict;
            public byte[] Buffer;
            public TableIndex(List<BinTable> list, StrDict str_dict)
            {
                _list = list;
                _str_dict = str_dict;
            }

            public void GenBuff()
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);

                bw.Write7BitEncodedInt(_list.Count);
                foreach (var p in _list)
                {
                    string name = p.Table.SheetName;
                    bw.Write7BitEncodedInt(_str_dict.GetIndex(name));
                    bw.Write(p.Offset);//Offset ,  不用变长的, 要写两次,第一次占位, 第二次才是真正的offset
                    bw.Write7BitEncodedInt(p.Buffer.Length);
                }
                Buffer = AppendSize(bw, ms);
            }
        }

        public class StrDict
        {
            public List<string> _list = new List<string>();
            public Dictionary<string, int> _dict = new Dictionary<string, int>();
            public byte[] Buffer;
            public StrDict()
            {
                GetIndex(string.Empty);
            }

            public void GenBuff()
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(stream);
                bw.Write7BitEncodedInt(_list.Count);
                foreach (var s in _list)
                {
                    bw.Write(s);
                }

                Buffer = AppendSize(bw, stream);
            }

            public byte[] GetBuff()
            {
                return Buffer;
            }

            public int GetIndex(string name)
            {
                if (_dict.TryGetValue(name, out var id))
                    return id;
                id = _list.Count;
                _dict.Add(name, id);
                _list.Add(name);
                return id;
            }
        }

        public class BinTable
        {
            public static BinTableRow _row_writer = new BinTableRow();
            public StrDict _str_dict;
            public Table Table;
            public string[,] _body;
            public byte[] Buffer;
            public int Offset = 0;
            public EExportFlag _flag;
            public BinTable(StrDict str_dict, Table table, string[,] body, EExportFlag flag)
            {
                _str_dict = str_dict;
                Table = table;
                _body = body;
                _flag = flag;
            }

            public void GenBuff()
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(stream);

                //1. 写入header
                int count = 0;
                foreach (var p in Table.Header.List)
                {
                    if ((p.ExportFlag & _flag) == EExportFlag.none)
                        continue;
                    count++;
                }
                bw.Write7BitEncodedInt(count * 2);
                foreach (var p in Table.Header.List)
                {
                    if ((p.ExportFlag & _flag) == EExportFlag.none)
                        continue;
                    bw.Write7BitEncodedInt(_str_dict.GetIndex(p.Name));
                }
                foreach (var p in Table.Header.List)
                {
                    if ((p.ExportFlag & _flag) == EExportFlag.none)
                        continue;
                    bw.Write7BitEncodedInt(_str_dict.GetIndex(p.DataType.ToCsvStr()));
                }

                //2. 写行                
                int row_count = _body.GetLength(0);
                bw.Write7BitEncodedInt(row_count);
                for (int i = 0; i < row_count; i++)
                {
                    var buff = _row_writer.GetRowBuff(Table.Header, _flag, _body, i, _str_dict);
                    bw.Write7BitEncodedInt(buff.Count);
                    bw.Write(buff.Array, buff.Offset, buff.Count);
                }

                stream.TryGetBuffer(out var tt);
                Buffer = tt.ToArray();
            }
        }

        public class BinTableRow
        {
            public MemoryStream _stream;
            public BinaryWriter _bw;
            public BinTableRow()
            {
                _stream = new MemoryStream();
                _bw = new BinaryWriter(_stream);
            }

            public ArraySegment<byte> GetRowBuff(TableHeader header, EExportFlag flag, string[,] body, int row_index, StrDict str_dict)
            {
                _stream.SetLength(0);

                for (int i = 0; i < header.Count; i++)
                {
                    if ((header[i].ExportFlag & flag) == EExportFlag.none)
                        continue;

                    _WriteCell(header[i].DataType, body[row_index, i], str_dict);
                }

                _stream.TryGetBuffer(out var ret);
                return ret;
            }

            public void _WriteCell(DataType data_type, string cell, StrDict str_dict)
            {
                if (data_type.IsList)
                {
                    if (string.IsNullOrEmpty(cell))
                    {
                        _bw.Write7BitEncodedInt(0);
                    }
                    else
                    {
                        string[] list = cell.Split(ConstDef.C_LIST_SPLIT);
                        _bw.Write7BitEncodedInt(list.Length);

                        if (data_type.IsTuple)
                        {
                            foreach (var v in list)
                            {
                                string[] pair = v.Split(ConstDef.C_TUPLE_SPLIT);
                                for (int i = 0; i < data_type.Count; i++)
                                {
                                    _WriteVal(data_type.Get(i), pair[i], str_dict);
                                }
                            }
                        }
                        else
                        {
                            foreach (var v in list)
                            {
                                _WriteVal(data_type.type0, v, str_dict);
                            }
                        }
                    }
                }
                else if (data_type.IsTuple)
                {
                    if (string.IsNullOrEmpty(cell))
                    {
                        _bw.Write7BitEncodedInt(0);
                    }
                    else
                    {
                        _bw.Write7BitEncodedInt(1);
                        string[] pair = cell.Split(ConstDef.C_TUPLE_SPLIT);
                        for (int i = 0; i < data_type.Count; i++)
                        {
                            _WriteVal(data_type.Get(i), pair[i], str_dict);
                        }
                    }
                }
                else
                {
                    _WriteVal(data_type.type0, cell, str_dict);
                }
            }

            public void _WriteVal(EDataType type, string v, StrDict str_dict)
            {
                try
                {
                    switch (type)
                    {
                        case EDataType.Bool:
                            _bw.Write((byte)int.Parse(v));
                            break;

                        case EDataType.Int32:
                            _bw.Write7BitEncodedInt(int.Parse(v));
                            break;

                        case EDataType.UInt32:
                            _bw.Write7BitEncodedInt((int)(uint.Parse(v)));
                            break;

                        case EDataType.Int64:
                            _bw.Write7BitEncodedInt64((long.Parse(v)));
                            break;
                        case EDataType.UInt64:
                            _bw.Write7BitEncodedInt64((long)(ulong.Parse(v)));
                            break;
                        case EDataType.Float32:
                            _bw.Write(float.Parse(v));
                            break;
                        case EDataType.Float64:
                            _bw.Write(double.Parse(v));
                            break;
                        case EDataType.String:
                        case EDataType.LocStr:
                            _bw.Write7BitEncodedInt(str_dict.GetIndex(v));
                            break;

                        case EDataType.LocId:
                            _bw.Write7BitEncodedInt(int.Parse(v));
                            break;

                        default:
                            ErrSet.E($"写入出错 {type},{v}");
                            break;
                    }
                }catch (Exception e)
                {
                    ErrSet.E($"写入出错 {type},{v}");
                }
            }
        }
    }
}
