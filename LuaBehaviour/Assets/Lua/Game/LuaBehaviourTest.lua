local class = require "Core.Middleclass"
local LuaBehaviour = require "Core.LuaBehaviour"

local LuaBehaviourTest = class("LuaBehaviourTest", LuaBehaviour)

--------------------------需要序列化的数据--------------------------
LuaBehaviourTest:AddDefineList({
    {name = "colorValue", type = CS.UnityEngine.Color}, 
    {name = "textCmp", type = CS.UnityEngine.UI.Text}, 
    {name = "rectCmp", type = CS.UnityEngine.RectTransform},
    {name = "stringColorList", type = CS.System.Collections.Generic.List(typeof(CS.StringAndColor))},
    {name = "int32Value", type = CS.System.Int32},
    {name = "DoubleValue", type = CS.System.Double},
    {name = "boolValue", type = CS.System.Boolean},
    {name = "vector3Value", type = CS.UnityEngine.Vector3}, 
    {name = "season", type = CS.Season}, 
})
--在编辑器下只加载DefineList,不管其他部分
if ExecuteInEditorScript then return LuaBehaviourTest end
--------------------------需要序列化的数据--------------------------

function LuaBehaviourTest:initialize()
    --LuaBehaviour的构造函数
    LuaBehaviour.initialize(self)
    --输出注入的对象
    for key, value in pairs(self) do
        print("key : "..key..", value : "..tostring(value))
    end

    self.updateTextTime = CS.UnityEngine.Time.time
    self.stringIndex = 0
    self.textCmp.text = self.stringColorList[0].s
    self.textCmp.color = self.stringColorList[0].c
end

function LuaBehaviourTest:OnEnable()
    print("OnEnable")
end

function LuaBehaviourTest:Update()
    if CS.UnityEngine.Time.time - self.updateTextTime > 3 then
        self.updateTextTime = CS.UnityEngine.Time.time
        self.stringIndex = (self.stringIndex + 1) % self.stringColorList.Count
        self.textCmp.text = self.stringColorList[self.stringIndex].s
        self.textCmp.color = self.stringColorList[self.stringIndex].c
    end
end
--[[
function LuaBehaviourTest:FixedUpdate()
    print("FixedUpdate")
end

function LuaBehaviourTest:LateUpdate()
    print("LateUpdate")
end
]]--

function LuaBehaviourTest:OnPointerClick(eventData)
    print("click pos : "..tostring(eventData.position))
end

function LuaBehaviourTest:OnDisable()
    print("OnDisable")
end

function LuaBehaviourTest:OnDestroy()
    print("OnDestroy")
end

return LuaBehaviourTest
