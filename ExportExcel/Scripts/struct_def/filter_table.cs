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
        public List<(TableField, int)> _header;
        public string[,] _body;
        public int _row_count = 0;
        public int _col_count = 0;
        public string SheetName;
        public bool MultiLang;

        public FilterTable(Table table, EExportFlag flag)
        {
            MultiLang = false;
            SheetName = table.SheetName;
            if ((table.TableExportFlag & flag) == 0)
                return;

            var header = _filter_header(table, flag);
            if (header.Count == 0)
                return;

            _header = header;
            _body = table.Body;
            _row_count = _body.GetLength(0);
            _col_count = _header.Count;
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


        public int RowCount { get { return _row_count; } }

        public int ColCount { get { return _col_count; } }

        public string this[int r, int c]
        {
            get
            {
                if (r < 0 || r >= _row_count)
                    return null;
                if (c < 0 || c >= _col_count)
                    return null;

                c = _header[c].Item2;
                return _body[r, c];
            }
        }

        public TableField PK
        {
            get
            {
                foreach (var p in _header)
                {
                    if (p.Item1.AttrPK != null)
                        return p.Item1;
                }
                return null;
            }
        }

        public List<TableField> GetHeader()
        {
            if (_header == null)
                return null;
            List<TableField> ret = new List<TableField>(_header.Count);
            foreach (var p in _header)
                ret.Add(p.Item1);
            return ret;
        }

        public List<ValueTuple<TableField, int>> _filter_header(Table table, EExportFlag flag)
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
