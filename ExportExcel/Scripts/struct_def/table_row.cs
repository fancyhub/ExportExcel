using System;
using System.Collections.Generic;

namespace ExportExcel
{
    public struct TableRow
    {
        public Table Table;
        public string[,] Body;
        public int RowIndex;

        public TableRow(Table table, int row_index, string[,] body)
        {
            Table = table;
            RowIndex = row_index;
            Body = body;
        }

        public int ColCount => Table.Header.Count;
    }
}
