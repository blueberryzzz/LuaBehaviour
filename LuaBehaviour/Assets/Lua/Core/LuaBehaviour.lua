local class = require "Core.middleClass"
local LuaBehaviour = class("LuaBehaviour")

local typeDict = {}
setmetatable(typeDict, { __index = function(t, k)
    t[k] = typeof(k)
    return t[k]
end})
function LuaBehaviour:AddDefineList(defineList)
    self._DefineList = self._DefineList or {}
    for key, value in pairs(defineList) do
        table.insert(self._DefineList, {name = value.name, type = typeDict[value.type]})
    end
end
--在编辑器下只加载DefineList,不管其他部分
if ExecuteInEditorScript then return LuaBehaviour end

local AssistantDict = {
    ["Update"] = typeof(CS.LuaUpdateAssistant),
    ["FixedUpdate"] = typeof(CS.LuaFixedUpdateAssistant),
    ["LateUpdate"] = typeof(CS.LuaLateUpdateAssistant),
    ["OnPointerClick"] = typeof(CS.LuaOnPointerClickAssistant)
}
function LuaBehaviour:initialize()
    for funcName, assistant in pairs(AssistantDict) do
        if self[funcName] then
            self.gameObject:AddComponent(assistant)
        end
    end
end

return LuaBehaviour
