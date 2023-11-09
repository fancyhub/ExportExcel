using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/2 14:27:45
 * Title   : 
 * Desc    :  路径检查
*************************************************************************************/
namespace ExportExcel
{
    public class PPConstraintFilePath : IProcessNode
    {
        public string GetName()
        {
            return "约束 FilePath 的检查";
        }

        public FilePathChecker _checker;
        public PPConstraintFilePath(string search_root)
        {
            _checker = new FilePathChecker(search_root);
        }

        public void Process(DataBase data_base)
        {
            List<string> path_list = new List<string>();
            data_base.ForeachCol((col) =>
            {
                if (!_checker.SetAttr(col.Col.AttrFilePath))
                    return;
                bool is_list = col.Col.DataType.IsList;

                col.ForeachCell((cell) =>
                {
                    _get_path_list(cell.Value, ref path_list, is_list);

                    foreach (string sub_path in path_list)
                    {
                        var rslt = _checker.Check(sub_path, out string real_path);
                        switch (rslt)
                        {
                            case E_FILE_PATH_CHECK_RSLT.ok:
                                break;

                            case E_FILE_PATH_CHECK_RSLT.contain_splash:
                                ErrSet.E(cell, $"路径不能含有 \\, 要改成 /, {sub_path}");
                                break;

                            case E_FILE_PATH_CHECK_RSLT.case_faile:
                                ErrSet.E(cell, $"路径大小写不对, {sub_path} -> {real_path}");
                                break;

                            case E_FILE_PATH_CHECK_RSLT.not_exist:
                                ErrSet.E(cell, $"找不到路径, {sub_path} -> {real_path}");
                                break;

                            default:
                                ErrSet.E(cell, $"未知类型 {rslt}");
                                break;
                        }
                    }
                });
            });
        }

        public void _get_path_list(string cell_v, ref List<string> path_list, bool is_list)
        {
            path_list.Clear();
            if (string.IsNullOrEmpty(cell_v))
                return;

            if (!is_list)
                path_list.Add(cell_v);
            else
                path_list.AddRange(cell_v.Split(";"));
        }
    }

    public enum E_FILE_PATH_CHECK_RSLT
    {
        ok,
        contain_splash, // 包含 \
        not_exist,//
        case_faile,//
    }

    public class FilePathChecker
    {
        public string _search_root;
        public string _suffix;
        public string _search_dir;

        public FilePathChecker(string search_root)
        {
            _search_root = System.IO.Path.GetFullPath(search_root);
        }

        public bool SetAttr(ConAttrFilePath attr)
        {
            //1. 检查空
            if (attr == null)
                return false;

            //2. 获取dir
            _search_dir = _search_root;
            if (!string.IsNullOrEmpty(attr._dir_prefix))
                _search_dir = Path.Combine(_search_root, attr._dir_prefix);
            _search_dir = _search_dir.Replace('\\', '/');
            if (!_search_dir.EndsWith("/"))
                _search_dir = _search_dir + "/";

            //3. 获取后缀
            _suffix = attr._file_suffix;
            if (string.IsNullOrEmpty(_suffix))
                _suffix = string.Empty;
            else
                _suffix = "." + _suffix;

            return true;
        }

        public E_FILE_PATH_CHECK_RSLT Check(string v, out string real_path)
        {
            real_path = null;
            if (string.IsNullOrEmpty(v))
                return E_FILE_PATH_CHECK_RSLT.ok;

            if (v.IndexOf("\\") >= 0)
                return E_FILE_PATH_CHECK_RSLT.contain_splash;

            string path = _search_dir + v + _suffix;
            if (!File.Exists(path))
            {
                real_path = path;
                return E_FILE_PATH_CHECK_RSLT.not_exist;
            }

            real_path = _get_real_abs_path(path);
            real_path = real_path.Substring(_search_dir.Length, v.Length);
            real_path = real_path.Replace('\\', '/');
            if (v != real_path)
                return E_FILE_PATH_CHECK_RSLT.case_faile;
            return E_FILE_PATH_CHECK_RSLT.ok;
        }

        public string _get_real_abs_path(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string file_name = Path.GetFileName(path);

            var files = Directory.GetFiles(dir, file_name);
            return files.FirstOrDefault();
        }
    }
}
