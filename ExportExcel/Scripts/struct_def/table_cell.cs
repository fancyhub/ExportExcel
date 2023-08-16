using System;
using System.Collections.Generic;

namespace ExportExcel
{
    public struct TableCell
    {
        public Table Table;
        public TableHeaderItem Col;
        public int ColIndex;
        public int RowIndex;

        public TableCell(Table table, int col_index, int row_index)
        {
            Table = table;
            Col = Table.Header[col_index];
            ColIndex = col_index;
            RowIndex = row_index;
        }

        public DataType DataType => Col.DataType;

        public string SheetName { get { return Table.SheetName; } }
        public string ColName { get { return Col.Name; } }
        public string SheetColName { get { return $"{Table.SheetName}.{Col.Name}"; } }
        public string Value
        {
            get
            {
                return Table.Body[RowIndex, ColIndex];
            }
            set
            {
                Table.Body[RowIndex, ColIndex] = value;
            }
        }

        public string GetCellValue(int col_index)
        {
            return Table.Body[RowIndex, col_index];
        }

        public string FindDataPath()
        {
            return Table.FindDataPath(RowIndex);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
