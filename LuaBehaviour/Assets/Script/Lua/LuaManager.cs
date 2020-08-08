using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using XLua;

public class LuaManager
{
    public const string LuaPath = "Assets/Lua/";
    //通过路径加载lua文件
    public static byte[] BaseLoader(ref string fileName)
    {
        if (!fileName.EndsWith(".lua", StringComparison.Ordinal))
        {
            fileName = fileName.Replace('.', '/');
            fileName += ".lua";
        }
        else
        {
            fileName = fileName.Replace('.', '/').Replace("/lua", ".lua");
        }
#if UNITY_EDITOR
        var path = Path.Combine(LuaPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogErrorFormat("Load lua file failed: {0}, file is not existed.", path);
            return null;
        }
        return File.ReadAllBytes(path);
#else
        var path = Path.Combine("Assets/LuaBundle/", fileName);
        path += ".bytes";
        var assetBundle = AssetBundleManagerForLua.LuaAssetBundle;        
        var luaAsset = assetBundle.LoadAsset<TextAsset>(path);
        return luaAsset.bytes;
#endif
    }

    private static LuaEnv mLuaEnvInstance;
    public static LuaEnv LuaEnvInstance
    {
        get
        {
            if(mLuaEnvInstance != null)
            {
                return mLuaEnvInstance;
            }
            mLuaEnvInstance = new LuaEnv();
            mLuaEnvInstance.AddLoader(BaseLoader);
            return mLuaEnvInstance;
        }
    }
}
