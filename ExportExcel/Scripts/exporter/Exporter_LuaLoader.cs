using System;
using System.Collections.Generic;
using System.IO;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/13 16:44:09
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class Exporter_LuaLoader : IProcessNode
    {
        public const string C_FILE_NAME = "table_loader.lua";
        public StringFormater _formater = new StringFormater();
        public ExeConfig.LuaConfig _config;
        public EExportFlag _flag;

        public Exporter_LuaLoader(EExportFlag flag, ExeConfig.LuaConfig config)
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
            sw.WriteLine(@"
-- 此文件由工具自动生成，请勿手动修改
local Log = Log
local this = LuaTableMgr
this._name_2_list = {}
this._name_2_map = {}
");
            foreach (var p in tables)
            {
                if (data.Config.localization.IsLocalizationSheet(p.SheetName ))
                    continue;

                _export_load_func(p, sw);
            }

            sw.WriteLine(@"
local name_2_loader = {}
this._name_2_loader = name_2_loader");

            foreach (var p in tables)
            {
                string class_name = _formater["class_prefix"] + p.SheetName;
                sw.WriteLine("name_2_loader[\"{0}\"]= _Load{1}", class_name, p.SheetName);
            }
            sw.Close();
        }

        public void _export_load_func(FilterTable table, StreamWriter sw)
        {
            List<TableHeaderItem> header = table.Header;
            string multi_name = "";
            if (table.MultiLang)
                multi_name = "sheet_name = sheet_name ..\"_\".. lang;";
            _formater["sheet_name_lang"] = multi_name;
            _formater["col_count"] = header.Count.ToString();
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _formater["class_prefix"]+ table.SheetName;
            var pk = table.PK;

            if (pk != null)
            {
                _formater["pk_name"] = pk.Name;
                if (pk.AttrPK.IsCompose())
                {
                    _formater["pk_sec_name"] = pk.AttrPK._sec_key.Name;
                }
            }

            sw.WriteLine("");
            sw.WriteLineExt(_formater, @"
local function _Load{sheet_name}()
    local sheet_name = ""{sheet_name}""
    {sheet_name_lang}
    local csv_reader = this.CreateCsvReader(sheet_name)
    if csv_reader == nil then 
        return
    end
    local csv_line_reader = csv_reader:lines()

    local first_line = csv_line_reader()
    if first_line == nil or #first_line ~= {col_count} then
        Log.E(""加载 表格失败, 格式不对 "" .. sheet_name)
        return
    end
    local sec_line = csv_line_reader()
    if sec_line == nil or #sec_line ~= {col_count} then
        Log.E(""加载 表格失败, 格式不对 "" .. sheet_name)
        return
    end

    local list_data = { }
    local map_data = { }
    for line in csv_line_reader do
        local data = { }");


            for (int i = 0; i < header.Count; i++)
            {
                TableHeaderItem c = header[i];
                sw.WriteLine("\t\tdata.{0}= {2}(line[{1}])", c.Name, i + 1, c.DataType.ToLuaParseStr());
            }

            sw.WriteLine("\t\ttable.insert(list_data, data)");
            if (pk != null)
            {
                if (!pk.AttrPK.IsCompose())
                    sw.WriteLineExt(_formater, "\t\tmap_data[data.{pk_name}] = data");
                else
                {
                    
                    sw.WriteLineExt(_formater, "\t\tmap_data[(data.{pk_name}<<32) |data.{pk_sec_name} ] = data");
                }
            }

            sw.WriteLine("\tend");
            //sw.WriteLine("\tthis._name_2_list[sheet_name] = list_data");
            if (pk != null)
            {
                sw.WriteLine("\treturn list_data,map_data");
                //sw.WriteLine("\tthis._name_2_map[sheet_name] = map_data");
            }
            else
                sw.WriteLine("\treturn list_data");
            sw.WriteLine("end");
        }
    }
}
