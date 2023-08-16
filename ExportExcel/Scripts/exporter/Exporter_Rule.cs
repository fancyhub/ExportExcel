using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/1 10:51:40
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class Exporter_Rule : I_ProcessNode
    {
        public string _dir;
        public Exporter_Rule(string dir)
        {
            _dir = dir;
        }
        public string GetName()
        {
            return "Export";
        }
        public void Process(DataBase data)
        {
            foreach (var p in data.Tables)
            {
                //1. 获取数据
                Table table = p.Value;

                //2. 创建内存里面的表格
                IWorkbook work_book = ExcelUtil.CreateWorkBook();
                var sheet_name = _build_sheet_name(table);
                ISheet work_sheet = work_book.CreateSheet(sheet_name);
                var cell_style = work_book.CreateCellStyle();
                cell_style.WrapText = true;

                //3. 每一列单独处理
                int col_index = 0;
                foreach (TableHeaderItem col in table.Header.List)
                {
                    //3.1 写名字
                    _set_value(work_sheet, 0, col_index, col.Name);

                    //3.2 写类型 & 约束
                    List<string> constraint_list = _build_constraint(col);
                    constraint_list.Insert(0, col.DataType.ToCsvStr());
                    var cell = _set_value(work_sheet, 1, col_index, string.Join("\n", constraint_list));
                    cell.CellStyle = cell_style;

                    //3.3 写注释
                    _set_value(work_sheet, 2, col_index, col.Desc);

                    col_index++;
                }

                //4. 保存
                string file_path = System.IO.Path.Combine(_dir, "R_" + table.SheetName + ".xlsx");
                work_book.Write(System.IO.File.OpenWrite(file_path), false);
            }
        }

        public string _build_sheet_name(Table table)
        {
            string ret = table.SheetName;
            if (table.TableExportFlag == E_EXPORT_FLAG.client)
            {
                ret = ret + " | Export_Client";
            }
            else if (table.TableExportFlag == E_EXPORT_FLAG.svr)
            {
                ret = ret + " | Export_Svr";
            }
            else if (table.TableExportFlag == E_EXPORT_FLAG.none)
            {
                ret = ret + " | Export_None";
            }
            return ret;
        }

        public List<string> _build_constraint(TableHeaderItem col)
        {
            List<string> ret = new List<string>();
            if (col.AttrPK != null)
            {
                ret.Add(col.AttrPK.ToString());
            }
            if (col.ExportFlag == E_EXPORT_FLAG.client)
            {
                ret.Add("Export[Client]");
            }
            else if (col.ExportFlag == E_EXPORT_FLAG.svr)
            {
                ret.Add("Export[Svr]");
            }
            if (col.DataType.enum_type != null)
            {
                ret.Add(string.Format("Enum[{0}]", col.DataType.enum_type.Name));
            }
            return ret;
        }

        public ICell _set_value(ISheet sheet, int row_idx, int col_idx, string v)
        {
            IRow row = sheet.GetRow(row_idx);
            ICell cell = row.GetCell(col_idx);
            cell.SetCellValue(v);
            return cell;
        }
    }
}
