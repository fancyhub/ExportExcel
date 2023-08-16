using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/2 11:05:36
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class PPFieldTypeChecker : I_ProcessNode
    {
        public FTC_TypeGroup _type_group;
        public PPFieldTypeChecker()
        {
            _type_group = new FTC_TypeGroup();
        }

        public string GetName()
        {
            return "字段类型检查";
        }

        public void Process(DataBase data_base)
        {
            data_base.ForeachCol((col) =>
            {
                _type_group.SetCurCol(col.Col);
                col.ForeachCell(_process_cell, _type_group);
            });
        }

        public void _process_cell(TableCell cell, FTC_TypeGroup type_checker)
        {
            string cell_v = cell.Value;
            E_FTC_RESULT result = type_checker.Valid(ref cell_v);
            switch (result)
            {
                case E_FTC_RESULT.valid:
                    cell.Value = cell_v;
                    break;
                case E_FTC_RESULT.valid_modify:
                    cell.Value = cell_v;
                    break;

                case E_FTC_RESULT.invalid:
                    ErrSet.E(cell, $"数据 {cell_v} 不符合格式 {cell.Col.DataType.ToCsvStr()}");
                    break;
                case E_FTC_RESULT.invalid_blank:
                    ErrSet.E(cell, "该字段被标记为不允许空");
                    break;
                default:
                    ErrSet.E(cell, $"数据 {cell_v} 检查结果未知 {cell.Col.DataType.ToCsvStr()}");
                    break;
            }
        }
    }


    public class FTC_TypeGroup
    {
        public Dictionary<EDataType, I_FieldTypeCheck> _checker = new Dictionary<EDataType, I_FieldTypeCheck>();
        public FTC_Pair _pair_checker = new FTC_Pair();
        public FTC_List _list_checker = new FTC_List();

        public I_FieldTypeCheck _cur_checker;
        public bool _blank_forbid;

        public FTC_TypeGroup()
        {
            _checker.Add(EDataType.Bool, new FTC_Bool());
            _checker.Add(EDataType.Int32, new FTC_Int32());
            _checker.Add(EDataType.UInt32, new FTC_UInt32());
            _checker.Add(EDataType.Int64, new FTC_Int64());
            _checker.Add(EDataType.UInt64, new FTC_UInt64());
            _checker.Add(EDataType.Float, new FTC_Float());
            _checker.Add(EDataType.Double, new FTC_Double());
            _checker.Add(EDataType.String, new FTC_String());
            _checker.Add(EDataType.LocStr, new FTC_LocStr());
        }

        public void SetCurCol(TableHeaderItem col)
        {
            //1. 计算当前的检查 类型
            _cur_checker = _checker[col.DataType.type0];
            if (col.DataType.IsPair)
            {
                _pair_checker._types.Clear();
                for (int i = 0; i < col.DataType.Count; i++)
                {
                    _pair_checker._types.Add(_checker[col.DataType.Get(i)]);
                }
                _cur_checker = _pair_checker;
            }
            if (col.DataType.IsList)
            {
                _list_checker._type = _cur_checker;
                _cur_checker = _list_checker;
            }

            //2. 
            _blank_forbid = col.AttrBlankForbid;
        }

        public E_FTC_RESULT Valid(ref string v)
        {
            if (!string.IsNullOrEmpty(v))
                return _cur_checker.Valid(true, ref v);

            if (_blank_forbid)
                return E_FTC_RESULT.invalid_blank;

            v = _cur_checker.DefaultVal();
            return E_FTC_RESULT.valid_modify;
        }
    }

    public enum E_FTC_RESULT
    {
        valid,          //合法
        valid_modify,   //合法，但是需要修改
        invalid,        //非法
        invalid_blank, //非法,空
    }

    public interface I_FieldTypeCheck
    {
        E_FTC_RESULT Valid(bool enable_empty, ref string v);
        string DefaultVal();
    }

    public class FTC_None : I_FieldTypeCheck
    {
        public E_FTC_RESULT Valid(bool enable_empty, ref string v) { return E_FTC_RESULT.valid; }
        public string DefaultVal() { return ""; }
    }

    public class FTC_Bool : I_FieldTypeCheck
    {
        //只要是里面的数值,都是true, 转换成1,其他的全部变成 0
        public static HashSet<string> TRUE_SET = new HashSet<string>() { "1", "yes", "true", "是" };
        public E_FTC_RESULT Valid(bool enable_empty, ref string v)
        {
            bool contain = TRUE_SET.Contains(v.ToLower());
            string new_v = contain ? "1" : "0";
            if (v == new_v)
                return E_FTC_RESULT.valid;

            v = new_v;
            return E_FTC_RESULT.valid_modify;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_Int32 : I_FieldTypeCheck
    {
        public E_FTC_RESULT Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return E_FTC_RESULT.valid_modify;
            }
            if (int.TryParse(v, out _))
                return E_FTC_RESULT.valid;
            return E_FTC_RESULT.invalid;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_UInt32 : I_FieldTypeCheck
    {
        public E_FTC_RESULT Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return E_FTC_RESULT.valid_modify;
            }
            if (int.TryParse(v, out _))
                return E_FTC_RESULT.valid;
            return E_FTC_RESULT.invalid;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_Int64 : I_FieldTypeCheck
    {
        public E_FTC_RESULT Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return E_FTC_RESULT.valid_modify;
            }
            if (uint.TryParse(v, out _))
                return E_FTC_RESULT.valid;
            return E_FTC_RESULT.invalid;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_UInt64 : I_FieldTypeCheck
    {
        public E_FTC_RESULT Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return E_FTC_RESULT.valid_modify;
            }
            if (ulong.TryParse(v, out _))
                return E_FTC_RESULT.valid;
            return E_FTC_RESULT.invalid;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_Float : I_FieldTypeCheck
    {
        public E_FTC_RESULT Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return E_FTC_RESULT.valid_modify;
            }
            if (float.TryParse(v, out _))
                return E_FTC_RESULT.valid;
            return E_FTC_RESULT.invalid;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_Double : I_FieldTypeCheck
    {
        public E_FTC_RESULT Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return E_FTC_RESULT.valid_modify;
            }
            if (double.TryParse(v, out _))
                return E_FTC_RESULT.valid;
            return E_FTC_RESULT.invalid;
        }

        public string DefaultVal() { return "0"; }
    }

    public class FTC_String : I_FieldTypeCheck
    {
        public E_FTC_RESULT Valid(bool enable_empty, ref string v) { return E_FTC_RESULT.valid; }
        public string DefaultVal() { return ""; }
    }

    public class FTC_LocStr : I_FieldTypeCheck
    {
        public E_FTC_RESULT Valid(bool enable_empty, ref string v) { return E_FTC_RESULT.valid; }
        public string DefaultVal() { return ""; }
    }

    //Pair 类型
    public class FTC_Pair : I_FieldTypeCheck
    {
        public List<I_FieldTypeCheck> _types = new List<I_FieldTypeCheck>(DataType.C_MAX_COUNT);
        public List<E_FTC_RESULT> _result = new List<E_FTC_RESULT>();
        public E_FTC_RESULT Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                if (enable_empty)
                    return E_FTC_RESULT.valid;
                else
                    return E_FTC_RESULT.invalid;
            }

            var temp = v.Split(ConstDef.C_PAIR_SPLIT);
            if (temp.Length != _types.Count)
            {
                return E_FTC_RESULT.invalid;
            }


            _result.Clear();
            for (int i = 0; i < _types.Count; i++)
            {
                var sub_result = _types[i].Valid(enable_empty, ref temp[i]);
                if (sub_result == E_FTC_RESULT.invalid)
                    return E_FTC_RESULT.invalid;
                _result.Add(sub_result);
            }

            foreach (var p in _result)
            {
                if (p != E_FTC_RESULT.valid)
                {
                    v = string.Join(ConstDef.C_PAIR_SPLIT, temp);
                    return E_FTC_RESULT.valid_modify;
                }
            }

            return E_FTC_RESULT.valid;
        }

        public string DefaultVal() { return string.Empty; }
    }

    public class FTC_List : I_FieldTypeCheck
    {
        public I_FieldTypeCheck _type;
        public List<E_FTC_RESULT> _result = new List<E_FTC_RESULT>();
        public E_FTC_RESULT Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
                return E_FTC_RESULT.valid;
            var temp = v.Split(ConstDef.C_LIST_SPLIT);
            _result.Clear();

            E_FTC_RESULT ret = E_FTC_RESULT.valid;
            for (int i = 0; i < temp.Length; i++)
            {
                var sub_result = _type.Valid(false, ref temp[i]);
                if (sub_result == E_FTC_RESULT.invalid)
                    return E_FTC_RESULT.invalid;
                _result.Add(sub_result);
            }

            foreach (var p in _result)
            {
                if (p != E_FTC_RESULT.valid)
                {
                    v = string.Join(ConstDef.C_LIST_SPLIT, temp);
                    return E_FTC_RESULT.valid_modify;
                }
            }
            return E_FTC_RESULT.valid;
        }

        public string DefaultVal()
        {
            return string.Empty;
        }
    }
}
