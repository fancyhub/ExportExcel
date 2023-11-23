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
            if (config_loc.Mode == ExeConfig.ELocalizationMode.None)
                return;

            string loc_sheet_name = config_loc.GetLocSheetName();
            string default_lang_name = config_loc.GetDefaultLang();
            data_base.Tables.TryGetValue(loc_sheet_name, out Table table_loc);

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

                foreach (var p in table_loc.Header.List)
                {
                    if (p.DataType.type0 != EDataType.String || p.DataType.IsTuple || p.DataType.IsList)
                    {
                        ErrSet.E($"多语言表格 {loc_sheet_name}, {p.Name} 不是 String类型");
                        return;
                    }
                }
            }

            Table newLocTable = _CreateNewLocTableForMultiExport(table_loc);
            data_base.TableLocOld = table_loc;
            data_base.Tables[loc_sheet_name] = newLocTable;
            data_base.LangList.Clear();
            for(int i=1;i< table_loc.Header.Count;i++)
            {
                data_base.LangList.Add(table_loc.Header[i].Name);
            }                

            //3. 翻译表不存在, 直接分表
            switch (config_loc.Mode)
            {
                case ExeConfig.ELocalizationMode.Normal:
                    {
                        Dictionary<string, string[,]> multi_lang_body = _SplitBody2MultiLangBody(table_loc);
                        newLocTable.MultiLangBody = multi_lang_body;

                        data_base.LangDefault = _ConvertArray2Dict(multi_lang_body[default_lang_name], 1);


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
                        Dictionary<string, string> default_lang_dict = _GenAutoLangDict(data_base);
                        data_base.LangDefault = default_lang_dict;

                        //3.2 生成多语言的 MultiLangBody
                        Dictionary<string, string[,]> multi_lang_body = new Dictionary<string, string[,]>();
                        newLocTable.MultiLangBody = multi_lang_body;
                        Dictionary<string, Dictionary<string, string>> loc_trans_dict = new Dictionary<string, Dictionary<string, string>>();
                        for (int i = 1; i < table_loc.Header.List.Count; i++)
                        {
                            string lang_name = table_loc.Header[i].Name;
                            Dictionary<string, string> lang_dict = _ConvertArray2Dict(table_loc.Body, i);
                            loc_trans_dict.Add(lang_name, lang_dict);

                            if (lang_name == default_lang_name)
                            {
                                multi_lang_body.Add(lang_name, _ConvertDict2Arrary(default_lang_dict));
                            }
                            else
                            {                                
                                string[,] lang_body = _LangTrans(default_lang_dict, lang_dict);
                                multi_lang_body.Add(lang_name, lang_body);
                            }
                        }
                        

                        //3.3  创建新的翻译表, 用来后续的导出, 方便翻译, 有新旧的比较
                        if (data_base.Config.exportCommon.localizationTranslate.enable) //需要导出 翻译表
                        {
                            data_base.TableLocTrans = _CreateNewTableTrans(table_loc, default_lang_name, default_lang_dict, loc_trans_dict);
                        }
                    }
                    break;
            }
        }

        private static Table _CreateNewTableTrans(Table table_loc,
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
            table_trans.SheetName = table_loc.SheetName;

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

        private static Table _CreateNewLocTableForMultiExport(Table table_loc)
        {
            Table newTable = new Table();
            newTable.SheetName = table_loc.SheetName;
            newTable.TableExportFlag = table_loc.TableExportFlag;
            newTable.FilePath = table_loc.FilePath;

            TableHeader header = newTable.Header;
            header.Add(table_loc.Header.Pk.Clone());
            TableHeaderItem val_col = new TableHeaderItem();
            val_col.Name = "Val";
            val_col.Desc = "";
            val_col.DataType = new DataType();
            val_col.DataType.AddType(EDataType.String);            
            header.Add(val_col);

            newTable.Body = new string[0, 0];
            newTable.Header = header;

            return newTable;
        }

        private static Dictionary<string, string[,]> _SplitBody2MultiLangBody(Table table_loc)
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

        private static string[,] _ConvertDict2Arrary(Dictionary<string, string> dict)
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

        private static Dictionary<string, string> _ConvertArray2Dict(string[,] body, int val_col_idx)
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

        //翻译
        private string[,] _LangTrans(Dictionary<string, string> default_lang_dict, Dictionary<string, string> other_lang)
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

        private static Dictionary<string, string> _GenAutoLangDict(DataBase data_base)
        {
            //1. 获取符合规则的col
            List<TableCol> col_list = data_base.GetAllCols((col) =>
            {
                if (col.Col.DataType.type0 != EDataType.LocStr)
                    return false;

                int pk_idx = col.Table.Header.PkIdx;
                if (pk_idx >= 0)
                    return true;

                ErrSet.E(col, "LocStr 字段,该表格必须要有主key "+ col.Table.FilePath);
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
