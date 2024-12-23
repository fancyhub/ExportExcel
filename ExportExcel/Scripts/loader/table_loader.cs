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
            TupleAliasTable,
            Invalid,
        }

        

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
                    ETableNameType sheet_name_type = _ParseSheetName(sheet, out string table_name, out EExportFlagMask flag);

                    try
                    {
                        switch (sheet_name_type)
                        {
                            case ETableNameType.Ignore:
                                break;

                            case ETableNameType.RefTable:
                                if (ConstDef.CalculateFormula)
                                    sheet.CalculateFormula();
                                _ref_loader.Load(data_base, sheet);
                                break;

                            case ETableNameType.TupleAliasTable:
                                if (ConstDef.CalculateFormula)
                                    sheet.CalculateFormula();
                                _alias_loader.Load(data_base, sheet);
                                break;

                            case ETableNameType.EnumConfig:
                                if (ConstDef.CalculateFormula)
                                    sheet.CalculateFormula();
                                _enum_loader.Load(data_base, sheet);
                                break;

                            case ETableNameType.DataTable:
                                if (ConstDef.CalculateFormula)
                                    sheet.CalculateFormula();
                                _data_loader.Load(data_base, sheet, table_name, flag);
                                break;

                            case ETableNameType.Invalid:
                                ErrSet.E($"表名不合法 {sheet.SheetName}", file_path);
                                break;

                            default:
                                ErrSet.E($"未处理类型 {sheet_name_type}, {sheet.SheetName}", file_path);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrSet.E($"表格出错 {sheet.SheetName}", file_path);
                        ErrSet.E(ex.Message + "\n" + ex.StackTrace);
                    }
                }
                wk.Close();
            }


            foreach (var p in data_base.Tables)
            {
                p.Value.Header.FormatColIndex();
            }

        }

        private static ETableNameType _ParseSheetName(ISheet sheet, out string table_name, out EExportFlagMask flag)
        {
            table_name = null;
            flag = EExportFlagMask.All;

            if (sheet == null || !sheet.IsVisible())
                return ETableNameType.Ignore;

            string name = sheet.SheetName;
            if (string.IsNullOrWhiteSpace(name))
                return ETableNameType.Ignore;
            name = name.Trim();

            if (name.StartsWith(ConstDef.Comment))
                return ETableNameType.Ignore;


            string[] str_array_1 = name.Split(ConstDef.SheetNameConstraintSeparator, StringSplitOptions.RemoveEmptyEntries);
            string[] str_array_2 = str_array_1[0].Split(ConstDef.SheetNamePartialSeparator, StringSplitOptions.RemoveEmptyEntries);
            table_name = str_array_2[0].Trim();

            switch (table_name)
            {
                case ConstDef.SpecSheetNameEnum:
                    return ETableNameType.EnumConfig;

                case ConstDef.SpecSheetNameRef:
                    return ETableNameType.RefTable;

                case ConstDef.SpecSheetNameAlias:
                    return ETableNameType.TupleAliasTable;

                default:
                    flag = _ParseTableExportFlag(str_array_1);
                    if (!S_SHEET_NAME_REGEX.IsMatch(table_name))
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
