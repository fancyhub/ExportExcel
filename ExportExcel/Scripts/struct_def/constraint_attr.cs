using System;
using System.Collections.Generic;

namespace ExportExcel
{
    //约束的属性
    public class ConstraintAttr
    {
    }

    public class ConAttrLookup : ConstraintAttr
    {
        public string _sheet_name;
        public string _col_name;

        public override string ToString()
        {
            return $"LookUp[{_sheet_name}.{_col_name}]";
        }
    }

    public class ConAttrFilePath : ConstraintAttr
    {
        public string _dir_prefix;
        public string _file_suffix;
    }

    public class ConAttrRange : ConstraintAttr
    {
        public string _min;
        public string _max;
    }

    public class ConAttrPK : ConstraintAttr
    {
        public string _sec_key_col_name;
        public TableHeaderItem _sec_key;
        public int _sec_key_idx;
        /// <summary>
        /// 是否为组合key
        /// </summary>
        public bool IsCompose()
        {
            return !string.IsNullOrEmpty(_sec_key_col_name);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_sec_key_col_name))
                return "PK";
            else
                return "PK[" + _sec_key_col_name + "]";
        }
    }

    public class ConAttrTupleAlias : ConstraintAttr
    {
        public string AliasName;

        public ConAttrTupleAlias(string name)
        {
            AliasName = name;
        }
    }
}
