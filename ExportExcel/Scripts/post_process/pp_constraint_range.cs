using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/3 15:55:04
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class PPConstraintRange : I_ProcessNode
    {
        public DataRange _range = new DataRange();
        public void Process(DataBase data_base)
        {
            data_base.ForeachCol((col) =>
            {
                if (!_range.SetCol(col.Col))
                    return;

                //3. 开始遍历字段，检查了
                col.ForeachCell(_process_cell, _range);
            });
        }

        public string GetName()
        {
            return "约束 Range 的检查";
        }


        public void _process_cell(TableCell cell, DataRange range)
        {
            string cell_v = cell.Value;
            if (string.IsNullOrEmpty(cell_v))
                return;

            if (range.IsInRange(cell_v))
                return;

            ErrSet.E(cell, $"{cell_v} 不在范围 Range {range.ToString()}");
        }
    }

    public interface I_DataRange
    {
        bool IsInRange(string v);
        ConAttrRange Range { set; get; }
    }

    public class DataRange
    {
        public Dictionary<EDataType, I_DataRange> _dict = new Dictionary<EDataType, I_DataRange>();
        public I_DataRange _cur;
        public TableHeaderItem _col;
        public DataRange()
        {
            _dict.Add(EDataType.Int32, new DataRangeInt());
            _dict.Add(EDataType.UInt32, new DataRangeUInt());
            _dict.Add(EDataType.Int64, new DataRangeInt64());
            _dict.Add(EDataType.UInt64, new DataRangeUInt64());
            _dict.Add(EDataType.Float32, new DataRangeFloat());
            _dict.Add(EDataType.Float64, new DataRangeDouble());
        }

        public bool SetCol(TableHeaderItem col)
        {
            _col = col;
            _cur = null;
            if (col == null || col.AttrRange == null)
                return false;

            if (!_dict.TryGetValue(col.DataType.type0, out _cur))
            {
                Logger.PrintError("未知的类型 {0}", col.DataType.type0);
                return false;
            }
            return true;
        }
        public override string ToString()
        {
            return $"[{_col.AttrRange._min},{_col.AttrRange._max}]";
        }

        public bool IsInRange(string v)
        {
            return _cur.IsInRange(v);
        }
    }

    public class DataRangeInt : I_DataRange
    {
        public int _min;
        public int _max;
        public ConAttrRange _range;

        public DataRangeInt()
        {

        }

        public ConAttrRange Range
        {
            set
            {
                _range = value;
                if (!int.TryParse(_range._min, out _min))
                    _min = int.MaxValue;

                if (!int.TryParse(_range._max, out _max))
                    _max = int.MinValue;
            }
            get => _range;
        }

        public override string ToString()
        {
            return $"[{_range._min},{_range._max}]";
        }

        public bool IsInRange(string v)
        {
            if (string.IsNullOrEmpty(v))
                return true;
            var list = v.Split(ConstDef.C_LIST_SPLIT);
            foreach (var t in list)
            {
                //如果是list类型, 中间不允许为 空?
                if (string.IsNullOrEmpty(t))
                    return false;

                //如果填写错误,格式不对, 不在这里处理,在类型检查那边处理
                if (!int.TryParse(t, out int t2))
                    continue;

                if (t2 < _min || t2 > _max)
                    return false;
            }
            return true;
        }
    }

    public class DataRangeUInt : I_DataRange
    {
        public uint _min;
        public uint _max;
        public ConAttrRange _range;

        public DataRangeUInt()
        {

        }

        public ConAttrRange Range
        {
            set
            {
                _range = value;
                if (!uint.TryParse(_range._min, out _min))
                    _min = uint.MaxValue;

                if (!uint.TryParse(_range._max, out _max))
                    _max = uint.MinValue;
            }
            get => _range;
        }

        public override string ToString()
        {
            return $"[{_range._min},{_range._max}]";
        }

        public bool IsInRange(string v)
        {
            if (string.IsNullOrEmpty(v))
                return true;
            var list = v.Split(ConstDef.C_LIST_SPLIT);
            foreach (var t in list)
            {
                //如果是list类型, 中间不允许为 空?
                if (string.IsNullOrEmpty(t))
                    return false;

                //如果填写错误,格式不对, 不在这里处理,在类型检查那边处理
                if (!uint.TryParse(t, out uint t2))
                    continue;

                if (t2 < _min || t2 > _max)
                    return false;
            }
            return true;
        }
    }

    public class DataRangeInt64 : I_DataRange
    {
        public long _min;
        public long _max;
        public ConAttrRange _range;

        public DataRangeInt64()
        {
        }

        public ConAttrRange Range
        {
            set
            {
                _range = value;
                if (!long.TryParse(_range._min, out _min))
                    _min = int.MaxValue;

                if (!long.TryParse(_range._max, out _max))
                    _max = int.MinValue;
            }
            get => _range;
        }

        public override string ToString()
        {
            return $"[{_range._min},{_range._max}]";
        }

        public bool IsInRange(string v)
        {
            if (string.IsNullOrEmpty(v))
                return true;
            var list = v.Split(ConstDef.C_LIST_SPLIT);
            foreach (var t in list)
            {
                //如果是list类型, 中间不允许为 空?
                if (string.IsNullOrEmpty(t))
                    return false;

                //如果填写错误,格式不对, 不在这里处理,在类型检查那边处理
                if (!long.TryParse(t, out long t2))
                    continue;

                if (t2 < _min || t2 > _max)
                    return false;
            }
            return true;
        }
    }

    public class DataRangeUInt64 : I_DataRange
    {
        public ulong _min;
        public ulong _max;
        public ConAttrRange _range;

        public DataRangeUInt64()
        {
        }

        public ConAttrRange Range
        {
            set
            {
                _range = value;
                if (!ulong.TryParse(_range._min, out _min))
                    _min = ulong.MaxValue;

                if (!ulong.TryParse(_range._max, out _max))
                    _max = ulong.MinValue;
            }
            get => _range;
        }

        public override string ToString()
        {
            return $"[{_range._min},{_range._max}]";
        }

        public bool IsInRange(string v)
        {
            if (string.IsNullOrEmpty(v))
                return true;
            var list = v.Split(ConstDef.C_LIST_SPLIT);
            foreach (var t in list)
            {
                //如果是list类型, 中间不允许为 空?
                if (string.IsNullOrEmpty(t))
                    return false;

                //如果填写错误,格式不对, 不在这里处理,在类型检查那边处理
                if (!ulong.TryParse(t, out ulong t2))
                    continue;

                if (t2 < _min || t2 > _max)
                    return false;
            }
            return true;
        }
    }

    public class DataRangeFloat : I_DataRange
    {
        public float _min;
        public float _max;
        public ConAttrRange _range;

        public DataRangeFloat()
        {
        }

        public ConAttrRange Range
        {
            set
            {
                _range = value;
                if (!float.TryParse(_range._min, out _min))
                    _min = int.MaxValue;

                if (!float.TryParse(_range._max, out _max))
                    _max = int.MinValue;
            }
            get => _range;
        }

        public override string ToString()
        {
            return $"[{_range._min},{_range._max}]";
        }

        public bool IsInRange(string v)
        {
            if (string.IsNullOrEmpty(v))
                return true;
            var list = v.Split(ConstDef.C_LIST_SPLIT);
            foreach (var t in list)
            {
                //如果是list类型, 中间不允许为 空?
                if (string.IsNullOrEmpty(t))
                    return false;

                //如果填写错误,格式不对, 不在这里处理,在类型检查那边处理
                if (!float.TryParse(t, out float t2))
                    continue;

                if (t2 < _min || t2 > _max)
                    return false;
            }
            return true;
        }
    }

    public class DataRangeDouble : I_DataRange
    {
        public double _min;
        public double _max;
        public ConAttrRange _range;

        public DataRangeDouble()
        {
        }

        public ConAttrRange Range
        {
            set
            {
                _range = value;
                if (!double.TryParse(_range._min, out _min))
                    _min = int.MaxValue;

                if (!double.TryParse(_range._max, out _max))
                    _max = int.MinValue;
            }
            get => _range;
        }

        public override string ToString()
        {
            return $"[{_range._min},{_range._max}]";
        }

        public bool IsInRange(string v)
        {
            if (string.IsNullOrEmpty(v))
                return true;
            var list = v.Split(ConstDef.C_LIST_SPLIT);
            foreach (var t in list)
            {
                //如果是list类型, 中间不允许为 空?
                if (string.IsNullOrEmpty(t))
                    return false;

                //如果填写错误,格式不对, 不在这里处理,在类型检查那边处理
                if (!double.TryParse(t, out double t2))
                    continue;

                if (t2 < _min || t2 > _max)
                    return false;
            }
            return true;
        }
    }
}
