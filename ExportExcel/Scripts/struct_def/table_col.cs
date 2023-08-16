using System;
using System.Collections.Generic;
 

namespace ExportExcel
{
    public struct TableCol
    {
        public Table Table;
        public TableHeaderItem Col;
        public int ColIndex;

        public TableCol(Table table, int col_index)
        {
            Table = table;
            ColIndex = col_index;
            Col = table.Header[col_index];
        }

        public string SheetName { get { return Table.SheetName; } }
        public string ColName { get { return Col.Name; } }
        public string SheetColName { get { return $"{Table.SheetName}.{Col.Name}"; } }

        public int GetDataLen()
        {
            if (Table == null || Table.Body == null)
                return 0;
            return Table.Body.GetLength(0);
        }

        public string GetData(int row_idx)
        {
            return Table.Body[row_idx, ColIndex];
        }

        public void SetData(int row_idx, string new_v)
        {
            Table.Body[row_idx, ColIndex] = new_v;
        }

        public string FindDataPath(int row_idx)
        {
            return Table.FindDataPath(row_idx);
        }
    }

}
