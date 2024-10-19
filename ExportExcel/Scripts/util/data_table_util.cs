using System;
using System.Collections.Generic;

namespace ExportExcel
{
    public static class DataTableUtil
    {
        #region DataBase Ext
        public static bool FindCol(this DataBase self, ConAttrLookup attr, out TableCol out_data)
        {
            out_data = default;
            if (self == null || attr == null)
                return false;

            if (string.IsNullOrEmpty(attr.SheetName) || string.IsNullOrEmpty(attr.ColName))
            {
                out_data = default;
                return false;
            }

            self.Tables.TryGetValue(attr.SheetName, out Table table);
            if (table == null)
            {
                out_data = default;
                return false;
            }

            int col_idx = table.Header.IndexOfCol(attr.ColName);
            if (col_idx < 0)
            {
                out_data = default;
                return false;
            }
            out_data = new TableCol(table, col_idx);
            return true;
        }

        public static void ForeachTable(this DataBase self, Action<Table> func)
        {
            foreach (var t in self.Tables)
            {
                func(t.Value);
            }
        }

        public static void ForeachCol(this DataBase self, Action<TableCol> func)
        {
            foreach (var t in self.Tables)
            {
                Table table = t.Value;
                List<TableField> col_list = table.Header.List;
                for (int i = 0; i < col_list.Count; i++)
                {
                    TableCol col_data = new TableCol(table, i);
                    func(col_data);
                }
            }
        }

        public static void ForeachCol<T>(this DataBase self, Action<TableCol, T> func, T user_data)
        {
            foreach (var t in self.Tables)
            {
                Table table = t.Value;
                List<TableField> col_list = table.Header.List;
                for (int i = 0; i < col_list.Count; i++)
                {
                    TableCol col_data = new TableCol(table, i);
                    func(col_data, user_data);
                }
            }
        }

        public static void ForeachCol<T1, T2>(this DataBase self, Action<TableCol, T1, T2> func, T1 user_data1, T2 user_data2)
        {
            foreach (var t in self.Tables)
            {
                Table table = t.Value;
                List<TableField> col_list = table.Header.List;
                for (int i = 0; i < col_list.Count; i++)
                {
                    TableCol col_data = new TableCol(table, i);
                    func(col_data, user_data1, user_data2);
                }
            }
        }

        public static void ForeachCell(this DataBase self, Action<TableCell> func)
        {
            foreach (var t in self.Tables)
            {
                Table table = t.Value;
                int col_count = table.Header.Count;
                int row_count = table.Body.GetLength(0);
                for (int c = 0; c < col_count; c++)
                {
                    for (int r = 0; r < row_count; r++)
                    {
                        TableCell cell = new TableCell(table, c, r);
                        func(cell);
                    }
                }
            }
        }

        public static List<TableCol> GetAllCols(this DataBase self, Func<TableCol, bool> func_suit)
        {
            List<TableCol> ret = new List<TableCol>();
            foreach (var p in self.Tables)
            {
                var table = p.Value;
                var cols = table.Header.List;
                for (int i = 0; i < cols.Count; i++)
                {
                    var data = new TableCol(table, i);
                    if (func_suit == null || func_suit(data))
                        ret.Add(data);
                }
            }
            return ret;
        }
        #endregion

        #region DataTable Ext
        public static void ForeachCol(this Table self, Action<TableCol> func)
        {
            for (int i = 0; i < self.Header.List.Count; i++)
            {
                TableCol col = new TableCol(self, i);
                func(col);
            }
        }

        public static void ForeachCol<T>(this Table self, Action<TableCol, T> func, T user_data)
        {
            for (int i = 0; i < self.Header.List.Count; i++)
            {
                TableCol col = new TableCol(self, i);
                func(col, user_data);
            }
        }

        public static void ForeachCell(this Table self, Action<TableCell> func)
        {
            int row_count = self.Body.GetLength(0);
            int col_count = self.Header.List.Count;
            for (int c = 0; c < col_count; c++)
            {
                for (int r = 0; r < row_count; r++)
                {
                    TableCell cell = new TableCell(self, c, r);
                    func(cell);
                }
            }
        }

        #endregion

        #region DBCol Ext
        public static void ForeachCell(this TableCol self, Action<TableCell> func)
        {
            int row_count = self.Table.Body.GetLength(0);
            for (int r = 0; r < row_count; r++)
            {
                TableCell cell = new TableCell(self.Table, self.ColIndex, r);
                func(cell);
            }
        }

        public static void ForeachCell<T>(this TableCol self, Action<TableCell, T> func, T user_data)
        {
            int row_count = self.Table.Body.GetLength(0);
            for (int r = 0; r < row_count; r++)
            {
                TableCell cell = new TableCell(self.Table, self.ColIndex, r);
                func(cell, user_data);
            }
        }

        public static void ForeachCell<T1, T2>(this TableCol self, Action<TableCell, T1, T2> func, T1 user_data1, T2 user_data2)
        {
            int row_count = self.Table.Body.GetLength(0);
            for (int r = 0; r < row_count; r++)
            {
                TableCell cell = new TableCell(self.Table, self.ColIndex, r);
                func(cell, user_data1, user_data2);
            }
        }

        #endregion


        public static string CellStrExt(this IRow row, int cell_index)
        {
            if (row == null)
                return string.Empty;
            var cell = row.GetCell(cell_index);
            return GetStrExt(cell);
        }

        public static string GetStrExt(this ICell cell)
        {
            if (cell == null)
                return string.Empty;
            return cell.Text.Trim();
        }
    }
}
