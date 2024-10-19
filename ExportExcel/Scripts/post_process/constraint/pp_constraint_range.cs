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
    public class PPConstraintRange : IProcessNode
    {
        public Dictionary<EDataType, IDataRange> _dict = new Dictionary<EDataType, IDataRange>();
        public IDataRange _cur;
        public TableField _col;

        public PPConstraintRange()
        {
            _dict.Add(EDataType.Int32, new DataRangeInt());
            _dict.Add(EDataType.UInt32, new DataRangeUInt());
            _dict.Add(EDataType.Int64, new DataRangeInt64());
            _dict.Add(EDataType.UInt64, new DataRangeUInt64());
            _dict.Add(EDataType.Float32, new DataRangeFloat());
            _dict.Add(EDataType.Float64, new DataRangeDouble());
        }

        public string GetName()
        {
            return "约束 Range 的检查";
        }


        public void Process(DataBase data_base)
        {
            data_base.ForeachCol((col) =>
            {
                var range = _find_range(col.Field);
                if (range == null)
                    return;

                //3. 开始遍历字段，检查了
                col.ForeachCell(_process_cell, range, col.Field);
            });
        }

        private IDataRange _find_range(TableField col)
        {
            if (col == null || col.AttrRange == null)
                return null;

            if (!_dict.TryGetValue(col.DataType.type0, out _cur))
            {
                Logger.PrintError("未知的类型 {0}", col.DataType.type0);
                return null;
            }

            _cur.Range = col.AttrRange;
            return _cur;
        }

        public void _process_cell(TableCell cell, IDataRange range, TableField col)
        {
            string cell_v = cell.Value;
            if (string.IsNullOrEmpty(cell_v))
                return;

            if (range.IsInRange(cell_v))
                return;

            ErrSet.E(cell, $"{cell_v} 不在范围 Range {col.AttrRange}");
        }

        public interface IDataRange
        {
            bool IsInRange(string v);
            ConAttrRange Range { set; get; }
        }

        public class DataRangeInt : IDataRange
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
                    if (!int.TryParse(_range.Min, out _min))
                        _min = int.MaxValue;

                    if (!int.TryParse(_range.Max, out _max))
                        _max = int.MinValue;
                }
                get => _range;
            }

            public override string ToString()
            {
                return _range.ToString();
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

        public class DataRangeUInt : IDataRange
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
                    if (!uint.TryParse(_range.Min, out _min))
                        _min = uint.MaxValue;

                    if (!uint.TryParse(_range.Max, out _max))
                        _max = uint.MinValue;
                }
                get => _range;
            }

            public override string ToString()
            {
                return _range.ToString();
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

        public class DataRangeInt64 : IDataRange
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
                    if (!long.TryParse(_range.Min, out _min))
                        _min = int.MaxValue;

                    if (!long.TryParse(_range.Max, out _max))
                        _max = int.MinValue;
                }
                get => _range;
            }

            public override string ToString()
            {
                return _range.ToString();
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

        public class DataRangeUInt64 : IDataRange
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
                    if (!ulong.TryParse(_range.Min, out _min))
                        _min = ulong.MaxValue;

                    if (!ulong.TryParse(_range.Max, out _max))
                        _max = ulong.MinValue;
                }
                get => _range;
            }

            public override string ToString()
            {
                return _range.ToString();
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

        public class DataRangeFloat : IDataRange
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
                    if (!float.TryParse(_range.Min, out _min))
                        _min = int.MaxValue;

                    if (!float.TryParse(_range.Max, out _max))
                        _max = int.MinValue;
                }
                get => _range;
            }

            public override string ToString()
            {
                return _range.ToString();
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

        public class DataRangeDouble : IDataRange
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
                    if (!double.TryParse(_range.Min, out _min))
                        _min = int.MaxValue;

                    if (!double.TryParse(_range.Max, out _max))
                        _max = int.MinValue;
                }
                get => _range;
            }

            public override string ToString()
            {
                return $"[{_range.Min},{_range.Max}]";
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

    // 格式: Range[min,max]
    public class ConParserRange : IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db)
        {
            var col = db_col.Field;
            col.AttrRange = _ParseRange(col);
            if (col.AttrRange == null)
                return;

            if (!_is_data_type_valid(col))
                ErrSet.E(db_col, $"Range[],只能支持 int,uint,int64,uint64,float,double 以及对应的list类型, 不支持枚举");
        }

        private static bool _is_data_type_valid(TableField field)
        {
            if (field.AttrEnum != null)
                return false;

            if (field.DataType.IsTuple)
                return false;

            if (field.DataType.type0 != EDataType.Int32
                && field.DataType.type0 != EDataType.UInt32
                && field.DataType.type0 != EDataType.Int64
                && field.DataType.type0 != EDataType.UInt64
                && field.DataType.type0 != EDataType.Float32
                && field.DataType.type0 != EDataType.Float64)
                return false;
            return true;
        }


        private static ConAttrRange _ParseRange(TableField col)
        {
            foreach (var p in col.StrConstraints)
            {
                var temp = p.Trim();
                if (!temp.ToLower().StartsWith("range["))
                    continue;

                int start_index = "range[".Length;
                int end_index = temp.Length - 1;
                var ret = temp.Substring(start_index, end_index - start_index);

                var tt = ret.Split(',');
                return new ConAttrRange(tt[0], tt[1]);
            }
            return null;
        }
    }
}
