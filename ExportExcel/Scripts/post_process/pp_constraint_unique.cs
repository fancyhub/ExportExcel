using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/2 10:57:27
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    /// <summary>
    /// unique & look up的检查 & empty的检查
    /// </summary>
    public class PPConstraint_Unique_LoopUp_BlankForbid : IProcessNode
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct ValUnion
        {
            [FieldOffset(0)] public ulong _u64;
            [FieldOffset(0)] public int _i32_0;
            [FieldOffset(4)] public int _i32_1;

            public static ulong Convert(int hi, int low)
            {
                var v = new ValUnion()
                {
                    _i32_0 = hi,
                    _i32_1 = low,
                };
                return v._u64;
            }
        }

        public string GetName()
        {
            return "约束 Unique & LookUp & BlankForbid 的检查";
        }

        public void Process(DataBase data_base)
        {
            //1. 获取所有 Unique 的列,以及 被作为查询目标的 BlankForbid的列
            Dictionary<string, TableCol> all_col_dict = new Dictionary<string, TableCol>(400);
            data_base.ForeachCol(_collect_all_target_cols, all_col_dict, data_base);

            //2. 检查unique属性，循环 tables
            HashSet<string> lookup_table = new HashSet<string>(10000);
            foreach (var p in all_col_dict)
            {
                TableCol data_col = p.Value;

                //2.1 收集所有的数据
                lookup_table.Clear();
                data_col.ForeachCell(_collect_col_values, lookup_table);

                //2.2 检查 以data_col 为目标的LookUp 列
                data_base.ForeachCol((col) =>
                {
                    ConAttrLookup attr = col.Field.AttrLookUp;
                    if (attr == null || attr._sheet_name != data_col.SheetName || attr._col_name != data_col.ColName)
                        return;

                    col.ForeachCell(_process_lookup, lookup_table);
                });
            }

            //3. 组合pk
            HashSet<ulong> temp = new HashSet<ulong>();
            data_base.ForeachCol(_check_compose_pk, temp);
        }

        public static void _check_compose_pk(TableCol col, HashSet<ulong> dict)
        {
            var attr = col.Field.AttrPK;
            if (attr == null || !attr.IsCompose())
                return;
            dict.Clear();

            int sec_key_idx = attr._sec_key_idx;
            col.ForeachCell((cell) =>
            {
                string str_v1 = cell.Value;
                string str_v2 = cell.GetCellValue(sec_key_idx);
                if (!int.TryParse(str_v1, out int v1) ||
                    !int.TryParse(str_v2, out int v2))
                {
                    ErrSet.E(cell, $"组合key里面的值转换int 失败 {str_v1}, {str_v2} ");
                    return;
                }

                if (v1 < 0 || v2 < 0)
                {
                    ErrSet.E(cell, $"组合key里面的值不允许小于0 {str_v1}, {str_v2} ");
                    return;
                }

                ulong k = ValUnion.Convert(v1, v2);
                if (dict.Add(k))
                    return;
                ErrSet.E(cell, $"组合key {v1}, {v2} 存在重复");
            });
        }

        /// <summary>
        /// 找到所有被引用的 Cols, 该col 必须标记为  PK 或 Unique 
        /// 如果该列是作为 LookUp 目标的,也添加进去
        /// </summary>
        public static void _collect_all_target_cols(TableCol col, Dictionary<string, TableCol> dict, DataBase db)
        {
            //1. 如果col 是 unique的, 添加到dict里面
            if (col.Field.AttrUnique)
                dict[col.SheetName] = col;

            //2. 如果自身有lookup 属性
            ConAttrLookup attr = col.Field.AttrLookUp;
            if (attr == null)
                return;

            //3. 找到目标列
            if (!db.FindCol(attr, out TableCol tar_col))
            {
                ErrSet.E(col, $"对应的约束，{attr}, 找不到对应目标列, " + col.Table.FilePath);
                return;
            }

            //4. 检查目标列的属性,是否 包含BlankForbid
            if (!tar_col.Field.AttrBlankForbid)
            {
                ErrSet.E(col, $"对应的约束，{attr}, 目标列 必须标记为 PK 或 Unique 或 BlankForbid, " + tar_col.Table.FilePath);
                return;
            }
            dict[tar_col.SheetColName] = tar_col;
        }

        //处理所有 LookUP[sheet_name.col_name] 的字段
        public static void _process_lookup(TableCell cell, HashSet<string> lookup_table)
        {
            //1.  如果为空,就不管
            string cell_v = cell.Value;
            if (string.IsNullOrEmpty(cell_v))
                return;

            if (!cell.Col.DataType.IsList)
            {
                if (!lookup_table.Contains(cell_v))
                    ErrSet.E(cell, $"{cell.Col.AttrLookUp} 找不到值 \"{cell_v}\"");
                return;
            }

            foreach (var sub_v in cell_v.Split(ConstDef.C_LIST_SPLIT))
            {
                if (lookup_table.Contains(sub_v))
                    continue;
                ErrSet.E(cell, $"{cell.Col.AttrLookUp} 找不到值 \"{sub_v}\"");
            }
        }

        public static void _collect_col_values(TableCell cell, HashSet<string> out_lookup_table)
        {
            string cell_v = cell.Value;
            if (string.IsNullOrEmpty(cell_v))
            {
                ErrSet.E(cell, $"有空数据 , 该字段不允许空 ");
                return;
            }

            if (out_lookup_table.Add(cell_v))
                return;

            if (!cell.Col.AttrUnique)
                return;

            ErrSet.E(cell, $"有重复数据 {cell_v}, 该字段不允重复 ");
        }
    }
}
