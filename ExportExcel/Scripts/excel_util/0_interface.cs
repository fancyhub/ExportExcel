using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportExcel
{

    /*
     * WorkSheet: [0,SheetCount)
     * Row: [0,RowCount)
     * Col: [0,ColCount)
     */

    public interface IWorkbook
    {
        string FilePath { get; }
        ISheet CreateSheet(string sheet_name);
        ICellStyle CreateCellStyle();

        int SheetCount { get; }

        /// <summary>
        /// [0,SheetCount)
        /// </summary>
        ISheet GetSheetAt(int sheet_idx);

        void Write(System.IO.Stream stream);
        void Write(System.IO.Stream stream, bool level_open);
        void Close();
    }

    public interface ISheet
    {
        string SheetName { get; }
        IWorkbook Workbook { get; }

        int RowCount { get; }
        /// <summary>
        /// [0,RowCount)
        /// </summary>
        IRow GetRow(int row_index);

        bool IsVisible();

        void CalculateFormula();
    }

    public interface IRow
    {
        int ColCount { get; }
        /// <summary>
        /// [0,ColCount)
        /// </summary>
        ICell GetCell(int cell_index);
    }

    public interface ICellStyle
    {
        bool WrapText { get; set; }
    }

    public interface ICell
    {
        void SetCellValue(string value);
        ICellStyle CellStyle { set; }

        string Text { get; }
    }
}
