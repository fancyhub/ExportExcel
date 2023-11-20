using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.Table.PivotTable;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/9 12:12:00
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class ExporterGOStruct : IProcessNode
    {
        public const string C_FILE_NAME = "table_struct.go";
        public StringFormater _formater = new StringFormater();
        public EExportFlag _flag;
        public ExeConfig.GoConfig _config;

        public ExporterGOStruct(EExportFlag flag, ExeConfig.GoConfig config)
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
            if(_config == null || !_config.enable)            
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

            var type_list = GetAllDateTypes(tables);
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

                foreach (TableHeaderItem c in t.Header)
                {
                    //写注释                    
                    if (c.AttrPK != null)
                        sw.WriteLine("\t// " + c.AttrPK.ToString());
                    sw.WriteLine("\t// " + c.Desc.Replace("\n", "\n\t// "));
                    if (c.DataType.enum_type != null)
                    {
                        sw.WriteLine("\t{0} {1}", c.Name, c.DataType.enum_type.Name);
                    }
                    else
                        sw.WriteLine("\t{0} {1}", c.Name, c.DataType.ToGoStr());
                    sw.WriteLine("");
                }
                sw.WriteLine("}");
            }


            sw.WriteLine(@"
type ILogger interface {
	Error(msg string)
}
type CsvLoader func() error
type IDataReader interface {
	Read2Array(file_name string) ([][]string, error)	 
}
type CsvDataMgr struct {
    logger ILogger
    reader IDataReader
    FileName2Func map[string]CsvLoader
    FileName2ListData map[string]interface{}
    FileName2MapData map[string]interface{}
    
"
);
            foreach (var t in tables)
            {
                _formater["class_name"] = _config.GetClassName(t.SheetName);
                _formater["sheet_name"] = t.SheetName;
                sw.WriteLineExt(_formater, "\t{class_name}Mux  sync.RWMutex");
                sw.WriteLineExt(_formater, "\t{class_name}List []{class_name}");
                var pk = t.PK;
                if (t.PK != null)
                {
                    _formater["pk_type"] = pk.DataType.ToGoStr();

                    if (t.PK.AttrPK.IsCompose())
                    {
                        _formater["pk_sec_type"] = pk.AttrPK._sec_key.DataType.ToGoStr();
                        sw.WriteLineExt(_formater, "\t{class_name}Map  map[{pk_type}]map[{pk_sec_type}]*{class_name}");
                    }
                    else
                    {
                        sw.WriteLineExt(_formater, "\t{class_name}Map  map[{pk_type}]*{class_name}");
                    }

                }

                sw.WriteLine();
            }
            sw.WriteLine("}");

            sw.Close();
        }

        public static List<DataType> GetAllDateTypes(List<FilterTable> tables)
        {
            List<DataType> ret = new List<DataType>();

            foreach (var p in tables)
            {
                foreach (var p2 in p._header)
                {
                    DataType t = p2.Item1.DataType;
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
