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
    public class Exporter_CSLoader : I_ProcessNode
    {
        public const string C_FILE_NAME = "cs_loader.cs";

        public StringFormater _formater = new StringFormater();

        public Exporter_CSLoader()
        {
        }
        public string GetName()
        {
            return "Export";
        }

        public void Process(DataBase data)
        {
            var config = data.Config.csharp;
            string name_space = config.@namespace;
            string dest_file_path = System.IO.Path.Combine(data.Config.csharp.export_dir_client, C_FILE_NAME);
            _formater["class_prefix"] = data.Config.csharp.class_prefix;

            FileUtil.CreateFileDir(dest_file_path);
            List<FilterTable> tables = FilterTable.Filter(data, E_EXPORT_FLAG.client);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("//自动生成的");
            sw.WriteLine(data.Config.csharp.header);

            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("namespace " + name_space + "{");

            _export_mgr(tables, sw);
            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("}");
            sw.Close();
        }

        public void _export_mgr(List<FilterTable> tables, StreamWriter sw)
        {
            _formater["table_count"] = tables.Count.ToString();


            sw.WriteLineExt(_formater,
                @"
    public partial class TableMgr
    {
        private static List<System.Object> _temp = new List<System.Object>(10000);
        public TableMgr()
        {
            _all = new Dictionary<Type, Table>(20+{table_count});
            _loader_dict = new Dictionary<Type, TableLoader>(20+{table_count});
");

            foreach (var table in tables)
            {
                _formater["sheet_name"] = table.SheetName;
                _formater["class_name"] = _formater["class_prefix"] + table.SheetName;

                sw.WriteLineExt(_formater, "\t\t\t_loader_dict.Add(typeof({class_name}),_Load{sheet_name});");

                if (table.MultiLang)
                {
                    sw.WriteLineExt(_formater, "\t\t\t_lang_set.Add(typeof({class_name}));");
                }
            }

            sw.WriteLine("\t\t}");

            foreach (var p in tables)
            {
                _export_load_func(p, sw);
            }
            sw.WriteLine("}");
        }

        public void _export_load_func(FilterTable table, StreamWriter sw)
        {
            List<TableHeaderItem> header = table.Header;
            string multi_name = "";
            if (!table.MultiLang)
                multi_name = "lang = null;";
            _formater["sheet_name_lang"] = multi_name;
            _formater["col_count"] = header.Count.ToString();
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _formater["class_prefix"] + table.SheetName;

            sw.WriteLine("");
            sw.WriteLineExt(_formater, @"
        private static Table _Load{sheet_name}(string lang)
        {
            string sheet_name = ""{sheet_name}"";
            {sheet_name_lang}
            int col_count = {col_count};

            if(!_CreateReader(sheet_name,lang,out var reader))
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
                if (!reader.NextRow())
                    break;                
                var row = new {class_name}();");

            for (int i = 0; i < header.Count; i++)
            {
                sw.WriteLine($"reader.ExRead(ref row.{header[i].Name});");
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
                ulong key = _MakeKey((uint)p.{pk_name}, (uint)p.{pk_sec_name});
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

            sw.WriteLineExt(_formater,
                    @"      
        public static List<{class_name}> Get{class_name}List()
        {
            return GetList<{class_name}>();
        }
        ");

            if (pk != null)
            {
                if (!pk.AttrPK.IsCompose())
                {
                    sw.WriteLineExt(_formater,
                        @"
        public static {class_name} Get{class_name}({pk_type} {pk_name})
        {
            return Get<{pk_type},{class_name}>({pk_name});
        }

        public static Dictionary<{pk_type}, {class_name}> Get{class_name}Dict()
        {
            return GetDict<{pk_type}, {class_name}>();
        }
        ");
                }
                else
                {
                    sw.WriteLineExt(_formater,
                    @"
        public static {class_name} Get{class_name}({pk_type} {pk_name},{pk_sec_type} {pk_sec_name})
        {        
            return Get<{class_name}>((uint){pk_name},(uint){pk_sec_name});
        }

        public static Dictionary<ulong, {class_name}> Get{class_name}Dict()
        {
            return GetDict<ulong, {class_name}>();
        }
        ");
                }
            }
        }
    }
}
