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
    public class Exporter_LuaStructDef : IProcessNode
    {
        public const string C_FILE_NAME = "Lua_StructDef.lua";
        public StringFormater _formater = new StringFormater();
        public ExeConfig.LuaConfig _config;
        public E_EXPORT_FLAG _flag;

        public Exporter_LuaStructDef(E_EXPORT_FLAG flag, ExeConfig.LuaConfig config)
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
            if (_config == null || !_config.enable)
                return;

            _formater["class_prefix"] = _config.classPrefix;
            List<FilterTable> tables = FilterTable.Filter(data, _flag);
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("-- 此文件由工具自动生成，请勿手动修改");

            foreach (var table in tables)
            {
                if (data.Config.localization.IsLocalizationSheet(table.SheetName ))
                    continue;

                sw.WriteLine("---@class {0}{1}", _config.classPrefix, table.SheetName);
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
