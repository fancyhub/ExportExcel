using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/7 17:46:13
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    //导出 CSV 结构
    public class ExporterCSV : IProcessNode
    {
        
        public EExportFlag _flag;
        public ExeConfig.CsvConfig _config;

        public ExporterCSV(EExportFlag flag, ExeConfig.CsvConfig config)
        {
            _flag = flag;
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

            Encoding encoding = new UTF8Encoding(_config.utf8bom);
            foreach (var p in data.Tables)
            {
                //判断是否需要导出
                if ((p.Value.TableExportFlag & _flag) == 0)
                    continue;

                List<FilterTable> multi_lang_tables = _get_multi_lang_table(p.Value);

                foreach (var table in multi_lang_tables)
                {
                    List<TableHeaderItem> header = table.Header;
                    string out_file_path = Path.Combine(_config.dir, table.SheetName + ".csv");
                    FileUtil.CreateFileDir(out_file_path);
                    using (StreamWriter sw = new StreamWriter(out_file_path, false, encoding))
                    {
                        //写文件头
                        int count = header.Count;
                        int index = 0;
                        foreach (TableHeaderItem head_col in header)
                        {
                            if (index > 0)
                                sw.Write(',');
                            sw.Write(head_col.Name);
                            index++;
                        }
                        sw.Write("\n");

                        index = 0;
                        foreach (TableHeaderItem head_col in header)
                        {
                            if (index > 0)
                                sw.Write(',');
                            sw.Write(head_col.DataType.ToCsvStr());
                            index++;
                        }
                        sw.Write("\n");

                        //写数据
                        int row_count = table.RowCount;
                        int col_count = table.ColCount;
                        for (int i = 0; i < row_count; i++)
                        {
                            for (int j = 0; j < col_count; j++)
                            {
                                if (j > 0)
                                    sw.Write(',');
                                string s = table[i, j];
                                sw.Write(_format_csv_str(s));
                            }
                            sw.Write("\n");
                        }
                    }
                }
            }
        }

        public List<FilterTable> _get_multi_lang_table(Table table)
        {
            List<FilterTable> ret = new List<FilterTable>();
            if (table.MultiLangBody == null)
            {
                ret.Add(new FilterTable(table, _flag));
                return ret;
            }

            foreach (var p in table.MultiLangBody)
            {
                FilterTable t = new FilterTable(table, _flag);
                t._body = p.Value;
                t.SheetName = table.SheetName + "_" + p.Key;
                t._row_count = p.Value.GetLength(0);
                ret.Add(t);
            }
            return ret;
        }

        public static string _format_csv_str(string s)
        {
            bool contain_qutos = s.Contains("\"");
            bool contain_newline = (s.Contains("\n") || s.Contains("\r"));
            bool contain_c = s.Contains(',');
            if (!contain_qutos && !contain_newline && !contain_c)
                return s;

            if (contain_qutos)
                s = s.Replace("\"", "\"\"");

            return string.Concat("\"", s, "\"");
        }
    }
}
