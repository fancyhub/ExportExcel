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

        private FilePathChecker _checker;
        public PPConstraintFilePath(string search_root)
        {
            _checker = new FilePathChecker(search_root);
        }

        public void Process(DataBase data_base)
        {
            List<string> path_list = new List<string>();
            data_base.ForeachCol((col) =>
            {
                if (!_checker.SetAttr(col.Field.AttrFilePath))
                    return;
                bool is_list = col.Field.DataType.IsList;

                col.ForeachCell((cell) =>
                {
                    _get_path_list(cell.Value, ref path_list, is_list);

                    foreach (string sub_path in path_list)
                    {
                        var rslt = _checker.Check(sub_path, out string real_path);
                        switch (rslt)
                        {
                            case EFilePathCheckResult.ok:
                                break;

                            case EFilePathCheckResult.contain_splash:
                                ErrSet.E(cell, $"路径不能含有 \\, 要改成 /, {sub_path}");
                                break;

                            case EFilePathCheckResult.case_fail:
                                ErrSet.E(cell, $"路径大小写不对, {sub_path} -> {real_path}");
                                break;

                            case EFilePathCheckResult.not_exist:
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

        private static void _get_path_list(string cell_v, ref List<string> path_list, bool is_list)
        {
            path_list.Clear();
            if (string.IsNullOrEmpty(cell_v))
                return;

            if (!is_list)
                path_list.Add(cell_v);
            else
                path_list.AddRange(cell_v.Split(";"));
        }


        public enum EFilePathCheckResult
        {
            ok,
            contain_splash, // 包含 \
            not_exist,//
            case_fail,//
        }

        private class FilePathChecker
        {
            public string _search_root;
            public string _suffix;
            public string _search_dir;

            public FilePathChecker(string search_root)
            {
                _search_root = Path.GetFullPath(search_root);
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

            public EFilePathCheckResult Check(string v, out string real_path)
            {
                real_path = null;
                if (string.IsNullOrEmpty(v))
                    return EFilePathCheckResult.ok;

                if (v.IndexOf("\\") >= 0)
                    return EFilePathCheckResult.contain_splash;

                string path = _search_dir + v + _suffix;
                if (!File.Exists(path))
                {
                    real_path = path;
                    return EFilePathCheckResult.not_exist;
                }

                real_path = _get_real_abs_path(path);
                real_path = real_path.Substring(_search_dir.Length, v.Length);
                real_path = real_path.Replace('\\', '/');
                if (v != real_path)
                    return EFilePathCheckResult.case_fail;
                return EFilePathCheckResult.ok;
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

    // 格式: FilePath[Dir,prefab]
    public class ConParserFilePath : IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db)
        {
            var col = db_col.Field;
            col.AttrFilePath = _ParseFilePath(col);
            if (col.AttrFilePath == null)
                return;

            if (!_is_data_type_valid(col.DataType))
                ErrSet.E(db_col, $"该字段是 FilePath[xx,yy] 约束的情况下, 只能支持 string,list_string 类型");
        }

        //只能 string 或者 List<string>
        private static bool _is_data_type_valid(DataType data_type)
        {
            if (data_type.IsTuple)
                return false;

            if (data_type.type0 != EDataType.String)
                return false;

            return true;
        }

        
        private static ConAttrFilePath _ParseFilePath(TableField col)
        {
            foreach (var p in col.StrConstraints)
            {
                var temp = p.Trim();
                if (!temp.ToLower().StartsWith("filepath["))
                    continue;

                int start_index = "filepath[".Length;
                int end_index = temp.Length - 1;
                var ret = temp.Substring(start_index, end_index - start_index);

                var tt = ret.Split(',');
                if (tt.Length == 1)
                {
                    return new ConAttrFilePath()
                    {
                        _dir_prefix = tt[0].Trim(),
                        _file_suffix = null,
                    };
                }
                else
                    return new ConAttrFilePath()
                    {
                        _dir_prefix = tt[0].Trim(),
                        _file_suffix = tt[1].Trim()
                    };

            }
            return null;
        }
    }

}
