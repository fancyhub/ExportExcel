using System;
using System.Collections.Generic;

//这个文件都是需要用户自己实现的

namespace Test
{
    public partial class TableMgr
    {
        private static byte[] _LoadResCsv(string dir, string sheet_name, string lang_name)
        {
            if (!string.IsNullOrEmpty(lang_name))
                sheet_name = sheet_name + "_" + lang_name;
            string file_path = System.IO.Path.Combine(dir, sheet_name + ".csv");
            string full_path = System.IO.Path.GetFullPath(file_path);

            if (!System.IO.File.Exists(full_path))
            {
                Log.E("找不到文件 {0}", full_path);
                return null;
            }

            using (System.IO.FileStream fs = System.IO.File.Open(full_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int numBytesToRead = Convert.ToInt32(fs.Length);
                var ret = new byte[(numBytesToRead)];
                fs.Read(ret, 0, numBytesToRead);

                return ret;
            }
        }

        private static byte[] _LoadResBin(string dir, string lang_name)
        {
            string file_path = System.IO.Path.Combine(dir, "data.bin");
            if (!string.IsNullOrEmpty(lang_name))
                file_path = System.IO.Path.Combine(dir, $"data_{lang_name}.bin");


            string full_path = System.IO.Path.GetFullPath(file_path);

            if (!System.IO.File.Exists(full_path))
            {
                Log.E("找不到文件 {0}", full_path);
                return null;
            }
            return System.IO.File.ReadAllBytes(full_path);
        }

        private void OnInstCreate()
        {
            AddPostProcesser<TLoc>(_PP_Loc);
        }

        private void OnAllLoaded()
        {

        }

        private void _PP_Loc(Table table)
        {
            //如果LocStr 是类, 那就没有LocMgr
            //foreach (var p in table.Dict as Dictionary<string, TLoc>)
            //{
            //    LocStr.Set(p.Key, p.Value.Val);
            //}

            //如果LocStr 是struct, 需要有LocMgr
            LocMgr loc_mgr = LocMgr.Inst;
            loc_mgr.Clear();

            Dictionary<string, TLoc> str_dict = table.Dict as Dictionary<string, TLoc>;
            Dictionary<int, TLoc> id_dict = table.Dict as Dictionary<int, TLoc>;
            if (str_dict != null)
            {
                foreach (var p in str_dict)
                {
                    loc_mgr.Add(p.Key, p.Value.Val);
                }
            }

            if (id_dict != null)
            {
                foreach (var p in id_dict)
                    loc_mgr.Add(p.Key, p.Value.Val);
            }
        }
    }
}
