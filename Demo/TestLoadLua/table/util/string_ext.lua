local string = string
local table = table

---@param s string
---@param sep string
---@return table<string>
string.split = function(s, sep)
    local fields = {}

    local sep = sep or " "
    local pattern = string.format("([^%s]+)", sep)
    string.gsub(
        s,
        pattern,
        function(c)
            fields[#fields + 1] = c
        end
    )
    return fields
end

---@param s string
---@param old_val string
---@param new_val string
---@return string
string.replace = function(s, old_val, new_val)
    local temp = {}
    local start_idx = 1
    while true do
        local i, j = string.find(s, old_val, start_idx, true)
        if i == nil then
            table.insert(temp, string.sub(s, start_idx))
            break
        end
        table.insert(temp, string.sub(s, start_idx, i - 1))
        table.insert(temp, new_val)
        start_idx = j + 1
    end
    return table.concat(temp)
end


string.start_with = function(s,start_val)
    return string.find(s,start_val)==1
end
 
---@param s string
---@param str_trim_chars string | nil @nil 的时候, 去除 (空格/制表)
---@return string
string.trim_start = function(s, str_trim_chars)
    local p_start = "^[\t ]*"
    if str_trim_chars ~= nil then
        p_start = string.format("^[%s]*", str_trim_chars)
    end
    local r = s:gsub(p_start, "")
    return r
end

---@param s string
---@param str_trim_chars string | nil @nil 的时候, 去除 (空格/制表)
---@return string
string.trim_end = function(s, str_trim_chars)
    local p_end = "[\t ]*$"
    if str_trim_chars ~= nil then
        p_end = string.format("[%s]*$", str_trim_chars)
    end

    local r = s:gsub(p_end, "")
    return r
end

---@param s string
---@param str_trim_chars string | nil @nil 的时候, 去除 (空格/制表)
---@return string
string.trim = function(s, str_trim_chars)
    return s:trim_start(str_trim_chars):trim_end(str_trim_chars)
end
