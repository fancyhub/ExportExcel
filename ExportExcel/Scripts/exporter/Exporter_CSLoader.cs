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
        public const string C_FILE_NAME = "cs_loader.cs";

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


            sw.WriteLineExt(_formater,@"
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
            sw.WriteLine("}");
        }

        public void _ExportLoaderFunc(FilterTable table, StreamWriter sw)
        {
            List<TableHeaderItem> header = table.Header;
            string multi_name = "";
            if (!table.MultiLang)
                multi_name = "lang = null;";
            _formater["sheet_name_lang"] = multi_name;
            _formater["col_count"] = header.Count.ToString();
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _formater["class_prefix"] + table.SheetName + _formater["class_suffix"];

            sw.WriteLine("");
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


            for (int i = 0; i < header.Count; i++)
            {
                sw.WriteLine("\t\t\thead_rst &= ((header[{0}] == \"{1}\") && (header[{0}+{2}] == \"{3}\"));", i, header[i].Name, header.Count, header[i].DataType.ToCsvStr());
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

            for (int i = 0; i < header.Count; i++)
            {
                sw.WriteLine($"\t\t\t\trowReader.ExRead(ref row.{header[i].Name});");
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
            var dict = new Dictionary<ulong, {class_name}>(list.Count);
            foreach (var p in list)
            {
                ulong key = Table.MakeKey((uint)p.{pk_name}, (uint)p.{pk_sec_name});
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
