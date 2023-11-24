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
                DataType data_type = col.Col.DataType;
                EnumType enum_ref = data_type.enum_type;
                if (enum_ref == null)
                    return;

                col.ForeachCell((cell) =>
                {
                    string new_v = _Convert(cell.Value, data_type, temp);
                    if (!string.IsNullOrEmpty(new_v))
                    {
                        cell.Value = new_v;
                        return;
                    }

                    ErrSet.E(cell, $"转换为枚举失败 \"{cell.Value}\"");
                });
            });
        }

        public static string _Convert(string v, DataType data_type, List<int> temp_list)
        {
            //1. 先把string 转换成list int
            temp_list.Clear();
            var temp = v.Split(ConstDef.C_LIST_SPLIT);
            EnumType enum_table = data_type.enum_type;
            foreach (var p in temp)
            {
                bool ok = enum_table.Convert(p, out int result);
                if (!ok)
                    return null;
                temp_list.Add(result);
            }

            //2.如果是list 类型, 直接join
            if (data_type.IsList)
                return string.Join(ConstDef.C_LIST_SPLIT, temp_list);

            //3. 按位合并
            int enum_val = 0;
            foreach (var ev in temp_list)
                enum_val = enum_val | ev;
            return enum_val.ToString();
        }
    }
}
