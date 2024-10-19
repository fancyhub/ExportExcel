using System;
using System.Collections.Generic;

namespace ExportExcel
{
    public interface IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db);
    }

    //后处理, 约束解析
    public class PPconstraintParser : IProcessNode
    {
        public static List<IConstraintParser> _constratint_parser_list = new List<IConstraintParser>
            {
                new ConParserEnum(),
                new ConParserPK(),
                new ConParserUnique(),
                new ConParserBlankForbid(),
                new ConParserRange(),
                new ConParserFilePath(),
                new ConParserLookUp(),
                new ConParserExportFlag(),
                new ConParserDefault(),
                new ConParserAlias(),
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
    }

    /// <summary>
    /// 格式 Alias[xxx]
    /// </summary>
    public class ConParserAlias : IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db)
        {
            TableField col = db_col.Field;
            string alias_name = _parse_alias(col);
            if (string.IsNullOrEmpty(alias_name))
                return;
            col.AttrAlias = ConAttrAlias.Create(db.AliasDB.Find(alias_name));
            if (col.AttrAlias == null)
            {
                ErrSet.E(db_col, $"找不到Alias \"{alias_name}\"");
            }
        }


        private static string _parse_alias(TableField col)
        {
            foreach (var str in col.StrConstraints)
            {
                var temp = str.Trim();
                if (!temp.ToLower().StartsWith("alias["))
                {
                    continue;
                }
                int start_index = "alias[".Length;
                int end_index = temp.Length - 1;
                var ret = temp.Substring(start_index, end_index - start_index);
                return ret;
            }
            return null;
        }
    }

    // 格式: Export[Client], Export[Svr], Export[None]
    // 不填写 默认all
    public class ConParserExportFlag : IConstraintParser
    {
        public void Process(TableCol db_col, DataBase db)
        {
            var col = db_col.Field;
            col.ExportFlag = _ParseExport(col);
        }

        private static EExportFlagMask _ParseExport(TableField col)
        {
            foreach (var str in col.StrConstraints)
            {
                var temp = str.ToLower().Trim();
                switch (temp)
                {
                    case "export[client]":
                        return EExportFlagMask.Client;
                    case "export[svr]":
                        return EExportFlagMask.Server;
                    case "export[none]":
                        return EExportFlagMask.None;
                    case "export[all]":
                        return EExportFlagMask.All;
                    default:
                        continue;
                }
            }
            return EExportFlagMask.All;
        }
    }
}
