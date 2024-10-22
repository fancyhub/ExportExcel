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
    public class Exporter_CppStructItem : IProcessNode
    {
        public const string C_FILE_NAME = "struct_item.h";
        public StringFormater _formater = new StringFormater();
        public Config.CppConfig _config;
        public EExportFlag _flag;

        public Exporter_CppStructItem(EExportFlag flag, Config.CppConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export C++ Struct Item";
        }

        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable)
                return;

            _formater["namespace_start"] = "";
            _formater["namespace_end"] = "";

            if (!string.IsNullOrEmpty(_config.namespaceName))
            {
                _formater["namespace_start"] = "namespace " + _config.namespaceName + " {";
                _formater["namespace_end"] = "}";
            }

			_formater["user_header"] = _config.header;
			_formater["parent_class"] = "";
			if(!string.IsNullOrEmpty(_config.itemParentClass))
			{
                _formater["parent_class"] = " : public "+ _config.itemParentClass;
            }

            string name_space = _config.namespaceName;
            List<FilterTable> tables = FilterTable.Filter(data, _flag);
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
          
            sw.WriteLineExt(_formater, @"
#pragma once
//自动生成的
#include <tuple>
#include <vector>
#include <string>
#include <unordered_map>
#include <typeinfo>
{user_header}
{namespace_start}
");

            //生成枚举结构
            foreach (var p in data.EnumDB)
            {
                sw.WriteLine(@"
enum class {enum_name}
{".Replace("{enum_name}", p.Key));

                List<EnumField> enum_field_list = p.Value.GetAllFields().ToList();
                enum_field_list.Sort((a, b) =>
                {
                    return a.Val - b.Val;
                });

                foreach (var p2 in enum_field_list)
                {
                    sw.WriteLine("\t//  " + p2.ExcelVal.Replace("\n", "\n\t\t/// "));
                    sw.WriteLine("\t{0} = {1},", p2.Name, p2.Val);
					sw.WriteLine();
                }
                sw.WriteLine("};");
            }

            //生成类结构
            foreach (FilterTable table in tables)
            {
                _formater["sheet_name"] = table.SheetName;
				_formater["class_name"] = _config.GetClassName(table.SheetName);

                List<TableField> header = table.FiltedHeader;

                sw.WriteLineExt(_formater,
                    @"
struct {class_name} {parent_class}
{");

                foreach (TableField col in header)
                {
                    //写注释
                    if (col.AttrPK != null)
                        sw.WriteLine("\t// " + col.AttrPK.ToString());
                    sw.WriteLine("\t// " + col.Desc.Replace("\n", "\n\t\t/// "));
                    sw.WriteLine("\t" + col.ToCppStr() + " " + col.Name + ";\n");
                }
                sw.WriteLine("};");
            }
            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("}");
            sw.Close();
        }
    }
}
