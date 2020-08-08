using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using XLua;

//---------------测试类型---------------
public enum Season
{
    spring = 0,
    summer = 1,
    autumn = 2,
    winter= 3,
}
[Serializable]
public class StringAndColor
{
    public string s;
    public Color c;
}
//---------------测试类型---------------

public class LuaBehaviour : MonoBehaviour
{
    public interface IObjWrap
    {
        object GetObj();
        void SetObj(object obj);
    }

    public class ObjWrap<T> : IObjWrap
    {
        public T obj;

        public object GetObj()
        {
            return obj;
        }

        public void SetObj(object obj)
        {
            this.obj = (T)obj;
        }
    }

    [Serializable]
    public class SerializedObjValue
    {
        public string key;
        public UnityEngine.Object value;
    }

    [Serializable]
    public class SerializedValue
    {
        public string key;
        public string jsonStr;
    }

    [HideInInspector]
    public string LuaScriptPath;
    [SerializeField]
    [HideInInspector]
    public List<SerializedObjValue> SerializedObjValues;
    [SerializeField]
    [HideInInspector]
    public List<SerializedValue> SerializedValues;

    public LuaTable luaInstance;
    public LuaTable LuaInstance
    {
        get
        {
            if(luaInstance == null)
            {
                InitLua();
            }
            return luaInstance;
        }
    }

    private Action<LuaTable> onEnableFunc;
    private Action<LuaTable> onDisableFunc;

    void InitLua()
    {
        luaInstance = LuaManager.LuaEnvInstance.NewTable();
        //注入自己
        luaInstance.Set("gameObject", gameObject);
        var rets = LuaManager.LuaEnvInstance.DoString($"return require \"{LuaScriptPath}\"");
        var luaClass = (LuaTable)rets[0];
        LuaTable defineTable;
        luaClass.Get("_DefineList", out defineTable);
        if (defineTable != null)
        {
            Dictionary<string, Type> infoDict = new Dictionary<string, Type>();
            string infoName;
            Type infoType;
            defineTable.ForEach<int, LuaTable>((index, infoTable) =>
            {
                infoTable.Get("name", out infoName);
                infoTable.Get("type", out infoType);
                infoDict.Add(infoName, infoType);
            });
            //注入object类型对象
            foreach (var serializedValue in SerializedObjValues)
            {
                //看了下set的代码，没有注册任何object类型的pushfunc,所以最后调用的都是pushany，
                //不用担心类型错误问题
                if (infoDict.ContainsKey(serializedValue.key) && serializedValue.value != null)
                {
                    luaInstance.Set(serializedValue.key, serializedValue.value);
                }
            }
            //注入其他类型
            foreach (var serializedValue in SerializedValues)
            {
                if (infoDict.ContainsKey(serializedValue.key))
                {
                    luaInstance.Set(serializedValue.key, JsonToValue(serializedValue.jsonStr, infoDict[serializedValue.key]));
                }
            }
        }
        var newWith = luaClass.Get<Action<LuaTable, LuaTable>>("newWith");
        newWith(luaClass, luaInstance);
        this.onEnableFunc = luaClass.Get<Action<LuaTable>>("OnEnable");
        this.onDisableFunc = luaClass.Get<Action<LuaTable>>("OnDisable");
    }

    void Awake()
    {
        if(luaInstance == null)
        {
            InitLua();
        }        
    }

    void OnEnable()
    {
        onEnableFunc?.Invoke(LuaInstance);      
    }

    void OnDisable()
    {
        onDisableFunc?.Invoke(LuaInstance);
    }

    void OnDestroy()
    {
        onEnableFunc = null;
        onDisableFunc = null;
        var onDestroyFunc = LuaInstance.Get<Action<LuaTable>>("OnDestroy");
        onDestroyFunc?.Invoke(LuaInstance);
        luaInstance = null;
    }

    public static string ValueToJson(object value)
    {
        Type type = value.GetType();
        Type wrapType = typeof(ObjWrap<>).MakeGenericType(type);
        var target = Activator.CreateInstance(wrapType);
        ((IObjWrap)target).SetObj(value);
        return JsonUtility.ToJson(target);
    }

    public static object JsonToValue(string json, Type type)
    {
        if(wrapTypeDict.ContainsKey(type))
        {
            Type wrapType = wrapTypeDict[type];
            var target = JsonUtility.FromJson(json, wrapType);
            return ((IObjWrap)target).GetObj();
        }
        else if(type.IsValueType())
        {
            //Debug.LogError($"msut register {type.FullName} in wrapTypeDict!");
            Debug.LogError($"msut register {type.FullName} in wrapTypeDict, because it is value type!");
            return null;
        }
        else
        {
            Type wrapType = typeof(ObjWrap<>).MakeGenericType(type);
            wrapTypeDict.Add(type, wrapType);
            var target = JsonUtility.FromJson(json, wrapType);
            return ((IObjWrap)target).GetObj();
        }
    }

    //因为值类型泛型的生成在IL2CPP下会有JIT异常，所以值类型必须要注册
    private static Dictionary<Type, Type> wrapTypeDict = new Dictionary<Type, Type>()
    {
        {typeof(Int32), typeof(ObjWrap<Int32>)},
        {typeof(Int64), typeof(ObjWrap<Int64>)},
        {typeof(Single), typeof(ObjWrap<Single>)},
        {typeof(Double), typeof(ObjWrap<Double>)},
        {typeof(Boolean), typeof(ObjWrap<Boolean>)},
        {typeof(String), typeof(ObjWrap<String>)},
        {typeof(Vector2), typeof(ObjWrap<Vector2>)},
        {typeof(Vector3), typeof(ObjWrap<Vector3>)},
        {typeof(Vector4), typeof(ObjWrap<Vector4>)},
        {typeof(Color), typeof(ObjWrap<Color>)},
        {typeof(Rect), typeof(ObjWrap<Rect>)},
        {typeof(Bounds), typeof(ObjWrap<Bounds>)},

        {typeof(List<int>), typeof(ObjWrap<List<int>>)},
        {typeof(List<string>), typeof(ObjWrap<List<string>>)},
        {typeof(List<Vector3>), typeof(ObjWrap<List<Vector3>>)},
        {typeof(List<Color>), typeof(ObjWrap<List<Color>>)},

        //测试类型
        //{typeof(List<StringAndColor>), typeof(ObjWrap<List<StringAndColor>>)},
        {typeof(Season), typeof(ObjWrap<Season>)},
    };
}
