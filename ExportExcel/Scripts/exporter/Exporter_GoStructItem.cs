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
    public class Exporter_GoStructItem : IProcessNode
    {
        public const string C_FILE_NAME = "struct_item.go";
        public StringFormater _formater = new StringFormater();
        public EExportFlag _flag;
        public Config.GoConfig _config;

        public Exporter_GoStructItem(EExportFlag flag, Config.GoConfig config)
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
                sw.WriteLine(")\n");
                sw.WriteLine();
            }

            var type_list = _GetAllTupleTypes(tables);
            foreach (var p in type_list)
            {
                string name = p.ToGoStr();
                sw.WriteLine($"type {name} struct{{");

                for (int i = 0; i < p.Count; i++)
                {
                    sw.WriteLine("\tItem{0} {1} `json:\"Item{0}\"`", i, p.Get(i).ToGoStr());
                }
                sw.WriteLine("}\n");
            }

            foreach (var p in _GetAllTupleTypes2(tables))
            {
                sw.WriteLine($"type {p.Key} struct{{");

                var aliasItem = data.AliasDB.Find(p.Key);
                int count = Math.Min(aliasItem.Fields.Length, p.Value.Count);
                for (int i = 0; i < count; i++)
                {
                    sw.WriteLine("\t{0} {1} `json:\"{2}\"`", DataTypeUtil.GoUpFirstCase(aliasItem.Fields[i]), p.Value.Get(i).ToGoStr(), aliasItem.Fields[i]);
                }
                sw.WriteLine("}\n");
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
                    sw.WriteLine("\t{0} {1} `csv:\"{2}\" json:\"{2}\"`", DataTypeUtil.GoUpFirstCase(c.Name), c.ToGoStr(), c.Name);
                    sw.WriteLine("");
                }
                sw.WriteLine("}\n");
            }

            sw.Close();
        }

        private static Dictionary<string, DataType> _GetAllTupleTypes2(List<FilterTable> tables)
        {
            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();

            foreach (var filterTable in tables)
            {
                foreach (var tableField in filterTable.FiltedHeader)
                {
                    DataType t = tableField.DataType;
                    if (!t.IsTuple)
                        continue;

                    if (tableField.AttrAlias == null || !string.IsNullOrEmpty(tableField.AttrAlias.Go))
                        continue;
                    t.IsList = false;

                    if (!ret.TryGetValue(tableField.AttrAlias.Name, out var old))
                        ret[tableField.AttrAlias.Name] = t;

                    if (old.Count < t.Count) //取数量最多的
                        ret[tableField.AttrAlias.Name] = t;
                }
            }
            return ret;
        }

        private static List<DataType> _GetAllTupleTypes(List<FilterTable> tables)
        {
            List<DataType> ret = new List<DataType>();

            foreach (var filterTable in tables)
            {
                foreach (var tableField in filterTable.FiltedHeader)
                {
                    DataType t = tableField.DataType;
                    if (!t.IsTuple)
                        continue;
                    if (tableField.AttrAlias != null)
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
