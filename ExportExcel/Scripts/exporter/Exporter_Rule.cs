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
        public Config.TableDataLoaderConfig _tableConfig;
        public Exporter_Rule(Config.RuleConfig config, Config.TableDataLoaderConfig tableConfig)
        {
            _config = config;
            _tableConfig = tableConfig;
        }

        public string GetName()
        {
            return "Export Rule Excel File";
        }
        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable)
                return;

            {
                IWorkbook work_book = ExcelUtil.CreateWorkBook();
                ISheet work_sheet = work_book.CreateSheet($"@EnumConfig");

                var cell_style = work_book.CreateCellStyle();
                cell_style.WrapText = true;

                _SetValue(work_sheet, 0, 0, "EnumName");
                _SetValue(work_sheet, 0, 1, "EnumFieldName");
                _SetValue(work_sheet, 0, 2, "ExcelVal");
                _SetValue(work_sheet, 0, 3, "Val");

                int row_index = 1;
                foreach (var p in data.EnumDB)
                {
                    foreach (var f in p.Value.Dict)
                    {
                        _SetValue(work_sheet, row_index, 0, p.Key);
                        _SetValue(work_sheet, row_index, 1, f.Key);
                        _SetValue(work_sheet, row_index, 2, f.Value.ExcelVal);
                        _SetValue(work_sheet, row_index, 3, f.Value.Val.ToString());
                        row_index++;
                    }

                    row_index++;
                }

                string file_path = System.IO.Path.Combine(_config.dir, "EnumConfig.xlsx");
                FileUtil.CreateFileDir(file_path);
                work_book.Write(System.IO.File.OpenWrite(file_path), false);
                work_book.Close();
            }


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
                var sheet_name = _BuildSheetName(table);
                ISheet work_sheet = work_book.CreateSheet(sheet_name);
                var cell_style = work_book.CreateCellStyle();
                cell_style.WrapText = true;

                //3. 每一列单独处理
                int col_index = 0;
                foreach (TableField col in table.Header.List)
                {
                    //3.1 写名字
                    _SetValue(work_sheet, _tableConfig.nameRowIndex, col_index, col.Name);

                    //3.2 写类型 & 约束
                    List<string> constraint_list = _build_constraint(col);
                    constraint_list.Insert(0, col.DataType.ToRuleStr());
                    var cell = _SetValue(work_sheet, _tableConfig.typeRowIndex, col_index, string.Join("\n", constraint_list));
                    cell.CellStyle = cell_style;

                    //3.3 写注释
                    _SetValue(work_sheet, _tableConfig.descRowIndex, col_index, col.Desc);

                    col_index++;
                }

                //4. 保存
                string file_path = System.IO.Path.Combine(_config.dir, "R_" + table.SheetName + ".xlsx");
                FileUtil.CreateFileDir(file_path);
                work_book.Write(System.IO.File.OpenWrite(file_path), false);
                work_book.Close();
            }
        }



        public string _BuildSheetName(Table table)
        {
            string ret = table.SheetName;
            if (table.TableExportFlag == EExportFlagMask.Client)
            {
                ret = ret + " | Export_Client";
            }
            else if (table.TableExportFlag == EExportFlagMask.Server)
            {
                ret = ret + " | Export_Svr";
            }
            else if (table.TableExportFlag == EExportFlagMask.None)
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
            if (col.ExportFlag == EExportFlagMask.Client)
            {
                ret.Add("Export[Client]");
            }
            else if (col.ExportFlag == EExportFlagMask.Server)
            {
                ret.Add("Export[Svr]");
            }
            if (col.AttrEnum != null)
            {
                ret.Add(string.Format("Enum[{0}]", col.AttrEnum.Name));
            }
            return ret;
        }

        public ICell _SetValue(ISheet sheet, int row_idx, int col_idx, string v)
        {
            IRow row = sheet.GetRow(row_idx);
            ICell cell = row.GetCell(col_idx);
            cell.SetCellValue(v);
            return cell;
        }
    }
}
