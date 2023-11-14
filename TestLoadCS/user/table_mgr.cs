using System;
using System.Collections.Generic;

//这个文件都是需要用户自己实现的

namespace Test
{
    public partial class TableMgr
    {
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
