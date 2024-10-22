using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/7 16:50:44
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    //导出 csharp的 加载
    public class Exporter_CSLoaderFromCsv : IProcessNode
    {
        public const string C_FILE_NAME = "table_loader_csv.cs";

        public StringFormater _formater = new StringFormater();

        public Config.CSharpConfig _config;
        public EExportFlag _flag;
        public DBAlias _AliasDB;

        public Exporter_CSLoaderFromCsv(EExportFlag flag, Config.CSharpConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export C# Loader Csv";
        }

        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable || !_config.loader.csv)
                return;
            _AliasDB = data.AliasDB;
            List<FilterTable> tables = FilterTable.Filter(data, _flag);

            string name_space = _config.namespaceName;
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("//自动生成的");
            sw.WriteLine(_config.header);
            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("namespace " + name_space + "{");

            _ExportTableBase(data, tables, sw);
            _ExportLoaderUtil(data, tables, sw);

            foreach(var p in tables)
            {
                _ExportLoaderFunc(p, sw);
            }

            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("}");
            sw.Close();
        }

        public void _ExportTableBase(DataBase data_base, List<FilterTable> tables, StreamWriter sw)
        {
            _formater["table_count"] = tables.Count.ToString();
            _formater["lang_count"] = data_base.LangList.Count.ToString();

            int row_count = 0;
            foreach (var p in tables)
            {
                row_count = Math.Max(p.RowCount, row_count);
            }
            row_count += 100;

            _formater["row_count_max"] = row_count.ToString();

            sw.WriteLineExt(_formater, @"
    public partial class Table
    {
        protected static List<System.Object> _tempItemsForCsv = new List<System.Object>({row_count_max});
        public abstract bool LoadFromCsv(ITableReader reader);
    }
");
        }

        public void _ExportLoaderUtil(DataBase data_base, List<FilterTable> tables, StreamWriter sw)
        {
            sw.WriteLine(@"
    public static class TableLoaderCsvUtil
	{
        private static bool _Init=false;
		public static void Init()
		{		
            if(_Init) return;
            _Init=true;
");
            foreach (var p in data_base.EnumDB)
            {
                sw.WriteLine($"\t\t\tEnumConverterMgr.RegFunc((v) => ({p.Key})v, (v) => (int)v);");
            }

            sw.WriteLine("\t\t}");

            sw.Write(@"
        #region Base Reader
        public static void Read(ITableDataReader reader, ref bool v)
        {
            v = reader.ReadBool();
        }
        public static void Read(ITableDataReader reader, ref int v)
        {
            v = reader.ReadInt32();
        }
        public static void Read(ITableDataReader reader, ref uint v)
        {
            v = reader.ReadUInt32();
        }
        public static void Read(ITableDataReader reader, ref long v)
        {
            v = reader.ReadInt64();
        }
        public static void Read(ITableDataReader reader, ref ulong v)
        {
            v = reader.ReadUInt64();
        }
        public static void Read(ITableDataReader reader, ref float v)
        {
            v = reader.ReadF32();
        }
        public static void Read(ITableDataReader reader, ref double v)
        {
            v = reader.ReadF64();
        }
        public static void Read(ITableDataReader reader, ref string v)
        {
            v = reader.ReadString();
        }
        public static void Read(ITableDataReader reader, ref LocStr v)
        {
            v = new LocStr(reader.ReadString());
        }
        public static void Read(ITableDataReader reader, ref LocId v)
        {
            v =new LocId(reader.ReadInt32());
        }
        public static void Read<T>(ITableDataReader reader, ref T v) where T : Enum
        {
            if (!EnumConverterMgr.Convert(reader.ReadInt32(), ref v))
            {
                Log.E(""没有找到类型 {0} 的转换"", typeof(T));
            }
        }
        #endregion
");

            const int ListElementType_Normal = 0;
            const int ListElementType_Tuple = 1;
            const int ListElementType_StringAlias = 2;
            Dictionary<(string aliasName, string name), int> list_types = new Dictionary<(string, string), int>();
            Dictionary<(string aliasName, string name), DataType> tuple_types = new Dictionary<(string, string), DataType>();
            Dictionary<(string aliasName, string name), bool> string_alias_types = new Dictionary<(string, string), bool>();

            foreach (var table in tables)
            {
                List<TableField> header_list = table.FiltedHeader;
                foreach (var field in header_list)
                {
                    DataType data_type = field.DataType;
                    string alias_name = field.AliasCSharp;

                    if (data_type.IsList)
                    {
                        data_type.IsList = false;
                        if (alias_name != null)
                        {
                            if (data_type.IsTuple)
                                list_types[(alias_name, data_type.ToCSharpStrForLoader())] = ListElementType_Tuple;
                            else if (data_type.type0 == EDataType.String)
                                list_types[(alias_name, data_type.ToCSharpStrForLoader())] = ListElementType_StringAlias;
                            else
                                list_types[(alias_name, data_type.ToCSharpStrForLoader())] = ListElementType_Normal;
                        }
                        else
                        {
                            if (data_type.IsTuple)
                                list_types[(string.Empty, data_type.ToCSharpStrForLoader())] = ListElementType_Tuple;
                            else
                                list_types[(string.Empty, data_type.ToCSharpStrForLoader())] = ListElementType_Normal;
                        }

                    }

                    if (data_type.IsTuple)
                    {
                        if (alias_name != null)
                            tuple_types[(alias_name, data_type.ToCSharpStrForLoader())] = data_type;
                        else
                            tuple_types[(string.Empty, data_type.ToCSharpStrForLoader())] = data_type;
                    }
                    else if (data_type.type0 == EDataType.String && alias_name != null)
                    {
                        string_alias_types[(alias_name, data_type.ToCSharpStrForLoader())] = true;
                    }
                }
            }


            sw.WriteLine("\t\t#region Tuple Reader");
            foreach (var p in tuple_types)
            {
                _formater["data_type"] = p.Key.name;
                _formater["alias_name"] = p.Key.aliasName;
                if (string.IsNullOrEmpty(p.Key.aliasName))
                {
                    sw.WriteLineExt(_formater, @"
        public static void ReadTuple(ITableTupleReader tupleReader, ref {data_type} v)
        {
            if(tupleReader==null)
                return;
");
                    for (int i = 0; i < p.Value.Count; i++)
                    {
                        sw.WriteLine($"\t\t\tRead(tupleReader,ref v.Item{i + 1});");
                    }
                    sw.WriteLine("\t\t}");
                }
                else
                {
                    sw.WriteLineExt(_formater, @"
        public static void ReadTuple(ITableTupleReader tupleReader, ref {alias_name} v, out {data_type} v2)
        {
            v2=default;
            if(tupleReader==null)
            {
                TableAlias.Create(ref v, false,v2);                
                return;
            }");
                    for (int i = 0; i < p.Value.Count; i++)
                    {
                        sw.WriteLine($"\t\t\tRead(tupleReader,ref v2.Item{i + 1});");
                    }

                    sw.WriteLineExt(_formater, @"
             TableAlias.Create(ref v,false,v2);
        }");
                }
            }
            sw.WriteLine("\t\t#endregion\n");

            sw.WriteLine("\t\t#region List Reader");
            foreach (var p in list_types)
            {
                _formater["data_type"] = p.Key.name;
                _formater["alias_name"] = p.Key.aliasName;

                if (string.IsNullOrEmpty(p.Key.aliasName))
                {
                    _formater["reader_item"] = "Read(listReader, ref item);";
                    if (p.Value == ListElementType_Tuple)
                        _formater["reader_item"] = "ReadTuple(listReader.BeginTuple(), ref item);";

                    sw.WriteLineExt(_formater, @"
        public static void ReadList(ITableRowReader rowReader, ref {data_type}[]v)
        {
            var listReader = rowReader.BeginList();
            int count = listReader != null ? listReader.GetCount() : 0;
            if (count == 0)
                v = Array.Empty<{data_type}>();
            else
            {
                v = new {data_type}[count];
                for (int i = 0; i < count; i++)
                {
                    {data_type} item = default;
                    {reader_item}                    
                    v[i] = item;
                }
            }
        }");
                }
                else
                {
                    _formater["reader_item"] = "ReadTuple(listReader.BeginTuple(), ref item, out v2);";
                    if (p.Value == ListElementType_StringAlias)
                        _formater["reader_item"] = "ReadStringAlias(listReader.BeginTuple(), ref item);";

                    sw.WriteLineExt(_formater, @"
        public static void ReadList(ITableRowReader rowReader, ref {alias_name}[] v, out {data_type} v2)
        {
            v2=default;
            var listReader = rowReader.BeginList();
            int count = listReader != null ? listReader.GetCount() : 0;
            if (count == 0)
                v = Array.Empty<{alias_name}>();
            else
            {
                v = new {alias_name}[count];
                for (int i = 0; i < count; i++)
                {
                    {alias_name} item = default;
                    {reader_item}
                    v[i] = item;
                }
            }
        }");
                }

            }
            sw.WriteLine("\t\t#endregion");

            sw.WriteLine("\t\t#region String alias");
            foreach (var p in string_alias_types)
            {
                _formater["alias_name"] = p.Key.aliasName;

                sw.WriteLineExt(_formater, @"
        public static void ReadStringAlias(ITableDataReader reader, ref {alias_name} v)
        {
            string str= reader.ReadString();
            TableAlias.Create(ref v, str);
        }");
            }
            sw.WriteLine("\t\t#endregion");

            sw.WriteLine("}");
        }
        
        public void _ExportLoaderFunc(FilterTable table, StreamWriter sw)
        {
            List<TableField> header_list = table.FiltedHeader;                        
            _formater["col_count"] = header_list.Count.ToString();
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _config.GetClassName(table.SheetName);

            sw.WriteLineExt(_formater, @"
    public sealed partial class Table{class_name}
    {        
        public override bool LoadFromCsv(ITableReader reader)
        {   
            int col_count = {col_count};

            //Check Header
            var header = reader.ReadHeader();
            if (header==null || header.Count != (col_count*2))
            {
                Log.E(""加载错误 {0},表头数量不对"", SheetName);
                return false;
            }
            bool head_rst = true;
            ");


            for (int i = 0; i < header_list.Count; i++)
            {
                sw.WriteLine("\t\t\thead_rst &= ((header[{0}] == \"{1}\") && (header[{0}+{2}] == \"{3}\"));", i, header_list[i].Name, header_list.Count, header_list[i].DataType.ToCsvStr());
            }

            sw.WriteLineExt(_formater,
                @"
            if (!head_rst)
            {
                Log.E(""加载错误 {0}, 表头不匹配"", SheetName);
                return false;
            }

            //加载数据
            _tempItemsForCsv.Clear();
            for (; ; )
            {
                if (!reader.NextRow(out var rowReader))
                    break;                
                var row = new {class_name}();");

            for (int i = 0; i < header_list.Count; i++)
            {
                TableField field = header_list[i];
                string alias_name = field.AliasCSharp;

                DataType data_type = field.DataType;
                if (data_type.IsList)
                {
                    data_type.IsList = false;
                    if (alias_name != null)
                        sw.WriteLine($"\t\t\t\tTableLoaderCsvUtil.ReadList(rowReader, ref row.{field.Name},out {data_type.ToCSharpStrForLoader()} __{field.Name});");
                    else
                        sw.WriteLine($"\t\t\t\tTableLoaderCsvUtil.ReadList(rowReader, ref row.{field.Name});");
                }
                else if (data_type.IsTuple)
                {
                    if (alias_name != null)
                        sw.WriteLine($"\t\t\t\tTableLoaderCsvUtil.ReadTuple(rowReader.BeginTuple(), ref row.{field.Name}, out {data_type.ToCSharpStrForLoader()} __{field.Name});");
                    else
                        sw.WriteLine($"\t\t\t\tTableLoaderCsvUtil.ReadTuple(rowReader.BeginTuple(), ref row.{field.Name});");
                }
                else if (data_type.type0 == EDataType.String && alias_name != null)
                {
                    sw.WriteLine($"\t\t\t\tTableLoaderCsvUtil.ReadStringAlias(rowReader, ref row.{field.Name});");
                }
                else
                {
                    sw.WriteLine($"\t\t\t\tTableLoaderCsvUtil.Read(rowReader, ref row.{field.Name});");
                }
            }

            sw.WriteLineExt(_formater,
                @"
                _tempItemsForCsv.Add(row);
            }

            //转换数据
            List.Clear();
            if (List.Capacity < _tempItemsForCsv.Count)
                List.Capacity = _tempItemsForCsv.Count;
            foreach (var p in _tempItemsForCsv)
            {
                List.Add(p as {class_name});
            }          
            _tempItemsForCsv.Clear();
            return true;
            }");             

            sw.WriteLine("\t\t}");
        }
    }
}
