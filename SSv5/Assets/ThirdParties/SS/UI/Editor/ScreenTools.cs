using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if ADDRESSABLE
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

public static class ScreenTools
{
    private const string DefaultLocalGroupName = "Default Local Group";

#if ADDRESSABLE
    [MenuItem("SS/Add All Screen Refs to Resources")]
    public static void AddAllScreenRefsToResources()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            return;
        }

        var group = settings.FindGroup(DefaultLocalGroupName);

        if (group == null)
        {
            group = settings.DefaultGroup;
            if (group == null)
            {
                return;
            }
        }

        var entries = new List<AddressableAssetEntry>(group.entries);

        var prefabs = new List<GameObject>();

        foreach (var entry in entries)
        {
            if (entry == null) continue;

            var path = AssetDatabase.GUIDToAssetPath(entry.guid);
            if (string.IsNullOrEmpty(path)) continue;

            if (path.EndsWith(".prefab"))
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null) prefabs.Add(go);

                var assetPath = CreateAsset(entry.address);
                SetupAsset(assetPath, go);
            }
        }
    }
#endif

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