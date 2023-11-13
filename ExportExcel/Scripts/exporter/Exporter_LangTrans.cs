using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/3 17:14:46
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    //导出 翻译语言表
    public class Exporter_LangTrans : IProcessNode
    {
        public ExeConfig.LocTransConfig _config;
        public Exporter_LangTrans(ExeConfig.LocTransConfig config)
        {
            _config = config;
        }
        public string GetName()
        {
            return "Export";
        }
        public void Process(DataBase data_base)
        {
            //1. 检查, 并创建对应的dir
            if (_config == null || !_config.enable || data_base.Config.localization.Mode != ExeConfig.ELocalizationMode.AutoGenKey)
                return;
            var loc_config = data_base.Config.localization;

            Table table_trans = data_base.TableLocTrans;
            if (table_trans == null)
                return;

            string file_path = System.IO.Path.Combine(_config.dir, loc_config.sheetName+ "_New.xlsx");
            FileUtil.CreateFileDir(file_path);

            //2. 创建新的
            IWorkbook work_book = ExcelUtil.CreateWorkBook();
            ISheet work_sheet = work_book.CreateSheet(loc_config.sheetName);

            //3. 写表头
            foreach (var p in table_trans.Header.List)
            {
                _set_value(work_sheet, 0, p.ExcelColIdx, p.Name);
                _set_value(work_sheet, 1, p.ExcelColIdx, p.DataType.ToCsvStr());
                _set_value(work_sheet, 2, p.ExcelColIdx, p.Desc);
            }


            //4. 写文件内容
            string[,] body = table_trans.Body;
            int row_count = body.GetLength(0);
            int col_count = table_trans.Header.Count;
            for (int r = 0; r < row_count; r++)
            {
                IRow row = work_sheet.GetRow(r + 3);

                for (int c = 0; c < col_count; c++)
                {
                    ICell cell = row.GetCell(c);
                    string v = body[r, c];
                    cell.SetCellValue(v);
                }
            }

            //5.最后写入
            using (var fs_out = System.IO.File.OpenWrite(file_path))
            {
                work_book.Write(fs_out);
                work_book.Close();
            };
        }

        private void _set_value(ISheet sheet, int row_idx, int col_idx, string v)
        {
            IRow row = sheet.GetRow(row_idx);
            ICell cell = row.GetCell(col_idx);
            cell.SetCellValue(v);
        }
    }
}
