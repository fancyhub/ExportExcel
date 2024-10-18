using System;
using System.Collections.Generic;

namespace ExportExcel
{
    public class PPLocalizationHashId : IProcessNode
    {
        public string GetName()
        {
            return "LocStr to LocId";
        }

        public void Process(DataBase data_base)
        {
            //1. 检查
            string loc_sheet_name = data_base.Config.localization.GetLocSheetName();
            if (string.IsNullOrEmpty(loc_sheet_name))
                return;
            if (!data_base.Config.localization.useHashId)
                return;


            //2. 生成Id 并检查是否有冲突
            Dictionary<int, string> id_2_str = new Dictionary<int, string>(data_base.LangDefault.Count);

            bool has_error = false;
            foreach (var p in data_base.LangDefault)
            {
                int hash_id = p.Key.ToLocId();
                if (id_2_str.TryGetValue(hash_id, out string old_key))
                {
                    ErrSet.E($"Loc Id \"{old_key}\" 和 \"{p.Key}\" 生成的 HashId 冲突");
                    has_error = true;
                    continue;
                }
                id_2_str.Add(hash_id, p.Key);
            }

            if (has_error)
                return;


            //3. 修改所有LocId
            data_base.ForeachCol((col) =>
            {
                if (col.Field.DataType.type0 != EDataType.LocStr)
                    return;

                col.Field.DataType.type0 = EDataType.LocId;
                if (col.Field.DataType.IsList)
                {
                    col.ForeachCell((cell) =>
                    {
                        var tt = cell.Value.Split(ConstDef.C_LIST_SPLIT, StringSplitOptions.RemoveEmptyEntries);
                        if (tt.Length == 0)
                        {
                            cell.Value = string.Empty;
                        }
                        else
                        {
                            List<int> temp = new List<int>(tt.Length);
                            foreach (var t in tt)
                            {
                                temp.Add(t.ToLocId());
                            }

                            
                            cell.Value = string.Join(ConstDef.C_LIST_SPLIT, temp.ToArray());
                        }
                    });
                }
                else
                {
                    col.ForeachCell((cell) =>
                    {
                        cell.Value = cell.Value.ToLocId().ToString();
                    });
                }
            });


            //4. 修改导出表
            Table table_loc = data_base.Tables[loc_sheet_name];
            table_loc.Header[0].DataType.type0 = EDataType.Int32;

            foreach (var p in table_loc.MultiLangBody)
            {
                string[,] body = p.Value;
                int row = body.GetLength(0);
                for (int r = 0; r < row; r++)
                {
                    body[r, 0] = body[r, 0].ToLocId().ToString();
                }
            }

            //5. 增加Key的导出
            {
                string[,] body = new string[id_2_str.Count, 2];
                int row = 0;
                foreach (var p in id_2_str)
                {
                    body[row, 0] = p.Key.ToString();
                    body[row, 1] = p.Value;
                    row++;
                }

                table_loc.MultiLangBody.Add("KEY", body);
            }
        }
    }
}
