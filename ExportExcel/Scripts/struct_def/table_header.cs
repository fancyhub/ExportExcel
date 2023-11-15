using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/8 11:00:12
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    [Flags]
    public enum EExportFlag
    {
        none = 0,
        client = 1 << 0,
        svr = 1 << 1,
        all = client | svr
    }


    public class TableHeaderItem
    {
        public string Name;
        public string Desc;
        public int ExcelColIdx;
        public string[] StrConstraints;

        public EExportFlag ExportFlag = EExportFlag.all; //默认全部导出
        public DataType DataType;

        public ConAttrPK AttrPK; //主key
        public bool AttrUnique = false; //是否唯一, 如果是主key, Unique也是为true
        public bool AttrBlankForbid = false;  //是否阻止空,如果是Unique,该字段也是为true
        public ConAttrLookup AttrLookUp; //
        public ConAttrFilePath AttrFilePath; //路径检查
        public ConAttrRange AttrRange; //范围检查
        public ConAttrTupleAlias AttrTupleAlias;

        public TableHeaderItem Clone()
        {
            return new TableHeaderItem()
            {
                Name = Name,
                Desc = Desc,
                ExcelColIdx = ExcelColIdx,
                StrConstraints = StrConstraints,
                ExportFlag = ExportFlag,
                DataType = DataType,
                AttrPK = AttrPK,
                AttrUnique = AttrUnique,
                AttrBlankForbid = AttrBlankForbid,
                AttrLookUp = AttrLookUp,
                AttrFilePath = AttrFilePath,
                AttrRange = AttrRange
            };
        }
    }

    public class TableHeader
    {
        public Dictionary<string, TableHeaderItem> Dict;
        public List<TableHeaderItem> List;

        public TableHeader()
        {
            Dict = new Dictionary<string, TableHeaderItem>();
            List = new List<TableHeaderItem>();
        }

        //检查表格, 是否两边的头一致, 返回 缺少的字段
        public bool CheckCols(TableHeader other_header, TableHeaderCompareResult out_rslt)
        {
            //1. 准备
            out_rslt.Clear();
            Dictionary<string, TableHeaderItem> dict_rule = Dict;
            Dictionary<string, TableHeaderItem> dict_data = other_header.Dict;

            //2. 检查列的缺失
            foreach (var p in dict_rule)
            {
                //如果该列不允许为空, 并且在数据表里面没有找到
                if (p.Value.AttrBlankForbid && !dict_data.ContainsKey(p.Key))
                {
                    out_rslt.AddMissing(p.Key);
                }
            }
            foreach (var p in dict_data)
            {
                if (!dict_rule.ContainsKey(p.Key))
                    out_rslt.AddNew(p.Key);
            }

            //3. 检查数据类型
            foreach (var p in dict_rule)
            {
                if (!dict_data.ContainsKey(p.Key))
                    continue;
                DataType data_col_type = dict_data[p.Key].DataType;
                DataType rule_col_type = p.Value.DataType;

                if (!data_col_type.IsEuqal(rule_col_type))
                {
                    out_rslt.AddMismatch(p.Key, rule_col_type, data_col_type);
                }
            }

            //4. 如果 都为空,说明一样
            return out_rslt.Count == 0;
        }

        public bool Add(TableHeaderItem col)
        {
            //1. 检查            
            if (Dict.ContainsKey(col.Name))
            {
                return false;
            }

            //2. 添加            
            List.Add(col);
            Dict.Add(col.Name, col);
            return true;
        }

        public int PkIdx
        {
            get
            {
                return List.FindIndex((o) => o.AttrPK != null);
            }
        }

        public TableHeaderItem Pk
        {
            get
            {
                return List.Find((o) => o.AttrPK != null);
            }
        }

        public int IndexOfCol(string col_name)
        {
            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].Name == col_name)
                    return i;
            }
            return -1;
        }

        public TableHeaderItem this[int index]
        {
            get
            {
                return List[index];
            }
        }

        public TableHeaderItem this[string name]
        {
            get
            {
                Dict.TryGetValue(name, out var ret);
                return ret;
            }
        }

        public int Count { get { return List.Count; } }
    }

    public class TableHeaderCompareResult
    {
        public List<string> _miss_list;
        public List<string> _new_list;
        public List<(string col_name, DataType rule_type, DataType data_type)> _mismatch_list;

        public TableHeaderCompareResult()
        {
            _miss_list = new List<string>();
            _new_list = new List<string>();
            _mismatch_list = new List<(string, DataType, DataType)>();
        }

        public void AddMissing(string col_name)
        {
            _miss_list.Add(col_name);
        }

        public void AddNew(string col_name)
        {
            _new_list.Add(col_name);
        }

        public void AddMismatch(string col_name, DataType rule_type, DataType data_type)
        {
            _mismatch_list.Add((col_name, rule_type, data_type));
        }

        public int Count
        {
            get { return _miss_list.Count + _new_list.Count + _mismatch_list.Count; }
        }

        public void Clear()
        {
            _miss_list.Clear();
            _new_list.Clear();
            _mismatch_list.Clear();
        }

        public void PrintErr(string sheet_name, string data_file_path)
        {
            foreach (var new_col in _new_list)
            {
                ErrSet.E($"+{sheet_name}.{new_col} 比规则表多了字段", data_file_path);
            }

            foreach (var missing_col in _miss_list)
            {
                ErrSet.E($"-{sheet_name}.{missing_col} 比规则表少了字段 ", data_file_path);
            }

            foreach (var item in _mismatch_list)
            {
                ErrSet.E($"{sheet_name}.{item.col_name} 数据类型不一致,规则表 {item.rule_type.ToCsvStr()}, 数据表 {item.data_type.ToCsvStr()}", data_file_path);
            }
        }
    }
}
