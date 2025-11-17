using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ScreenTools
{
    static string CreateAsset(string screenName)
    {
        string targetRelativePath = System.IO.Path.Combine("Resources/Screens", screenName + ".asset");
        string targetFullPath = SS.IO.File.Copy("ScreenTemplate.asset", targetRelativePath);

        if (targetFullPath == null)
        {
            return null;
        }

        SS.IO.File.ReplaceFileContent(targetFullPath, "ScreenTemplate", screenName);

        var assetPath = SS.IO.Path.GetRelativePathWithAssets(targetRelativePath);

        AssetDatabase.ImportAsset(assetPath);

        return assetPath;
    }

    static void SetupAsset(string assetPath, GameObject prefab)
    {
        var asset = AssetDatabase.LoadAssetAtPath<ScreenReference>(assetPath);

        if (asset != null)
        {
            asset.ScreenPrefab = prefab;

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        AssetDatabase.ImportAsset(assetPath);
    }
}