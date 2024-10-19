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
    public class PPFieldTypeChecker : IProcessNode
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
                _type_group.SetCurCol(col.Field);
                col.ForeachCell(_process_cell, _type_group);
            });
        }

        public void _process_cell(TableCell cell, FTC_TypeGroup type_checker)
        {
            string cell_v = cell.Value;
            EFtcResult result = type_checker.Valid(ref cell_v);
            switch (result)
            {
                case EFtcResult.valid:
                    cell.Value = cell_v;
                    break;
                case EFtcResult.valid_modify:
                    cell.Value = cell_v;
                    break;

                case EFtcResult.invalid:
                    ErrSet.E(cell, $"数据 {cell_v} 不符合格式 {cell.Col.DataType.ToCsvStr()}");
                    break;
                case EFtcResult.invalid_blank:
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
        public Dictionary<EDataType, IFieldTypeCheck> _checker = new Dictionary<EDataType, IFieldTypeCheck>();
        public FTC_Tuple _tuple_checker = new FTC_Tuple();
        public FTC_List _list_checker = new FTC_List();

        public IFieldTypeCheck _cur_checker;
        public bool _blank_forbid;

        public FTC_TypeGroup()
        {
            _checker.Add(EDataType.Bool, new FTC_Bool());
            _checker.Add(EDataType.Int32, new FTC_Int32());
            _checker.Add(EDataType.UInt32, new FTC_UInt32());
            _checker.Add(EDataType.Int64, new FTC_Int64());
            _checker.Add(EDataType.UInt64, new FTC_UInt64());
            _checker.Add(EDataType.Float32, new FTC_Float());
            _checker.Add(EDataType.Float64, new FTC_Double());
            _checker.Add(EDataType.String, new FTC_String());
            _checker.Add(EDataType.LocStr, new FTC_LocStr());
        }

        public void SetCurCol(TableField col)
        {
            //1. 计算当前的检查 类型
            _cur_checker = _checker[col.DataType.type0];
            if (col.DataType.IsTuple)
            {
                _tuple_checker._types.Clear();
                for (int i = 0; i < col.DataType.Count; i++)
                {
                    _tuple_checker._types.Add(_checker[col.DataType.Get(i)]);
                }
                _cur_checker = _tuple_checker;
            }
            if (col.DataType.IsList)
            {
                _list_checker._type = _cur_checker;
                _cur_checker = _list_checker;
            }

            //2. 
            _blank_forbid = col.AttrBlankForbid != null;
        }

        public EFtcResult Valid(ref string v)
        {
            if (!string.IsNullOrEmpty(v))
                return _cur_checker.Valid(true, ref v);

            if (_blank_forbid)
                return EFtcResult.invalid_blank;

            v = _cur_checker.DefaultVal();
            return EFtcResult.valid_modify;
        }
    }

    public enum EFtcResult
    {
        valid,          //合法
        valid_modify,   //合法，但是需要修改
        invalid,        //非法
        invalid_blank, //非法,空
    }

    public interface IFieldTypeCheck
    {
        EFtcResult Valid(bool enable_empty, ref string v);
        string DefaultVal();
    }

    public class FTC_None : IFieldTypeCheck
    {
        public EFtcResult Valid(bool enable_empty, ref string v) { return EFtcResult.valid; }
        public string DefaultVal() { return ""; }
    }

    public class FTC_Bool : IFieldTypeCheck
    {
        //只要是里面的数值,都是true, 转换成1,其他的全部变成 0
        public static HashSet<string> TRUE_SET = new HashSet<string>() { "1", "yes", "true", "是" };
        public EFtcResult Valid(bool enable_empty, ref string v)
        {
            bool contain = TRUE_SET.Contains(v.ToLower());
            string new_v = contain ? "1" : "0";
            if (v == new_v)
                return EFtcResult.valid;

            v = new_v;
            return EFtcResult.valid_modify;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_Int32 : IFieldTypeCheck
    {
        public EFtcResult Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return EFtcResult.valid_modify;
            }
            if (int.TryParse(v, out _))
                return EFtcResult.valid;
            return EFtcResult.invalid;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_UInt32 : IFieldTypeCheck
    {
        public EFtcResult Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return EFtcResult.valid_modify;
            }
            if (uint.TryParse(v, out _))
                return EFtcResult.valid;
            return EFtcResult.invalid;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_Int64 : IFieldTypeCheck
    {
        public EFtcResult Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return EFtcResult.valid_modify;
            }
            if (uint.TryParse(v, out _))
                return EFtcResult.valid;
            return EFtcResult.invalid;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_UInt64 : IFieldTypeCheck
    {
        public EFtcResult Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return EFtcResult.valid_modify;
            }
            if (ulong.TryParse(v, out _))
                return EFtcResult.valid;
            return EFtcResult.invalid;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_Float : IFieldTypeCheck
    {
        public EFtcResult Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return EFtcResult.valid_modify;
            }
            if (float.TryParse(v, out _))
                return EFtcResult.valid;
            return EFtcResult.invalid;
        }
        public string DefaultVal() { return "0"; }
    }

    public class FTC_Double : IFieldTypeCheck
    {
        public EFtcResult Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                v = "0";
                //但是需要修改
                return EFtcResult.valid_modify;
            }
            if (double.TryParse(v, out _))
                return EFtcResult.valid;
            return EFtcResult.invalid;
        }

        public string DefaultVal() { return "0"; }
    }

    public class FTC_String : IFieldTypeCheck
    {
        public EFtcResult Valid(bool enable_empty, ref string v) { return EFtcResult.valid; }
        public string DefaultVal() { return ""; }
    }

    public class FTC_LocStr : IFieldTypeCheck
    {
        public EFtcResult Valid(bool enable_empty, ref string v) { return EFtcResult.valid; }
        public string DefaultVal() { return ""; }
    }

    //Pair 类型
    public class FTC_Tuple : IFieldTypeCheck
    {
        public List<IFieldTypeCheck> _types = new List<IFieldTypeCheck>(DataType.C_MAX_COUNT);
        public List<EFtcResult> _result = new List<EFtcResult>();
        public EFtcResult Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                if (enable_empty)
                    return EFtcResult.valid;
                else
                    return EFtcResult.invalid;
            }

            var temp = v.Split(ConstDef.C_TUPLE_SPLIT);
            if (temp.Length != _types.Count)
            {
                return EFtcResult.invalid;
            }


            _result.Clear();
            for (int i = 0; i < _types.Count; i++)
            {
                var sub_result = _types[i].Valid(enable_empty, ref temp[i]);
                if (sub_result == EFtcResult.invalid)
                    return EFtcResult.invalid;
                _result.Add(sub_result);
            }

            foreach (var p in _result)
            {
                if (p != EFtcResult.valid)
                {
                    v = string.Join(ConstDef.C_TUPLE_SPLIT, temp);
                    return EFtcResult.valid_modify;
                }
            }

            return EFtcResult.valid;
        }

        public string DefaultVal() { return string.Empty; }
    }

    public class FTC_List : IFieldTypeCheck
    {
        public IFieldTypeCheck _type;
        public List<EFtcResult> _result = new List<EFtcResult>();
        public EFtcResult Valid(bool enable_empty, ref string v)
        {
            if (string.IsNullOrEmpty(v))
                return EFtcResult.valid;
            var temp = v.Split(ConstDef.C_LIST_SPLIT);
            _result.Clear();

            EFtcResult ret = EFtcResult.valid;
            for (int i = 0; i < temp.Length; i++)
            {
                var sub_result = _type.Valid(false, ref temp[i]);
                if (sub_result == EFtcResult.invalid)
                    return EFtcResult.invalid;
                _result.Add(sub_result);
            }

            foreach (var p in _result)
            {
                if (p != EFtcResult.valid)
                {
                    v = string.Join(ConstDef.C_LIST_SPLIT, temp);
                    return EFtcResult.valid_modify;
                }
            }
            return EFtcResult.valid;
        }

        public string DefaultVal()
        {
            return string.Empty;
        }
    }
}
