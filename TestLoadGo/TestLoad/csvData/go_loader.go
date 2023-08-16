package csvData

import (
	"errors"
	"fmt"
	"strconv"
	"strings"

	"go.uber.org/zap"
)

const (
	TItemDataFileName       = "ItemData.csv"
	TTestComposeKeyFileName = "TestComposeKey.csv"
)

func (p *CsvDataMgr) LoadAll() {
	for _, loader := range p.FileName2Func {
		loader()
	}
}

func CreateCsvDataMgr(logger *zap.Logger, reader IDataReader) (*CsvDataMgr, error) {
	cd := CsvDataMgr{
		logger:            logger,
		reader:            reader,
		FileName2Func:     make(map[string]csvLoader, 2),
		FileName2ListData: make(map[string]interface{}, 2),
		FileName2MapData:  make(map[string]interface{}, 2),

		TItemDataList:       make([]TItemData, 0),
		TItemDataMap:        make(map[int32]*TItemData, 0),
		TTestComposeKeyList: make([]TTestComposeKey, 0),
		TTestComposeKeyMap:  make(map[uint32]map[int32]*TTestComposeKey, 0),
	}
	cd.FileName2Func[TItemDataFileName] = cd.loadItemData
	cd.FileName2Func[TTestComposeKeyFileName] = cd.loadTestComposeKey

	return &cd, nil
}

func parse_bool(v string) bool {
	lowerStr := strings.ToLower(v)
	return lowerStr == "1" || lowerStr == "true"
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

func parse_int32_bool(v string) CsvPair_int32_bool {
	temp := strings.Split(v, "|")
	len := len(temp)
	ret := CsvPair_int32_bool{}
	if len > 0 {
		ret.Item0 = parse_int32(temp[0])
	}
	if len > 1 {
		ret.Item1 = parse_bool(temp[1])
	}
	return ret
}
func parse_int32_int64(v string) CsvPair_int32_int64 {
	temp := strings.Split(v, "|")
	len := len(temp)
	ret := CsvPair_int32_int64{}
	if len > 0 {
		ret.Item0 = parse_int32(temp[0])
	}
	if len > 1 {
		ret.Item1 = parse_int64(temp[1])
	}
	return ret
}
func parse_float32_float32_float32(v string) CsvPair_float32_float32_float32 {
	temp := strings.Split(v, "|")
	len := len(temp)
	ret := CsvPair_float32_float32_float32{}
	if len > 0 {
		ret.Item0 = parse_float32(temp[0])
	}
	if len > 1 {
		ret.Item1 = parse_float32(temp[1])
	}
	if len > 2 {
		ret.Item2 = parse_float32(temp[2])
	}
	return ret
}
func parse_array_int32_int64(v string) []CsvPair_int32_int64 {
	temp := strings.Split(v, ";")
	len := len(temp)
	ret := make([]CsvPair_int32_int64, len)
	for i := 0; i < len; i++ {
		ret[i] = parse_int32_int64(temp[i])
	}
	return ret
}
func parse_array_int32(v string) []int32 {
	temp := strings.Split(v, ";")
	len := len(temp)
	ret := make([]int32, len)
	for i := 0; i < len; i++ {
		ret[i] = parse_int32(temp[i])
	}
	return ret
}

func (cd *CsvDataMgr) loadItemData() error {
	//1. lock
	cd.TItemDataMux.Lock()
	defer cd.TItemDataMux.Unlock()
	file_name := TItemDataFileName

	//2. read file content
	all_rows, err := cd.reader.Read2Array(file_name)
	if err != nil {
		cd.logger.Debug("Read Csv Error " + file_name)
		return err
	}
	if len(all_rows) < 2 {
		cd.logger.Debug("csv data row count < 2 " + file_name)
		return err
	}

	//3. check ids and types
	row_ids := all_rows[0]
	row_types := all_rows[1]
	if len(row_ids) != len(row_types) || len(row_ids) != 7 {
		cd.logger.Debug("csv data col count error" + file_name)
		return err
	}

	if row_ids[0] != "Id" || row_types[0] != "int" {
		err := errors.New("Col fomrat error 0 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	if row_ids[1] != "Type" || row_types[1] != "int" {
		err := errors.New("Col fomrat error 1 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	if row_ids[2] != "SubType" || row_types[2] != "int" {
		err := errors.New("Col fomrat error 2 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	if row_ids[3] != "Quality" || row_types[3] != "int" {
		err := errors.New("Col fomrat error 3 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	if row_ids[4] != "PairField" || row_types[4] != "int_bool" {
		err := errors.New("Col fomrat error 4 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	if row_ids[5] != "PairFieldList" || row_types[5] != "list_int_int64" {
		err := errors.New("Col fomrat error 5 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	if row_ids[6] != "ListField" || row_types[6] != "list_int" {
		err := errors.New("Col fomrat error 6 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	// 4.parse data to list
	row_count := len(all_rows)
	list_data := make([]TItemData, row_count-2)
	for i := 0; i < row_count-2; i++ {
		row_data := all_rows[i+2]
		if len(row_data) != 7 {
			err := errors.New("CSV  error " + fmt.Sprint(i) + " in " + file_name)
			cd.logger.Debug(err.Error())
			return err
		}
		row_struct := TItemData{}

		row_struct.Id = parse_int32(row_data[0])
		row_struct.Type = EItemType(parse_int32(row_data[1]))
		row_struct.SubType = EItemSubType(parse_int32(row_data[2]))
		row_struct.Quality = EItemQuality(parse_int32(row_data[3]))
		row_struct.PairField = parse_int32_bool(row_data[4])
		row_struct.PairFieldList = parse_array_int32_int64(row_data[5])
		row_struct.ListField = parse_array_int32(row_data[6])

		list_data[i] = row_struct
	}

	// 5. gen map data
	map_data := make(map[int32]*TItemData, len(list_data))
	data_count := len(list_data)
	for i := 0; i < data_count; i++ {
		map_id := list_data[i].Id
		_, exist := map_data[map_id]
		if exist {
			err := errors.New("CSV  Mulit Key  " + fmt.Sprint(map_id) + " in " + file_name)
			cd.logger.Debug(err.Error())
			return err
		}
		map_data[map_id] = &list_data[i]
	}

	//6. assign to cd
	cd.TItemDataMap = map_data
	cd.FileName2MapData[file_name] = &map_data
	cd.TItemDataList = list_data
	cd.FileName2ListData[file_name] = &list_data
	return nil
}

func (cd *CsvDataMgr) loadTestComposeKey() error {
	//1. lock
	cd.TTestComposeKeyMux.Lock()
	defer cd.TTestComposeKeyMux.Unlock()
	file_name := TTestComposeKeyFileName

	//2. read file content
	all_rows, err := cd.reader.Read2Array(file_name)
	if err != nil {
		cd.logger.Debug("Read Csv Error " + file_name)
		return err
	}
	if len(all_rows) < 2 {
		cd.logger.Debug("csv data row count < 2 " + file_name)
		return err
	}

	//3. check ids and types
	row_ids := all_rows[0]
	row_types := all_rows[1]
	if len(row_ids) != len(row_types) || len(row_ids) != 4 {
		cd.logger.Debug("csv data col count error" + file_name)
		return err
	}

	if row_ids[0] != "Id" || row_types[0] != "uint" {
		err := errors.New("Col fomrat error 0 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	if row_ids[1] != "Level" || row_types[1] != "int" {
		err := errors.New("Col fomrat error 1 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	if row_ids[2] != "Name" || row_types[2] != "locid" {
		err := errors.New("Col fomrat error 2 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	if row_ids[3] != "Pos" || row_types[3] != "float_float_float" {
		err := errors.New("Col fomrat error 3 in " + file_name)
		cd.logger.Debug(err.Error())
		return err
	}

	// 4.parse data to list
	row_count := len(all_rows)
	list_data := make([]TTestComposeKey, row_count-2)
	for i := 0; i < row_count-2; i++ {
		row_data := all_rows[i+2]
		if len(row_data) != 4 {
			err := errors.New("CSV  error " + fmt.Sprint(i) + " in " + file_name)
			cd.logger.Debug(err.Error())
			return err
		}
		row_struct := TTestComposeKey{}

		row_struct.Id = parse_uint32(row_data[0])
		row_struct.Level = parse_int32(row_data[1])
		row_struct.Name = parse_int32(row_data[2])
		row_struct.Pos = parse_float32_float32_float32(row_data[3])

		list_data[i] = row_struct
	}

	// 5. gen map data
	map_data := make(map[uint32]map[int32]*TTestComposeKey, len(list_data))
	data_count := len(list_data)
	for i := 0; i < data_count; i++ {
		map_id := list_data[i].Id
		sub_map_id := list_data[i].Level
		sub_map_data, exist := map_data[map_id]
		if !exist {
			sub_map_data = make(map[int32]*TTestComposeKey)
			map_data[map_id] = sub_map_data
		}

		_, exist = sub_map_data[sub_map_id]
		if exist {
			err := errors.New("CSV  Mulit Key  " + fmt.Sprint(map_id) + ":" + fmt.Sprint(sub_map_id) + " in " + file_name)
			cd.logger.Debug(err.Error())
			return err
		}
		sub_map_data[sub_map_id] = &list_data[i]
	}

	//6. assign to cd
	cd.TTestComposeKeyMap = map_data
	cd.FileName2MapData[file_name] = &map_data
	cd.TTestComposeKeyList = list_data
	cd.FileName2ListData[file_name] = &list_data
	return nil
}
