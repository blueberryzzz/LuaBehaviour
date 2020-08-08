using System;
using XLua;
using UnityEngine;

public class LuaFixedUpdateAssistant : LuaAssistantBase
{
    private Action<LuaTable> fixedUpdateFunc;

    protected override void Awake()
    {
        base.Awake();
        fixedUpdateFunc = luaBehaviour.LuaInstance.Get<Action<LuaTable>>("FixedUpdate");
    }

    void FixedUpdate()
    {
        if (luaBehaviour.LuaInstance != null)
        {
            fixedUpdateFunc?.Invoke(luaBehaviour.LuaInstance);
        }
    }

    void OnDestroy()
    {
        fixedUpdateFunc = null;
    }
}
