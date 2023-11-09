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
            public ELocalizationMode EMode;

            public string mode;
            public bool useHashId = true;
            public LocalizationModeNormalConfig modeNormal = new LocalizationModeNormalConfig();
            public LocalizationModeAutoGenKeyConfig modeAutoGenKey = new LocalizationModeAutoGenKeyConfig();


            public bool IsLocalizationSheet(string sheetName)
            {
                switch (EMode)
                {
                    case ELocalizationMode.Normal:
                        return modeNormal.sheetName == sheetName;
                    case ELocalizationMode.AutoGenKey:
                        if (sheetName == modeAutoGenKey.sheetName || sheetName == modeAutoGenKey.transSheetName)
                            return true;
                        return false;
                    default:
                        return false;
                }
            }

            public string GetDefaultLang()
            {
                switch (EMode)
                {
                    case ELocalizationMode.Normal:
                        return modeNormal.defaultLang;
                    case ELocalizationMode.AutoGenKey:
                        return modeAutoGenKey.defaultLang;
                    default:
                        return null;
                }
            }

            public string GetLocSheetName()
            {
                switch (EMode)
                {
                    case ELocalizationMode.Normal:
                        return modeNormal.sheetName ;
                    case ELocalizationMode.AutoGenKey:
                        return modeAutoGenKey.sheetName;
                    default:
                        return null;
                }
            }

            public void Validate()
            {
                if (mode == "normal")
                {
                    if (string.IsNullOrEmpty(modeNormal.sheetName))                    
                        throw new Exception("Config.json localization/modeNormal/sheetName is null");                    
                    if (string.IsNullOrEmpty(modeNormal.defaultLang))
                        throw new Exception("Config.json localization/modeNormal/defaultLang is null");

                    EMode = ELocalizationMode.Normal;
                }
                else if (mode == "auto_gen_key")
                {
                    if (string.IsNullOrEmpty(modeAutoGenKey.sheetName))
                        throw new Exception("Config.json localization/modeAutoGenKey/sheetName is null");
                    if (string.IsNullOrEmpty(modeAutoGenKey.defaultLang))
                        throw new Exception("Config.json localization/modeAutoGenKey/defaultLang is null");
                    if (string.IsNullOrEmpty(modeAutoGenKey.transSheetName))
                        throw new Exception("Config.json localization/modeAutoGenKey/transSheetName is null");
                    if(modeAutoGenKey.sheetName == modeAutoGenKey.transSheetName)
                        throw new Exception("Config.json localization/modeAutoGenKey/sheetName  == localization/modeAutoGenKey/transSheetName ");

                    EMode = ELocalizationMode.AutoGenKey;
                }
                else
                    EMode = ELocalizationMode.None;
            }
        }

        public class LocalizationModeNormalConfig
        {
            public string sheetName = "";
            public string defaultLang = "";
        }

        public class LocalizationModeAutoGenKeyConfig
        {
            public string sheetName = "";
            public string defaultLang = "";
            public string transSheetName = "";
            public bool exportTrans;
            public string exportTransDir = "";
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

        public class BinConfig
        {
            public bool enable;
            public string dir;
        }

        public class CsvConfig
        {
            public bool enable;
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
