using System;
using System.Collections.Generic;

namespace ExportExcel
{
    //后处理, 约束解析
    public class PPconstraintParser : IProcessNode
    {
        public List<ConstraintParser> _constratint_parser_list = new List<ConstraintParser>
            {
                new ConParserEnum(),
                new ConParserPK(),
                new ConParserUnique(),
                new ConParserBlankForbid(),
                new ConParserRange(),
                new ConParserFilePath(),
                new ConParserLookUp(),
                new ConParserExportFlag(),
                new ConParserTupleAlias(),
            };

        public string GetName()
        {
            return "解析 Table 的约束";
        }

        public void Process(DataBase data_base)
        {
            foreach (var p in _constratint_parser_list)
            {
                data_base.ForeachCol(p.Process, data_base);
            }
        }

        public abstract class ConstraintParser
        {
            public abstract void Process(TableCol db_col, DataBase db);
        }

        public class ConParserPK : ConstraintParser
        {
            public override void Process(TableCol db_col, DataBase db)
            {
                //1. 解析 PK
                TableHeaderItem col = db_col.Col;
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
                TableHeaderItem sec_col = db_col.Table.Header[sec_key_name];
                if (sec_col == null)
                {
                    ErrSet.E(db_col, $"{attr_pk} 第二个key 找不到");
                    return;
                }
                attr_pk._sec_key = sec_col;
                attr_pk._sec_key_idx = db_col.Table.Header.IndexOfCol(sec_key_name);
                sec_col.AttrBlankForbid = true; //不允许为空

                if (!_is_data_type_valid(col.DataType, sec_col.DataType))
                    ErrSet.E(db_col, $"组合PK, 只能支持 int/uint");
            }

            public static bool _is_data_type_valid(DataType data_type1, DataType data_type2)
            {
                if (data_type1.IsList || data_type2.IsList)
                    return false;
                if (data_type1.IsTuple || data_type2.IsList)
                    return false;
                if (data_type1.enum_type != null || data_type2.enum_type != null)
                    return false;
                if (data_type1.type0 != EDataType.Int32 && data_type1.type0 != EDataType.UInt32)
                    return false;
                if (data_type2.type0 != EDataType.Int32 && data_type2.type0 != EDataType.UInt32)
                    return false;
                return true;
            }

            public static ConAttrPK _parse_pk(TableHeaderItem col)
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

        public class ConParserEnum : ConstraintParser
        {
            public override void Process(TableCol db_col, DataBase db)
            {
                TableHeaderItem col = db_col.Col;
                string ref_enum_name = _parse_enum(col);
                if (string.IsNullOrEmpty(ref_enum_name))
                    return;

                col.DataType.enum_type = db.EnumDB.Find(ref_enum_name);
                if (col.DataType.enum_type == null)
                {
                    ErrSet.E(db_col, $"找不到对应的枚举类型 {ref_enum_name}");
                    return;
                }

                if (col.DataType.enum_type != null && !_is_support_enum(col.DataType))
                {
                    ErrSet.E(db_col, $"只有 int,list_int 支持枚举类型 {ref_enum_name}");
                    return;
                }
            }

            // 格式 : Enum[E_Enum_Name] 
            public static string _parse_enum(TableHeaderItem col)
            {
                foreach (var str in col.StrConstraints)
                {
                    var temp = str.Trim();
                    if (!temp.ToLower().StartsWith("enum["))
                    {
                        continue;
                    }
                    int start_index = "enum[".Length;
                    int end_index = temp.Length - 1;
                    var ret = temp.Substring(start_index, end_index - start_index);
                    return ret;
                }
                return null;
            }

            public bool _is_support_enum(DataType data_type)
            {
                //第一个类型如果不是int32, 不支持
                if (data_type.type0 != EDataType.Int32)
                    return false;

                //如果是pair 也不支持
                if (data_type.IsTuple)
                    return false;

                //List 不支持
                if (data_type.IsList)
                    return false;

                return true;
            }

        }

        public class ConParserUnique : ConstraintParser
        {
            public override void Process(TableCol db_col, DataBase db)
            {
                var col = db_col.Col;
                if (col.AttrUnique)
                    return;

                col.AttrUnique = ParseUnique(col);
                if (col.AttrUnique)
                    col.AttrBlankForbid = true;

                if (col.AttrUnique && !IsDataTypeValid(db_col.Col.DataType))
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

            // 格式: Unique
            public static bool ParseUnique(TableHeaderItem col)
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

        public class ConParserBlankForbid : ConstraintParser
        {
            public override void Process(TableCol db_col, DataBase db)
            {
                var col = db_col.Col;
                if (col.AttrBlankForbid)
                    return;

                col.AttrBlankForbid = ParseBlankForbid(col);
            }

            // 格式 : BlankForbid
            public static bool ParseBlankForbid(TableHeaderItem col)
            {
                foreach (var p in col.StrConstraints)
                {
                    var temp = p.ToLower().Trim();
                    if (temp == "blankforbid")
                        return true;
                }
                return false;
            }
        }

        public class ConParserExportFlag : ConstraintParser
        {
            public override void Process(TableCol db_col, DataBase db)
            {
                var col = db_col.Col;
                col.ExportFlag = ParseExport(col);
            }

            // 格式: Export[Client], Export[Svr], Export[None]
            // 不填写 默认all
            public static EExportFlag ParseExport(TableHeaderItem col)
            {
                foreach (var str in col.StrConstraints)
                {
                    var temp = str.ToLower().Trim();
                    switch (temp)
                    {
                        case "export[client]":
                            return EExportFlag.client;
                        case "export[svr]":
                            return EExportFlag.svr;
                        case "export[none]":
                            return EExportFlag.none;
                        case "export[all]":
                            return EExportFlag.all;
                        default:
                            continue;
                    }
                }
                return EExportFlag.all;
            }
        }

        public class ConParserFilePath : ConstraintParser
        {
            public override void Process(TableCol db_col, DataBase db)
            {
                var col = db_col.Col;
                col.AttrFilePath = ParseFilePath(col);
                if (col.AttrFilePath == null)
                    return;

                if (!_is_data_type_valid(col.DataType))
                    ErrSet.E(db_col, $"该字段是 FilePath[xx,yy] 约束的情况下, 只能支持 string,list_string 类型");
            }

            //只能 string 或者 List<string>
            public static bool _is_data_type_valid(DataType data_type)
            {
                if (data_type.IsTuple)
                    return false;

                if (data_type.type0 != EDataType.String)
                    return false;

                return true;
            }

            // 格式: FilePath[Dir,prefab]
            public static ConAttrFilePath ParseFilePath(TableHeaderItem col)
            {
                foreach (var p in col.StrConstraints)
                {
                    var temp = p.Trim();
                    if (!temp.ToLower().StartsWith("filepath["))
                        continue;

                    int start_index = "filepath[".Length;
                    int end_index = temp.Length - 1;
                    var ret = temp.Substring(start_index, end_index - start_index);

                    var tt = ret.Split(',');
                    if (tt.Length == 1)
                    {
                        return new ConAttrFilePath()
                        {
                            _dir_prefix = tt[0].Trim(),
                            _file_suffix = null,
                        };
                    }
                    else
                        return new ConAttrFilePath()
                        {
                            _dir_prefix = tt[0].Trim(),
                            _file_suffix = tt[1].Trim()
                        };

                }
                return null;
            }
        }

        public class ConParserLookUp : ConstraintParser
        {
            public override void Process(TableCol db_col, DataBase db)
            {
                var col = db_col.Col;
                col.AttrLookUp = ParseLookUp(col);

                if (col.AttrLookUp == null)
                    return;

                if (!_is_data_type_valid(col.DataType))
                    ErrSet.E(db_col, $"LookUp 约束, 只能支持 int,int64,string 以及对应的list类型, 不支持枚举");
            }

            public bool _is_data_type_valid(DataType data_type)
            {
                if (data_type.enum_type != null)
                    return false;

                if (data_type.IsTuple)
                    return false;

                if (data_type.type0 != EDataType.Int32
                    && data_type.type0 != EDataType.Int64
                    && data_type.type0 != EDataType.String)
                    return false;
                return true;
            }

            // 格式: LookUp[SheetName.ColName]
            public static ConAttrLookup ParseLookUp(TableHeaderItem col)
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
                    return new ConAttrLookup()
                    {
                        _sheet_name = tt[0].Trim(),
                        _col_name = tt[1].Trim(),
                    };
                }
                return null;
            }
        }

        public class ConParserRange : ConstraintParser
        {
            public override void Process(TableCol db_col, DataBase db)
            {
                var col = db_col.Col;
                col.AttrRange = ParseRange(col);
                if (col.AttrRange == null)
                    return;

                if (!_is_data_type_valid(col.DataType))
                    ErrSet.E(db_col, $"Range[],只能支持 int,int64,float,double 以及对应的list类型, 不支持枚举");
            }

            public static bool _is_data_type_valid(DataType data_type)
            {
                if (data_type.enum_type != null)
                    return false;

                if (data_type.IsTuple)
                    return false;

                if (data_type.type0 != EDataType.Int32
                    && data_type.type0 != EDataType.Int64
                    && data_type.type0 != EDataType.Float32
                    && data_type.type0 != EDataType.Float64)
                    return false;
                return true;
            }

            // 格式: Range[min,max]
            public static ConAttrRange ParseRange(TableHeaderItem col)
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
                    return new ConAttrRange()
                    {
                        _min = tt[0].Trim(),
                        _max = tt[1].Trim()
                    };
                }
                return null;
            }
        }

        public class ConParserTupleAlias : ConstraintParser
        {
            public override void Process(TableCol db_col, DataBase db)
            {
                var col = db_col.Col;
                col.AttrTupleAlias = ParseTupleAlias(col);
                if (col.AttrTupleAlias == null)
                    return;

                if (!_is_data_type_valid(col.DataType))
                    ErrSet.E(db_col, $"TupleAlias[],只能支持 tuple");
            }

            public static bool _is_data_type_valid(DataType data_type)
            {
                if (data_type.enum_type != null)
                    return false;

                return data_type.IsTuple;
            }

            // 格式: TupleAlias[name]
            public static ConAttrTupleAlias ParseTupleAlias(TableHeaderItem col)
            {
                foreach (var p in col.StrConstraints)
                {
                    var temp = p.Trim();
                    if (!temp.ToLower().StartsWith("tuplealias["))
                        continue;

                    int start_index = "tuplealias[".Length;
                    int end_index = temp.Length - 1;
                    var ret = temp.Substring(start_index, end_index - start_index).Trim();
                    if (string.IsNullOrEmpty(ret))
                        return null;
                    return new ConAttrTupleAlias(ret);
                }
                return null;
            }
        }
    }
}
