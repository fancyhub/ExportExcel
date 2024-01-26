using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/2 11:43:00
 * Title   : 
 * Desc    :  
*************************************************************************************/
namespace ExportExcel
{
    public class PPConstraintDefault : IProcessNode
    {
        public string GetName()
        {
            return "约束 Default 的检查&转换";
        }

        public void Process(DataBase data_base)
        {
            List<int> temp = new List<int>();
            data_base.ForeachCol((col) =>
            {
                if (col.Field.AttrDefault == null)
                    return;

                col.ForeachCell((cell) =>
                {
                    if (string.IsNullOrEmpty(cell.Value))
                        cell.Value = col.Field.AttrDefault;
                });
            });
        }         
    }

    // 格式 : Default[default_value] 
    public class ConParserDefault : IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db)
        {
            TableField col = db_col.Field;
            string default_value = _parse_default(col);
            if (string.IsNullOrEmpty(default_value))
                return;
            col.AttrDefault = default_value;
        }


        private static string _parse_default(TableField col)
        {
            foreach (var str in col.StrConstraints)
            {
                var temp = str.Trim();
                if (!temp.ToLower().StartsWith("default["))
                {
                    continue;
                }
                int start_index = "default[".Length;
                int end_index = temp.Length - 1;
                var ret = temp.Substring(start_index, end_index - start_index);
                return ret;
            }
            return null;
        }
    }
}
