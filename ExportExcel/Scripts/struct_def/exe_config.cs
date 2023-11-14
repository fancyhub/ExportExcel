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

        public List<string> excelPaths = new List<string>();
        public ValidationConfig validation = new ValidationConfig();
        public LocalizationConfig localization = new LocalizationConfig();
        public LocTransConfig exportLocTrans = new LocTransConfig();
        public RuleConfig exportRule = new RuleConfig();
        public ExportConfig exportClient = new ExportConfig();
        public ExportConfig exportServer = new ExportConfig();


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
                ExeConfig ret = JsonConvert.DeserializeObject<ExeConfig>(text);
                ret.localization.Validate();
                return ret;
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

        public enum ELocalizationMode
        {
            None,
            Normal,
            AutoGenKey,
        }

        public class LocalizationConfig
        {
            public ELocalizationMode Mode;

            public bool enable = false;
            public string sheetName = "";
            public string defaultLang = "";
            public bool useHashId = true;
            public bool autoGenKey = false;

            public bool IsLocalizationSheet(string sheetName)
            {
                switch (Mode)
                {
                    case ELocalizationMode.Normal:
                    case ELocalizationMode.AutoGenKey:
                        return this.sheetName == sheetName;
                    default:
                        return false;
                }
            }

            public string GetDefaultLang()
            {
                switch (Mode)
                {
                    case ELocalizationMode.Normal:
                    case ELocalizationMode.AutoGenKey:
                        return defaultLang;
                    default:
                        return null;
                }
            }

            public string GetLocSheetName()
            {
                switch (Mode)
                {
                    case ELocalizationMode.Normal:
                    case ELocalizationMode.AutoGenKey:
                        return sheetName;
                    default:
                        return null;
                }
            }

            public void Validate()
            {
                Mode = ELocalizationMode.None;
                if (!enable)
                    return;

                if (string.IsNullOrEmpty(sheetName))
                    throw new Exception("Config.json localization/sheetName is null");
                if (string.IsNullOrEmpty(defaultLang))
                    throw new Exception("Config.json localization/defaultLang is null");

                if (!autoGenKey)
                    Mode = ELocalizationMode.Normal;
                else
                    Mode = ELocalizationMode.AutoGenKey;
            }
        }


        public class ValidationConfig
        {
            public string sheetNameReg;
            public string colNameReg;
            public string enumNameReg;
            public string enumFieldNameReg;
            public string searchFileRoot = "./";
        }

        public class ExportConfig
        {
            public CsvConfig csv = new CsvConfig();
            public BinConfig bin = new BinConfig();
            public CSharpConfig csharp = new CSharpConfig();
            public LuaConfig lua = new LuaConfig();
            public GoConfig go = new GoConfig();
        }

        public class CSharpConfig
        {
            public bool enable;
            public string namespaceName;
            public string classPrefix;
            public string dir;
            public string header;
            public string locIdPrefix;

            public string GetClassName(string sheet_name)
            {
                if (string.IsNullOrEmpty(classPrefix))
                    return sheet_name;
                return classPrefix + sheet_name;
            }
        }


        public class LocTransConfig
        {
            public bool enable;
            public string dir;
        }

        public class RuleConfig
        {
            public bool enable;
            public string dir;
        }

        public class BinConfig
        {
            public bool enable;
            public string dir;
        }

        public class CsvConfig
        {
            public bool enable;
            public bool utf8bom;
            public string dir;
        }

        public class LuaConfig
        {
            public bool enable;
            public string classPrefix;
            public string dir;
            public string locIdPrefix;
        }

        public class GoConfig
        {
            public bool enable;
            public string packageName;
            public string classPrefix;
            public string dir;
        }
    }
}
