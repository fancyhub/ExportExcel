using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/9 12:12:00
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class Exporter_GoStruct : IProcessNode
    {
        public const string C_FILE_NAME = "table_struct.go";
        public StringFormater _formater = new StringFormater();
        public EExportFlag _flag;
        public Config.GoConfig _config;

        public Exporter_GoStruct(EExportFlag flag, Config.GoConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export Go Struct";
        }
        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable)
                return;


            string package_name = _config.packageName;
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            List<FilterTable> tables = FilterTable.Filter(data, _flag);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("package " + package_name);
            sw.WriteLine(@"
import (
    ""sync""
);");

            foreach (var p in data.EnumDB)
            {
                sw.WriteLine("// " + p.Key);
                sw.WriteLine("type {0} int32", p.Key);
                sw.WriteLine("const (");
                foreach (var p2 in p.Value.GetAllFields())
                {
                    sw.WriteLine("\t// " + p2.ExcelVal);
                    sw.WriteLine("\t{0}_{1} {0} = {2}", p.Key, p2.Name, p2.Val);
                }
                sw.WriteLine(")");
                sw.WriteLine();
            }

            var type_list = _GetAllTupleTypes(tables);
            foreach (var p in type_list)
            {
                string name = p.ToGoStr();
                sw.WriteLine($"type {name} struct{{");

                for (int i = 0; i < p.Count; i++)
                {
                    sw.WriteLine($"\tItem{i} {p.Get(i).ToGoStr()}");
                }
                sw.WriteLine("}");
            }

            foreach (FilterTable t in tables)
            {
                _formater["class_name"] = _config.GetClassName(t.SheetName);
                sw.WriteLineExt(_formater, "type {class_name} struct {");

                foreach (TableField c in t.FiltedHeader)
                {
                    //写注释                    
                    if (c.AttrPK != null)
                        sw.WriteLine("\t// " + c.AttrPK.ToString());
                    sw.WriteLine("\t// " + c.Desc.Replace("\n", "\n\t// "));
                    if (c.AttrEnum != null)
                    {
                        sw.WriteLine("\t{0} {1}", c.Name, c.AttrEnum.Name);
                    }
                    else
                        sw.WriteLine("\t{0} {1}", c.Name, c.ToGoStr());
                    sw.WriteLine("");
                }
                sw.WriteLine("}");
            }  

            sw.Close();
        }

        private static List<DataType> _GetAllTupleTypes(List<FilterTable> tables)
        {
            List<DataType> ret = new List<DataType>();

            foreach (var p in tables)
            {
                foreach (var p2 in p.FiltedHeader)
                {
                    DataType t = p2.DataType;
                    if (!t.IsTuple)
                        continue;

                    t.IsList = false;

                    bool found = false;
                    foreach (var p3 in ret)
                    {
                        if (p3.IsEuqal(t))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        ret.Add(t);
                }
            }
            return ret;
        }
    }
}
