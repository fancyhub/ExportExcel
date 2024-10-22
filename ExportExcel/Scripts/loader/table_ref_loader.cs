using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportExcel
{
    public class TableRefLoader
    {
        public void Load(DataBase data_base, ISheet sheet)
        {
            //1. 读取表头
            List<(string key, int col_idx)> col_list = new List<(string, int)>();
            {
                IRow row_ids = sheet.GetRow(0);
                int cell_num = row_ids.ColCount;
                if (cell_num <= 1)
                    return;
                HashSet<string> col_set = new HashSet<string>();
                for (int i = 1; i < cell_num; i++)
                {
                    string field_name = row_ids.CellStrExt(i);
                    if (string.IsNullOrEmpty(field_name))
                        continue;
                    if (field_name.StartsWith(ConstDef.Comment))
                        continue;

                    if (col_set.Contains(field_name))
                    {
                        ErrSet.E($"+{sheet.SheetName}.{field_name} 重复", sheet.Workbook.FilePath);
                        return;
                    }

                    col_set.Add(field_name);
                    col_list.Add((field_name, i));
                }
            }

            //2. 读取数据
            {
                int start_row = 1;
                int end_row = sheet.RowCount;
                for (int r = start_row; r < end_row; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null)
                        continue;

                    string key = row.CellStrExt(0);
                    if (string.IsNullOrEmpty(key) || key.StartsWith(ConstDef.Comment))
                        continue;

                    if (!data_base.RefDB.AddKey(key))
                    {
                        ErrSet.E($"Key {key} 重复", sheet.Workbook.FilePath);
                        continue;
                    }

                    foreach (var c in col_list)
                    {
                        var v = row.CellStrExt(c.col_idx);
                        data_base.RefDB.AddValue(key, c.key, v);
                    }
                }
            }
        }
    }

}
