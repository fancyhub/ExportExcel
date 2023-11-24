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

        public List<(TableField, int)> Header;
        public string SheetName;
        public bool MultiLang;

        public FilterTable(Table table, EExportFlag flag)
        {
            MultiLang = false;
            SheetName = table.SheetName;
            if ((table.TableExportFlag & flag) == 0)
                return;

            var header = _FilterHeader(table, flag);
            if (header.Count == 0)
                return;

            Header = header;
            _Body = table.Body;
            if (table.MultiLangBody != null)
                MultiLang = true;
        }

        public static List<FilterTable> Filter(DataBase data, EExportFlag flag)
        {
            List<FilterTable> ret = new List<FilterTable>();
            foreach (var t in data.Tables)
            {
                if ((t.Value.TableExportFlag & flag) == 0)
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

        public int ColCount { get { return Header.Count; } }
        public int RowCount { get { return _Body == null ? 0 : _Body.GetLength(0); } }

        public string this[int r, int c]
        {
            get
            {
                if (r < 0 || r >= RowCount)
                    return null;
                if (c < 0 || c >= ColCount)
                    return null;

                c = Header[c].Item2;
                return _Body[r, c];
            }
        }

        public TableField PK
        {
            get
            {
                foreach (var p in Header)
                {
                    if (p.Item1.AttrPK != null)
                        return p.Item1;
                }
                return null;
            }
        }

        public List<TableField> GetHeader()
        {
            if (Header == null)
                return null;
            List<TableField> ret = new List<TableField>(Header.Count);
            foreach (var p in Header)
                ret.Add(p.Item1);
            return ret;
        }

        private List<ValueTuple<TableField, int>> _FilterHeader(Table table, EExportFlag flag)
        {
            List<(TableField, int)> ret = new List<(TableField, int)>();

            var col_list = table.Header.List;
            for (int i = 0; i < col_list.Count; i++)
            {
                var col = col_list[i];
                if ((col.ExportFlag & flag) == 0)
                    continue;
                ret.Add((col, i));
            }
            return ret;
        }
    }
}
