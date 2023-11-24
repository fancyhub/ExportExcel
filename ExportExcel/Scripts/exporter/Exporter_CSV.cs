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
    public class Exporter_CSV : IProcessNode
    {

        public EExportFlag _flag;
        public Config.CsvConfig _config;

        public Exporter_CSV(EExportFlag flag, Config.CsvConfig config)
        {
            _flag = flag;
            _config = config;
        }
        public string GetName()
        {
            return "Export Csv";
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

                List<FilterTable> multi_lang_tables = FilterTable.SplitMultiLangTable(p.Value, _flag);

                foreach (var table in multi_lang_tables)
                {
                    List<TableField> header = table.GetHeader();
                    string out_file_path = Path.Combine(_config.dir, table.SheetName + ".csv");
                    FileUtil.CreateFileDir(out_file_path);
                    using (StreamWriter sw = new StreamWriter(out_file_path, false, encoding))
                    {
                        //写文件头
                        int count = header.Count;
                        int index = 0;
                        foreach (TableField head_col in header)
                        {
                            if (index > 0)
                                sw.Write(',');
                            sw.Write(head_col.Name);
                            index++;
                        }
                        sw.Write("\n");

                        index = 0;
                        foreach (TableField head_col in header)
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
                                sw.Write(_FormatCsvStr(s));
                            }
                            sw.Write("\n");
                        }
                    }
                }
            }
        }

        public static string _FormatCsvStr(string s)
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
