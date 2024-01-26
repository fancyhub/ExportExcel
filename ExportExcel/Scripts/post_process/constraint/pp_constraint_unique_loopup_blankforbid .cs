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

        private static void _check_compose_pk(TableCol col, HashSet<ulong> dict)
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
        private static void _collect_all_target_cols(TableCol col, Dictionary<string, TableCol> dict, DataBase db)
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
        private static void _process_lookup(TableCell cell, HashSet<string> lookup_table)
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

        private static void _collect_col_values(TableCell cell, HashSet<string> out_lookup_table)
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

    // 格式: Unique
    public class ConParserUnique : IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db)
        {
            var col = db_col.Field;
            if (col.AttrUnique)
                return;

            col.AttrUnique = _ParseUnique(col);
            if (col.AttrUnique)
                col.AttrBlankForbid = true;

            if (col.AttrUnique && !IsDataTypeValid(db_col.Field.DataType))
                ErrSet.E(db_col, $"该字段是 pk 或者 Unique 约束的情况下, 只能支持 int,uint,int64,uint64,string 这几种类型");
        }

        public static bool IsDataTypeValid(DataType data_type)
        {
            if (data_type.IsList)
                return false;

            if (data_type.IsTuple)
                return false;

            if (data_type.type0 != EDataType.Int32
                && data_type.type0 != EDataType.UInt32
                && data_type.type0 != EDataType.Int64
                && data_type.type0 != EDataType.UInt64
                && data_type.type0 != EDataType.String)
                return false;

            return true;
        }
        
        private static bool _ParseUnique(TableField col)
        {
            foreach (var p in col.StrConstraints)
            {
                var temp = p.ToLower().Trim();
                if (temp == "unique")
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 格式 PK  或者 PK[second_key_col_name]
    /// </summary>
    public class ConParserPK : IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db)
        {
            //1. 解析 PK
            TableField col = db_col.Field;
            ConAttrPK attr_pk = _parse_pk(col);
            if (attr_pk == null)
                return;

            //2. 检查是否已经有PK了
            if (db_col.Table.Header.Pk != null)
                ErrSet.E(db_col, $"出现多个 PK, 删除一个");
            col.AttrPK = attr_pk;

            //3. pk 默认不允许空
            col.AttrBlankForbid = true;

            //4. 如果是非组合key
            if (!attr_pk.IsCompose())
            {
                //需要设置为unique
                col.AttrUnique = true;

                // 检查数据类型
                if (!ConParserUnique.IsDataTypeValid(col.DataType))
                    ErrSet.E(db_col, $"该字段是 pk 或者 Unique 约束的情况下, 只能支持 int,uint,int64,uint64,string 这几种类型");
                return;
            }

            //5. 检查第二个key
            string sec_key_name = attr_pk._sec_key_col_name;
            if (sec_key_name == col.Name)
            {
                ErrSet.E(db_col, $"{attr_pk} 第二个key 不能是自己");
                return;
            }
            TableField sec_col = db_col.Table.Header[sec_key_name];
            if (sec_col == null)
            {
                ErrSet.E(db_col, $"{attr_pk} 第二个key 找不到");
                return;
            }
            attr_pk._sec_key = sec_col;
            attr_pk._sec_key_idx = db_col.Table.Header.IndexOfCol(sec_key_name);
            sec_col.AttrBlankForbid = true; //不允许为空

            if (!_is_data_type_combine(col) || !_is_data_type_combine(sec_col))
                ErrSet.E(db_col, $"组合PK, 只能支持 int/uint");
        }

        private static bool _is_data_type_combine(TableField header_item)
        {
            if (header_item.DataType.IsTuple || header_item.DataType.IsList)
                return false;
            if (header_item.AttrEnum != null)
                return false;

            if (header_item.DataType.type0 == EDataType.Int32 || header_item.DataType.type0 == EDataType.UInt32)
                return true;
            return false;
        }

        private static ConAttrPK _parse_pk(TableField col)
        {
            foreach (var p in col.StrConstraints)
            {
                string f = p.ToLower().Trim();
                if (f == "pk")
                    return new ConAttrPK();

                if (!f.StartsWith("pk["))
                    continue;
                int start_index = "pk[".Length;
                int end_index = p.Length - 1;
                var ret = p.Substring(start_index, end_index - start_index);
                return new ConAttrPK()
                {
                    _sec_key_col_name = ret
                };
            }
            return null;
        }
    }

    // 格式: LookUp[SheetName.ColName]
    public class ConParserLookUp : IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db)
        {
            var col = db_col.Field;
            try
            {
                col.AttrLookUp = _ParseLookUp(col);
            }
            catch (Exception e)
            {
                ErrSet.E(db_col, e.Message + " " + db_col.Table.FilePath);
            }

            if (col.AttrLookUp == null)
                return;

            if (!_is_data_type_valid(col))
                ErrSet.E(db_col, $"LookUp 约束, 只能支持 int,int64,string 以及对应的list类型, 不支持枚举");
        }

        private bool _is_data_type_valid(TableField field)
        {
            if (field.AttrEnum != null)
                return false;

            if (field.DataType.IsTuple)
                return false;

            if (field.DataType.type0 != EDataType.Int32
                && field.DataType.type0 != EDataType.Int64
                && field.DataType.type0 != EDataType.String)
                return false;
            return true;
        }


        private static ConAttrLookup _ParseLookUp(TableField col)
        {
            foreach (var p in col.StrConstraints)
            {
                var temp = p.Trim();
                if (!temp.ToLower().StartsWith("lookup["))
                    continue;

                int start_index = "lookup[".Length;
                int end_index = temp.Length - 1;
                var ret = temp.Substring(start_index, end_index - start_index);

                var tt = ret.Split('.', StringSplitOptions.RemoveEmptyEntries);
                if (tt.Length != 2)
                {
                    throw new Exception($"约束 {p} 不符合规则");
                }
                return new ConAttrLookup()
                {
                    _sheet_name = tt[0].Trim(),
                    _col_name = tt[1].Trim(),
                };
            }
            return null;
        }
    }

    // 格式 : BlankForbid
    public class ConParserBlankForbid : IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db)
        {
            var col = db_col.Field;
            if (col.AttrBlankForbid)
                return;

            foreach (var p in col.StrConstraints)
            {
                var temp = p.ToLower().Trim();
                if (temp == "blankforbid")
                {
                    col.AttrBlankForbid = true;
                    return;
                }
            }
            col.AttrBlankForbid = false;
        }
    }
}
