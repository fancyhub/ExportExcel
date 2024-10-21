using System;
using System.Collections.Generic;
using System.IO;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/7 16:50:44
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    //导出 csharp的 加载
    public class Exporter_CSStructTable : IProcessNode
    {
        public const string C_FILE_NAME = "struct_table.cs";

        public StringFormater _formater = new StringFormater();


        public Config.CSharpConfig _config;
        public EExportFlag _flag;

        public Exporter_CSStructTable(EExportFlag flag, Config.CSharpConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export C# Struct Table";
        }

        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable )
                return;

            List<FilterTable> tables = FilterTable.Filter(data, _flag);

            string name_space = _config.namespaceName;
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("//自动生成的");
            sw.WriteLine(_config.header);
            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("namespace " + name_space + "{");

            sw.WriteLine(@"
    public abstract partial class Table
    {
        public virtual void BuildMap() { }
        public abstract bool IsMutiLang { get; }
        public abstract string SheetName { get; }
    }

    [System.Serializable]
    public abstract partial class Table<TTableItem> : Table where TTableItem : class
    {
        public List<TTableItem> List = new List<TTableItem>();
    }
");

            _ExportTables(tables, sw);

            _ExportTableMgr(tables, sw);
            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("}");
            sw.Close();

        }

        public void _ExportTableMgr(List<FilterTable> tables, StreamWriter sw)
        {
            _formater["table_mgr_name"] = _config.tableMgrName;
            _formater["table_count"] = tables.Count.ToString();

            sw.WriteLineExt(_formater, @"
    [System.Serializable]
    public sealed partial class {table_mgr_name}
    {  
");

            foreach (var p in tables)
            {
                sw.WriteLine("\t\tpublic Table{0} {1} = new();", _config.GetClassName(p.SheetName), p.SheetName);
            }

            sw.WriteLineExt(_formater, @"        
        private List<Table> _AllTables;
        public List<Table> AllTables
        {            
            get {
                if(_AllTables!=null)
                    return _AllTables;
                _AllTables = new List<Table>({table_count});            
");


            foreach (var p in tables)
            {
                sw.WriteLine("\t\t\t\t_AllTables.Add({0});", p.SheetName);
            }

            sw.WriteLine(@"
                return _AllTables;
            }
        }
    }");

        }

        public void _ExportTables(List<FilterTable> tables, StreamWriter sw)
        {
            foreach (var p in tables)
            {
                _BuildPkFormater(p);
                _ExportTableHeader(p, sw);
                _ExportGetterFunc(p, sw);
                _ExportBuildFunc(p, sw);
                sw.WriteLine("\t}");
            }
        }

        public void _ExportTableHeader(FilterTable table, StreamWriter sw)
        {
            List<TableField> header = table.FiltedHeader;
            _formater["multi_lang"] = table.MultiLang.ToString().ToLower();
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _config.GetClassName(table.SheetName);

            TableField pk = table.PK;
            sw.WriteLineExt(_formater, @"
    [System.Serializable]
    public sealed partial class Table{class_name}: Table<{class_name}>
    {
        public override bool IsMutiLang => {multi_lang};
        public override string SheetName => ""{sheet_name}"";
    ");

            if (pk != null)
            {
                if (pk.AttrPK.IsCompose())
                {
                    sw.WriteLineExt(_formater, @"
        [System.NonSerialized] public Dictionary<({key_list}), {class_name}> Map;        

");
                }
                else
                {
                    sw.WriteLineExt(_formater, @"
        [System.NonSerialized] public Dictionary<{pk_type}, {class_name}> Map;        
");
                }
            }
        }

        private void _BuildPkFormater(FilterTable table)
        {
            TableField pk = table.PK;
            if (pk == null)
                return;

            _formater["pk_type"] = pk.ToCSharpStr();
            _formater["pk_name"] = pk.Name;

            List<string> temp = new List<string>();
            temp.Add(pk.ToCSharpStr() + " " + pk.Name);
            foreach (var p in pk.AttrPK.SubKeys)
            {
                temp.Add(p.ToCSharpStr() + " " + p.Name);
            }
            string key_list = string.Join(", ", temp);

            temp.Clear();
            temp.Add(pk.ToCSharpStr());
            foreach (var p in pk.AttrPK.SubKeys)
            {
                temp.Add(p.ToCSharpStr());
            }
            string key_type_list = string.Join(", ", temp);


            temp.Clear();
            temp.Add(pk.Name);
            foreach (var p in pk.AttrPK.SubKeys)
            {
                temp.Add(p.Name);
            }
            string key_name_list = string.Join(",", temp);

            temp.Clear();
            temp.Add("p." + pk.Name);
            foreach (var p in pk.AttrPK.SubKeys)
            {
                temp.Add("p." + p.Name);
            }
            string key_name_list_with_item = string.Join(",", temp);

            _formater["key_list"] = key_list;
            _formater["key_type_list"] = key_type_list;
            _formater["key_name_list"] = key_name_list;
            _formater["key_name_list_with_item"] = key_name_list_with_item;
        }

        public void _ExportBuildFunc(FilterTable table, StreamWriter sw)
        {
            TableField pk = table.PK;
            if (pk == null)
                return;

            if (pk.AttrPK.IsCompose())
            {
                sw.WriteLineExt(_formater, @"
        public override void BuildMap()
        {
            if (Map == null)
                Map = new (List.Count);
            Map.Clear();
            foreach (var p in List)
            {
                Map[({key_name_list_with_item})] = p;
            }
        }");
            }
            else
            {
                sw.WriteLineExt(_formater, @"
        public override void BuildMap()
        {
            if (Map == null)
                Map = new (List.Count);
            Map.Clear();
            foreach (var p in List)
            {
                Map[p.{pk_name}] = p;
            }
        }");
            }

        }

        public void _ExportGetterFunc(FilterTable table, StreamWriter sw)
        {
            List<TableField> header = table.FiltedHeader;
            _formater["multi_lang"] = table.MultiLang.ToString().ToLower();
            _formater["col_count"] = header.Count.ToString();
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _config.GetClassName(table.SheetName);

            TableField pk = table.PK;
            if (pk == null)
                return;

            if (!pk.AttrPK.IsCompose())
            {
                _formater["pk_type"] = pk.ToCSharpStr();
                _formater["pk_name"] = pk.Name;
                _formater["param_check"] = "";
                _formater["param_nodefault"] = "";
                if (pk.DataType.type0 == EDataType.String)
                {
                    _formater["param_check"] = @$"
            if(string.IsNullOrEmpty({pk.Name}))
            {{
                Log.E(""param is null"");
                return null;
            }}
";
                }
                else if (pk.DataType.type0 == EDataType.Int32 || pk.DataType.type0 == EDataType.UInt32 || pk.DataType.type0 == EDataType.Int64 || pk.DataType.type0 == EDataType.UInt64)
                {
                    _formater["param_nodefault"] = "if(" + pk.Name + "!=default)";
                }

                sw.WriteLineExt(_formater, @"
        public {class_name} Find({pk_type} {pk_name})
        {
            if (Map == null)
            {
                Log.E(""Table{class_name}'s map is null"");
                return null;
            }
            {param_check};
            Map.TryGetValue({pk_name}, out var ret);
            {param_nodefault};
            Log.Assert(ret != null, ""can't find {0} in Table{class_name}"", {pk_name});
            return ret;
        }    
");
            }
            else
            {
                sw.WriteLineExt(_formater, @"
        public {class_name} Find({key_list})
        {
            if (Map == null)
            {
                Log.E(""Table{class_name}'s map is null"");
                return null;
            }
            Map.TryGetValue(({key_name_list}), out var ret);
            Log.Assert(ret != null, ""can't find {0} in Table{class_name}"", ({key_name_list}));
            return ret;
        }    
");
            }
        }
    }
}
