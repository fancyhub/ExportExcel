_G.Log = {
    I=function(msg)
        print("Info",msg)
    end,

    E=function (msg)
        print("Error",msg)
    end,
}
require "table.LuaTableMgr"

LuaTableMgr.BaseDir = "../TestData/0_no_loc/Output/Client/Data";
LuaTableMgr.BaseDir = "../TestData/1_loc/Output/Client/Data";
LuaTableMgr.BaseDir = "../TestData/2_loc_auto_key/Output/Client/Data";

local itemDataList= LuaTableMgr.GetTItemDataList()
local item = LuaTableMgr.GetTItemData(1)

print("hello")



