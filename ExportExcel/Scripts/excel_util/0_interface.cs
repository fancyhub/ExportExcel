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

        void SaveTo(System.IO.Stream stream, bool level_open);
        void SaveAs(string file_path);
        bool Save();
        void Close();
    }

    public interface ISheet
    {
        string SheetName { get; set; }
        IWorkbook Workbook { get; }

        int RowCount { get; }
        int ColCount { get; }

        /// <summary>
        /// [0,RowCount)
        /// </summary>
        ICellArray GetRow(int row_index);

        /// <summary>
        /// [0,ColCount)
        /// </summary>
        ICellArray GetCol(int col_index);

        bool IsVisible();

        void CalculateFormula();
    }


    public enum ECellArrayType
    {
        Row,
        Col,
    }

    public interface ICellArray
    {
        int Count { get; }        

        ECellArrayType ArrayType { get; }
        int ColCount { get; } // = Count when ArrayType = Row
        int RowCount { get; } // = Count when ArrayType = Col

        /// <summary>
        /// [0,Count)
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
