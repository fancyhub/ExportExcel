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
    public class Exporter_Rule : IProcessNode
    {
        public Config.RuleConfig _config;
        public Exporter_Rule(Config.RuleConfig config)
        {
            _config = config;
        }

        public string GetName()
        {
            return "Export";
        }
        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable)
                return;

            List<Table> tables = new List<Table>();
            foreach (var p in data.Tables)
            {
                if (data.Config.localization.IsLocalizationSheet(p.Key))
                    continue;
                tables.Add(p.Value);
            }
            if (data.TableLocOld != null)
            {
                tables.Add(data.TableLocOld);
            }

            foreach (var table in tables)
            {
                //2. 创建内存里面的表格
                IWorkbook work_book = ExcelUtil.CreateWorkBook();
                var sheet_name = _build_sheet_name(table);
                ISheet work_sheet = work_book.CreateSheet(sheet_name);
                var cell_style = work_book.CreateCellStyle();
                cell_style.WrapText = true;

                //3. 每一列单独处理
                int col_index = 0;
                foreach (TableField col in table.Header.List)
                {
                    //3.1 写名字
                    _set_value(work_sheet, 0, col_index, col.Name);

                    //3.2 写类型 & 约束
                    List<string> constraint_list = _build_constraint(col);
                    constraint_list.Insert(0, col.DataType.ToRuleStr());
                    var cell = _set_value(work_sheet, 1, col_index, string.Join("\n", constraint_list));
                    cell.CellStyle = cell_style;

                    //3.3 写注释
                    _set_value(work_sheet, 2, col_index, col.Desc);

                    col_index++;
                }

                //4. 保存
                string file_path = System.IO.Path.Combine(_config.dir, "R_" + table.SheetName + ".xlsx");
                FileUtil.CreateFileDir(file_path);
                work_book.Write(System.IO.File.OpenWrite(file_path), false);
                work_book.Close();
            }
        }

        public string _build_sheet_name(Table table)
        {
            string ret = table.SheetName;
            if (table.TableExportFlag == EExportFlag.client)
            {
                ret = ret + " | Export_Client";
            }
            else if (table.TableExportFlag == EExportFlag.svr)
            {
                ret = ret + " | Export_Svr";
            }
            else if (table.TableExportFlag == EExportFlag.none)
            {
                ret = ret + " | Export_None";
            }
            return ret;
        }

        public List<string> _build_constraint(TableField col)
        {
            List<string> ret = new List<string>();
            if (col.AttrPK != null)
            {
                ret.Add(col.AttrPK.ToString());
            }
            if (col.ExportFlag == EExportFlag.client)
            {
                ret.Add("Export[Client]");
            }
            else if (col.ExportFlag == EExportFlag.svr)
            {
                ret.Add("Export[Svr]");
            }
            if (col.AttrEnum != null)
            {
                ret.Add(string.Format("Enum[{0}]", col.AttrEnum.Name));
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
