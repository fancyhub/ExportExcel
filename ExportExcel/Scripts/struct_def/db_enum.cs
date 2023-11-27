using System;
using System.Collections;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/1 17:12:48
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class EnumField
    {
        public string Name;
        public string ExcelVal;
        public int Val;
    }

    public class EnumType
    {
        public string Name;

        //Key: excel val
        public Dictionary<string, EnumField> Dict = new Dictionary<string, EnumField>();
        public HashSet<int> _all_vals = new HashSet<int>();

        public IEnumerable<EnumField> GetAllFields()
        {
            return Dict.Values;
        }

        /// <summary>
        /// 枚举的字段名是否存在
        /// </summary>
        public bool IsFieldNameExist(string field_name)
        {
            foreach (var p in Dict)
            {
                if (p.Value.Name == field_name)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 枚举的字段中文名是否存在
        /// </summary>        
        public bool IsFieldStrValueExist(string filed_str_val)
        {
            return Dict.ContainsKey(filed_str_val);
        }

        public void Add(EnumField field)
        {
            Dict.Add(field.ExcelVal, field);
            _all_vals.Add(field.Val);
        }

        public bool Convert(string v, out int result)
        {            
            if(Dict.TryGetValue(v, out EnumField field) && field!=null)
            {
                result = field.Val;
                return true;
            }

            if (string.IsNullOrEmpty(v) && _all_vals.Contains(0))
            {
                result = 0;
                return true;
            }

            if (int.TryParse(v, out result) && _all_vals.Contains(result))
            {
                return true;
            }

            result = 0;
            return false;
        }
    }

    public enum EEnumAddError
    {
        Succ,
        DuplicateFieldName,
        DuplicateExcelVal,
    }

    public sealed class DBEnum : IEnumerable<KeyValuePair<string, EnumType>>
    {
        public Dictionary<string, EnumType> _dict = new Dictionary<string, EnumType>();

        public EnumType Find(string enum_name)
        {
            _dict.TryGetValue(enum_name, out EnumType ret);
            return ret;
        }

        public EEnumAddError AddEnumField(string enum_name, string enum_field_name, string excel_val, int enum_val)
        {
            _dict.TryGetValue(enum_name, out EnumType table);
            if (table == null)
            {
                table = new EnumType();
                table.Name = enum_name;
                _dict.Add(table.Name, table);
            }

            if (table.IsFieldNameExist(enum_field_name))
            {

                return EEnumAddError.DuplicateFieldName;
            }

            if (table.IsFieldStrValueExist(excel_val))
            {
                return EEnumAddError.DuplicateExcelVal;
            }

            table.Add(new EnumField()
            {
                ExcelVal = excel_val,
                Name = enum_field_name,
                Val = enum_val
            });
            return EEnumAddError.Succ;
        }

        public IEnumerator<KeyValuePair<string, EnumType>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
    }
}
