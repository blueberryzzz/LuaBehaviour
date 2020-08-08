using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleManagerForLua
{
    static private AssetBundle mLuaAssetBundle;
    static public AssetBundle LuaAssetBundle
    {
        get
        {
            if(mLuaAssetBundle == null)
            {
                mLuaAssetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundle" + "/lua");
            }
            return mLuaAssetBundle;
        }
    }
}
