-- 此文件由工具自动生成，请勿手动修改
EItemType = {
	-- 无
	None = 0,
	-- 武器
	Weapon = 1,
	-- 消耗品
	Cosume = 2,
}
EItemSubType = {
	-- 无
	None = 0,
	-- 手枪
	ShotGun = 1,
	-- 加农炮
	Cannon = 2,
}
EItemQuality = {
	-- 普通
	None = 0,
	-- 灰色
	Gray = 2,
	-- 绿色
	Green = 3,
	-- 紫色
	Purple = 4,
}
EItemFlag = {
	-- 无
	None = 0,
	-- 可堆叠
	Stack = 1,
	-- 可删除
	CanDelete = 2,
}
local LuaTableMgr = LuaTableMgr

---@return table<number,TItemData>
function LuaTableMgr.GetTItemDataList()
    local list_data, map_data = LuaTableMgr.LoadTable("TItemData")
    return list_data
end

---@return TItemData
function LuaTableMgr.GetTItemData(Id)
    local data = LuaTableMgr.Get("TItemData", Id)
    return data
end

---@return table<int32,TItemData>
function LuaTableMgr.GetTItemDataDict()
    local list_data, map_data = LuaTableMgr.LoadTable("TItemData")
    return map_data
end

---@return table<number,TTestComposeKey>
function LuaTableMgr.GetTTestComposeKeyList()
    local list_data, map_data = LuaTableMgr.LoadTable("TTestComposeKey")
    return list_data
end

---@return TTestComposeKey
function LuaTableMgr.GetTTestComposeKey(Id,Level)
    local data = LuaTableMgr.Get2("TTestComposeKey", Id,Level)
    return data
end

---@return table<long,TTestComposeKey>
function LuaTableMgr.GetTTestComposeKeyDict()
    local list_data, map_data = LuaTableMgr.LoadTable("TTestComposeKey")
    return map_data
end
