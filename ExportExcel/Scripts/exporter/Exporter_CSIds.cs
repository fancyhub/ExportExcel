using System;
using System.Collections.Generic;
using System.IO;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/12 11:06:16
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class Exporter_CSIds : I_ProcessNode
    {
        public Exporter_CSIds()
        {
        }
        public string GetName()
        {
            return "Export";
        }

        public void Process(DataBase data)
        {
            if (string.IsNullOrEmpty(data.Config.loc.client_loc_id_prefix)
                || string.IsNullOrEmpty(data.Config.loc.sheet_name))
                return;

            string dest_file_path = System.IO.Path.Combine(data.Config.csharp.export_dir_client, "LocDef.cs");
            FileUtil.CreateFileDir(dest_file_path);
            string name_space = data.Config.csharp.@namespace;
            using (StreamWriter sw = new StreamWriter(dest_file_path))
            {
                sw.WriteLine("//自动生成的");
                sw.WriteLine(data.Config.csharp.header);

                if (!string.IsNullOrEmpty(name_space))
                    sw.WriteLine("namespace " + name_space + "{");

                _export_loc_ids(data, sw);

                if (!string.IsNullOrEmpty(name_space))
                    sw.WriteLine("}");
            }
        }

        public void _export_loc_ids(DataBase data, StreamWriter sw)
        {
            sw.WriteLine("\tpublic static class LocDef\n\t{");

            Dictionary<string, string> dict = data.LangDefault;
            foreach (var p in dict)
            {
                if (!p.Key.StartsWith(data.Config.loc.client_loc_id_prefix))
                    continue;

                //写注释
                sw.WriteLine("\t\t/// <summary>");
                sw.WriteLine("\t\t/// " + p.Value.Replace("\n", "\n\t\t/// "));
                sw.WriteLine("\t\t/// </summary>");
                sw.WriteLine("\t\tpublic const string {0}=\"{1}\";", p.Key, p.Key);
            }
            sw.WriteLine("\t}");
        }
    }
}
