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
                    if (attr == null || attr.SheetName != data_col.SheetName || attr.ColName != data_col.ColName)
                        return;

                    col.ForeachCell(_process_lookup, lookup_table);
                });
            }

            //3. 组合pk
            HashSet<string> temp = new HashSet<string>();
            data_base.ForeachCol(_check_compose_pk, temp);
        }

        private static void _check_compose_pk(TableCol col, HashSet<string> dict)
        {
            var attr = col.Field.AttrPK;
            if (attr == null || !attr.IsCompose())
                return;
            dict.Clear();

            List<string> list = new List<string>();
            col.ForeachCell((cell) =>
            {
                list.Clear();
                list.Add(cell.Value);
                foreach (var p in attr.SubKeys)
                {
                    list.Add(cell.GetCellValue(p.FieldIndex));
                }

                string compose_key = string.Join(ConstDef.SeparatorTuple, list);

                if (dict.Add(compose_key))
                    return;
                ErrSet.E(cell, $"组合key {string.Join(" , ", list)} 存在重复");
            });
        }

        /// <summary>
        /// 找到所有被引用的 Cols, 该col 必须标记为  PK 或 Unique 
        /// 如果该列是作为 LookUp 目标的,也添加进去
        /// </summary>
        private static void _collect_all_target_cols(TableCol col, Dictionary<string, TableCol> dict, DataBase db)
        {
            //1. 如果col 是 unique的, 添加到dict里面
            if (col.Field.AttrUnique != null)
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
            if (tar_col.Field.AttrBlankForbid == null)
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

            foreach (var sub_v in cell_v.Split(ConstDef.SeparatorList))
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

            if (cell.Col.AttrUnique == null)
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
            if (col.AttrUnique != null)
                return;

            col.AttrUnique = _ParseUnique(col) ? ConAttrUnique.Inst : null;
            if (col.AttrUnique != null)
                col.AttrBlankForbid = ConAttrBlankForbid.Inst;

            if (col.AttrUnique != null && !IsDataTypeValid(db_col.Field.DataType))
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
            ConAttrPK attr_pk = _parse_pk(db_col);
            if (attr_pk == null)
                return;

            //2. 检查是否已经有PK了
            if (db_col.Table.Header.Pk != null)
                ErrSet.E(db_col, $"出现多个 PK, 删除一个");
            col.AttrPK = attr_pk;

            //3. pk 默认不允许空
            col.AttrBlankForbid = ConAttrBlankForbid.Inst;

            //4. 如果是非组合key
            if (!attr_pk.IsCompose())
            {
                //需要设置为unique
                col.AttrUnique = ConAttrUnique.Inst;

                // 检查数据类型
                if (!ConParserUnique.IsDataTypeValid(col.DataType))
                    ErrSet.E(db_col, $"该字段是 pk 或者 Unique 约束的情况下, 只能支持 int,uint,int64,uint64,string 这几种类型");
                return;
            }

            //5. 检查第二个key
            var pk_visible = attr_pk.Field.ExportFlag;
            foreach (var p in attr_pk.SubKeys)
            {
                if (p == col)
                {
                    ErrSet.E(db_col, $"{attr_pk} 第二个key 不能是自己");
                }

                if (p.ExportFlag != pk_visible)
                {
                    ErrSet.E(db_col, $"{attr_pk} PK 的Export Flag 不一致");
                }
                p.AttrBlankForbid = ConAttrBlankForbid.Inst; //不允许为空
            }

            if (!_check_pk_data_type(col))
                ErrSet.E(db_col, $"组合PK, 只能支持 int,uint,int64,uint64,string, 不支持alias, tuple, list  ");
        }

        private static bool _check_pk_data_type(TableField header_item)
        {
            if (header_item.DataType.IsTuple || header_item.DataType.IsList)
                return false;

            var t = header_item.DataType.type0;
            if (t == EDataType.Int32 || t == EDataType.UInt32 || t == EDataType.Int64 || t == EDataType.UInt64 || t == EDataType.String)
                return true;
            return false;
        }

        private static ConAttrPK _parse_pk(TableCol col)
        {
            string key = null;
            foreach (var p in col.Field.StrConstraints)
            {
                string p2 = p.Trim();
                string f = p2.ToLower();
                if (f == "pk")
                    return new ConAttrPK(col.Field);

                if (!f.StartsWith("pk["))
                    continue;
                key = p2;
                break;
            }

            if (key == null)
                return null;

            int start_index = "pk[".Length;
            int end_index = key.Length - 1;
            var sub_keys = key.Substring(start_index, end_index - start_index).Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<TableField> sub_fields = new List<TableField>();
            foreach (var p in sub_keys)
            {
                var field = col.Table.Header[p.Trim()];
                if (field == null)
                {
                    ErrSet.E(col, $"组合PK, 找不到 {p} ");
                }
                else if (sub_fields.IndexOf(field) >= 0)
                {
                    ErrSet.E(col, $"组合PK, 重复 {p} ");
                }
                else
                {
                    sub_fields.Add(field);
                }
            }
            return new ConAttrPK(col.Field, sub_fields.ToArray());
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
                ErrSet.E(db_col, $"LookUp 约束, 只能支持 int,uint,int64,uint64,string 以及对应的list类型, 不支持枚举");
        }

        private bool _is_data_type_valid(TableField field)
        {
            if (field.AttrEnum != null)
                return false;

            if (field.DataType.IsTuple)
                return false;

            if (field.DataType.type0 != EDataType.Int32
                && field.DataType.type0 != EDataType.UInt32
                && field.DataType.type0 != EDataType.Int64
                && field.DataType.type0 != EDataType.UInt64
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
                return new ConAttrLookup(tt[0], tt[1]);
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
            if (col.AttrBlankForbid != null)
                return;

            foreach (var p in col.StrConstraints)
            {
                var temp = p.ToLower().Trim();
                if (temp == "blankforbid")
                {
                    col.AttrBlankForbid = ConAttrBlankForbid.Inst;
                    return;
                }
            }
            col.AttrBlankForbid = null;
        }
    }
}
