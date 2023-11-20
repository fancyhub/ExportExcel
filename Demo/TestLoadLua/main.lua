_G.Log = {
    I=function(msg)
        print("Info",msg)
    end,

    E=function (msg)
        print("Error",msg)
    end,
}
require "table.LuaTableMgr"

LuaTableMgr.BaseDir = "../Data/0_no_loc/Output/Client/Data";
LuaTableMgr.BaseDir = "../Data/1_loc/Output/Client/Data";
LuaTableMgr.BaseDir = "../Data/2_loc_auto_key/Output/Client/Data";

local itemDataList= LuaTableMgr.GetTItemDataList()
local item = LuaTableMgr.GetTItemData(1)

print("hello")



