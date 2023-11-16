local LuaTableMgr = {}
_G.LuaTableMgr = LuaTableMgr

csv = require "table.util.csv"
require "table.util.TableUtil"
require "table.gen.Lua_Struct"
require "table.gen.Lua_Loader"
require "table.gen.LocDef"
require "table.util.string_ext"

local tables_name_2_list = {}
local tables_name_2_map = {}
local this = LuaTableMgr

function LuaTableMgr.Get(table_name, key)
    local list, dict = this.LoadTable(table_name, false)
    if dict == nil then
        return nil
    end

    local val = dict[key]
    if val == nil then
        Log.E(table_name .. " at key " .. tostring(key) .. " is nil!!!")
    end

    return val
end

function LuaTableMgr.Get2(table_name, key, key2)
    local list, dict = this.LoadTable(table_name, false)
    if dict == nil then
        return nil
    end

    local val = dict[(key << 32) | key2]
    if val == nil then
        Log.E(string.format("%s, at key %d,%d is nil", table_name, key, key2))
    end

    return val
end

function LuaTableMgr.LoadTable(table_name, reload)
    if reload then
        tables_name_2_list[table_name] = nil
        tables_name_2_map[table_name] = nil
    end

    list_data = tables_name_2_list[table_name]
    map_data = tables_name_2_map[table_name]

    if list_data == nil then
        local loader = this._name_2_loader[table_name]
        if loader == nil then
            return nil
        end

        list_data, map_data = loader()
        tables_name_2_list[table_name] = list_data
        tables_name_2_map[table_name] = map_data
    end
    return list_data, map_data
end

function LuaTableMgr.CreateCsvReader(table_name)
    local txt = CSharp.Read(table_name)
    if txt == nil then
        return nil
    end
    local reader = csv.openstring(txt)
    return reader
end

function LuaTableMgr.CreateCsvReader(table_name)
    local path = this.BaseDir .. "/" .. table_name .. ".csv"
    local reader = csv.open(path)    
    return reader
end

