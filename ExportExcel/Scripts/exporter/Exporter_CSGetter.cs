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
    public class Exporter_CSGetter : IProcessNode
    {
        public const string C_FILE_NAME = "table_getter.cs";

        public StringFormater _formater = new StringFormater();

        public Config.CSharpConfig _config;
        public EExportFlag _flag;

        public Exporter_CSGetter(EExportFlag flag, Config.CSharpConfig config)
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
            if (_config == null || !_config.enable || !_config.getter.enable)
                return;
            
            List<FilterTable> tables = FilterTable.Filter(data, _flag);

            string name_space = _config.namespaceName;
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("//自动生成的");
            sw.WriteLine(_config.header);
            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("namespace " + name_space + "{");

            _ExportGetterMgr(tables, sw);

            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("}");
            sw.Close();

        }

        public void _ExportGetterMgr(List<FilterTable> tables, StreamWriter sw)
        {
            _formater["table_count"] = tables.Count.ToString();
            _formater["class_name"] = _config.getter.className;
            _formater["static_flag"] = "";
            if(_config.getter.useStatic)
                _formater["static_flag"] = "static";
            sw.WriteLineExt(_formater,
                @"    
    public partial class {class_name}
    {   
");

            foreach (var p in tables)
            {
                _ExportGetterFunc(p, sw);
            }
            sw.WriteLine("}");
        }

        public void _ExportGetterFunc(FilterTable table, StreamWriter sw)
        {
            List<TableHeaderItem> header = table.Header;
            string multi_name = "";
            if (!table.MultiLang)
                multi_name = "lang = null;";
            _formater["sheet_name_lang"] = multi_name;
            _formater["col_count"] = header.Count.ToString();
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _config.GetClassName(table.SheetName);

            TableHeaderItem pk = table.PK;

            sw.WriteLineExt(_formater,
                @"      
        public {static_flag} List<{class_name}> Get{class_name}List()
        {
            return FindTable<{class_name}>()?.GetList<{class_name}>();
        }
        ");

            _formater["pk_name"] = "";
            if (pk != null)
            {
                _formater["pk_name"] = pk.Name;
                _formater["pk_type"] = pk.DataType.ToCSharpStr();

                if (!pk.AttrPK.IsCompose())
                {
                    sw.WriteLineExt(_formater,
                        @"
        public {static_flag} {class_name} Get{class_name}({pk_type} {pk_name})
        {
            return FindTable<{class_name}>()?.Get<{pk_type},{class_name}>({pk_name});
        }

        public {static_flag} Dictionary<{pk_type}, {class_name}> Get{class_name}Dict()
        {
            return FindTable<{class_name}>()?.GetDict<{pk_type}, {class_name}>();
        }
        ");
                }
                else
                {
                    _formater["pk_sec_name"] = pk.AttrPK._sec_key.Name;
                    _formater["pk_sec_type"] = pk.AttrPK._sec_key.DataType.ToCSharpStr();
                    sw.WriteLineExt(_formater,
                    @"
        public {static_flag} {class_name} Get{class_name}({pk_type} {pk_name},{pk_sec_type} {pk_sec_name})
        {        
            return FindTable<{class_name}>()?.Get<({pk_type},{pk_sec_type}), {class_name}>(({pk_name},{pk_sec_name}));
        }

        public {static_flag} Dictionary<({pk_type},{pk_sec_type}), {class_name}> Get{class_name}Dict()
        {
            return FindTable<{class_name}>()?.GetDict<({pk_type},{pk_sec_type}), {class_name}>();            
        }
        ");
                }
            }
        }
    }
}
