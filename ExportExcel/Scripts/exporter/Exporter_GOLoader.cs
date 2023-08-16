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
    public class ExporterGOLoader : I_ProcessNode
    {
        public const string C_FILE_NAME = "go_loader.go";
        public StringFormater _formater = new StringFormater();
        public ExporterGOLoader()
        {
        }
        public string GetName()
        {
            return "Export";
        }
        public void Process(DataBase data)
        {
            if (string.IsNullOrEmpty(data.Config.go.export_dir_svr))
                return;

            _formater["class_prefix"] = data.Config.go.class_prefix;
            string package_name = data.Config.go.package_name;
            string dest_file_path = System.IO.Path.Combine(data.Config.go.export_dir_svr, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            List<FilterTable> tables = FilterTable.Filter(data, E_EXPORT_FLAG.svr);
            StreamWriter sw = new StreamWriter(dest_file_path);
            sw.WriteLine("package " + package_name);
            sw.WriteLine(@"
import (	
    ""errors""
	""fmt""
	""strconv""
	""strings""
	""go.uber.org/zap""
)
const (	
"
);
            foreach (FilterTable t in tables)
            {
                _formater["class_name"] = _formater["class_prefix"] + t.SheetName;
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

func CreateCsvDataMgr(logger *zap.Logger,reader IDataReader) (*CsvDataMgr, error) {
    cd := CsvDataMgr{
        logger:     logger,
        reader:     reader,
        FileName2Func: make(map[string]csvLoader, {table_count}),
        FileName2ListData: make(map[string]interface{}, {table_count}),
        FileName2MapData: make(map[string]interface{}, {table_count}),
");

            foreach (var t in tables)
            {
                _formater["class_name"] = _formater["class_prefix"] + t.SheetName;
                sw.WriteLineExt(_formater, "\t\t{class_name}List: make([]{class_name}, 0),");
                var pk = t.PK;
                if (t.PK != null)
                {
                    _formater["pk_type"] = pk.DataType.ToGoStr();
                    if (t.PK.AttrPK.IsCompose())
                    {
                        _formater["pk_sec_type"] = pk.AttrPK._sec_key.DataType.ToGoStr();
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
                _formater["class_name"] = _formater["class_prefix"] + t.SheetName;
                _formater["sheet_name"] = t.SheetName;
                sw.WriteLineExt(_formater, "\tcd.FileName2Func[{class_name}FileName] = cd.load{sheet_name}");
            }

            sw.WriteLine(@"
	return &cd, nil
}
");

            _export_base_parse(sw);
            _export_parse(sw, GetAllDateTypes(tables));

            foreach (FilterTable t in tables)
            {
                _export_load_func(t, sw);
            }
            sw.Close();
        }

        public void _export_parse(StreamWriter sw, List<DataType> type_list)
        {
            //导出Pair
            foreach (var p in type_list)
            {
                DataType t = p;
                if (!t.IsPair)
                    continue;
                if (t.IsList)
                    continue;

                sw.WriteLine($"func {p.ToGoParseStr()}(v string) {p.ToGoStr()} {{");
                sw.WriteLine($"\ttemp:= strings.Split(v, \"{ConstDef.C_PAIR_SPLIT}\")");
                sw.WriteLine("\tlen:= len(temp)");
                sw.WriteLine($"\tret:= {p.ToGoStr()}{{ }}");
                for (int i = 0; i < t.Count; i++)
                {
                    sw.WriteLine($"\tif len > {i} {{");
                    sw.WriteLine($"\t\tret.Item{i} = {t.Get(i).ToGoParseStr()}(temp[{i}])");
                    sw.WriteLine("\t}");
                }
                sw.WriteLine("\treturn ret\n}");
            }

            //导出List
            foreach (var p in type_list)
            {
                DataType t = p;
                if (!t.IsList)
                    continue;
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

        public void _export_base_parse(StreamWriter sw)
        {
            string msg = @"                        
            
func parse_bool(v string) bool {
	lowerStr := strings.ToLower(v)
	return lowerStr == ""1"" || lowerStr == ""true""
}

func parse_int32(v string) int32 {
	i, err := strconv.ParseInt(v, 10, 32)
	if err != nil {
		return 0
	}
	return int32(i)
}

func parse_uint32(v string) uint32 {
	i, err := strconv.ParseUint(v, 10, 32)
	if err != nil {
		return 0
	}
	return uint32(i)
}

func parse_uint64(v string) uint64 {
	i, err := strconv.ParseUint(v, 10, 64)
	if err != nil {
		return 0
	}
	return i
}

func parse_int64(v string) int64 {
	i, err := strconv.ParseInt(v, 10, 64)
	if err != nil {
		return 0
	}
	return i
}
func parse_float32(v string) float32 {
	f, err := strconv.ParseFloat(v, 32)
	if err != nil {
		return 0
	}
	return float32(f)
}

func parse_float64(v string) float64 {
	f, err := strconv.ParseFloat(v, 64)
	if err != nil {
		return 0
	}
	return f
}            
func parse_string(v string) string {
	return v
}
";
            sw.WriteLine(msg);
        }

        public void _export_load_func(FilterTable table, StreamWriter sw)
        {
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _formater["class_prefix"] + table.SheetName;

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
		cd.logger.Debug(""Read Csv Error "" + file_name)
		return err
    }
	if len(all_rows) <2 {
		cd.logger.Debug(""csv data row count < 2 "" + file_name)
		return err
    }

    //3. check ids and types
    row_ids:= all_rows[0]
    row_types:= all_rows[1]
    if len(row_ids) != len(row_types) || len(row_ids) != {col_count} {
		cd.logger.Debug(""csv data col count error"" + file_name)
        return err
    }
");

            List<TableHeaderItem> header = table.Header;
            _formater["col_count"] = header.Count.ToString();
            for (int i = 0; i < header.Count; i++)
            {
                TableHeaderItem col = header[i];
                _formater["col_name"] = col.Name;
                _formater["col_idx"] = i.ToString();
                _formater["col_type"] = col.DataType.ToCsvStr();
                sw.WriteLineExt(_formater,
                    @"
    if row_ids[{col_idx}] != ""{col_name}"" || row_types[{col_idx}] != ""{col_type}"" {
        err:= errors.New(""Col fomrat error {col_idx} in "" + file_name)
        cd.logger.Debug(err.Error())
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
            cd.logger.Debug(err.Error())
            return err
        }
        row_struct := {class_name}{}
");

            for (int i = 0; i < header.Count; i++)
            {
                TableHeaderItem c = header[i];
                if (c.DataType.enum_type != null)
                    sw.WriteLine("\t\trow_struct.{0}= {3}({2}(row_data[{1}]))", c.Name, i, c.DataType.ToGoParseStr(), c.DataType.enum_type.Name);
                else
                    sw.WriteLine("\t\trow_struct.{0}= {2}(row_data[{1}])", c.Name, i, c.DataType.ToGoParseStr());

            }

            sw.WriteLine(@"
        list_data[i] = row_struct
    }");

            TableHeaderItem pk = table.PK;
            if (pk != null)
            {
                _formater["pk_type"] = pk.DataType.ToGoStr();
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
            cd.logger.Debug(err.Error())
            return err
        }
        map_data[map_id] = &list_data[i]
    }
");
                }

                else
                {
                    _formater["pk_sec_type"] = pk.AttrPK._sec_key.DataType.ToGoStr();
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
			cd.logger.Debug(err.Error())
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


        public static List<DataType> GetAllDateTypes(List<FilterTable> tables)
        {
            List<DataType> ret = new List<DataType>();

            foreach (var p in tables)
            {
                foreach (var p2 in p._header)
                {
                    DataType t = p2.Item1.DataType;
                    if (!t.IsPair && !t.IsList)
                        continue;

                    t.IsList = false;
                    _AddType(ret, p2.Item1.DataType);
                    _AddType(ret, t);

                }
            }
            return ret;
        }


        public static void _AddType(List<DataType> list, DataType t)
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

