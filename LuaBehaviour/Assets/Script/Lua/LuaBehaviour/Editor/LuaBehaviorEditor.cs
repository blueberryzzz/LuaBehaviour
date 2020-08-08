using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEditor;
using XLua;

[CustomEditor(typeof(LuaBehaviour), true)]
[CanEditMultipleObjects]
public class LuaBehaviourEditor : Editor
{
    public class SerializedInfo
    {
        public string ValueName;
        public Type ValueType;
    }
    
    static Dictionary<Type, Type> objWrapEditorDict = new Dictionary<Type, Type>();
    static ModuleBuilder editorModule;
    static LuaBehaviourEditor()
    {
        AppDomain myDomain = Thread.GetDomain();
        AssemblyName myAsmName = new AssemblyName();
        myAsmName.Name = "LuaBehaviourEditor";
        AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly(myAsmName,AssemblyBuilderAccess.RunAndSave);
        editorModule = myAsmBuilder.DefineDynamicModule("LuaBehaviourEditorModule","LuaBehaviourEditor.dll");
    }

    static Type GetWrapType(Type objType)
    {
        if (objWrapEditorDict.ContainsKey(objType))
        {
            return objWrapEditorDict[objType];
        }
        TypeBuilder wrapTypeBld = editorModule.DefineType("wrap" + objType.FullName, TypeAttributes.Public, typeof(ScriptableObject));
        FieldBuilder objField = wrapTypeBld.DefineField("obj", objType, FieldAttributes.Public);
        Type wrapType = wrapTypeBld.CreateType();
        objWrapEditorDict.Add(objType, wrapType);
        return wrapType;
    }

    public DefaultAsset LuaScript
    {
        get
        {
            return AssetDatabase.LoadAssetAtPath(LuaManager.LuaPath + m_LuaScriptPath.stringValue, typeof(DefaultAsset)) as DefaultAsset;
        }
        set
        {
            string path = AssetDatabase.GetAssetPath(value);
            path = path.Replace(LuaManager.LuaPath, "");
            if((path.EndsWith(".lua") || value == null) && m_LuaScriptPath.stringValue != path)
            {
                m_LuaScriptPath.stringValue = path;
                serializedObject.ApplyModifiedProperties();
                lastWriteTime = 0;
                infoList = new List<SerializedInfo>();
                mLuaBehaviour.SerializedObjValues = new List<LuaBehaviour.SerializedObjValue>();
                mLuaBehaviour.SerializedValues = new List<LuaBehaviour.SerializedValue>();
                serializedObject.Update();
            }            
        }
    }

    private SerializedProperty m_LuaScriptPath;
    private LuaBehaviour mLuaBehaviour;
    private List<SerializedInfo> infoList;
    private long lastWriteTime = 0;

    protected void OnEnable()
    {
        m_LuaScriptPath = serializedObject.FindProperty("LuaScriptPath");
        mLuaBehaviour = target as LuaBehaviour;
        lastWriteTime = 0;
        infoList = new List<SerializedInfo>();
    }
    
    //如果Lua文件改变了，则需要重新初始化一遍需要序列化的信息。同时保留已经序列化的数据。
    private void ReloadLua()
    {
        if (File.Exists(LuaManager.LuaPath + m_LuaScriptPath.stringValue))
        {
            long curTime = File.GetLastWriteTime(LuaManager.LuaPath + m_LuaScriptPath.stringValue).Ticks;
            if (curTime != lastWriteTime)
            {
                lastWriteTime = curTime;
                infoList = GetInfoList();
                var preObjValues = mLuaBehaviour.SerializedObjValues;
                var preValues = mLuaBehaviour.SerializedValues;
                mLuaBehaviour.SerializedObjValues = new List<LuaBehaviour.SerializedObjValue>();
                mLuaBehaviour.SerializedValues = new List<LuaBehaviour.SerializedValue>();
                foreach (var info in infoList)
                {
                    if(info.ValueType.IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        mLuaBehaviour.SerializedObjValues.Add(
                            new LuaBehaviour.SerializedObjValue() {
                                key = info.ValueName,
                                value = GetObjValueInLuaBehavior(info, preObjValues)
                            });
                    }
                    else
                    {
                        mLuaBehaviour.SerializedValues.Add(
                            new LuaBehaviour.SerializedValue() {
                                key = info.ValueName,
                                jsonStr = GetValueInLuaBehavior(info, preValues)
                            });
                    }
                }
            }
        }
    }    

    private UnityEngine.Object GetObjValueInLuaBehavior(SerializedInfo info, List<LuaBehaviour.SerializedObjValue> values)
    {
        foreach(var value in values)
        {
            if(value.key == info.ValueName)
            {
                return value.value;
            }
        }
        return null;
    }

    private string GetValueInLuaBehavior(SerializedInfo info, List<LuaBehaviour.SerializedValue> values)
    {
        foreach (var value in values)
        {
            if (value.key == info.ValueName)
            {
                var obj1 = LuaBehaviour.JsonToValue(value.jsonStr, info.ValueType);
                return LuaBehaviour.ValueToJson(obj1);
            }
        }
        Type wrapType = typeof(LuaBehaviour.ObjWrap<>).MakeGenericType(info.ValueType);
        var target = Activator.CreateInstance(wrapType);
        return JsonUtility.ToJson(target);
    }

    //运行时和编辑时获取的方式不一样，运行时直接从实例中取即可，编辑时需要起一个虚拟机来加载序列化信息。
    private List<SerializedInfo> GetInfoList()
    {      
        List<SerializedInfo> infoList = new List<SerializedInfo>();
        LuaEnv luaEnv = null;
        LuaTable luaClass = null;
        LuaTable defineTable;
        if (!Application.isPlaying)
        {
            luaEnv = new LuaEnv();
            luaEnv.AddLoader(LuaManager.BaseLoader);
            luaEnv.DoString("ExecuteInEditorScript = true");
            var rets = luaEnv.DoString($"return require \"{m_LuaScriptPath.stringValue}\"");
            luaClass = (LuaTable)rets[0];
        }
        else
        {
            var rets = LuaManager.LuaEnvInstance.DoString($"return require \"{m_LuaScriptPath.stringValue}\"");
            luaClass = (LuaTable)rets[0];
        }
        luaClass.Get("_DefineList", out defineTable);
        defineTable.ForEach<int,LuaTable>((index, infoTable)=> {
            SerializedInfo info = new SerializedInfo();
            infoTable.Get("name",out info.ValueName);
            infoTable.Get("type", out info.ValueType);
            infoList.Add(info);
        });
        if (!Application.isPlaying)
        {
            luaEnv.Dispose();
        }
        return infoList;
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        //绘制Lua路径
        LuaScript = EditorGUILayout.ObjectField("Lua Script", LuaScript, typeof(DefaultAsset), true) as DefaultAsset;
        ReloadLua();
        //绘制所有需要注入的对象
        foreach (var info in infoList)
        {
            if (info.ValueType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                DrawObjValueView(info);
            }
            else
            {
                DrawValueView(info);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    //UnityEngine.Object都一个样，所以直接绘制即可
    private void DrawObjValueView(SerializedInfo info)
    {
        for (int i = 0; i < mLuaBehaviour.SerializedObjValues.Count; i++)
        {
            if(info.ValueName == mLuaBehaviour.SerializedObjValues[i].key)
            {
                mLuaBehaviour.SerializedObjValues[i].value =
                    EditorGUILayout.ObjectField(
                        mLuaBehaviour.SerializedObjValues[i].key,
                        mLuaBehaviour.SerializedObjValues[i].value,
                        info.ValueType,
                        true
                    );
            }
        }
    }

    //感觉分类型去绘制有点麻烦，所以用ScriptableObject包装一下，通过SerializedObject让unity自己绘制。
    private void DrawValueView(SerializedInfo info)
    {
        for(int i = 0; i < mLuaBehaviour.SerializedValues.Count; i++) { 
            if (info.ValueName == mLuaBehaviour.SerializedValues[i].key)
            {
                var editorType = GetWrapType(info.ValueType);
                var objField = editorType.GetField("obj");
                var target = CreateInstance(editorType);
                var value = LuaBehaviour.JsonToValue(mLuaBehaviour.SerializedValues[i].jsonStr, info.ValueType);
                objField.SetValue(target, value);
                var so = new SerializedObject(target);
                EditorGUILayout.PropertyField(so.FindProperty("obj"), new GUIContent(info.ValueName));
                so.ApplyModifiedProperties();
                mLuaBehaviour.SerializedValues[i].jsonStr = LuaBehaviour.ValueToJson(objField.GetValue(target));
            }
        }
    }
}
