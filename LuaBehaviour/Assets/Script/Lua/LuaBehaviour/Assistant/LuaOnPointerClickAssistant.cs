using System;
using XLua;
using UnityEngine.EventSystems;

public class LuaOnPointerClickAssistant : LuaAssistantBase, IPointerClickHandler
{
    private Action<LuaTable, PointerEventData> onPointerClickFunc;

    protected override void Awake()
    {
        base.Awake();
        onPointerClickFunc = luaBehaviour.LuaInstance.Get<Action<LuaTable, PointerEventData>>("OnPointerClick");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (luaBehaviour.LuaInstance != null)
        {
            onPointerClickFunc?.Invoke(luaBehaviour.LuaInstance, eventData);
        }
    }

    void OnDestroy()
    {
        onPointerClickFunc = null;
    }
}
