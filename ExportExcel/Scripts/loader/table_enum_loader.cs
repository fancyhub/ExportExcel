using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExportExcel
{
    public class TableAliasLoader
    {
        public const string SuppertLangList = "CSharp, Go, Lua, Cpp,";

        public TableAliasLoader(Config config)
        {

        }

        public void Load(DataBase data_base, ISheet sheet)
        {
            int row_count = sheet.RowCount;
            for (int i = 1; i < row_count; i++)
            {
                IRow row = sheet.GetRow(i);

                string code_name = row.CellStrExt(0);
                if (string.IsNullOrEmpty(code_name) || code_name.StartsWith("#"))
                    continue;

                EAliasCode code_type = DBAlias.ToAliasCode(code_name);
                if (code_type == EAliasCode.None)
                {
                    ErrSet.E($"{sheet.SheetName} Alias, [{i + 1} : A] 对应的CodeType {code_name} 解析出错, 目前只支持 {SuppertLangList} ", sheet.Workbook.FilePath);
                    continue;
                }

                string sheet_name = row.CellStrExt(1);
                string col_name = row.CellStrExt(2);

                string client_name = row.CellStrExt(3);
                string svr_name = row.CellStrExt(4);

                if (string.IsNullOrEmpty(sheet_name))
                {
                    ErrSet.E($"{sheet.SheetName} Alias,  [{i + 1} : B]  对应的 SheetName is empty", sheet.Workbook.FilePath);
                    continue;
                }

                if (string.IsNullOrEmpty(col_name))
                {
                    ErrSet.E($"{sheet.SheetName} Alias,  [{i + 1} : C]  对应的 Column is empty", sheet.Workbook.FilePath);
                    continue;
                }

                data_base.AliasDB.Add(code_type, sheet_name, col_name, client_name, svr_name);
            }
        }
    }
}
