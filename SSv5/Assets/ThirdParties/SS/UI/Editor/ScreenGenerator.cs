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

        public string sceneName;
        public string sceneDirectoryPath;
        public string sceneTemplateFile;

        string scenePath;
        string prefabPath;
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
            sceneName = string.Empty;
        }

        void LoadPrefs()
        {
            sceneDirectoryPath = EditorPrefs.GetString("SS_SCREEN_SCENE_DIRECTORY_PATH", "Project/Screens/");
            sceneTemplateFile = EditorPrefs.GetString("SS_SCREEN_SCENE_TEMPLATE_FILE", "ScreenTemplate.prefab");
        }

        void SavePrefs()
        {
            EditorPrefs.SetString("SS_SCREEN_SCENE_DIRECTORY_PATH", sceneDirectoryPath);
            EditorPrefs.SetString("SS_SCREEN_SCENE_TEMPLATE_FILE", sceneTemplateFile);
        }

        void OnGUI()
        {
            GUILayout.Label("Scene Generator", EditorStyles.boldLabel);
            sceneName = EditorGUILayout.TextField("Screen Name", sceneName);
            sceneDirectoryPath = EditorGUILayout.TextField("Screen Directory Path", sceneDirectoryPath);
            sceneTemplateFile = EditorGUILayout.TextField("Screen Template File", sceneTemplateFile);

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
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("You have to input an unique name to 'Screen Name'");
                return false;
            }

            string targetRelativePath = System.IO.Path.Combine(sceneDirectoryPath, sceneName + "/" + sceneName + ".unity");
            string targetFullPath = SS.IO.Path.GetAbsolutePath(targetRelativePath);

            if (System.IO.File.Exists(targetFullPath))
            {
                Debug.LogWarning("This screen is already exist!");
                return false;
            }

            if (string.IsNullOrEmpty(sceneTemplateFile))
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
            string targetRelativePath = System.IO.Path.Combine(sceneDirectoryPath, sceneName + "/" + sceneName + ".prefab");
            string targetFullPath = SS.IO.File.Copy(sceneTemplateFile, targetRelativePath);

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
                var type = GetAssemblyType(sceneName + "Controller");

                prefab.AddComponent(type);

                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

                PrefabUtility.UnloadPrefabContents(prefab);
            }

            AssetDatabase.ImportAsset(prefabPath);
        }

        void CreateController()
        {
            string targetRelativePath = System.IO.Path.Combine(sceneDirectoryPath, sceneName + "/" + sceneName + "Controller.cs");
            string targetFullPath = SS.IO.File.Copy("ScreenTemplateController.cs", targetRelativePath);

            SS.IO.File.ReplaceFileContent(targetFullPath, "ScreenTemplate", sceneName);

            controllerPath = SS.IO.Path.GetRelativePathWithAssets(targetRelativePath);

            AssetDatabase.ImportAsset(controllerPath);
        }

        void CreateScene()
        {
            string targetRelativePath = System.IO.Path.Combine(sceneDirectoryPath, sceneName + "/" + sceneName + ".unity");
            string targetFullPath = SS.IO.File.Copy("ScreenTemplate.unity", targetRelativePath);

            scenePath = SS.IO.Path.GetRelativePathWithAssets(targetRelativePath);

            AssetDatabase.ImportAsset(scenePath);

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
    }
}