using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/7 10:06:37
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class TableLoader : IProcessNode
    {
        public enum ETableNameType
        {
            Ignore,
            DataTable,
            EnumConfig,
            RefTable,
            AliasTable,
            Invalid,
        }

        private static char[] S_SHEET_NAME_SPLIT = new char[] { '|' };
        //表名的检查
        private static Regex S_SHEET_NAME_REGEX = new Regex("^[A-Z][a-zA-Z0-9]*$");


        public TableRefLoader _ref_loader;
        public TableEnumLoader _enum_loader;
        public TableDataLoader _data_loader;
        public TableAliasLoader _alias_loader;

        public TableLoader(Config config)
        {
            S_SHEET_NAME_REGEX = new Regex(config.validation.sheetNameReg);
            _ref_loader = new TableRefLoader();
            _enum_loader = new TableEnumLoader(config);
            _data_loader = new TableDataLoader(config);
            _alias_loader = new TableAliasLoader(config);
        }

        public string GetName()
        {
            return "加载Excel";
        }

        public void Process(DataBase data_base)
        {
            List<string> files = ExcelUtil.CollectExcelFiles(data_base.Config.excelPaths);

            foreach (string file_path in files)
            {
                IWorkbook wk = ExcelUtil.Load(file_path);
                if (wk == null)
                {
                    ErrSet.E("读取文件失败", file_path);
                    continue;
                }

                int sheet_count = wk.SheetCount;
                for (int i = 0; i < sheet_count; i++)
                {
                    ISheet sheet = wk.GetSheetAt(i);
                    ETableNameType sheet_name_type = _ParseSheetName(sheet, out string sheet_name, out EExportFlagMask flag);
                    switch (sheet_name_type)
                    {

                        case ETableNameType.Ignore:
                            break;


                        case ETableNameType.RefTable:
                            sheet.CalculateFormula();
                            _ref_loader.Load(data_base, sheet);
                            break;

                        case ETableNameType.AliasTable:
                            sheet.CalculateFormula();
                            _alias_loader.Load(data_base, sheet);
                            break;

                        case ETableNameType.EnumConfig:
                            sheet.CalculateFormula();
                            _enum_loader.Load(data_base, sheet);
                            break;

                        case ETableNameType.DataTable:
                            sheet.CalculateFormula();
                            _data_loader.Load(data_base, sheet, sheet_name, flag);
                            break;

                        case ETableNameType.Invalid:
                            ErrSet.E($"表名不合法 {sheet.SheetName}", file_path);
                            break;

                        default:
                            ErrSet.E($"未处理类型 {sheet_name_type}, {sheet.SheetName}", file_path);
                            break;
                    }
                }

                wk.Close();
            }
        }


        private static ETableNameType _ParseSheetName(ISheet sheet, out string sheet_name, out EExportFlagMask flag)
        {
            sheet_name = null;
            flag = EExportFlagMask.All;

            if (sheet == null || !sheet.IsVisible())
                return ETableNameType.Ignore;

            string name = sheet.SheetName;
            if (string.IsNullOrWhiteSpace(name))
                return ETableNameType.Ignore;
            name = name.Trim();

            if (name.StartsWith("#"))
                return ETableNameType.Ignore;
            else if (name.StartsWith("@"))
            {
                if (name == "@EnumConfig" || name.StartsWith("@EnumConfig_"))
                    return ETableNameType.EnumConfig;
                else if (name == "@RefTable" || name.StartsWith("@RefTable_"))
                    return ETableNameType.RefTable;
                else if (name == "@Alias" || name.StartsWith("@Alias_"))
                    return ETableNameType.AliasTable;

                ErrSet.E($"非法表格名 {sheet.SheetName} ", sheet.Workbook.FilePath);
                return ETableNameType.Invalid;
            }
            else
            {
                string[] str_arrays = name.Split(S_SHEET_NAME_SPLIT, StringSplitOptions.RemoveEmptyEntries);
                sheet_name = str_arrays[0].Trim();
                flag = _ParseTableExportFlag(str_arrays);

                if (!S_SHEET_NAME_REGEX.IsMatch(sheet_name))
                {
                    ErrSet.E($"{sheet.SheetName} 表名不符合命名规范", sheet.Workbook.FilePath);
                    return ETableNameType.Invalid;
                }
                return ETableNameType.DataTable;
            }
        }

        private static EExportFlagMask _ParseTableExportFlag(string[] sheet_name_array)
        {
            for (int i = 1; i < sheet_name_array.Length; i++)
            {
                string temp = sheet_name_array[i].Trim().ToLower();
                switch (temp)
                {
                    case "export_client":
                        return EExportFlagMask.Client;
                    case "export_svr":
                        return EExportFlagMask.Server;
                    case "export_none":
                        return EExportFlagMask.None;
                    default:
                        break;
                }
            }
            return EExportFlagMask.All;
        }

    }
}
