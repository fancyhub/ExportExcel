using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/13 16:43:53
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class Exporter_LuaStructDef : I_ProcessNode
    {
        public const string C_FILE_NAME = "Lua_StructDef.lua";
        public StringFormater _formater = new StringFormater();
        public string GetName()
        {
            return "Export";
        }
        public void Process(DataBase data)
        {
            if (string.IsNullOrEmpty(data.Config.lua.export_dir_client))
                return;

            _formater["class_prefix"] = data.Config.lua.class_prefix;
            List<FilterTable> tables = FilterTable.Filter(data, E_EXPORT_FLAG.client);
            string dest_file_path = System.IO.Path.Combine(data.Config.lua.export_dir_client, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("-- 此文件由工具自动生成，请勿手动修改");

            foreach (var table in tables)
            {
                if (table.SheetName == data.Config.loc.sheet_name)
                    continue;

                sw.WriteLine("---@class {0}{1}", data.Config.lua.class_prefix, table.SheetName);
                foreach (var c in table.Header)
                {
                    string field_name = c.Name;
                    string field_type = c.DataType.ToLuaStr();
                    sw.WriteLine($"---@field {field_name} {field_type}");
                }
                sw.WriteLine("local t={}");
                sw.WriteLine();
            }
            sw.Close();
        }
    }
}
