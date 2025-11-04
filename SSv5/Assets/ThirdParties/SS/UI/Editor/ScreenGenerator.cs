/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEditor;
using UnityEngine;
using System;

namespace SS.UI
{
    public class ScreenGenerator : EditorWindow
    {
        enum State
        {
            IDLE,
            GENERATING,
            COMPILING,
            COMPILING_AGAIN
        }

        public string screenName;
        public string screenDirectoryPath;
        public string screenResourcePath;
        public string screenTemplateFile;
        public bool addScreenRefToResources;

        string screenPath;
        string prefabPath;
        string assetPath;
        string controllerPath;
        State state = State.IDLE;

        [MenuItem("SS/Screen Generator")]
        public static void ShowWindow()
        {
            ScreenGenerator win = ScriptableObject.CreateInstance<ScreenGenerator>();

            win.minSize = new Vector2(400, 200);
            win.maxSize = new Vector2(400, 200);

            win.ResetParams();
            win.ShowUtility();

            win.LoadPrefs();
        }

        void ResetParams()
        {
            screenName = string.Empty;
        }

        void LoadPrefs()
        {
            screenDirectoryPath = EditorPrefs.GetString("SS_SCREEN_DIRECTORY_PATH", "Project/Screens/");
            screenResourcePath = EditorPrefs.GetString("SS_SCREEN_RESOURCE_PATH", "Resources/Screens/");
            screenTemplateFile = EditorPrefs.GetString("SS_SCREEN_TEMPLATE_FILE", "ScreenTemplate.prefab");
            addScreenRefToResources = EditorPrefs.GetBool("SS_SCREEN_REF_RESOURCES", true);
        }

        void SavePrefs()
        {
            EditorPrefs.SetString("SS_SCREEN_DIRECTORY_PATH", screenDirectoryPath);
            EditorPrefs.SetString("SS_SCREEN_RESOURCE_PATH", screenResourcePath);
            EditorPrefs.SetString("SS_SCREEN_TEMPLATE_FILE", screenTemplateFile);
            EditorPrefs.SetBool("SS_SCREEN_REF_RESOURCES", addScreenRefToResources);
        }

        void OnGUI()
        {
            GUILayout.Label("Screen Generator", EditorStyles.boldLabel);
            screenName = EditorGUILayout.TextField("Screen Name", screenName);
            screenDirectoryPath = EditorGUILayout.TextField("Screen Directory Path", screenDirectoryPath);
            screenResourcePath = EditorGUILayout.TextField("Screen Resource Path", screenResourcePath);
            screenTemplateFile = EditorGUILayout.TextField("Screen Template File", screenTemplateFile);
            addScreenRefToResources = EditorGUILayout.Toggle("Add screen ref to Resources", addScreenRefToResources);

            switch (state)
            {
                case State.IDLE:
                    if (GUILayout.Button("Generate"))
                    {
                        if (GenerateScene())
                        {
                            state = State.GENERATING;
                        }
                    }
                    break;
                case State.GENERATING:
                    if (EditorApplication.isCompiling)
                    {
                        state = State.COMPILING;
                    }
                    break;
                case State.COMPILING:
                    if (EditorApplication.isCompiling)
                    {
                        EditorApplication.delayCall += () => {
                            EditorUtility.DisplayProgressBar("Compiling Scripts", "Wait for a few seconds...", 0.33f);
                        };
                    }
                    else
                    {
                        EditorUtility.ClearProgressBar();
                        SetupPrefab();

                        if (addScreenRefToResources)
                        {
                            CreateAsset();
                            SetupAsset();
                        }
                        
                        state = State.COMPILING_AGAIN;
                    }
                    break;
                case State.COMPILING_AGAIN:
                    if (EditorApplication.isCompiling)
                    {
                        EditorApplication.delayCall += () => {
                            EditorUtility.DisplayProgressBar("Compiling Scripts", "Wait for a few seconds...", 0.66f);
                        };
                    }
                    else
                    {
                        state = State.IDLE;
                        EditorUtility.ClearProgressBar();
                        SetupScene();
                        SaveScene();
                        EditorApplication.delayCall += () => {
                            EditorUtility.DisplayDialog("Successful!", "Screen was generated.", "OK");
                        };

                    }
                    break;
            }
        }

        bool GenerateScene()
        {
            if (string.IsNullOrEmpty(screenName))
            {
                Debug.LogWarning("You have to input an unique name to 'Screen Name'");
                return false;
            }

            string targetRelativePath = System.IO.Path.Combine(screenDirectoryPath, screenName + "/" + screenName + ".unity");
            string targetFullPath = SS.IO.Path.GetAbsolutePath(targetRelativePath);

            if (System.IO.File.Exists(targetFullPath))
            {
                Debug.LogWarning("This screen is already exist!");
                return false;
            }

            if (string.IsNullOrEmpty(screenTemplateFile))
            {
                Debug.LogWarning("You have to input screen template file!");
                return false;
            }

            SavePrefs();
            if (!CreatePrefab())
            {
                Debug.LogWarning("Screen template file is not exist!");
                return false;
            }
            CreateScene();
            CreateController();
            return true;
        }

        bool CreatePrefab()
        {
            string targetRelativePath = System.IO.Path.Combine(screenDirectoryPath, screenName + "/" + screenName + ".prefab");
            string targetFullPath = SS.IO.File.Copy(screenTemplateFile, targetRelativePath);

            if (targetFullPath == null)
            {
                return false;
            }

            prefabPath = SS.IO.Path.GetRelativePathWithAssets(targetRelativePath);

            AssetDatabase.ImportAsset(prefabPath);

            return true;
        }

        void SetupPrefab()
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefab != null)
            {
                var type = GetAssemblyType(screenName + "Controller");

                prefab.AddComponent(type);

                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

                PrefabUtility.UnloadPrefabContents(prefab);
            }

            AssetDatabase.ImportAsset(prefabPath);
        }

        void CreateController()
        {
            string targetRelativePath = System.IO.Path.Combine(screenDirectoryPath, screenName + "/" + screenName + "Controller.cs");
            string targetFullPath = SS.IO.File.Copy("ScreenTemplateController.cs", targetRelativePath);

            SS.IO.File.ReplaceFileContent(targetFullPath, "ScreenTemplate", screenName);

            controllerPath = SS.IO.Path.GetRelativePathWithAssets(targetRelativePath);

            AssetDatabase.ImportAsset(controllerPath);
        }

        void CreateScene()
        {
            string targetRelativePath = System.IO.Path.Combine(screenDirectoryPath, screenName + "/" + screenName + ".unity");
            string targetFullPath = SS.IO.File.Copy("ScreenTemplate.unity", targetRelativePath);

            screenPath = SS.IO.Path.GetRelativePathWithAssets(targetRelativePath);

            AssetDatabase.ImportAsset(screenPath);

            SS.Tool.Scene.OpenScene(targetFullPath);
        }

        void SetupScene()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                PrefabUtility.InstantiatePrefab(prefab, FindObjectOfType<Canvas>().transform);
            }
        }

        void SaveScene()
        {
            SS.Tool.Scene.MarkCurrentSceneDirty();
            SS.Tool.Scene.SaveScene();
        }

        Type GetAssemblyType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        bool CreateAsset()
        {
            string targetRelativePath = System.IO.Path.Combine(screenResourcePath, screenName + ".asset");
            string targetFullPath = SS.IO.File.Copy("ScreenTemplate.asset", targetRelativePath);

            if (targetFullPath == null)
            {
                return false;
            }

            SS.IO.File.ReplaceFileContent(targetFullPath, "ScreenTemplate", screenName);

            assetPath = SS.IO.Path.GetRelativePathWithAssets(targetRelativePath);

            AssetDatabase.ImportAsset(assetPath);

            return true;
        }

        void SetupAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScreenReference>(assetPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (asset != null)
            {
                asset.ScreenPrefab = prefab;

                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.ImportAsset(assetPath);
        }
    }
}