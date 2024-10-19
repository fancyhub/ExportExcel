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
    public class Config
    {
        public List<string> excelPaths = new List<string>();
        public ValidationConfig validation = new ValidationConfig();
        public TableDataLoaderConfig tableDataRule = new TableDataLoaderConfig();
        public LocalizationConfig localization = new LocalizationConfig();
        public ExportCommonConfig exportCommon = new ExportCommonConfig();
        public ExportConfig exportClient = new ExportConfig();
        public ExportConfig exportServer = new ExportConfig();


        public static Config Load(string file_path)
        {
            if (!File.Exists(file_path))
            {
                ErrSet.E("配置文件不存在 " + file_path);
                return null;
            }

            try
            {
                string text = File.ReadAllText(file_path, System.Text.Encoding.UTF8);
                Config ret = JsonConvert.DeserializeObject<Config>(text);
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

        public void Save(string file_path)
        {
            try
            {
                File.WriteAllText(file_path, JsonConvert.SerializeObject(this, Formatting.Indented));
                Console.WriteLine("创建配置成功 " + file_path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("创建文件失败 " + file_path + " " + ex.Message);
            }
        }

        public enum ELocalizationMode
        {
            None,
            Normal,
            AutoGenKey,
        }

        public class TableDataLoaderConfig
        {
            public int nameRowIndex = 0;
            public int typeRowIndex = 1;
            public int descRowIndex = 2;
            public int dataStartRowIndex = 3;
            public bool calculateFormula = true;
            public string emptyPlaceholder = "null";
        }

        public class LocalizationConfig
        {
            [JsonIgnore]
            public ELocalizationMode Mode;

            public bool enable = false;
            public string sheetName = "";
            public string defaultLang = "";
            public bool useHashId = true;
            public bool autoGenKey = false;
            public bool checkKeyExist = true;

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
            public string searchFileRoot = "./";
            public string sheetNameReg = "^[A-Z][a-zA-Z0-9]*$";
            public string colNameReg = "^[A-Z][a-zA-Z0-9_]*$";
            public string enumNameReg = "^E[A-Z][a-zA-Z0-9_]*$";
            public string enumFieldNameReg = "^[A-Z][a-zA-Z0-9_]*$";
        }

        public class ExportCommonConfig
        {
            public LocTransConfig localizationTranslate = new LocTransConfig();
            public RuleConfig ruleExcel = new RuleConfig();
            public SchemaConfig schema = new SchemaConfig();
        }

        public class ExportConfig
        {
            public CsvConfig csv = new CsvConfig();
            public BinConfig bin = new BinConfig();
            public JsonConfig json = new JsonConfig();
            public BsonConfig bson = new BsonConfig();
            public CSharpConfig csharp = new CSharpConfig();
            public LuaConfig lua = new LuaConfig();
            public GoConfig go = new GoConfig();
            public CppConfig cpp = new CppConfig();
        }


        public class CppGetterConfig
        {
            public bool enable;
        }

        public class CppLoaderConfig
        {
            public bool enable;
        }

        public class CSharpGetterConfig
        {
            public bool enable;
            public string className = "TableMgr";
            public bool useStatic = false;
        }

        public class CSharpLoaderConfig
        {
            public bool enable;
        }

        public class CSharpLocIdConfig
        {
            public bool enable;
            public string locIdStartWith = "";
        }

        public class CSharpConfig
        {
            public bool enable;
            public string namespaceName = "";
            public string parentClass = "";
            public string classPrefix = "";
            public string classSuffix = "";
            public string dir = "Output";
            public string header = @"using System;\nusing System.Collections;\nusing System.Collections.Generic;";

            public CSharpLoaderConfig loader = new CSharpLoaderConfig();
            public CSharpGetterConfig getter = new CSharpGetterConfig();
            public CSharpLocIdConfig locId = new CSharpLocIdConfig();

            public string GetClassName(string sheet_name)
            {
                return classPrefix + sheet_name + classSuffix;
            }
        }

        public class CppConfig
        {
            public bool enable;
            public string namespaceName = "";
            public string parentClass = "";
            public string classPrefix = "";
            public string classSuffix = "";
            public string dir = "Output";
            public string header = "";

            public CppLoaderConfig loader = new CppLoaderConfig();
            public CppGetterConfig getter = new CppGetterConfig();

            public string GetClassName(string sheet_name)
            {
                return classPrefix + sheet_name + classSuffix;
            }
        }


        public class LocTransConfig
        {
            public bool enable;
            public string dir = "Output";
        }

        public class RuleConfig
        {
            public bool enable;
            public string dir = "Output";
        }

        public class SchemaConfig
        {
            public bool enable;
            public string dir = "Output";
        }

        public class BinConfig
        {
            public bool enable;
            public string dir = "Output";
        }


        public class JsonConfig
        {
            public bool enable;
            public bool header = false;
            public string dir = "";
        }

        public class BsonConfig
        {
            public bool enable;
            public string dir = "";
        }

        public class CsvConfig
        {
            public bool enable;
            public bool utf8bom = true;
            public string dir = "";
        }

        public class LuaConfig
        {
            public bool enable;
            public string classPrefix = "";
            public string dir = "Output";
            public string locIdStartWith = "~";

            public string GetClassName(string sheet_name)
            {
                return classPrefix + sheet_name;
            }
        }

        public class GoConfig
        {
            public bool enable;
            public string packageName = "config";
            public string classPrefix = "";
            public string dir = "Output";

            public string GetClassName(string sheet_name)
            {
                return classPrefix + sheet_name;
            }
        }
    }
}
