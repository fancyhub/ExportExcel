using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/2 11:43:00
 * Title   : 
 * Desc    :  枚举类型的转换, 变成int
*************************************************************************************/
namespace ExportExcel
{
    public class PPConstraintEnum : IProcessNode
    {
        public string GetName()
        {
            return "约束 Enum 的检查&转换";
        }

        public void Process(DataBase data_base)
        {
            List<int> temp = new List<int>();
            data_base.ForeachCol((col) =>
            {
                if (col.Field.AttrEnum == null)
                    return;

                col.ForeachCell((cell) =>
                {
                    string new_v = _Convert(cell.Value, col.Field, temp);
                    if (!string.IsNullOrEmpty(new_v))
                    {
                        cell.Value = new_v;
                        return;
                    }

                    ErrSet.E(cell, $"转换为枚举失败 \"{cell.Value}\"");
                });
            });
        }

        public static string _Convert(string v, TableField header_item, List<int> temp_list)
        {
            //1. 先把string 转换成list int
            temp_list.Clear();
            var temp = v.Split(ConstDef.C_LIST_SPLIT);
            EnumType enum_table = header_item.AttrEnum;
            foreach (var p in temp)
            {
                bool ok = enum_table.Convert(p, out int result);
                if (!ok)
                    return null;
                temp_list.Add(result);
            }

            //2.如果是list 类型, 直接join
            if (header_item.DataType.IsList)
                return string.Join(ConstDef.C_LIST_SPLIT, temp_list);

            //3. 按位合并
            int enum_val = 0;
            foreach (var ev in temp_list)
                enum_val = enum_val | ev;
            return enum_val.ToString();
        }
    }

    // 格式 : Enum[E_Enum_Name] 
    public class ConParserEnum : IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db)
        {
            TableField col = db_col.Field;
            string ref_enum_name = _parse_enum(col);
            if (string.IsNullOrEmpty(ref_enum_name))
                return;

            col.AttrEnum = db.EnumDB.Find(ref_enum_name);
            if (col.AttrEnum == null)
            {
                ErrSet.E(db_col, $"找不到对应的枚举类型 {ref_enum_name}");
                return;
            }

            if (col.AttrEnum != null && !_is_support_enum(col.DataType))
            {
                ErrSet.E(db_col, $"只有 int,list_int 支持枚举类型 {ref_enum_name}");
                return;
            }
        }

        
        private static string _parse_enum(TableField col)
        {
            foreach (var str in col.StrConstraints)
            {
                var temp = str.Trim();
                if (!temp.ToLower().StartsWith("enum["))
                {
                    continue;
                }
                int start_index = "enum[".Length;
                int end_index = temp.Length - 1;
                var ret = temp.Substring(start_index, end_index - start_index);
                return ret;
            }
            return null;
        }

        private bool _is_support_enum(DataType data_type)
        {
            //第一个类型如果不是int32, 不支持
            if (data_type.type0 != EDataType.Int32)
                return false;

            //如果是pair 也不支持
            if (data_type.IsTuple)
                return false;

            //List 不支持
            if (data_type.IsList)
                return false;

            return true;
        }

    }


}
