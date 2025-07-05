/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEditor;
using System.Reflection;
using System;

namespace SS.Tool
{
    public class GameWindow
    {
        #region NATIVE_GAME_VIEW
        static object s_GameViewSizes_instance;

        static Type s_GameViewType;
        static MethodInfo s_GameView_SizeSelectionCallback;

        static Type s_GameViewSizesType;
        static MethodInfo s_GameViewSizes_GetGroup;

        static Type s_GameViewSizeSingleType;

        public static void Resize(int screenWidth, int screenHeight)
        {
            var sizeToCapture = string.Format("{0}x{1}", screenWidth, screenHeight);
            if (FindSize(GetCurrentGroupType(), sizeToCapture) == -1)
            {
                AddCustomSize(GameViewSizeType.FixedResolution, GetCurrentGroupType(), screenWidth, screenHeight, sizeToCapture);
            }

            TrySetSize(sizeToCapture, GetCurrentGroupType());
        }

        static GameWindow()
        {
            s_GameViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            s_GameView_SizeSelectionCallback = s_GameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // gameViewSizesInstance  = ScriptableSingleton<GameViewSizes>.instance;
            s_GameViewSizesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            s_GameViewSizeSingleType = typeof(ScriptableSingleton<>).MakeGenericType(s_GameViewSizesType);
            s_GameViewSizes_GetGroup = s_GameViewSizesType.GetMethod("GetGroup");

            var instanceProp = s_GameViewSizeSingleType.GetProperty("instance");
            s_GameViewSizes_instance = instanceProp.GetValue(null, null);
        }

        /// <summary>
        /// Try to find and set game view size to specified query.
        /// Size must be already exists in game view setting.
        /// You must send the right game view (your current platform) in order to get the right result.
        /// </summary>
        /// <param name="sizeText">Query string such as 1280x720 or 16:9</param>
        /// <param name="currentView">The game view group that is related to your current platform. Like GameViewSizeGroupType.Android</param>
        static bool TrySetSize(string sizeText, GameViewSizeGroupType currentView = GameViewSizeGroupType.Standalone)
        {
            int foundIndex = FindSize(currentView, sizeText);
            if (foundIndex < 0)
            {
                UnityEngine.Debug.LogError($"Size {sizeText} was not found in game view settings");
                return false;
            }

            SetSizeIndex(foundIndex);
            return true;
        }

        /// <summary>
        /// Set current gameview size to target resolution index.
        /// Index must be known beforehand.
        /// </summary>
        static void SetSizeIndex(int index)
        {
            // Calling GameView.SizeSelectionCallback will also auto focus game view,
            // We will restore focus if it is something else
            EditorWindow currentWindow = EditorWindow.focusedWindow;
            SceneView lastSceneView = SceneView.lastActiveSceneView;

            EditorWindow gv = EditorWindow.GetWindow(s_GameViewType);
            s_GameView_SizeSelectionCallback.Invoke(gv, new object[] { index, null });

            // Hack, will mock re-active scene view, in case it was active,
            // Because EditorWindow.focusedWindow could now be inspector
            // If scene view and game view were in same docking group,
            // SizeSelectionCallback will switch to game view without knowing if user left scene view visible or not.
            // - If last active was actually game view, it should be corrected by currentWindow.Focus, no problem
            // - If last active is something else, like console for inspector, this will bring up scene view, should be no harm.
            // Remove this out if you do not want this behavior
            if (lastSceneView != null)
                lastSceneView.Focus();

            if (currentWindow != null)
                currentWindow.Focus();
        }

        /// <summary>
        /// Finding text could be fixed resoluation as WxH "1280x720"
        /// or ratio like W:H "16:9"
        /// </summary>
        static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
        {
            var group = GetGroup(sizeGroupType); // class GameViewSizeGroup
            var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
            var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
            for (int i = 0; i < displayTexts.Length; i++)
            {
                string display = displayTexts[i];

                bool found = display.Contains(text);
                if (found)
                    return i;
            }
            return -1;
        }

        static object GetGroup(GameViewSizeGroupType type)
        {
            return s_GameViewSizes_GetGroup.Invoke(s_GameViewSizes_instance, new object[] { (int)type });
        }

        enum GameViewSizeType
        {
            AspectRatio, FixedResolution
        }

        static GameViewSizeGroupType GetCurrentGroupType()
        {
#if UNITY_STANDALONE
            return GameViewSizeGroupType.Standalone;
#elif UNITY_IOS
            return GameViewSizeGroupType.iOS;
#elif UNITY_ANDROID
            return GameViewSizeGroupType.Android;
#endif
            // Add your own
        }

        static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
        {
            var group = GetGroup(sizeGroupType);
            var addCustomSize = s_GameViewSizes_GetGroup.ReturnType.GetMethod("AddCustomSize"); // or group.GetType().
            var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            string assemblyName = "UnityEditor.dll";
            Assembly assembly = Assembly.Load(assemblyName);
            Type gameViewSize = assembly.GetType("UnityEditor.GameViewSize");
            Type gameViewSizeType = assembly.GetType("UnityEditor.GameViewSizeType");
            ConstructorInfo ctor = gameViewSize.GetConstructor(new Type[]
                {
                 gameViewSizeType,
                 typeof(int),
                 typeof(int),
                 typeof(string)
                });
            var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
            addCustomSize.Invoke(group, new object[] { newSize });
        }
        #endregion
    }
}
