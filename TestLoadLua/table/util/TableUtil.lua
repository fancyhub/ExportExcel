local Log=Log
TableUtil = {}

local this = TableUtil

local SPLITTER1 = ';'
local SPLITTER2 = ':'

local cacheSplitResult1 = {}
local cacheSplitResult2 = {}

local AllParser = {}

-- endregion
AllParser["int32"] = function(s)
    return tonumber(s) or 0
end

AllParser["int64"] = function(s)
    return tonumber(s) or 0
end

AllParser["float32"] = function(s)
    return tonumber(s) or 0
end

AllParser["locstr"] = function(s)
    return s
    -- return LocStrGet(s)
end

AllParser["locid"] = function(s)
    return tonumber(s) or 0
end

AllParser["float64"] = function(s)
    return tonumber(s) or 0
end

AllParser["bool"] = function(s)
    if s == nil or string.len(s) == 0 or s == '0' then
        return false
    end
    return true
end

AllParser["string"] = function(s)
    return s or ''
end

local tupleItemNames={"Item1","Item2","Item3","Item4","Item5","Item6","Item7"}

local function _CreateTupleFunc(types)
    local count = #types 
    if count == 0 then 
        return nil 
    elseif count==1 then 
        return AllParser[types[1]]
    else
        local func_list ={}        
        for i=1,count do
            local tempFunc = AllParser[types[i]]
            if tempFunc == nil then 
                Log.E("Can not find func "..types[i])
                return nil
            end
            table.insert(func_list,tempFunc)
        end

        return function(s)
            local ret ={}
            local ss = string.split(s,'|')
            for i=1,#ss do 
                ret[tupleItemNames[i]] = func_list[i](ss[i])
            end
            return ret
        end
    end
end

local function _CreateListFunc(types)
    local itemFunc = _CreateTupleFunc(types)
    
    return function(s)
        local ret ={}
        local ss = string.split(s,';')
        for i=1,#ss do 
            local item = itemFunc(ss[i])
            table.insert(ret,item)            
        end
        return ret
    end
end
 
local TableUtilMeta = {
    __index = function(t, func_name)
        local func = AllParser[func_name]
        if func ~= nil then
            return func
        end
    
        local array = string.split(func_name, '_')
        local count = #array
        if count < 2 or array[1] ~= "Parse" then
            Log.E("Can not find func " .. func_name)
            return nil  
        elseif array[2] == "list" then
            table.remove(array,1)
            table.remove(array,1)
    
            func = _CreateListFunc(array)
            AllParser[func_name] = func
        else 
            table.remove(array,1)
            func = _CreateTupleFunc(array)
            AllParser[func_name] = func
        end
    
        if func == nil then 
            Log.E("Can not find func " .. func_name)
        end
        return func
    end
}
setmetatable(TableUtil, TableUtilMeta)
 