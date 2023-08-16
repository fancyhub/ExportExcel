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
    public class TableLoader : I_ProcessNode
    {
        public enum E_TableNameType
        {
            Ignore,
            DataTable,
            EnumConfig,
            RefTable,
            Invalid,
        }

        private static char[] S_SHEET_NAME_SPLIT = new char[] { '|' };
        //表名的检查
        private static Regex S_SHEET_NAME_REGEX = new Regex("^[A-Z][a-zA-Z0-9]*$");


        public TableRefLoader _ref_loader;
        public TableEnumLoader _enum_loader;
        public TableDataLoader _data_loader;

        public TableLoader(ExeConfig config)
        {
            S_SHEET_NAME_REGEX = new Regex(config.validation.sheet_name_reg);
            _ref_loader = new TableRefLoader();
            _enum_loader = new TableEnumLoader(config);
            _data_loader = new TableDataLoader(config);
        }

        public string GetName()
        {
            return "加载Excel";
        }

        public void Process(DataBase data_base)
        {
            List<string> files = ExcelUtil.CollectExcelFiles(data_base.Config.excel_paths);

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
                    E_TableNameType sheet_name_type = _ParseSheetName(sheet, out string sheet_name, out E_EXPORT_FLAG flag);
                    switch (sheet_name_type)
                    {
                        case E_TableNameType.RefTable:
                            sheet.CalculateFormula();
                            _ref_loader.Load(data_base, sheet);
                            break;

                        case E_TableNameType.EnumConfig:
                            sheet.CalculateFormula();
                            _enum_loader.Load(data_base, sheet);
                            break;

                        case E_TableNameType.Ignore:
                            break;

                        case E_TableNameType.DataTable:
                            sheet.CalculateFormula();
                            _data_loader.Load(data_base, sheet, sheet_name, flag);
                            break;

                        case E_TableNameType.Invalid:
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


        private static E_TableNameType _ParseSheetName(ISheet sheet, out string sheet_name, out E_EXPORT_FLAG flag)
        {
            sheet_name = null;
            flag = E_EXPORT_FLAG.all;

            if (sheet == null || !sheet.IsVisible())
                return E_TableNameType.Ignore;

            string name = sheet.SheetName;
            if (string.IsNullOrWhiteSpace(name))
                return E_TableNameType.Ignore;
            name = name.Trim();

            if (name.StartsWith("#"))
                return E_TableNameType.Ignore;
            else if (name.StartsWith("@"))
            {
                if (name == "@EnumConfig" || name.StartsWith("@EnumConfig_"))
                    return E_TableNameType.EnumConfig;
                else if (name == "@RefTable" || name.StartsWith("@RefTable_"))
                    return E_TableNameType.RefTable;
                ErrSet.E("非法表格名 {sheet.SheetName} ", sheet.Workbook.FilePath);
                return E_TableNameType.Invalid;
            }
            else
            {
                string[] str_arrays = name.Split(S_SHEET_NAME_SPLIT, StringSplitOptions.RemoveEmptyEntries);
                sheet_name = str_arrays[0].Trim();
                flag = _ParseTableExportFlag(str_arrays);

                if (!S_SHEET_NAME_REGEX.IsMatch(sheet_name))
                {
                    ErrSet.E($"{sheet.SheetName} 表名不符合命名规范", sheet.Workbook.FilePath);
                    return E_TableNameType.Invalid;
                }
                return E_TableNameType.DataTable;
            }
        }

        private static E_EXPORT_FLAG _ParseTableExportFlag(string[] sheet_name_array)
        {
            for (int i = 1; i < sheet_name_array.Length; i++)
            {
                string temp = sheet_name_array[i].Trim().ToLower();
                switch (temp)
                {
                    case "export_client":
                        return E_EXPORT_FLAG.client;
                    case "export_svr":
                        return E_EXPORT_FLAG.svr;
                    case "export_none":
                        return E_EXPORT_FLAG.none;
                    default:
                        break;
                }
            }
            return E_EXPORT_FLAG.all;
        }

    }
}
