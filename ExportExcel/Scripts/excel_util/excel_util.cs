
using System;
using System.Collections.Generic;

namespace ExportExcel
{
    public static class ExcelUtil
    {
        public static List<string> ExtSupport_Epplus = new List<string>() { ".xlsx", ".xls", ".xlsm" };
        public static List<string> ExtSupport_Npoi = new List<string>() { ".xlsx", ".xls", ".xlsm" };

        public static IWorkbook CreateWorkBook()
        {
            return ExcelEPPlus.WorkBookImp.CreateNew();
            //return ExcelNPOI.WorkBookImp.CreateNew();
        }

        public static IWorkbook Load(string file_path)
        {
            return ExcelEPPlus.WorkBookImp.Load(file_path);
            //return ExcelNPOI.WorkBookImp.Load(file_path);
        }

        public static List<string> CollectExcelFiles(List<string> path_list)
        {
            List<string> ret = new List<string>(200);
            foreach (var path in path_list) //按照文件的添加顺序
            {
                var temp = _CollectExcelFiles(path);
                foreach (var p in temp)
                {
                    if (!ret.Contains(p))
                        ret.Add(p);
                }
            }
            return ret;
        }

        private static List<string> _CollectExcelFiles(string path)
        {
            List<string> ret = new List<string>();
            string full_path = System.IO.Path.GetFullPath(path);
            if (System.IO.File.Exists(full_path))
            {
                if (_IsSupport(full_path))
                    ret.Add(full_path);
            }
            else if (System.IO.Directory.Exists(full_path))
            {
                string[] sub_files = System.IO.Directory.GetFiles(full_path, "*.*", System.IO.SearchOption.AllDirectories);
                foreach (var p in sub_files)
                {
                    if (_IsSupport(p))
                        ret.Add(p);
                }
                //按照路径从小到大,排序
                ret.Sort();
            }
            else
            {
                Logger.Print("路径不存在 {0}", path);
            }
            return ret;
        }

        private static bool _IsSupport(string file_path)
        {
            if (System.IO.Path.GetFileName(file_path).StartsWith("~"))
                return false;

            var support_list = ExtSupport_Epplus;
            //var support_list = ExtSupport_Npoi;
            foreach (string p in support_list)
            {
                if (file_path.EndsWith(p, true, null))
                    return true;
            }

            return false;
        }
    }
}
