using System;
using System.Collections.Generic;

namespace ExportExcel
{
    //处理 数字引用的替换
    public class PostProcessRef : I_ProcessNode
    {
        public string GetName()
        {
            return "处理引用";
        }

        public void Process(DataBase data_base)
        {
            DBRef number_db = data_base.RefDB;
            data_base.ForeachCol((col) =>
            {
                if (!_is_type_can_ref(col.Col.DataType))
                    return;

                col.ForeachCell((cell) =>
                {
                    string cell_v = cell.Value;
                    if (!_try_get_key_pair(cell_v, out var key_val, out string col_name))
                        return;

                    if (number_db.GetValue(key_val, col_name, out string new_v))
                    {
                        cell.Value = new_v;
                        return;
                    }
                    ErrSet.E(cell, $"找不到 {cell_v}");
                });
            });
        }

        public bool _try_get_key_pair(string cell_v, out string key_val, out string col_name)
        {
            key_val = null;
            col_name = null;
            if (string.IsNullOrEmpty(cell_v))
                return false;

            if (!cell_v.StartsWith("R(") && !cell_v.StartsWith("D("))
                return false;

            if (!cell_v.EndsWith(")"))
                return false;

            int index = cell_v.IndexOf(",");
            if (index < 0)
                return false;

            key_val = cell_v.Substring(2, index - 2).Trim();
            col_name = cell_v.Substring(index + 1, cell_v.Length - index - 2).Trim();
            return true;
        }

        public bool _is_type_can_ref(DataType data_type)
        {
            //switch (data_type.type1)
            //{
            //    case E_DATA_TYPE.LocStr:
            //    case E_DATA_TYPE.String:
            //        return false;
            //}

            //if (!data_type.is_pair)
            //    return true;

            //switch (data_type.type2)
            //{
            //    case E_DATA_TYPE.LocStr:
            //    case E_DATA_TYPE.String:
            //        return false;
            //}
            return true;
        }
    }
}
