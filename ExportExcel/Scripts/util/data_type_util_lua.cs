using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/3 10:54:57
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public static class DataTypeUtilLua
    {
        public static Dictionary<EDataType, string> _data_type_2_lua_str = new Dictionary<EDataType, string>()
        {
            [EDataType.Bool] = "bool",
            [EDataType.Int32] = "int32",
            [EDataType.Int64] = "int64",
            [EDataType.UInt32] = "uint32",
            [EDataType.UInt64] = "uint64",
            [EDataType.Float32] = "float32",
            [EDataType.Float64] = "float64",
            [EDataType.String] = "string",
            [EDataType.LocStr] = "LocStr",
            [EDataType.LocId] = "LocId",
        };

        public static Dictionary<EDataType, string> _data_type_2_lua_parse_str = new Dictionary<EDataType, string>()
        {
            [EDataType.Bool] = "bool",
            [EDataType.Int32] = "int32",
            [EDataType.Int64] = "int64",
            [EDataType.UInt32] = "uint32",
            [EDataType.UInt64] = "uint64",
            [EDataType.Float32] = "float32",
            [EDataType.Float64] = "float64",
            [EDataType.String] = "string",
            [EDataType.LocStr] = "locstr",
            [EDataType.LocId] = "locid",
        };
         

        public static string ToLuaStr(this DataType data)
        {
            string ret = _data_type_2_lua_str[data.type0];
            if (data.IsList)
            {
                if (data.IsTuple)
                    return string.Format("table<Tuple_{0}_{1}>", _data_type_2_lua_str[data.type0], _data_type_2_lua_str[data.type1]);
                else if (data.enum_type == null)
                    return string.Format("table<{0}>", ret);
                else
                    return string.Format("table<{0}>", data.enum_type.Name);
            }
            else
            {
                if (data.IsTuple)
                    return string.Format("Tuple_{0}_{1}", _data_type_2_lua_str[data.type0], _data_type_2_lua_str[data.type1]);
                else if (data.enum_type == null)
                    return ret;
                else
                    return data.enum_type.Name;
            }
        }

        public static string ToLuaParseStr(this DataType data)
        {
            string ret = _data_type_2_lua_parse_str[data.type0];

            if (data.IsList)
            {
                if (data.IsTuple)
                    return string.Format("TableUtil.Parse_list_{0}_{1}", _data_type_2_lua_parse_str[data.type0], _data_type_2_lua_parse_str[data.type1]);
                else
                    return string.Format("TableUtil.Parse_list_{0}", _data_type_2_lua_parse_str[data.type0]);
            }
            else
            {
                if (data.IsTuple)
                    return string.Format("TableUtil.Parse_{0}_{1}", _data_type_2_lua_parse_str[data.type0], _data_type_2_lua_parse_str[data.type1]);
                else
                    return string.Format("TableUtil.Parse_{0}", _data_type_2_lua_parse_str[data.type0]);
            }
        }         
    }
}
