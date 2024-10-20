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
    public static class DataTypeUtil
    {

        private static Dictionary<EDataType, string> _data_type_2_go_str = new Dictionary<EDataType, string>()
        {
            [EDataType.Bool] = "bool",
            [EDataType.Int32] = "int32",
            [EDataType.Int64] = "int64",
            [EDataType.UInt32] = "uint32",
            [EDataType.UInt64] = "uint64",
            [EDataType.Float32] = "float32",
            [EDataType.Float64] = "float64",
            [EDataType.String] = "string",
            [EDataType.LocStr] = "string",
            [EDataType.LocId] = "int32",
        };

        private static StringBuilder _sb = new StringBuilder();

        public static string ToGoStr(this EDataType type)
        {
            return _data_type_2_go_str[type];
        }

        public static string ToGoStr(this TableField field)
        {
            if (field.DataType.IsTuple)
            {
                if (field.AttrAlias != null)
                {
                    if (field.AttrAlias.Go != null)
                    {
                        if (field.DataType.IsList)
                            return "[]" + field.AttrAlias.Go;
                        return field.AttrAlias.Go;
                    }
                    else
                    {
                        if (field.DataType.IsList)
                            return "[]" + field.AttrAlias.Name;
                        return field.AttrAlias.Name;
                    }

                }
            }
            else if (field.DataType.type0 == EDataType.String)
            {
                if (field.AttrAlias != null && field.AttrAlias.Go != null)
                {
                    if (field.DataType.IsList)
                        return "[]" + field.AttrAlias.Go;
                    return field.AttrAlias.Go;
                }
            }


            _sb.Clear();
            var type = field.DataType;
            if (type.IsList)
                _sb.Append("[]");

            if (type.IsTuple)
            {
                _sb.Append("Tuple");
                _sb.Append(_UpFirst(_data_type_2_go_str[type.type0]));

                for (int i = 1; i < type.Count; i++)
                {
                    _sb.Append(_UpFirst(_data_type_2_go_str[type.Get(i)]));
                }
            }
            else
            {
                if (field.AttrEnum == null)
                    _sb.Append(_data_type_2_go_str[type.type0]);
                else
                    _sb.Append(field.AttrEnum.Name);
            }

            return _sb.ToString();
        }

        public static string GoUpFirstCase(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            if ('a' <= name[0] && name[0] <= 'z')
            {
                return name.Substring(0, 1).ToUpper() + name.Substring(1);
            }
            return name;
        }

        public static string ToGoStr(this DataType data_type)
        {
            _sb.Clear();

            if (data_type.IsList)
                _sb.Append("[]");

            if (data_type.IsTuple)
            {
                _sb.Append("Tuple");
                _sb.Append(_UpFirst(_data_type_2_go_str[data_type.type0]));
            }
            else
                _sb.Append(_data_type_2_go_str[data_type.type0]);

            for (int i = 1; i < data_type.Count; i++)
            {
                _sb.Append(_UpFirst(_data_type_2_go_str[data_type.Get(i)]));
            }
            return _sb.ToString();
        }

        public static string ToGoParseStr(this EDataType type)
        {
            return "parse" + _UpFirst(_data_type_2_go_str[type]);
        }


        public static string ToGoParseStr(this DataType type)
        {
            _sb.Clear();
            _sb.Append("parse");

            if (type.IsList)
                _sb.Append("Array");

            if (type.IsTuple)
            {
                _sb.Append(_UpFirst(_data_type_2_go_str[type.type0]));

                for (int i = 1; i < type.Count; i++)
                {
                    _sb.Append(_UpFirst(_data_type_2_go_str[type.Get(i)]));
                }
            }
            else
            {
                _sb.Append(_UpFirst(_data_type_2_go_str[type.type0]));
            }

            return _sb.ToString();
        }

        private static string _UpFirst(string s)
        {
            if (s == null || s.Length == 0)
                return s;
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }
    }
}
