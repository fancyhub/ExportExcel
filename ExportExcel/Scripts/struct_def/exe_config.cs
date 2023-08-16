/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/2 16:19:10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExportExcel
{
    /// <summary>
    /// 工具设置类，记录运行工具需要的关键路径等信息。
    /// 序列化文件保存在exe同路径下的 config.json 里。
    /// </summary>
    public class ExeConfig
    {
        public const string C_FILE_NAME = "config.json";
        public static string GetFilePath()
        {
            string dir = Environment.CurrentDirectory;
            return Path.Combine(dir, C_FILE_NAME);
        }

        public List<string> excel_paths = new List<string>();

        public ValidationConfig validation = new ValidationConfig();
        public LocalizationConfig loc = new LocalizationConfig();

        public CsvConfig csv = new CsvConfig();
        public BinConfig bin = new BinConfig();
        public CSharpConfig csharp = new CSharpConfig();
        public LuaConfig lua = new LuaConfig();
        public GoConfig go = new GoConfig();

        public static ExeConfig Load()
        {
            string file_path = GetFilePath();
            if (!File.Exists(file_path))
            {
                ErrSet.E("配置文件不存在 " + file_path);
                return null;
            }

            try
            {
                string text = File.ReadAllText(file_path, System.Text.Encoding.UTF8);
                return JsonConvert.DeserializeObject<ExeConfig>(text);
            }
            catch (Exception ex)
            {
                ErrSet.E("载入配置文件失败 " + file_path);
                ErrSet.E(ex.Message);
                return null;
            }
        }

        public void Save()
        {
            string file_path = GetFilePath();
            File.WriteAllText(file_path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public class LocalizationConfig
        {
            public string sheet_name;
            public string default_lang = "SC";
            public string client_loc_id_prefix; //要导出代码的前缀
            public bool use_hash_id = true;

            public AutoGenKeyConfig auto_gen_key = new AutoGenKeyConfig();

            public class AutoGenKeyConfig
            {
                public string trans_sheet_name;
                public string trans_sheet_export_dir;
            }
        }

        public class ValidationConfig
        {
            public string sheet_name_reg;
            public string col_name_reg;
            public string enum_name_reg;
            public string enum_field_name_reg;
            public string search_file_root = "./";
        }

        public class CSharpConfig
        {
            public string @namespace;
            public string class_prefix;
            public string export_dir_client;
            public string header;

            public string GetClassName(string sheet_name)
            {
                if (string.IsNullOrEmpty(class_prefix))
                    return sheet_name;
                return class_prefix + sheet_name;
            }
        }

        public class BinConfig
        {
            public string export_dir_client;
        }

        public class CsvConfig
        {
            public string export_dir_client;
            public string export_dir_svr;
        }

        public class LuaConfig
        {
            public string class_prefix;
            public string export_dir_client;
        }


        public class GoConfig
        {
            public string package_name;
            public string class_prefix;
            public string export_dir_svr;
        }
    }
}
