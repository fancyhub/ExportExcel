using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/13 16:43:53
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class Exporter_LuaGetter : IProcessNode
    {
        public const string C_FILE_NAME = "table_getter.lua";
        public StringFormater _formater = new StringFormater();
        public ExeConfig.LuaConfig _config;
        public EExportFlag _flag;

        public Exporter_LuaGetter(EExportFlag flag, ExeConfig.LuaConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export";
        }
        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable)
                return;

            _formater["class_prefix"] = _config.classPrefix;
            List<FilterTable> tables = FilterTable.Filter(data, _flag);
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("-- 此文件由工具自动生成，请勿手动修改");

            //生成枚举结构
            foreach (var p in data.EnumDB)
            {
                sw.WriteLine("{0} = {{", p.Key);

                List<EnumField> enum_field_list = p.Value.GetAllFields().ToList();
                enum_field_list.Sort((a, b) =>
                {
                    return a.Val - b.Val;
                });

                foreach (var p2 in enum_field_list)
                {
                    sw.WriteLine("\t-- " + p2.ExcelVal.Replace("\n", "\n\t-- "));
                    sw.WriteLine("\t{0} = {1},", p2.Name, p2.Val);
                }
                sw.WriteLine("}");
            }

            sw.WriteLine("local LuaTableMgr = LuaTableMgr");

            foreach (var table in tables)
            {
                if (data.Config.localization.IsLocalizationSheet(table.SheetName))
                    continue;

                var pk = table.PK;
                _formater["class_name"] = _formater["class_prefix"] + table.SheetName;
                _formater["sheet_name"] = table.SheetName;
                sw.WriteLineExt(_formater,
                    @"
---@return table<number,{class_name}>
function LuaTableMgr.Get{class_name}List()
    local list_data, map_data = LuaTableMgr.LoadTable(""{class_name}"")
    return list_data
end");

                if (pk != null)
                {
                    _formater["pk_type"] = pk.DataType.ToLuaStr();
                    _formater["pk_name"] = pk.Name;
                    if (!pk.AttrPK.IsCompose())
                    {
                        sw.WriteLineExt(_formater,
                            @"
---@return {class_name}
function LuaTableMgr.Get{class_name}({pk_name})
    local data = LuaTableMgr.Get(""{class_name}"", {pk_name})
    return data
end

---@return table<{pk_type},{class_name}>
function LuaTableMgr.Get{class_name}Dict()
    local list_data, map_data = LuaTableMgr.LoadTable(""{class_name}"")
    return map_data
end");
                    }
                    else
                    {
                        _formater["pk_sec_name"] = pk.AttrPK._sec_key.Name;
                        sw.WriteLineExt(_formater,
                            @"
---@return {class_name}
function LuaTableMgr.Get{class_name}({pk_name},{pk_sec_name})
    local data = LuaTableMgr.Get2(""{class_name}"", {pk_name},{pk_sec_name})
    return data
end

---@return table<long,{class_name}>
function LuaTableMgr.Get{class_name}Dict()
    local list_data, map_data = LuaTableMgr.LoadTable(""{class_name}"")
    return map_data
end");
                    }
                }
            }
            sw.Close();
        }
    }
}
