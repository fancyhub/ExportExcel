using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/8 11:02:04
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class Table
    {
        public string SheetName;

        //如果这个存在,就不导出 body了
        public Dictionary<string, string[,]> MultiLangBody;
        public string[,] Body;

        public EExportFlag TableExportFlag = EExportFlag.all; //默认全部导出
        public TableHeader Header;
        public string FilePath;

        //路径 对应的数据段
        public List<(int row_start, int row_end, string file_path)> RowFileMap;

        public Table()
        {
            Header = new TableHeader();
            RowFileMap = new List<(int, int, string)>();
        }

        public string FindDataPath(int row_idx)
        {
            foreach (var p in RowFileMap)
            {
                if (row_idx >= p.row_start && row_idx < p.row_end)
                    return p.file_path;
            }
            return string.Empty;
        }
    }
}
