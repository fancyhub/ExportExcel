
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
            foreach (var path in path_list)
            {
                _CollectExcelFiles(path, ret);
            }
            return ret;
        }

        public static List<string> CollectExcelFiles(string path)
        {
            List<string> ret = new List<string>(200);
            _CollectExcelFiles(path, ret);
            return ret;
        }

        private static void _CollectExcelFiles(string path, List<string> inout_list)
        {
            string full_path = System.IO.Path.GetFullPath(path);
            if (System.IO.File.Exists(full_path))
            {
                if (_IsSupport(full_path) && !inout_list.Contains(full_path))
                    inout_list.Add(full_path);
            }
            else if (System.IO.Directory.Exists(full_path))
            {
                string[] sub_files = System.IO.Directory.GetFiles(full_path, "*.*", System.IO.SearchOption.AllDirectories);
                foreach (var p in sub_files)
                {
                    if (_IsSupport(p) && !inout_list.Contains(full_path))
                        inout_list.Add(p);
                }
            }
            else
            {
                Logger.Print("路径不存在 {0}", path);
            }
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
