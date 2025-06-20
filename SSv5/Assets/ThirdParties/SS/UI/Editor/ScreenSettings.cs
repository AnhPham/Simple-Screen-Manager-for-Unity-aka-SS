/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
    public class ScreenSettings : EditorWindow
    {
        public string screenCanvasPath;
        public int screenWidth = 720;
        public int screenHeight = 1600;

        [MenuItem("SS/Screen Settings")]
        public static void ShowScreenSettings()
        {
            ScreenSettings win = ScriptableObject.CreateInstance<ScreenSettings>();

            win.minSize = new Vector2(400, 200);
            win.maxSize = new Vector2(400, 200);

            win.ShowUtility();

            win.LoadPrefs();
        }

        [MenuItem("SS/Resize Game Window")]
        public static void ResizeGameWindow()
        {
            var prefabInstance = PrefabUtility.LoadPrefabContents(SS.IO.Searcher.SearchFileInProject("ScreenCanvas.prefab", SS.IO.Searcher.PathType.Relative));

            var canvasScaler = prefabInstance.GetComponentInChildren<CanvasScaler>();
            if (canvasScaler != null)
            {
                var width = canvasScaler.referenceResolution.x;
                var height = canvasScaler.referenceResolution.y;

                SS.Tool.GameWindow.Resize((int)width, (int)height);
            }
        }

        void LoadPrefs()
        {
            screenWidth = EditorPrefs.GetInt("SS_SCREEN_WIDTH", 720);
            screenHeight = EditorPrefs.GetInt("SS_SCREEN_HEIGHT", 1600);
            screenCanvasPath = SS.IO.Searcher.SearchFileInProject("ScreenCanvas.prefab", SS.IO.Searcher.PathType.Relative);
        }

        void SavePrefs()
        {
            EditorPrefs.SetInt("SS_SCREEN_WIDTH", screenWidth);
            EditorPrefs.SetInt("SS_SCREEN_HEIGHT", screenHeight);
        }

        void OnGUI()
        {
            GUILayout.Label("Scene Generator", EditorStyles.boldLabel);
            screenWidth = EditorGUILayout.IntField("Screen Width", screenWidth);
            screenHeight = EditorGUILayout.IntField("Screen Height", screenHeight);

            if (GUILayout.Button("Save"))
            {
                if (screenWidth > 0 && screenHeight > 0)
                {
                    SavePrefs();
                    EditScreenCanvas();
                    SS.Tool.GameWindow.Resize(screenWidth, screenHeight);
                    Close();
                }
            }
        }

        void EditScreenCanvas()
        {
            string prefabPath = screenCanvasPath;

            var prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            var canvasScaler = prefabInstance.GetComponentInChildren<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(screenWidth, screenHeight);

            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);

            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }
    }
}