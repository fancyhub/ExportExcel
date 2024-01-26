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
            [EDataType.Float32] = "float",
            [EDataType.Float64] = "double",
            [EDataType.String] = "string",
            [EDataType.LocStr] = "LocStr",
            [EDataType.LocId] = "LocId",
        };

        private static StringBuilder _sb = new StringBuilder();

        public static string ToCSharpStr(this TableField item)
        {
            if (item.DataType.IsTuple && item.AttrAlias != null)
            {
                if (item.AttrAlias.CSharp != null)
                {
                    if (item.DataType.IsList)
                        return item.AttrAlias.CSharp + "[]";
                    return item.AttrAlias.CSharp;
                }
            }

            var type = item.DataType;
            _sb.Clear();
            if (type.IsTuple)
            {
                _sb.Append("(");
                _sb.Append(_data_type_2_csharp_str[type.type0]);
                string field_name = item.GetAliasCsharpFieldName(0);
                if (field_name != null)
                {
                    _sb.Append(' ');
                    _sb.Append(field_name);
                }

                for (int i = 1; i < type.Count; i++)
                {
                    _sb.Append(",");
                    _sb.Append(_data_type_2_csharp_str[type.Get(i)]);

                    field_name = item.GetAliasCsharpFieldName(i);
                    if (field_name != null)
                    {
                        _sb.Append(' ');
                        _sb.Append(field_name);
                    }
                }
                _sb.Append(")");
            }
            else
            {
                if (item.AttrEnum == null)
                    _sb.Append(_data_type_2_csharp_str[type.type0]);
                else
                    _sb.Append(item.AttrEnum.Name);
            }

            if (type.IsList)
                _sb.Append("[]");

            return _sb.ToString();
        }

        public static string ToCSharpStr(this DataType type)
        {
            _sb.Clear();
            if (type.IsTuple)
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
                _sb.Append(_data_type_2_csharp_str[type.type0]);
            }

            if (type.IsList)
                _sb.Append("[]");

            return _sb.ToString();
        }
    }
}
