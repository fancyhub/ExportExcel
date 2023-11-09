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
    public class Exporter_CSIds : IProcessNode
    {
        public ExeConfig.CSharpConfig _config;
        public E_EXPORT_FLAG _flag;
        public Exporter_CSIds(E_EXPORT_FLAG flag, ExeConfig.CSharpConfig config)
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
            if (_config == null || !_config.enable || _config.locIdPrefix==null)
                return;

            if (data.Config.localization.EMode == ExeConfig.ELocalizationMode.None)
                return;

            string dest_file_path = System.IO.Path.Combine(_config.dir, "LocDef.cs");
            FileUtil.CreateFileDir(dest_file_path);
            string name_space = _config.namespaceName;
            using (StreamWriter sw = new StreamWriter(dest_file_path))
            {
                sw.WriteLine("//自动生成的");
                sw.WriteLine("using System;");

                if (!string.IsNullOrEmpty(name_space))
                    sw.WriteLine("namespace " + name_space + "{");

                if (!data.Config.localization.useHashId)
                    _export_loc_id_str(data, sw);
                else 
                    _export_loc_id_enum(data, sw);

                if (!string.IsNullOrEmpty(name_space))
                    sw.WriteLine("}");
            }
        }

        public void _export_loc_id_enum(DataBase data, StreamWriter sw)
        {
            sw.WriteLine("\tpublic enum ELocDef : int\n\t{");

            Dictionary<string, string> dict = data.LangDefault;
            foreach (var p in dict)
            {
                if (!p.Key.StartsWith(_config.locIdPrefix))
                    continue;

                //写注释
                sw.WriteLine("\t\t/// <summary>");
                sw.WriteLine("\t\t/// " + p.Value.Replace("\n", "\n\t\t/// "));
                sw.WriteLine("\t\t/// </summary>");
                sw.WriteLine("\t\t{0} = {1},", p.Key, LocStrUtil.ToLocId(p.Key));
            }
            sw.WriteLine("\t}");
        }

        public void _export_loc_id_str(DataBase data, StreamWriter sw)
        {
            sw.WriteLine("\tpublic static class LocDef\n\t{");

            Dictionary<string, string> dict = data.LangDefault;
            foreach (var p in dict)
            {
                if (!p.Key.StartsWith(_config.locIdPrefix))
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
