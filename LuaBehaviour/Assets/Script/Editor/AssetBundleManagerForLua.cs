using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleManagerForLua
{
    const string luaBundlePath = "Assets/LuaBundle";
    static public string assetBundlePath
    {
        get
        {
            return Application.streamingAssetsPath + "/AssetBundle";
        }
    }
    [MenuItem("AssetBundles/Build AssetBundle For Lua")]
    static public void BuildAssetBundleForLua()
    {
        //复制到LuaBundle目录下，准备打Bundle
        if (Directory.Exists(luaBundlePath))
        {
            FileUtil.DeleteFileOrDirectory(luaBundlePath);
        }
        FileUtil.CopyFileOrDirectory(LuaManager.LuaPath, luaBundlePath);
        string[] filePaths = Directory.GetFiles(luaBundlePath, "*", SearchOption.AllDirectories);
        foreach(var filePath in filePaths)
        {
            if (filePath.IndexOf(".meta") == -1)
            {
                File.Move(filePath, filePath + ".bytes");
            }
        }
        AssetDatabase.Refresh();
        //设置bundle名
        var importer = AssetImporter.GetAtPath(luaBundlePath);
        importer.assetBundleName = "lua";
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = importer.assetBundleName;
        build.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(importer.assetBundleName);
        //Build AssetBundle
        if (Directory.Exists(assetBundlePath))
        {
            FileUtil.DeleteFileOrDirectory(assetBundlePath);
        }
        Directory.CreateDirectory(assetBundlePath);
        BuildPipeline.BuildAssetBundles(
            assetBundlePath, 
            new AssetBundleBuild[] { build}, 
            BuildAssetBundleOptions.None, 
            EditorUserBuildSettings.activeBuildTarget
        );
        AssetDatabase.Refresh();
    }
}
