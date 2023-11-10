#define EPPLUS

using System;
using System.IO;

/**
 * EPPlus 5.7.1
 */
#if EPPLUS
namespace ExportExcel.ExcelEPPlus
{
    public class WorkBookImp : IWorkbook
    {
        public string _file_path;
        public OfficeOpenXml.ExcelPackage _work_book;

        private WorkBookImp(string file_path, OfficeOpenXml.ExcelPackage orig)
        {
            _file_path = file_path;
            _work_book = orig;
        }

        public static WorkBookImp CreateNew()
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            OfficeOpenXml.ExcelPackage book = new OfficeOpenXml.ExcelPackage();
            return new WorkBookImp(null, book);
        }

        public static WorkBookImp Load(string filePath)
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }
            if (!File.Exists(filePath))
            {
                return null;
            }
            //MemoryStream ms = null;
            FileStream fs = null;
            WorkBookImp ret = null;
            try
            {
                string ext = Path.GetExtension(filePath);
                fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                ret = new WorkBookImp(filePath, new OfficeOpenXml.ExcelPackage(fs));
                fs.Close();
            }
            catch (Exception e)
            {
                ErrSet.E("打开文件失败: " + e, filePath);
            }
            finally
            {
                fs?.Close();
            }
            return ret;
        }

        public ISheet CreateSheet(string sheet_name)
        {
            var sheet = _work_book.Workbook.Worksheets.Add(sheet_name);
            return SheetImp.Create(this, sheet);
        }

        public ICellStyle CreateCellStyle()
        {
            string name = System.Guid.NewGuid().ToString();
            return CellStyleImp.Create(_work_book.Workbook.Styles.CreateNamedStyle(name));
        }

        public void Write(System.IO.Stream stream)
        {
            _work_book.SaveAs(stream);
        }

        public void Write(System.IO.Stream stream, bool level_open)
        {
            _work_book.SaveAs(stream);            
        }

        public int SheetCount
        {
            get { return _work_book.Workbook.Worksheets.Count; }
        }

        public string FilePath => _file_path;

       

        public ISheet GetSheetAt(int sheet_idx)
        {
            return SheetImp.Create(this, _work_book.Workbook.Worksheets[sheet_idx]);
        }

        public void Close()
        {            
            _work_book.Dispose();
        }
    }


    public class SheetImp : ISheet
    {
        public OfficeOpenXml.ExcelWorksheet _sheet;
        public IWorkbook _work_book;
        private SheetImp(IWorkbook work_book, OfficeOpenXml.ExcelWorksheet sheet)
        {
            _work_book = work_book;
            _sheet = sheet;
        }

        public static SheetImp Create(IWorkbook work_book, OfficeOpenXml.ExcelWorksheet sheet)
        {
            if (sheet == null)
                return null;
            return new SheetImp(work_book, sheet);
        }

        public IWorkbook Workbook => _work_book;

        public bool IsVisible()
        {
            return _sheet.Hidden == OfficeOpenXml.eWorkSheetHidden.Visible;
        }

        public string SheetName => _sheet.Name;

        public void CalculateFormula()
        {
            OfficeOpenXml.CalculationExtension.Calculate(_sheet);
        }

        public int RowCount
        {
            get
            {
                var d = _sheet.Dimension;
                if (d == null)
                    return 0;
                return d.Rows;
            }
        }

        public IRow GetRow(int row_index)
        {
            return RowImp.Create(_sheet, row_index + 1);
        }
    }

    public class RowImp : IRow
    {
        public OfficeOpenXml.ExcelWorksheet _sheet;
        public int _row_idx;
        private RowImp(OfficeOpenXml.ExcelWorksheet sheet, int row_idx)
        {
            _sheet = sheet;
            _row_idx = row_idx;
        }
        public static RowImp Create(OfficeOpenXml.ExcelWorksheet sheet, int row_idx)
        {
            return new RowImp(sheet, row_idx);
        }

        public int ColCount => _sheet.Dimension.Columns;
        public ICell GetCell(int cell_index)
        {
            return CellImp.Create(_sheet, _row_idx, cell_index + 1);
        }
    }

    public class CellImp : ICell
    {
        public OfficeOpenXml.ExcelWorksheet _sheet;
        public int _row_idx;
        public int _col_idx;
        private CellImp(OfficeOpenXml.ExcelWorksheet sheet, int row_idx, int col_idx)
        {
            _sheet = sheet;
            _row_idx = row_idx;
            _col_idx = col_idx;

        }
        public static CellImp Create(OfficeOpenXml.ExcelWorksheet sheet, int row_idx, int col_idx)
        {
            return new CellImp(sheet, row_idx, col_idx);
        }

        public void SetCellValue(string value)
        {
            _sheet.SetValue(_row_idx, _col_idx, value);
        }

        public ICellStyle CellStyle
        {
            set
            {
                CellStyleImp s = value as CellStyleImp;
                if (s == null)
                    return;
                _sheet.Cells[_row_idx, _col_idx].StyleName = s._cell_style.Name;
            }
        }

        public string Text
        {
            get
            {
                var obj = _sheet.GetValue(_row_idx, _col_idx);
                if (obj == null)
                    return string.Empty;
                return obj.ToString();
            }
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class CellStyleImp : ICellStyle
    {
        public OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml _cell_style;
        private CellStyleImp(OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml cell_style)
        {
            _cell_style = cell_style;
        }
        public static CellStyleImp Create(OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml cell_style)
        {
            if (cell_style == null)
                return null;
            return new CellStyleImp(cell_style);
        }

        public bool WrapText
        {
            get { return _cell_style.Style.WrapText; }
            set { _cell_style.Style.WrapText = value; }
        }
    }
}
#endif