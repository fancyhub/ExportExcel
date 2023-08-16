using System;
using System.Collections.Generic;
using System.Text;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/3 10:54:57
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public static class DataTypeUtilCsv
    {
        private static Dictionary<EDataType, string> _data_type_2_csv_str = new Dictionary<EDataType, string>()
        {
            [EDataType.None] = "",
            [EDataType.Bool] = "bool",
            [EDataType.Int32] = "int",
            [EDataType.UInt32] = "uint",
            [EDataType.Int64] = "int64",
            [EDataType.UInt64] = "uint64",
            [EDataType.Float] = "float",
            [EDataType.Double] = "double",
            [EDataType.String] = "string",
            [EDataType.LocStr] = "locstr",
            [EDataType.LocId] = "locid",
        };

        private static StringBuilder _sb = new StringBuilder();

        public static string ToCsvStr(this DataType data)
        {
            string ret = _data_type_2_csv_str[data.type0];
            _sb.Clear();
            if (data.IsList)
                _sb.Append("list_");
            _sb.Append(_data_type_2_csv_str[data.type0]);
            for (int i = 1; i < data.Count; i++)
            {
                _sb.Append("_");
                _sb.Append(_data_type_2_csv_str[data.Get(i)]);
            }

            return _sb.ToString();
        }

        private static char[] S_CONSTRAINT_SPLIT = new char[] { '\n', '|' };
        private static string[] S_Empty_Constraint = new string[0];

        // 从表格的数据里面解析格式
        public static DataType ParseDataType(string value, out string[] str_constraint_array)
        {
            str_constraint_array = S_Empty_Constraint;
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception("未知的类型 " + value);
            }
            string[] temp = value.Split(S_CONSTRAINT_SPLIT, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length == 0)
            {
                throw new Exception("未知的类型 " + value);
            }

            //把后面的约束填到列表里面
            str_constraint_array = new string[temp.Length - 1];
            for (int i = 1; i < temp.Length; i++)
                str_constraint_array[i - 1] = temp[i];

            string type_value = temp[0].ToLower().Trim();
            DataType ret = new DataType();
            foreach (var a in type_value.Split("_", StringSplitOptions.RemoveEmptyEntries))
            {
                switch (a)
                {
                    case "list":
                        ret.IsList = true;
                        break;
                    case "bool":
                        ret.AddType(EDataType.Bool);
                        break;
                    case "double":
                        ret.AddType(EDataType.Double);
                        break;
                    case "int":
                    case "int32":
                        ret.AddType(EDataType.Int32);
                        break;
                    case "uint":
                    case "uint32":
                        ret.AddType(EDataType.UInt32);
                        break;
                    case "int64":
                        ret.AddType(EDataType.Int64);
                        break;
                    case "uint64":
                        ret.AddType(EDataType.UInt64);
                        break;
                    case "float":
                        ret.AddType(EDataType.Float);
                        break;
                    case "locstr":
                        ret.AddType(EDataType.LocStr);
                        break;
                    case "string":
                        ret.AddType(EDataType.String);
                        break;
                    default:
                        throw new Exception("未知的类型 " + a);
                }
            }
            return ret;
        }
    }
}
