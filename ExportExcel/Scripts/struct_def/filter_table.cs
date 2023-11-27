using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/8 11:10:25
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class FilterTable
    {
        private string[,] _Body;

        public readonly List<TableField> FiltedHeader = new List<TableField>();
        private List<int> _FieltedHeaderIndex = new List<int>();
        public string SheetName;
        public bool MultiLang;

        public FilterTable(Table table, EExportFlag flag)
        {
            MultiLang = false;
            SheetName = table.SheetName;
            if (!table.TableExportFlag.ExtContains(flag))
                return;

            _FilterHeader(table, flag, ref FiltedHeader, ref _FieltedHeaderIndex);
            if (FiltedHeader.Count == 0)
                return;
            _Body = table.Body;
            if (table.MultiLangBody != null)
                MultiLang = true;
        }

        public static List<FilterTable> Filter(DataBase data, EExportFlag flag)
        {
            List<FilterTable> ret = new List<FilterTable>();
            foreach (var t in data.Tables)
            {
                if (!t.Value.TableExportFlag.ExtContains(flag))
                    continue;

                var new_table = new FilterTable(t.Value, flag);
                if (new_table.ColCount > 0)
                    ret.Add(new_table);
            }
            return ret;
        }

        public static List<FilterTable> SplitMultiLangTable(Table table, EExportFlag flag)
        {
            List<FilterTable> ret = new List<FilterTable>();
            if (table.MultiLangBody == null)
            {
                ret.Add(new FilterTable(table, flag));
                return ret;
            }

            foreach (var p in table.MultiLangBody)
            {
                FilterTable t = new FilterTable(table, flag);
                t._Body = p.Value;
                t.SheetName = table.SheetName + "_" + p.Key;
                ret.Add(t);
            }
            return ret;
        }

        public int ColCount { get { return FiltedHeader.Count; } }
        public int RowCount { get { return _Body == null ? 0 : _Body.GetLength(0); } }

        public string this[int r, int c]
        {
            get
            {
                if (r < 0 || r >= RowCount)
                    return null;
                if (c < 0 || c >= ColCount)
                    return null;

                c = _FieltedHeaderIndex[c];
                return _Body[r, c];
            }
        }

        public TableField PK
        {
            get
            {
                foreach (var p in FiltedHeader)
                {
                    if (p.AttrPK != null)
                        return p;
                }
                return null;
            }
        }

        private void _FilterHeader(Table table, EExportFlag flag, ref List<TableField> header, ref List<int> header_index)
        {
            var col_list = table.Header.List;
            for (int i = 0; i < col_list.Count; i++)
            {
                var col = col_list[i];
                if (!col.ExportFlag.ExtContains(flag))
                    continue;
                header.Add(col);
                header_index.Add(i);
            }
        }
    }
}
