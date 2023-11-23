using ExportExcel.ExcelEPPlus;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/7 10:06:37
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class TableDataLoader
    {
        //列名的检查
        private static Regex S_COL_NAME_REGEX = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");        

        public TableHeaderCompareResult _header_compare_result = new TableHeaderCompareResult();
        public List<List<string>> _temp_data = new List<List<string>>(100000);
        public ExeConfig _config;
        public TableDataLoader(ExeConfig config)
        {
            _config = config;

            S_COL_NAME_REGEX = new Regex(config.validation.colNameReg);
        }

        public void Load(DataBase data_base, ISheet sheet, string sheet_name, EExportFlag export_flag)
        {
            sheet.CalculateFormula();
            Table rule_table = data_base.FindTable(sheet_name);
            Table data_table = _create_rule_table(sheet, sheet_name, export_flag);

            if (rule_table == null)
            {
                rule_table = data_table;
                data_base.Tables.Add(sheet_name, rule_table);
            }
            else if (!rule_table.Header.CheckCols(data_table.Header, _header_compare_result))
            {
                _header_compare_result.PrintErr(sheet_name, sheet.Workbook.FilePath);
                return;
            }

            _load_sheet_data(sheet, data_table.Header, _temp_data);
            _merge_data(rule_table, data_table.Header, _temp_data, sheet.Workbook.FilePath);
        }

        public void _load_sheet_data(
            ISheet sheet,
            TableHeader header,
            List<List<string>> out_data)
        {
            out_data.Clear();
            int start_row = 3;
            int end_row = sheet.RowCount;

            if (out_data.Capacity < end_row)
                out_data.Capacity = end_row;

            for (int i = start_row; i < end_row; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null)
                    continue;
                string first_col_cell_str = row.CellStrExt(0);
                if (string.IsNullOrEmpty(first_col_cell_str))
                    continue;
                if (first_col_cell_str.StartsWith("#"))
                    continue;

                List<string> row_data = new List<string>(header.Count);

                for (int c = 0; c < header.Count; c++)
                {
                    string cell_str = row.CellStrExt(header[c].ExcelColIdx);
                    row_data.Add(cell_str);
                }

                out_data.Add(row_data);
            }
        }

        public Table _create_rule_table(ISheet sheet, string sheet_name, EExportFlag export_flag)
        {
            //1. 先生成表格
            Table rule_table = new Table();
            rule_table.FilePath = sheet.Workbook.FilePath;
            rule_table.SheetName = sheet_name;
            rule_table.TableExportFlag = export_flag;

            //4. 获取各行,以及多少列
            IRow row_name = sheet.GetRow(0);   //名字那一行
            IRow row_type = sheet.GetRow(1);   //类型哪一行
            IRow row_desc = sheet.GetRow(2);   //描述行
            int col_count = row_name.ColCount;

            //5. 开始每列的操作
            for (int i = 0; i < col_count; i++)
            {
                //5.1 检查,获取字段名,如果名字以#开始,说明不导出
                string field_name = row_name.CellStrExt(i);
                if (string.IsNullOrEmpty(field_name) || field_name.StartsWith("#"))
                    continue;

                //5.2 检查名字是否合法, 多语言表的字段名不检查
                if (!_config.localization.IsLocalizationSheet(sheet_name)  && !S_COL_NAME_REGEX.IsMatch(field_name))
                {
                    ErrSet.E($"{rule_table.SheetName}.{field_name} 该字段不符合命名规范", sheet.Workbook.FilePath);
                    continue;
                }

                //5.3 检查该列的名字是否重复了
                if (rule_table.Header[field_name] != null)
                {
                    ErrSet.E($"{rule_table.SheetName}.{field_name} 该字段重复出现", sheet.Workbook.FilePath);
                    continue;
                }

                //5.4 获取列的类型
                string cell_val_type = row_type.CellStrExt(i);
                DataType field_type = new DataType();
                string[] StrConstraints = null;
                try
                {
                    field_type = DataTypeUtilCsv.ParseDataType(cell_val_type, out StrConstraints);
                    if (!field_type.Valid())
                    {
                        ErrSet.E($"{rule_table.SheetName}.{field_name} {cell_val_type} 数据类型未知", sheet.Workbook.FilePath);
                        continue;
                    }
                }
                catch (Exception e)
                {
                    ErrSet.E($"{rule_table.SheetName}.{field_name} 解析类型出错 " + e.Message,sheet.Workbook.FilePath);
                    continue;
                }

                TableHeaderItem col = new TableHeaderItem();
                col.Name = field_name;
                col.DataType = field_type;
                col.Desc = row_desc.CellStrExt(i);
                col.StrConstraints = StrConstraints;
                col.ExcelColIdx = i;
                rule_table.Header.Add(col);
            }
            rule_table.Body = new string[0, rule_table.Header.Count];
            return rule_table;
        }


        public static void _merge_data(Table tar, TableHeader data_header, List<List<string>> data, string file_path)
        {
            int old_row_count = tar.Body.GetLength(0);
            int other_row_count = data.Count;
            if (other_row_count == 0)
                return;

            int new_count = old_row_count + other_row_count;
            string[,] new_data = new string[new_count, tar.Header.Count];
            //复制旧的
            Array.Copy(tar.Body, new_data, tar.Body.Length);

            tar.RowFileMap.Add((old_row_count, new_count, file_path));

            //复制新的
            for (int rule_col_idx = 0; rule_col_idx < tar.Header.Count; rule_col_idx++)
            {
                //找到对应的数据 index
                TableHeaderItem rule_col = tar.Header[rule_col_idx];
                int data_col_idx = data_header.IndexOfCol(rule_col.Name);
                //没有找到, 填充空
                if (data_col_idx < 0)
                {
                    for (int r = 0; r < other_row_count; r++)
                        new_data[r + old_row_count, rule_col_idx] = string.Empty;
                    continue;
                }

                //如果规则表里面的描述为空, 从数据表里面获取描述
                if (string.IsNullOrEmpty(rule_col.Desc))
                    rule_col.Desc = data_header[data_col_idx].Desc;

                for (int r = 0; r < other_row_count; r++)
                    new_data[r + old_row_count, rule_col_idx] = data[r][data_col_idx];
            }
            tar.Body = new_data;
        }
    }
}
