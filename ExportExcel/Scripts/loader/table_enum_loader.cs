using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExportExcel
{
    /// <summary>
    /// 枚举表的加载器
    /// </summary>
    public class TableEnumLoader
    {
        //枚举名的检查
        private static Regex S_ENUM_NAME_REGEX = new Regex("^[A-Z][A-Z0-9_]*$");

        //枚举字段名的检查
        private static Regex S_ENUM_FIELD_NAME_REGEX = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");


        public TableEnumLoader(ExeConfig config)
        {
            S_ENUM_NAME_REGEX = new Regex(config.validation.enumNameReg);
            S_ENUM_FIELD_NAME_REGEX = new Regex(config.validation.enumFieldNameReg);
        }

        public static bool ValidEnumName(string enum_name)
        {
            return S_ENUM_NAME_REGEX.IsMatch(enum_name);
        }


        public static bool ValidEnumFieldName(string enum_field_name)
        {
            return S_ENUM_FIELD_NAME_REGEX.IsMatch(enum_field_name);
        }

        public void Load(DataBase data_base, ISheet sheet)
        {
            sheet.CalculateFormula();
            int row_count = sheet.RowCount;
            for (int i = 1; i < row_count; i++)
            {
                IRow row = sheet.GetRow(i);
                string enum_name = row.CellStrExt(0);
                if (string.IsNullOrEmpty(enum_name) || enum_name.StartsWith("#"))
                    continue;

                string enum_field_name = row.CellStrExt(1);
                string excel_val = row.CellStrExt(2);
                if (!int.TryParse(row.CellStrExt(3), out int enum_val))
                {
                    ErrSet.E($"{sheet.SheetName} 枚举 {enum_field_name}.{excel_val} 对应的int解析失败 {row.CellStrExt(3)} ", sheet.Workbook.FilePath);
                    continue;
                }

                if (!ValidEnumName(enum_name))
                {
                    ErrSet.E($"{sheet.SheetName} 枚举 {enum_name} 不符合命名规范", sheet.Workbook.FilePath);
                    continue;
                }

                if (!ValidEnumFieldName(enum_field_name))
                {
                    ErrSet.E($"{sheet.SheetName} 枚举 {enum_name}.{enum_field_name} 不符合命名规范", sheet.Workbook.FilePath);
                    continue;
                }

                var rslt = data_base.EnumDB.AddEnumField(enum_name, enum_field_name, excel_val, enum_val);
                switch (rslt)
                {
                    case EEnumAddError.DuplicateExcelVal:
                        ErrSet.E($"{sheet.SheetName} 枚举 {enum_name}.{excel_val} 该名字重复", sheet.Workbook.FilePath);
                        break;

                    case EEnumAddError.DuplicateFieldName:
                        ErrSet.E($"{sheet.SheetName} 枚举 {enum_name}.{enum_field_name} 该名字重复", sheet.Workbook.FilePath);
                        break;
                    case EEnumAddError.Succ:
                        break;
                }
            }
        }
    }
}
