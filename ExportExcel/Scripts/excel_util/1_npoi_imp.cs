//#define NPOI

using System;
using System.IO;

/*
 * NPOI 2.5.2
 */

namespace ExportExcel.ExcelNPOI
{
    public partial class WorkBookImp
    {

    }

#if NPOI
    public partial class WorkBookImp : IWorkbook
    {
        public string FilePath { get; private set; }
        public NPOI.SS.UserModel.IWorkbook _work_book;
        public NPOI.XSSF.UserModel.XSSFWorkbook _work_book_xssf;
        public NPOI.HSSF.UserModel.HSSFWorkbook _work_book_hssf;

        private WorkBookImp(NPOI.XSSF.UserModel.XSSFWorkbook orig, string filePath)
        {
            _work_book_xssf = orig;
            _work_book = orig;
            FilePath = filePath;
        }
        private WorkBookImp(NPOI.HSSF.UserModel.HSSFWorkbook orig, string filePath)
        {
            _work_book_hssf = orig;
            _work_book = orig;
            FilePath = filePath;
        }
        public static WorkBookImp CreateNew()
        {
            NPOI.XSSF.UserModel.XSSFWorkbook book = new NPOI.XSSF.UserModel.XSSFWorkbook();
            return new WorkBookImp(book, null);
        }

        public static WorkBookImp Load(string filePath)
        {
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
                if (ext == ".xls")
                {
                    //把xls文件中的数据写入wk中
                    ret = new WorkBookImp(new NPOI.HSSF.UserModel.HSSFWorkbook(fs), filePath);
                }
                else if (ext == ".xlsx" || ext == ".xlsm")
                {
                    //把xlsx文件中的数据写入wk中
                    ret = new WorkBookImp(new NPOI.XSSF.UserModel.XSSFWorkbook(fs), filePath);
                }

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
            var sheet = _work_book.CreateSheet(sheet_name);
            return SheetImp.Create(this, sheet);
        }

        public ICellStyle CreateCellStyle()
        {
            return CellStyleImp.Create(_work_book.CreateCellStyle());
        }

        public void Write(System.IO.Stream stream)
        {
            _work_book.Write(stream, false);
        }

        public void Write(System.IO.Stream stream, bool level_open)
        {
            _work_book_xssf?.Write(stream, level_open);
            _work_book_hssf?.Write(stream);
        }

        public int SheetCount
        {
            get { return _work_book.NumberOfSheets; }
        }

        public ISheet GetSheetAt(int sheet_idx)
        {
            return SheetImp.Create(this, _work_book.GetSheetAt(sheet_idx));
        }

        public void Close()
        {
            _work_book.Close();
        }
    }


    public class SheetImp : ISheet
    {
        public NPOI.SS.UserModel.ISheet _sheet;
        public IWorkbook _work_book;
        private SheetImp(IWorkbook work_book, NPOI.SS.UserModel.ISheet sheet)
        {
            _work_book = work_book;
            _sheet = sheet;
        }

        public static SheetImp Create(IWorkbook work_book, NPOI.SS.UserModel.ISheet sheet)
        {
            if (sheet == null)
                return null;
            return new SheetImp(work_book, sheet);
        }

        public IWorkbook Workbook => _work_book;

        public string SheetName => _sheet.SheetName;

        public void CalculateFormula()
        {
            _sheet.ForceFormulaRecalculation = true;
        }

        public int RowCount
        {
            get
            {
                int ret = _sheet.LastRowNum;
                if (ret == 0)
                    return 0;
                return ret + 1;
            }
        }

        public IRow GetRow(int row_index)
        {
            var row = _sheet.GetRow(row_index);
            if (row == null)
                row = _sheet.CreateRow(row_index);
            return RowImp.Create(row);
        }

        public bool IsVisible()
        {
            int sheet_idx = _sheet.Workbook.GetSheetIndex(_sheet);
            return !_sheet.Workbook.IsSheetHidden(sheet_idx);

        }
    }

    public class RowImp : IRow
    {
        public NPOI.SS.UserModel.IRow _row;

        public int ColCount
        {
            get
            {
                int a = _row.LastCellNum;
                if (a == -1)
                    return 0;
                return a;
            }
        }

        private RowImp(NPOI.SS.UserModel.IRow row)
        {
            _row = row;
        }
        public static RowImp Create(NPOI.SS.UserModel.IRow row)
        {
            if (row == null)
                return null;
            return new RowImp(row);
        }

        public ICell GetCell(int cell_index)
        {
            var row = _row.GetCell(cell_index);
            if (row == null)
                row = _row.CreateCell(cell_index);
            return CellImp.Create(row);
        }

    }

    public class CellImp : ICell
    {
        public NPOI.SS.UserModel.ICell _cell;
        private CellImp(NPOI.SS.UserModel.ICell cell)
        {
            _cell = cell;
        }
        public static CellImp Create(NPOI.SS.UserModel.ICell cell)
        {
            if (cell == null)
                return null;
            return new CellImp(cell);
        }

        public void SetCellValue(string value)
        {
            _cell.SetCellValue(value);
        }

        public ICellStyle CellStyle
        {
            get
            {
                return CellStyleImp.Create(_cell.CellStyle);
            }
            set
            {
                CellStyleImp s = value as CellStyleImp;
                if (s == null)
                    return;
                _cell.CellStyle = s._cell_style;
            }
        }

        public string Text
        {
            get
            {
                if (_cell == null)
                    return string.Empty;
                if (_cell.CellType == NPOI.SS.UserModel.CellType.Formula)
                {
                    switch (_cell.CachedFormulaResultType)
                    {
                        case NPOI.SS.UserModel.CellType.Numeric:
                            return _cell.NumericCellValue.ToString();
                        case NPOI.SS.UserModel.CellType.Error:
                            return string.Empty;
                        default:
                            return _cell.StringCellValue;
                    }
                }
                return _cell.ToString();
            }
        }

        public override string ToString()
        {
            return _cell.ToString();
        }

        public double NumericCellValue
        {
            get
            {
                return _cell.NumericCellValue;
            }
        }

        public string StringCellValue
        {
            get
            {
                return _cell.StringCellValue;
            }
        }


    }

    public class CellStyleImp : ICellStyle
    {
        public NPOI.SS.UserModel.ICellStyle _cell_style;
        private CellStyleImp(NPOI.SS.UserModel.ICellStyle cell_style)
        {
            _cell_style = cell_style;
        }
        public static CellStyleImp Create(NPOI.SS.UserModel.ICellStyle cell_style)
        {
            if (cell_style == null)
                return null;
            return new CellStyleImp(cell_style);
        }

        public bool WrapText
        {
            get { return _cell_style.WrapText; }
            set { _cell_style.WrapText = value; }
        }
    }
#endif
}
