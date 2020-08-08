local class = require "Core.Middleclass"
local LuaBehaviour = require "Core.LuaBehaviour"

local LuaBehaviourTest = class("LuaBehaviourTest", LuaBehaviour)

--------------------------需要序列化的数据--------------------------
LuaBehaviourTest:AddDefineList({
    {name = "Age", type = CS.System.Int32},
    {name = "Name", type = CS.System.String},
})
--在编辑器下只加载DefineList,不管其他部分
if ExecuteInEditorScript then return LuaBehaviourTest end
--------------------------需要序列化的数据--------------------------

function LuaBehaviourTest:initialize()
end

function LuaBehaviourTest:Awake()

end

function LuaBehaviourTest:OnEnable()

end

function LuaBehaviourTest:OnDisable()

end

return LuaBehaviourTest
