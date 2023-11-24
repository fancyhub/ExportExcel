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
        public const string C_FILE_NAME = "table_struct_def.lua";
        public Config.LuaConfig _config;
        public EExportFlag _flag;

        public Exporter_LuaStructDef(EExportFlag flag, Config.LuaConfig config)
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
            
            List<FilterTable> tables = FilterTable.Filter(data, _flag);
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("-- 此文件由工具自动生成，请勿手动修改");

            foreach (var table in tables)
            {
                if (data.Config.localization.IsLocalizationSheet(table.SheetName ))
                    continue;

                string class_name = _config.classPrefix + table.SheetName;

                sw.WriteLine($"---@class {class_name}");
                foreach (var c in table.Header)
                {
                    string field_name = c.Name;
                    string field_type = c.DataType.ToLuaStr();
                    sw.WriteLine($"---@field {field_name} {field_type}");
                }
                sw.WriteLine($"local {class_name}={{}}");
                sw.WriteLine();
            }
            sw.Close();
        }
    }
}
