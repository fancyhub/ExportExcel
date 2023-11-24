using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/9 12:12:00
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class ExporterGOLoader : IProcessNode
    {
        public const string C_FILE_NAME = "table_loader.go";
        public StringFormater _formater = new StringFormater();
        public EExportFlag _flag;
        public Config.GoConfig _config;

        public ExporterGOLoader(EExportFlag flag, Config.GoConfig config)
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

            string package_name = _config.packageName;
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            List<FilterTable> tables = FilterTable.Filter(data, _flag);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("package " + package_name);
            sw.WriteLine(@"
import (	
    ""errors""
	""fmt""
	""strconv""
	""strings""	
)
const (	
"
);
            foreach (FilterTable t in tables)
            {
                _formater["class_name"] = _config.GetClassName(t.SheetName); ;
                _formater["sheet_name"] = t.SheetName;

                sw.WriteLineExt(_formater, "\t{class_name}FileName = \"{sheet_name}.csv\"");
            }
            sw.WriteLine(")");



            _formater["table_count"] = tables.Count.ToString();
            sw.WriteLineExt(_formater, @"

func (p* CsvDataMgr) LoadAll(){
    for _, loader := range p.FileName2Func {
		loader()
	}
}

func CreateCsvDataMgr(logger ILogger,reader IDataReader) (*CsvDataMgr, error) {
    cd := CsvDataMgr{
        logger:     logger,
        reader:     reader,
        FileName2Func: make(map[string]CsvLoader, {table_count}),
        FileName2ListData: make(map[string]interface{}, {table_count}),
        FileName2MapData: make(map[string]interface{}, {table_count}),
");

            foreach (var t in tables)
            {
                _formater["class_name"] = _config.GetClassName(t.SheetName);
                sw.WriteLineExt(_formater, "\t\t{class_name}List: make([]{class_name}, 0),");
                var pk = t.PK;
                if (t.PK != null)
                {
                    _formater["pk_type"] = pk.ToGoStr();
                    if (t.PK.AttrPK.IsCompose())
                    {
                        _formater["pk_sec_type"] = pk.AttrPK._sec_key.ToGoStr();
                        sw.WriteLineExt(_formater, "\t\t{class_name}Map:  make(map[{pk_type}]map[{pk_sec_type}]*{class_name}, 0),");
                    }
                    else
                    {
                        sw.WriteLineExt(_formater, "\t\t{class_name}Map:  make(map[{pk_type}]*{class_name}, 0),");
                    }

                }
            }

            sw.WriteLine(@"
    }");

            foreach (var t in tables)
            {
                _formater["class_name"] = _config.GetClassName(t.SheetName);
                _formater["sheet_name"] = t.SheetName;
                sw.WriteLineExt(_formater, "\tcd.FileName2Func[{class_name}FileName] = cd.load{sheet_name}");
            }

            sw.WriteLine(@"
	return &cd, nil
}
");

            _ExportBaseParser(sw);
            _ExportTupleParaser(sw, tables);
            _ExportListParaser(sw, tables);
            foreach (FilterTable t in tables)
            {
                _ExportLoadFunc(t, sw);
            }
            sw.Close();
        }


        private void _ExportTupleParaser(StreamWriter sw, List<FilterTable> table_list)
        {
            List<DataType> all_types = new List<DataType>();
            foreach (var p in table_list)
            {
                foreach (var p2 in p._header)
                {
                    DataType t = p2.Item1.DataType;
                    if (t.IsTuple)
                    {
                        t.IsList = false;
                        _AddType(all_types, t);
                    }
                }
            }

            //导出Tuple
            foreach (var p in all_types)
            {
                sw.WriteLine($"func {p.ToGoParseStr()}(v string) {p.ToGoStr()} {{");
                sw.WriteLine($"\ttemp:= strings.Split(v, \"{ConstDef.C_TUPLE_SPLIT}\")");
                sw.WriteLine("\tlen:= len(temp)");
                sw.WriteLine($"\tret:= {p.ToGoStr()}{{ }}");
                for (int i = 0; i < p.Count; i++)
                {
                    sw.WriteLine($"\tif len > {i} {{");
                    sw.WriteLine($"\t\tret.Item{i} = {p.Get(i).ToGoParseStr()}(temp[{i}])");
                    sw.WriteLine("\t}");
                }
                sw.WriteLine("\treturn ret\n}");
            }
        }

        private void _ExportListParaser(StreamWriter sw, List<FilterTable> table_list)
        {
            List<DataType> all_types = new List<DataType>();
            foreach (var p in table_list)
            {
                foreach (var p2 in p._header)
                {
                    DataType t = p2.Item1.DataType;
                    if (t.IsList)
                    {
                        _AddType(all_types, t);
                    }
                }
            }

            //导出List
            foreach (var p in all_types)
            {
                DataType t = p;
                t.IsList = false;
                sw.WriteLine($"func {p.ToGoParseStr()} (v string) {p.ToGoStr()}{{");
                sw.WriteLine($"\ttemp := strings.Split(v, \"{ConstDef.C_LIST_SPLIT}\")");
                sw.WriteLine("\tlen := len(temp)");
                sw.WriteLine($"\tret := make( {p.ToGoStr()}, len)");
                sw.WriteLine("\tfor i := 0; i < len; i++ {");
                sw.WriteLine($"\t\tret[i] = {t.ToGoParseStr()}(temp[i])");
                sw.WriteLine("\t}");
                sw.WriteLine("\treturn ret\n}");
            }
        }

        private static void _ExportBaseParser(StreamWriter sw)
        {
            string msg = @"                        
            
func parseBool(v string) bool {
	lowerStr := strings.ToLower(v)
	return lowerStr == ""1"" || lowerStr == ""true""
}

func parseInt32(v string) int32 {
	i, err := strconv.ParseInt(v, 10, 32)
	if err != nil {
		return 0
	}
	return int32(i)
}

func parseUint32(v string) uint32 {
	i, err := strconv.ParseUint(v, 10, 32)
	if err != nil {
		return 0
	}
	return uint32(i)
}

func parseUint64(v string) uint64 {
	i, err := strconv.ParseUint(v, 10, 64)
	if err != nil {
		return 0
	}
	return i
}

func parseInt64(v string) int64 {
	i, err := strconv.ParseInt(v, 10, 64)
	if err != nil {
		return 0
	}
	return i
}
func parseFloat32(v string) float32 {
	f, err := strconv.ParseFloat(v, 32)
	if err != nil {
		return 0
	}
	return float32(f)
}

func parseFloat64(v string) float64 {
	f, err := strconv.ParseFloat(v, 64)
	if err != nil {
		return 0
	}
	return f
}            
func parseString(v string) string {
	return v
}
";
            sw.WriteLine(msg);
        }


        private void _ExportLoadFunc(FilterTable table, StreamWriter sw)
        {
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _config.GetClassName(table.SheetName);

            _formater["col_count"] = table.ColCount.ToString();
            sw.WriteLineExt(_formater,
                @"
func (cd *CsvDataMgr) load{sheet_name}() error {
	//1. lock
	cd.{class_name}Mux.Lock()
	defer cd.{class_name}Mux.Unlock()
    file_name :={class_name}FileName

	//2. read file content	 
    all_rows, err := cd.reader.Read2Array(file_name)
	if err != nil {
		cd.logger.Error(""Read Csv Error "" + file_name)
		return err
    }
	if len(all_rows) <2 {
		cd.logger.Error(""csv data row count < 2 "" + file_name)
		return err
    }

    //3. check ids and types
    row_ids:= all_rows[0]
    row_types:= all_rows[1]
    if len(row_ids) != len(row_types) || len(row_ids) != {col_count} {
		cd.logger.Error(""csv data col count error"" + file_name)
        return err
    }
");

            List<TableField> header = table.GetHeader();
            _formater["col_count"] = header.Count.ToString();
            for (int i = 0; i < header.Count; i++)
            {
                TableField col = header[i];
                _formater["col_name"] = col.Name;
                _formater["col_idx"] = i.ToString();
                _formater["col_type"] = col.DataType.ToCsvStr();
                sw.WriteLineExt(_formater,
                    @"
    if row_ids[{col_idx}] != ""{col_name}"" || row_types[{col_idx}] != ""{col_type}"" {
        err:= errors.New(""Col fomrat error {col_idx} in "" + file_name)
        cd.logger.Error(err.Error())
        return err
    }");
            }



            sw.WriteLineExt(_formater,
                @"
    // 4.parse data to list
	row_count := len(all_rows)
	list_data := make([]{class_name}, row_count-2)
	for i := 0; i < row_count-2; i++ {
		row_data := all_rows[i+2]
		if len(row_data) != {col_count} {
			err := errors.New(""CSV  error "" + fmt.Sprint(i) + "" in "" + file_name)
            cd.logger.Error(err.Error())
            return err
        }
        row_struct := {class_name}{}
");

            for (int i = 0; i < header.Count; i++)
            {
                TableField c = header[i];

                if (c.AttrEnum != null)
                    sw.WriteLine("\t\trow_struct.{0}= {3}({2}(row_data[{1}]))", c.Name, i, c.DataType.ToGoParseStr(), c.AttrEnum.Name);
                else
                    sw.WriteLine("\t\trow_struct.{0}= {2}(row_data[{1}])", c.Name, i, c.DataType.ToGoParseStr());

            }

            sw.WriteLine(@"
        list_data[i] = row_struct
    }");

            TableField pk = table.PK;
            if (pk != null)
            {
                _formater["pk_type"] = pk.ToGoStr();
                _formater["pk_name"] = pk.Name;

                if (!pk.AttrPK.IsCompose())
                {
                    sw.WriteLineExt(_formater,
                  @"
    // 5. gen map data
    map_data := make(map[{pk_type}]*{class_name},len(list_data))
    data_count := len(list_data)
	for i:=0; i< data_count;i++{        
        map_id := list_data[i].{pk_name}
		_, exist := map_data[map_id]
		if exist {
			err := errors.New(""CSV  Mulit Key  "" + fmt.Sprint(map_id) + "" in "" + file_name)
            cd.logger.Error(err.Error())
            return err
        }
        map_data[map_id] = &list_data[i]
    }
");
                }

                else
                {
                    _formater["pk_sec_type"] = pk.AttrPK._sec_key.ToGoStr();
                    _formater["pk_sec_name"] = pk.AttrPK._sec_key.Name;
                    sw.WriteLineExt(_formater,
                 @"
    // 5. gen map data
    map_data := make(map[{pk_type}]map[{pk_sec_type}]*{class_name},len(list_data))
    data_count := len(list_data)
	for i:=0; i< data_count;i++{        
        map_id := list_data[i].{pk_name}
        sub_map_id :=list_data[i].{pk_sec_name}
        sub_map_data, exist := map_data[map_id]
		if !exist {
			sub_map_data = make(map[int32]*TTestComposeKey)
			map_data[map_id] = sub_map_data
		}

		_, exist = sub_map_data[sub_map_id]
		if exist {
			err := errors.New(""CSV  Mulit Key  "" + fmt.Sprint(map_id) + "":"" + fmt.Sprint(sub_map_id) + "" in "" + file_name)
			cd.logger.Error(err.Error())
			return err
		}
		sub_map_data[sub_map_id] = &list_data[i] 
    }
");
                }
            }

            sw.WriteLine("\t//6. assign to cd");
            if (pk != null)
            {
                sw.WriteLine("\tcd.{0}Map = map_data", _formater["class_name"]);
                sw.WriteLine("\tcd.FileName2MapData[file_name] = &map_data");
            }
            sw.WriteLine("\tcd.{0}List = list_data", _formater["class_name"]);
            sw.WriteLine("\tcd.FileName2ListData[file_name] = &list_data");
            sw.WriteLine("\treturn nil");
            sw.WriteLine("}");

        }


        private static void _AddType(List<DataType> list, DataType t)
        {
            bool found = false;
            foreach (var p in list)
            {
                if (p.IsEuqal(t))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                list.Add(t);
        }
    }
}

