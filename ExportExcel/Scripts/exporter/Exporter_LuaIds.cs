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
    public class Exporter_LuaIds : I_ProcessNode
    {
        public string GetName()
        {
            return "Export";
        }
        public void Process(DataBase data)
        {
            if (string.IsNullOrEmpty(data.Config.loc.client_loc_id_prefix)
               || string.IsNullOrEmpty(data.Config.loc.sheet_name))
                return;

            if (string.IsNullOrEmpty(data.Config.lua.export_dir_client))
                return;

            string dest_file_path = System.IO.Path.Combine(data.Config.lua.export_dir_client, "LocDef.lua");
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
                if (!p.Key.StartsWith(data.Config.loc.client_loc_id_prefix))
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
