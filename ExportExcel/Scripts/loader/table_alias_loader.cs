using System;
using System.Collections.Generic;

namespace ExportExcel
{
    public class TableAliasLoader
    {
        public const string CName = "name";
        public const string CFields = "fields";
        public static List<string> SuppertLangList = new() { "csharp", "cpp", "go" };

        public TableAliasLoader(Config config)
        {
        }

        public void Load(DataBase data_base, ISheet sheet)
        {
            if (!_ParseHeader(sheet, out var fields_index, out var lang_index_map))
            {
                return;
            }

            int row_count = sheet.RowCount;
            for (int i = 1; i < row_count; i++)
            {
                IRow row = sheet.GetRow(i);

                string name = row.CellStrExt(0);
                if (string.IsNullOrEmpty(name) || name.StartsWith(ConstDef.Comment))
                    continue;

                string[] fields = row.CellStrExt(fields_index).Split("|", StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length == 0)
                {
                    ErrSet.E($"{sheet.SheetName} 的 {name} 对应的 Fileds 为空");
                    continue;
                }

                var item = new AliasItem(name, fields);
                item.CSharp = _GetLang(row, lang_index_map, 0);
                item.Cpp = _GetLang(row, lang_index_map, 1);
                item.Go = _GetLang(row, lang_index_map, 2);
                data_base.AliasDB.Add(item);
            }
        }

        private static string _GetLang(IRow row, int[] lang_index_map, int index)
        {
            int index_map = lang_index_map[index];
            if (index_map < 0)
                return null;
            string ret = row.CellStrExt(index_map);
            if (string.IsNullOrEmpty(ret))
                return null;
            return ret;
        }

        private static bool _ParseHeader(ISheet sheet, out int fields_index, out int[] lang_index_map)
        {
            fields_index = -1;
            lang_index_map = new int[SuppertLangList.Count];
            for (int i = 0; i < lang_index_map.Length; i++)
                lang_index_map[i] = -1;

            int row_count = sheet.RowCount;
            if (row_count == 0)
                return false;

            int name_index = -1;
            IRow header_row = sheet.GetRow(0);
            int count = header_row.ColCount;
            for (int i = 0; i < count; i++)
            {
                string col_name = header_row.CellStrExt(i).ToLower();
                if (col_name == CName)
                    name_index = i;
                else if (col_name == CFields)
                    fields_index = i;
                else
                {
                    var index = SuppertLangList.IndexOf(col_name);
                    if (index >= 0)
                    {
                        lang_index_map[index] = i;
                    }
                }
            }

            if (name_index != 0)
                return false;

            if (name_index >= 0 && fields_index >= 0)
                return true;
            return false;
        }
    }
}
