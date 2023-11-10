using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/12 11:35:15
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    //多语言处理
    public class PPLocalization : IProcessNode
    {
        public PPLocalization()
        {

        }

        public string GetName()
        {
            return "处理多语言";
        }

        public void Process(DataBase data_base)
        {
            //1. 如果 Loc 的表没有, 直接不处理了
            var config_loc = data_base.Config.localization;
            string loc_sheet_name = config_loc.GetLocSheetName();
            string default_lang_name = config_loc.GetDefaultLang();

            if (string.IsNullOrEmpty(loc_sheet_name))
                return;
            //2. 检查Table Loc

            Table table_loc = data_base.Tables[loc_sheet_name];
            //检查 TableLoc
            {
                if (table_loc == null)
                {
                    ErrSet.E($"找不到多语言表格 {loc_sheet_name}");
                    return;
                }
                if (table_loc.Header.IndexOfCol(default_lang_name) < 0)
                {
                    ErrSet.E($"多语言表格 {loc_sheet_name}, 没有找到列 {default_lang_name}");
                    return;
                }

                if (table_loc.Header.Pk == null)
                {
                    ErrSet.E($"多语言表格 {loc_sheet_name}, 没有主Key");
                    return;
                }

                if (table_loc.Header.Pk != table_loc.Header[0])
                {
                    ErrSet.E($"多语言表格 {loc_sheet_name}, 主Key不是第一个");
                    return;
                }

                foreach(var p in table_loc.Header.List)
                {
                    if(p.DataType.type0!= EDataType.String || p.DataType.IsTuple || p.DataType.IsList)
                    {
                        ErrSet.E($"多语言表格 {loc_sheet_name}, {p.Name} 不是 String类型");
                        return;
                    }
                }                
            }

            //3. 翻译表不存在, 直接分表
            switch(config_loc.EMode)
            {
                case ExeConfig.ELocalizationMode.Normal:
                    {
                        Dictionary<string, string[,]> multi_lang_body = _gen_loc_array(table_loc);
                        _split_table_loc_to_multi(table_loc, default_lang_name, multi_lang_body);
                        data_base.LangDefault = _convert(multi_lang_body[default_lang_name], 1);

                        data_base.LangList.Clear();
                        foreach (var p in multi_lang_body)
                            data_base.LangList.Add(p.Key);


                        //做检查
                        data_base.ForeachCol((col) =>
                        {
                            if (col.Col.DataType.type0 != EDataType.LocStr)
                                return;

                            col.ForeachCell((cell) =>
                            {
                                string v = cell.Value;
                                if (v == string.Empty)
                                    return;

                                bool contain = data_base.LangDefault.ContainsKey(v);
                                if (!contain)
                                    ErrSet.E(col, $"没有找到对应的多语言 Key {v}");
                            });
                        });
                    }
                    break;

                case ExeConfig.ELocalizationMode.AutoGenKey:
                    {
                        //3.1 生成Loc的key
                        Dictionary<string, string> default_lang_dict = _auto_gen_lang_dict(data_base);
                        data_base.LangDefault = default_lang_dict;

                        //3.2 生成多语言的 MultiLangBody
                        Dictionary<string, Dictionary<string, string>> loc_trans_dict = _gen_loc_dict(table_loc);
                        Dictionary<string, string[,]> multi_lang_body = new Dictionary<string, string[,]>();
                        for (int i = 1; i < table_loc.Header.List.Count; i++)
                        {
                            string lang_name = table_loc.Header[i].Name;
                            if (lang_name == default_lang_name)
                            {
                                multi_lang_body.Add(lang_name, _convert(default_lang_dict));
                            }
                            else
                            {
                                string[,] lang_body = _trans(default_lang_dict, loc_trans_dict[lang_name]);
                                multi_lang_body.Add(lang_name, lang_body);
                            }
                        }

                        //3.3  创建新的翻译表, 用来后续的导出, 方便翻译, 有新旧的比较
                        if (data_base.Config.exportLocTrans!=null && data_base.Config.exportLocTrans.enable) //需要导出 翻译表
                        {
                            var new_table_trans = _create_new_table_trans(table_loc, default_lang_name, default_lang_dict, loc_trans_dict);
                            new_table_trans.SheetName = "#"+loc_sheet_name;
                            data_base.Tables[new_table_trans.SheetName] = new_table_trans;
                        }

                        //3.4 重新生成 TableLoc
                        _split_table_loc_to_multi(table_loc, default_lang_name, multi_lang_body);


                        data_base.LangList.Clear();
                        foreach (var p in multi_lang_body)
                            data_base.LangList.Add(p.Key);
                    }
                    break;
            }
        }

        private static Table _create_new_table_trans(Table table_loc,
            string default_lang_name,
            Dictionary<string, string> default_lang_dict,
            Dictionary<string, Dictionary<string, string>> loc_trans_dict)
        {
            //1. 获取所有的语言列表
            List<string> lang_list = new List<string>();
            string old_lang_name = "#Old_" + default_lang_name;
            lang_list.Add(old_lang_name);
            for (int i = 1; i < table_loc.Header.Count; i++)
            {
                lang_list.Add(table_loc.Header[i].Name);
            }

            //2. 生成语言列表
            List<Dictionary<string, string>> lang_dict_list = new List<Dictionary<string, string>>();
            foreach (var name in lang_list)
            {
                if (name == default_lang_name)
                {
                    lang_dict_list.Add(default_lang_dict);
                }
                else if (name == old_lang_name)
                {
                    loc_trans_dict.TryGetValue(default_lang_name, out var old);
                    lang_dict_list.Add(old == null ? new Dictionary<string, string>() : old);
                }
                else
                {
                    loc_trans_dict.TryGetValue(name, out var old);
                    lang_dict_list.Add(old == null ? new Dictionary<string, string>() : old);
                }
            }


            //3. 生成表头
            Table table_trans = new Table();
            table_trans.TableExportFlag = EExportFlag.none;
            table_trans.Header.Add(table_loc.Header.Pk.Clone());
            TableHeaderItem header_col = table_loc.Header[default_lang_name];
            for (int i = 0; i < lang_list.Count; i++)
            {
                TableHeaderItem t = header_col.Clone();
                t.Name = lang_list[i];
                t.ExcelColIdx = i + 1;
                t.Desc = t.Name;
                table_trans.Header.Add(t);
            }

            //4. 生成body
            int row_count = default_lang_dict.Count;
            int col_count = lang_list.Count + 1;
            string[,] body = new string[row_count, col_count];
            table_trans.Body = body;
            int row = 0;
            foreach (var p in default_lang_dict)
            {
                body[row, 0] = p.Key;
                for (int c = 0; c < lang_list.Count; c++)
                {
                    lang_dict_list[c].TryGetValue(p.Key, out var v);
                    body[row, c + 1] = v;
                }
                row++;
            }

            return table_trans;
        }

        private static void _split_table_loc_to_multi(Table table_loc, string default_lang_name, Dictionary<string, string[,]> multi_body)
        {
            TableHeader header = new TableHeader();
            header.Add(table_loc.Header.Pk.Clone());
            TableHeaderItem val_col = table_loc.Header[default_lang_name].Clone();
            val_col.Name = "Val";
            header.Add(val_col);

            table_loc.Body = new string[0, 0];
            table_loc.Header = header;
            table_loc.MultiLangBody = multi_body;
        }

        private static Dictionary<string, string[,]> _gen_loc_array(Table table_loc)
        {
            Dictionary<string, string[,]> ret = new Dictionary<string, string[,]>();
            string[,] body = table_loc.Body;
            int row_count = body.GetLength(0);

            for (int c = 1; c < table_loc.Header.Count; c++)
            {
                string lang_name = table_loc.Header[c].Name;
                string[,] lang_array = new string[row_count, 2];
                ret.Add(lang_name, lang_array);

                for (int r = 0; r < row_count; r++)
                {
                    lang_array[r, 0] = body[r, 0];
                    lang_array[r, 1] = body[r, c];
                }
            }
            return ret;
        }

        private static string[,] _convert(Dictionary<string, string> dict)
        {
            string[,] ret = new string[dict.Count, 2];
            int r = 0;
            foreach (var p in dict)
            {
                ret[r, 0] = p.Key;
                ret[r, 1] = p.Value == null ? string.Empty : p.Value;
                r++;
            }
            return ret;
        }

        private static Dictionary<string, string> _convert(string[,] body, int val_col_idx)
        {
            int row = body.GetLength(0);
            Dictionary<string, string> ret = new Dictionary<string, string>();
            for (int r = 0; r < row; r++)
            {
                string key = body[r, 0];
                string v = body[r, val_col_idx];
                ret.Add(key, v == null ? string.Empty : v);
            }
            return ret;
        }

        private static Dictionary<string, Dictionary<string, string>> _gen_loc_dict(Table table_trans)
        {
            Dictionary<string, Dictionary<string, string>> ret = new Dictionary<string, Dictionary<string, string>>();
            string[,] body = table_trans.Body;

            for (int c = 1; c < table_trans.Header.Count; c++)
            {
                string lang_name = table_trans.Header[c].Name;
                Dictionary<string, string> lang_dict = _convert(body, c);
                ret.Add(lang_name, lang_dict);
            }
            return ret;
        }

        private string[,] _trans(Dictionary<string, string> default_lang_dict, Dictionary<string, string> other_lang)
        {
            string[,] ret = new string[default_lang_dict.Count, 2];
            int row = 0;
            foreach (var p in default_lang_dict)
            {
                ret[row, 0] = p.Key;
                string v = null;
                other_lang?.TryGetValue(p.Key, out v);
                ret[row, 1] = (v == null ? string.Empty : v);
                row++;
            }
            return ret;
        }

        private static Dictionary<string, string> _auto_gen_lang_dict(DataBase data_base)
        {
            //1. 获取符合规则的col
            List<TableCol> col_list = data_base.GetAllCols((col) =>
            {
                if (col.Col.DataType.type0 != EDataType.LocStr)
                    return false;

                int pk_idx = col.Table.Header.PkIdx;
                if (pk_idx >= 0)
                    return true;

                ErrSet.E(col, "LocStr 字段,该表格必须要有主key");
                return false;
            });


            PPLangKeyData key_data = new PPLangKeyData();
            foreach (var col in col_list)
            {
                key_data.BeginCol(col);
                col.ForeachCell((cell) =>
                {
                    cell.Value = key_data.GenKey(cell);
                });
            }
            return key_data.GetDict();
        }
    }


    public class PPLangKeyData
    {
        public Dictionary<string, string> _lang_dict = new Dictionary<string, string>(10000);
        public Dictionary<string, string> _val_2_key = new Dictionary<string, string>(10000);
        public TableCol _col;
        public bool _can_merge = false;
        public int _pk_idx = -1;
        public int _pk_sec_idx = -1;

        public Dictionary<string, string> GetDict()
        {
            return _lang_dict;
        }

        public void BeginCol(TableCol col)
        {
            _val_2_key.Clear();

            _col = col;
            _can_merge = false;
            _pk_idx = col.Table.Header.PkIdx;
            var attr_pk = col.Table.Header.Pk.AttrPK;
            _pk_sec_idx = -1;
            if (attr_pk != null && attr_pk._sec_key != null)
                _pk_sec_idx = attr_pk._sec_key_idx;
        }

        public string GenKey(TableCell cell)
        {
            string cell_v = cell.Value;
            if (string.IsNullOrEmpty(cell.Value))
                return "";

            string key = null;
            if (_can_merge && _val_2_key.TryGetValue(cell_v, out key))
                return key;

            if (_pk_sec_idx >= 0)
                key = $"{cell.SheetName}_{cell.ColName}_{cell.GetCellValue(_pk_idx)}_{cell.GetCellValue(_pk_sec_idx)}";
            else
                key = $"{cell.SheetName}_{cell.ColName}_{cell.GetCellValue(_pk_idx)}";
            key = key.ToUpper();

            if (_can_merge)
                _val_2_key.Add(cell_v, key);

            _lang_dict.Add(key, cell_v);
            return key;
        }
    }
}
