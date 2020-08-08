using System;
using XLua;

public class LuaLateUpdateAssistant : LuaAssistantBase
{
    private Action<LuaTable> lateUpdateFunc;

    protected override void Awake()
    {
        base.Awake();
        lateUpdateFunc = luaBehaviour.LuaInstance.Get<Action<LuaTable>>("LateUpdate");
    }

    void LateUpdate()
    {
        if (luaBehaviour.LuaInstance != null)
        {
            lateUpdateFunc?.Invoke(luaBehaviour.LuaInstance);
        }
    }

    void OnDestroy()
    {
        lateUpdateFunc = null;
    }
}
