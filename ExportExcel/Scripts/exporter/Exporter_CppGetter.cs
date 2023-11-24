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
    public class Exporter_CppGetter : IProcessNode
    {
        public const string C_FILE_NAME = "table_getter.h";

        public StringFormater _formater = new StringFormater();

        public Config.CppConfig _config;
        public EExportFlag _flag;

        public Exporter_CppGetter(EExportFlag flag, Config.CppConfig config)
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

            _formater["namespace_start"] = "";
            _formater["namespace_end"] = "";

            if (!string.IsNullOrEmpty(_config.namespaceName))
            {
                _formater["namespace_start"] = "namespace " + _config.namespaceName + " {";
                _formater["namespace_end"] = "}";
            }

            _ExportHeader(tables);
        }

        public void _ExportHeader(List<FilterTable> tables)
        {
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);

            sw.WriteLineExt(_formater, @"
#pragma once
//自动生成的
#include <unordered_map>
#include <tuple>
#include <string>
#include ""table_struct.h""

{namespace_start}
struct TableMgr
{
	std::unordered_map<TableTypeInfo, Table*, TableTypeInfo, TableTypeInfo> AllTableDict;

");

            foreach (var table in tables)
            {
                _formater["class_name"] = _config.GetClassName(table.SheetName);
                var pk = table.PK;
                if (pk==null)
                {
                    sw.WriteLineExt(_formater, "\tTableList<{class_name}> Table{className};");
                }
                else if(!pk.AttrPK.IsCompose())
                {
                    _formater["pk_type"] = pk.DataType.ToCppStr();
                    sw.WriteLineExt(_formater, "\tTableDict<{pk_type},{class_name}> Table{class_name};");
                }
                else
                {
                    _formater["pk_type"] = pk.DataType.ToCppStr();
                    _formater["pk_sec_type"] = pk.AttrPK._sec_key.DataType.ToCppStr();
                    sw.WriteLineExt(_formater, "\tTableDict<std::tuple<{pk_type},{pk_sec_type}>,{class_name}> Table{class_name};");
                }
            }

            sw.WriteLine("\n\tTableMgr():AllTableDict()");
            foreach (var table in tables)
            {
                _formater["class_name"] = _config.GetClassName(table.SheetName);
                _formater["multi_lang"] = table.MultiLang.ToString().ToLower();
                sw.WriteLineExt(_formater, @"       ,Table{class_name}(""{class_name}"",{multi_lang})");                
            }


            sw.WriteLine("\t{");
            foreach (var table in tables)
            {
                _formater["class_name"] = _config.GetClassName(table.SheetName);
                _formater["multi_lang"] = table.MultiLang.ToString().ToLower();
                sw.WriteLineExt(_formater, @"       AllTableDict[typeid({class_name})] = &Table{class_name};");
            }
            sw.WriteLine("\t}");



            foreach (var table in tables)
            {
                _formater["class_name"] = _config.GetClassName(table.SheetName);
                var pk = table.PK;
                if (pk == null)
                    continue;

                _formater["pk_name"] = pk.Name;
                _formater["pk_type"] = pk.DataType.ToCppStr();

                if (!pk.AttrPK.IsCompose())
                {   
                    sw.WriteLineExt(_formater, @"
    const {class_name}* Get{class_name}({pk_type} {pk_name})const
    {
        return Table{class_name}.Get({pk_name});
    }
");
                }
                else
                {
                    _formater["pk_sec_name"] = pk.AttrPK._sec_key.Name;
                    _formater["pk_sec_type"] = pk.AttrPK._sec_key.DataType.ToCppStr();

                    sw.WriteLineExt(_formater, @"
    const {class_name}* Get{class_name}({pk_type} {pk_name},{pk_sec_type} {pk_sec_name})const
    {
        return Table{class_name}.Get(std::tuple<{pk_type},{pk_sec_type}>({pk_name},{pk_sec_name}));
    }
");
                }
            }


            sw.WriteLine("};");

            sw.WriteLine(_formater["namespace_end"]);
            sw.Close();
        }         
    }
}
