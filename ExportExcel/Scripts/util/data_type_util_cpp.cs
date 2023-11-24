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
    public static class DataTypeUtilCpp
    {
        private static Dictionary<EDataType, string> _data_type_2_cpp_str = new Dictionary<EDataType, string>()
        {
            [EDataType.Bool] = "bool",
            [EDataType.UInt32] = "unsigned int",
            [EDataType.Int32] = "int",
            [EDataType.Int64] = "long long",
            [EDataType.UInt64] = "unsigned long long",
            [EDataType.Float32] = "float",
            [EDataType.Float64] = "double",
            [EDataType.String] = "std::string",
            [EDataType.LocStr] = "LocStr",
            [EDataType.LocId] = "LocId",
        };

        private static StringBuilder _sb = new StringBuilder();    

        public static string ToCppStr(this TableField field)
        {
            _sb.Clear();
            var type = field.DataType;

            if(type.IsList)
            {
                _sb.Append("std::vector<");
            }
            if (type.IsTuple)
            {
                _sb.Append("std::tuple<");
                _sb.Append(_data_type_2_cpp_str[type.type0]);

                for (int i = 1; i < type.Count; i++)
                {
                    _sb.Append(",");
                    _sb.Append(_data_type_2_cpp_str[type.Get(i)]);
                }
                _sb.Append(">");
            }
            else
            {
                if (field.AttrEnum == null)
                    _sb.Append(_data_type_2_cpp_str[type.type0]);
                else
                    _sb.Append(field.AttrEnum.Name);
            }

            if (type.IsList)
            {
                _sb.Append(">");
            }            

            return _sb.ToString();
        }
    }
}
