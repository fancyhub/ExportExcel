using System;
using System.Collections.Generic;
using System.IO;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/16 8:43:35
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class Exporter_LuaIds : IProcessNode
    {
        public ExeConfig.LuaConfig _config;
        public EExportFlag _flag;

        public Exporter_LuaIds(EExportFlag flag, ExeConfig.LuaConfig config)
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
            if (_config == null || !_config.enable || string.IsNullOrEmpty(_config.locIdStartWith))
                return;
            if (data.Config.localization.Mode == ExeConfig.ELocalizationMode.None)
                return;

            string dest_file_path = System.IO.Path.Combine(_config.dir, "LocDef.lua");
            FileUtil.CreateFileDir(dest_file_path);

            using (StreamWriter sw = new StreamWriter(dest_file_path))
            {
                sw.WriteLine("-- 此文件由工具自动生成，请勿手动修改");
                _export_loc_ids(data, sw);
            }
        }

        public void _export_loc_ids(DataBase data, StreamWriter sw)
        {
            Dictionary<string, string> dict = data.LangDefault;
            sw.WriteLine("LocDef = {");
            foreach (var p in dict)
            {
                if (!p.Key.StartsWith(_config.locIdStartWith))
                    continue;

                //写注释
                sw.WriteLine("\t--- " + p.Value.Replace("\n", "\n\t--- "));
                sw.WriteLine("\t---@type LocStr");
                sw.WriteLine("\t{0}=\"{1}\";", p.Key, p.Value);
            }
            sw.WriteLine("}");
        }
    }
}
