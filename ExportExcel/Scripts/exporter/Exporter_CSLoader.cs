using System;
using System.Collections.Generic;
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
    public class Exporter_CSLoader : IProcessNode
    {
        public const string C_FILE_NAME = "table_loader.cs";

        public StringFormater _formater = new StringFormater();

        public ExeConfig.CSharpConfig _config;
        public EExportFlag _flag;

        public Exporter_CSLoader(EExportFlag flag, ExeConfig.CSharpConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export";
        }

        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable || !_config.loader.enable)
                return;

            _formater["class_prefix"] = _config.classPrefix;
            _formater["class_suffix"] = _config.classSuffix;
            List<FilterTable> tables = FilterTable.Filter(data, _flag);

            string name_space = _config.namespaceName;
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("//自动生成的");
            sw.WriteLine(_config.header);
            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("namespace " + name_space + "{");

            _ExportLoaderMgr(data, tables, sw);

            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("}");
            sw.Close();

        }

        public void _ExportLoaderMgr(DataBase data_base, List<FilterTable> tables, StreamWriter sw)
        {
            _formater["table_count"] = tables.Count.ToString();
            _formater["lang_count"] = data_base.LangList.Count.ToString();


            sw.WriteLineExt(_formater,
                @"
    public delegate bool CreateTableReader(string sheet_name, string lang_name, out ITableReader reader);
    public delegate Table TableLoader(string lang);
    public struct TableInfo
    {
        public TableLoader Loader;
        public bool MultiLang;
        public TableInfo(TableLoader loader, bool multiLang)
        {
            this.Loader = loader;
            this.MultiLang = multiLang;
        }
    }
    public partial class TableLoaderMgr
    {
        private static List<System.Object> _temp = new List<System.Object>(10000);
        public static List<string> LangList;

        public CreateTableReader CreateTableReader;
        public Dictionary<Type, TableInfo> LoaderDict;
        static TableLoaderMgr()
        {        
            LangList= new List<string>({lang_count});
");

            foreach (var p in data_base.LangList)
            {
                sw.WriteLine($"\t\t\tLangList.Add(\"{p}\");");
            }


            foreach (var p in data_base.EnumDB)
            {
                sw.WriteLine($"\t\t\tEnumConverterMgr.RegFunc((v) => ({p.Key})v, (v) => (int)v);");
            }
            sw.WriteLine("\t\t}");


            sw.WriteLineExt(_formater, @"
        public TableLoaderMgr(CreateTableReader createTableReader)
        {
            CreateTableReader = createTableReader;            
            LoaderDict = new Dictionary<Type, TableInfo>(20+{table_count});
            
");

            foreach (var table in tables)
            {
                var class_name = _formater["class_prefix"] + table.SheetName + _formater["class_suffix"];
                var sheet_name = table.SheetName;
                sw.WriteLine($"\t\t\tLoaderDict.Add(typeof({class_name}),new TableInfo(_Load{table.SheetName},{table.MultiLang.ToString().ToLower()}));");
            }

            sw.WriteLine("\t\t}");

            foreach (var p in tables)
            {
                _ExportLoaderFunc(p, sw);
            }
            _ExportReadFunc(tables, sw);

            sw.WriteLine("\t}");
        }

        public void _ExportReadFunc(List<FilterTable> table_list, StreamWriter sw)
        {
            sw.Write(@"
        #region Base Reader
        private static void _Read(ITableDataReader reader, ref bool v)
        {
            v = reader.ReadBool();
        }
        private static void _Read(ITableDataReader reader, ref int v)
        {
            v = reader.ReadInt32();
        }
        private static void _Read(ITableDataReader reader, ref uint v)
        {
            v = reader.ReadUInt32();
        }
        private static void _Read(ITableDataReader reader, ref long v)
        {
            v = reader.ReadInt64();
        }
        private static void _Read(ITableDataReader reader, ref ulong v)
        {
            v = reader.ReadUInt64();
        }
        private static void _Read(ITableDataReader reader, ref float v)
        {
            v = reader.ReadF32();
        }
        private static void _Read(ITableDataReader reader, ref double v)
        {
            v = reader.ReadF64();
        }
        private static void _Read(ITableDataReader reader, ref string v)
        {
            v = reader.ReadString();
        }
        private static void _Read(ITableDataReader reader, ref LocStr v)
        {
            string s = reader.ReadString();
            v = s;
        }
        private static void _Read(ITableDataReader reader, ref LocId v)
        {
            v = reader.ReadInt32();
        }
        private static void _Read<T>(ITableDataReader reader, ref T v) where T : Enum
        {
            if (!EnumConverterMgr.Convert(reader.ReadInt32(), ref v))
            {
                Log.E(""没有找到类型 {0} 的转换"", typeof(T));
            }
        }
        #endregion
");

            Dictionary<(string aliasName, string name), bool> list_types = new Dictionary<(string, string), bool>();
            Dictionary<(string aliasName, string name), DataType> tuple_types = new Dictionary<(string, string), DataType>();
            foreach (var table in table_list)
            {
                List<TableHeaderItem> header_list = table.Header;
                foreach (var header in header_list)
                {
                    DataType data_type = header.DataType;
                    if (data_type.IsList)
                    {
                        data_type.IsList = false;
                        if (header.AttrTupleAlias != null)
                            list_types[(header.AttrTupleAlias.AliasName, data_type.ToCSharpStr())] = data_type.IsTuple;
                        else
                            list_types[(string.Empty, data_type.ToCSharpStr())] = data_type.IsTuple;
                    }

                    if (data_type.IsTuple)
                    {
                        if (header.AttrTupleAlias != null)
                            tuple_types[(header.AttrTupleAlias.AliasName, data_type.ToCSharpStr())] = data_type;
                        else
                            tuple_types[(string.Empty, data_type.ToCSharpStr())] = data_type;
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
        private static void _ReadTuple(ITableTupleReader tupleReader, ref {data_type} v)
        {
            if(tupleReader==null)
                return;
");
                    for (int i = 0; i < p.Value.Count; i++)
                    {
                        sw.WriteLine($"\t\t\t_Read(tupleReader,ref v.Item{i + 1});");
                    }
                    sw.WriteLine("\t\t}");
                }
                else
                {
                    sw.WriteLineExt(_formater, @"
        private static void _ReadTuple(ITableTupleReader tupleReader, ref {alias_name} v, out {data_type} v2)
        {
            v2=default;
            if(tupleReader==null)
            {
                v= {alias_name}.CreateInst(false,v2);
                return;
            }");
                    for (int i = 0; i < p.Value.Count; i++)
                    {
                        sw.WriteLine($"\t\t\t_Read(tupleReader,ref v2.Item{i + 1});");
                    }

                    sw.WriteLineExt(_formater,@"
             v = {alias_name}.CreateInst(true,v2);
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
                    _formater["reader_item"] = "_Read(listReader, ref item);";
                    if (p.Value)
                        _formater["reader_item"] = "_ReadTuple(listReader.BeginTuple(), ref item);";

                    sw.WriteLineExt(_formater, @"
        private static void _ReadList(ITableRowReader rowReader, ref {data_type}[]v)
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
                    sw.WriteLineExt(_formater, @"
        private static void _ReadList(ITableRowReader rowReader, ref {alias_name}[] v, out {data_type} v2)
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
                    _ReadTuple(listReader.BeginTuple(), ref item, out v2);
                    v[i] = item;
                }
            }
        }");
                }

            }
            sw.WriteLine("\t\t#endregion");
        }

        public void _ExportLoaderFunc(FilterTable table, StreamWriter sw)
        {
            List<TableHeaderItem> header_list = table.Header;
            string multi_name = "";
            if (!table.MultiLang)
                multi_name = "lang = null;";
            _formater["sheet_name_lang"] = multi_name;
            _formater["col_count"] = header_list.Count.ToString();
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _formater["class_prefix"] + table.SheetName + _formater["class_suffix"];

            sw.WriteLineExt(_formater, @"
        private Table _Load{sheet_name}(string lang)
        {
            string sheet_name = ""{sheet_name}"";
            {sheet_name_lang}
            int col_count = {col_count};

            if(!CreateTableReader(sheet_name,lang,out var reader))
                return null;

            //Check Header
            var header = reader.ReadHeader();
            if (header==null || header.Count != (col_count*2))
            {
                Log.E(""加载错误 {0},表头数量不对"", sheet_name);
                return null;
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
                Log.E(""加载错误 {0}, 表头不匹配"", sheet_name);
                return null;
            }

            //加载数据
            _temp.Clear();
            for (; ; )
            {
                if (!reader.NextRow(out var rowReader))
                    break;                
                var row = new {class_name}();");

            for (int i = 0; i < header_list.Count; i++)
            {
                var header = header_list[i];
                var data_type = header.DataType;
                if (data_type.IsList)
                {
                    data_type.IsList = false;
                    if (header.AttrTupleAlias != null)
                        sw.WriteLine($"\t\t\t\t_ReadList(rowReader, ref row.{header.Name},out {data_type.ToCSharpStr()} __{header.Name});");
                    else
                        sw.WriteLine($"\t\t\t\t_ReadList(rowReader, ref row.{header.Name});");
                }
                else if (data_type.IsTuple)
                {
                    if (header.AttrTupleAlias != null)
                        sw.WriteLine($"\t\t\t\t_ReadTuple(rowReader.BeginTuple(), ref row.{header.Name}, out {data_type.ToCSharpStr()} __{header.Name});");
                    else
                        sw.WriteLine($"\t\t\t\t_ReadTuple(rowReader.BeginTuple(), ref row.{header.Name});");
                }
                else
                {
                    sw.WriteLine($"\t\t\t\t_Read(rowReader, ref row.{header.Name});");
                }
            }

            sw.WriteLineExt(_formater,
                @"
                _temp.Add(row);
            }

            //转换数据
            List<{class_name}> list = new List<{class_name}>(_temp.Count);
            foreach (var p in _temp)
            {
                list.Add(p as {class_name});
            }            
            ");

            TableHeaderItem pk = table.PK;
            _formater["pk_name"] = "";
            if (pk != null)
            {
                _formater["pk_name"] = pk.Name;
                _formater["pk_type"] = pk.DataType.ToCSharpStr();

                if (!pk.AttrPK.IsCompose())
                {
                    sw.WriteLineExt(_formater,
                        @"
            var dict = new Dictionary<{pk_type}, {class_name}>(list.Count);
            foreach (var p in list)
            {
                if (dict.ContainsKey(p.{pk_name}))
                {
                    Log.E(""{0} Contain Multi Id: {1}, 如果允许ID重复, 修改表格"", typeof({class_name}), p.{pk_name});
                    continue;
                }
                dict.Add(p.{pk_name}, p);
            }
            return Table.Create(list,dict);
                ");
                }
                else
                {
                    _formater["pk_sec_name"] = pk.AttrPK._sec_key.Name;
                    _formater["pk_sec_type"] = pk.AttrPK._sec_key.DataType.ToCSharpStr();
                    sw.WriteLineExt(_formater,
                       @"
            var dict = new Dictionary<({pk_type},{pk_sec_type}), {class_name}>(list.Count);
            foreach (var p in list)
            {
                ({pk_type},{pk_sec_type}) key = (p.{pk_name}, p.{pk_sec_name});
                if (dict.ContainsKey(key))
                {
                    Log.E(""{0} Contain Multi Id: {1},{2}, 如果允许ID重复, 修改表格"", typeof({class_name}), p.{pk_name},p.{pk_sec_name});
                    continue;
                }
                dict.Add(key, p);
            }
            return Table.Create(list,dict);
                ");
                }
            }
            else
            {
                sw.WriteLine("return Table.Create(list);");
            }
            sw.WriteLine("\t\t}");
        }
    }
}
