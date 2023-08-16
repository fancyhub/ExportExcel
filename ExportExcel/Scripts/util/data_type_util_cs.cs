using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/3 10:54:57
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public static class DataTypeUtilCS
    {
        private static Dictionary<EDataType, string> _data_type_2_csharp_str = new Dictionary<EDataType, string>()
        {
            [EDataType.Bool] = "bool",
            [EDataType.UInt32] = "uint",
            [EDataType.Int32] = "int",
            [EDataType.Int64] = "long",
            [EDataType.UInt64] = "ulong",
            [EDataType.Float] = "float",
            [EDataType.Double] = "double",
            [EDataType.String] = "string",
            [EDataType.LocStr] = "LocStr",
            [EDataType.LocId] = "LocId",
        };

        private static StringBuilder _sb = new StringBuilder();
        public static string ToCSharpStr(this DataType type)
        {
            _sb.Clear();
            if (type.IsPair)
            {
                _sb.Append("(");
                _sb.Append(_data_type_2_csharp_str[type.type0]);

                for (int i = 1; i < type.Count; i++)
                {
                    _sb.Append(",");
                    _sb.Append(_data_type_2_csharp_str[type.Get(i)]);
                }
                _sb.Append(")");
            }
            else
            {
                if (type.enum_type == null)
                    _sb.Append(_data_type_2_csharp_str[type.type0]);
                else
                    _sb.Append(type.enum_type.Name);
            }

            if (type.IsList)
                _sb.Append("[]");

            return _sb.ToString();
        }
    }
}
