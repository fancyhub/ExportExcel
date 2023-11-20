
-- 此文件由工具自动生成，请勿手动修改
local this = LuaTableMgr
this._name_2_list = {}
this._name_2_map = {}



local function _LoadItemData()
    local sheet_name = "ItemData"
    
    local csv_reader = this.CreateCsvReader(sheet_name)
    if csv_reader == nil then 
        return
    end
    local csv_line_reader = csv_reader:lines()

    local first_line = csv_line_reader()
    if first_line == nil or #first_line ~= 8 then
        log("加载 表格失败, 格式不对 " .. sheet_name)
        return
    end
    local sec_line = csv_line_reader()
    if sec_line == nil or #sec_line ~= 8 then
        log("加载 表格失败, 格式不对 " .. sheet_name)
        return
    end

    local list_data = { }
    local map_data = { }
    for line in csv_line_reader do
        local data = { }
		data.Id= TableUtil.Parse_int32(line[1])
		data.Name= TableUtil.Parse_locid(line[2])
		data.Type= TableUtil.Parse_int32(line[3])
		data.SubType= TableUtil.Parse_int32(line[4])
		data.Quality= TableUtil.Parse_int32(line[5])
		data.PairField= TableUtil.Parse_int32_bool(line[6])
		data.PairFieldList= TableUtil.Parse_list_int32_int64(line[7])
		data.ListField= TableUtil.Parse_list_int32(line[8])
		table.insert(list_data, data)
		map_data[data.Id] = data
	end
	return list_data,map_data
end


local function _LoadTestComposeKey()
    local sheet_name = "TestComposeKey"
    
    local csv_reader = this.CreateCsvReader(sheet_name)
    if csv_reader == nil then 
        return
    end
    local csv_line_reader = csv_reader:lines()

    local first_line = csv_line_reader()
    if first_line == nil or #first_line ~= 4 then
        log("加载 表格失败, 格式不对 " .. sheet_name)
        return
    end
    local sec_line = csv_line_reader()
    if sec_line == nil or #sec_line ~= 4 then
        log("加载 表格失败, 格式不对 " .. sheet_name)
        return
    end

    local list_data = { }
    local map_data = { }
    for line in csv_line_reader do
        local data = { }
		data.Id= TableUtil.Parse_uint32(line[1])
		data.Level= TableUtil.Parse_int32(line[2])
		data.Name= TableUtil.Parse_locid(line[3])
		data.Pos= TableUtil.Parse_float32_float32(line[4])
		table.insert(list_data, data)
		map_data[(data.Id<<32) |data.Level ] = data
	end
	return list_data,map_data
end

local name_2_loader = {}
this._name_2_loader = name_2_loader
name_2_loader["TItemData"]= _LoadItemData
name_2_loader["TTestComposeKey"]= _LoadTestComposeKey
name_2_loader["TLoc"]= _LoadLoc
