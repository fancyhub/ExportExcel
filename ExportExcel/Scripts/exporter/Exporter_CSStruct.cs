using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/7 16:50:44
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    //导出 csharp的数据结构
    public class Exporter_CSStruct : IProcessNode
    {
        public const string C_FILE_NAME = "table_struct.cs";
        public StringFormater _formater = new StringFormater();
        public Config.CSharpConfig _config;
        public EExportFlag _flag;

        public Exporter_CSStruct(EExportFlag flag, Config.CSharpConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export CS";
        }

        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable)
                return;            

            string name_space = _config.namespaceName;
            List<FilterTable> tables = FilterTable.Filter(data, _flag);
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("//自动生成的");
            sw.WriteLine(_config.header);

            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("namespace " + name_space + "{");

            //生成枚举结构
            foreach (var p in data.EnumDB)
            {
                sw.WriteLine(@"
    public enum {enum_name}
    {".Replace("{enum_name}", p.Key));

                List<EnumField> enum_field_list = p.Value.GetAllFields().ToList();
                enum_field_list.Sort((a, b) =>
                {
                    return a.Val - b.Val;
                });

                foreach (var p2 in enum_field_list)
                {
                    sw.WriteLine("\t\t/// <summary>");
                    sw.WriteLine("\t\t/// " + p2.ExcelVal.Replace("\n", "\n\t\t/// "));
                    sw.WriteLine("\t\t/// </summary>");
                    sw.WriteLine("\t\t{0} = {1},", p2.Name, p2.Val);
                }
                sw.WriteLine("\t}");
            }

            _formater["parent_class"] = "";
            if (!string.IsNullOrEmpty(_config.parentClass))
            {
                _formater["parent_class"] = " : " + _config.parentClass;
            }
            //生成类结构
            foreach (FilterTable table in tables)
            {
                _formater["sheet_name"] = table.SheetName;
                _formater["class_name"] = _config.GetClassName(table.SheetName);;

                List<TableField> header = table.GetHeader();

                sw.WriteLineExt(_formater,
                    @"
    public sealed partial class {class_name} {parent_class}
    {");

                foreach (TableField col in header)
                {
                    //写注释
                    sw.WriteLine("\t\t/// <summary>");
                    if (col.AttrPK != null)
                        sw.WriteLine("\t\t/// " + col.AttrPK.ToString());
                    sw.WriteLine("\t\t/// " + col.Desc.Replace("\n", "\n\t\t/// "));
                    sw.WriteLine("\t\t/// </summary>");
                    sw.WriteLine("\t\tpublic " + col.ToCSharpStr() + " " + col.Name + ";");
                }
                sw.WriteLine(@"
    }");
            }
            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("}");
            sw.Close();
        }
    }
}
