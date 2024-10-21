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
    public class Exporter_CSLoaderFromJson : IProcessNode
    {
        public const string C_FILE_NAME = "table_loader_json.cs";

        public StringFormater _formater = new StringFormater();

        public Config.CSharpConfig _config;
        public EExportFlag _flag;
        public DBAlias _AliasDB;

        public Exporter_CSLoaderFromJson(EExportFlag flag, Config.CSharpConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export C# Loader Json";
        }

        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable || !_config.loader.json)
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

            foreach (var p in tables)
            {
                _ExportLoaderFunc(p, sw);
            }

            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("}");
            sw.Close();
        }

        public void _ExportTableBase(DataBase data_base, List<FilterTable> tables, StreamWriter sw)
        {   
            sw.WriteLineExt(_formater, @"
   public partial class Table
    {
        public abstract bool LoadFromJson(Newtonsoft.Json.JsonSerializer jsonSerializer, Newtonsoft.Json.JsonReader reader);
    }
");
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
        public override bool LoadFromJson(Newtonsoft.Json.JsonSerializer jsonSerializer, Newtonsoft.Json.JsonReader reader)
        {
            var items=  jsonSerializer.Deserialize<List<{class_name}>>(reader);
            if (items == null)
                return false;
            List = items;
            return true;
        }
    }");
        }
    }
}
