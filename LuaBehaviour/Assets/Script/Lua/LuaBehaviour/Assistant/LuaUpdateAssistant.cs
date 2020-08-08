using System;
using XLua;

public class LuaUpdateAssistant : LuaAssistantBase
{
    private Action<LuaTable> updateFunc;

    protected override void Awake()
    {
        base.Awake();
        updateFunc = luaBehaviour.LuaInstance.Get<Action<LuaTable>>("Update");
    }

    void Update()
    {
        if(luaBehaviour.LuaInstance != null)
        {
            updateFunc?.Invoke(luaBehaviour.LuaInstance);
        }        
    }

    void OnDestroy()
    {
        updateFunc = null;
    }
}
