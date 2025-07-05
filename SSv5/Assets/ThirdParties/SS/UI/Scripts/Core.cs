/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using static SS.UI.SceneManager;

namespace SS.UI
{
    public class Core
    {
        private static GeneralManager s_generalManager;
        private static ScreenManager s_screenManager;
        private static SceneManager s_sceneManager;
        private static ShieldManager s_shieldManager;
        private static TooltipManager s_tooltipManager;
        private static LoadingManager s_loadingManager;

        private static bool s_initialized = false;

        #region Public Static
        /// <summary>
        /// Init this system using default managers or customized managers. Call this init once when your game starts, before any other calls from the Core class.
        /// </summary>
        /// <param name="generalManagerPath"></param>
        /// <param name="screenManagerPath"></param>
        /// <param name="sceneManagerPath"></param>
        /// <param name="shieldManagerPath"></param>
        /// <param name="tooltipManagerPath"></param>
        /// <param name="loadingManagerPath"></param>
        public static void Init(string generalManagerPath = "Prefabs/GeneralManager", string screenManagerPath = "Prefabs/ScreenManager", string sceneManagerPath = "Prefabs/SceneManager", string shieldManagerPath = "Prefabs/ShieldManager", string tooltipManagerPath = "Prefabs/TooltipManager", string loadingManagerPath = "Prefabs/LoadingManager")
        {
            if (s_initialized)
                return;

            s_initialized = true;

            s_generalManager = Object.FindObjectOfType<SS.UI.GeneralManager>();
            if (s_generalManager == null)
            {
                s_generalManager = Object.Instantiate(Resources.Load<SS.UI.GeneralManager>(generalManagerPath));
            }

            s_screenManager = Object.FindObjectOfType<SS.UI.ScreenManager>();
            if (s_screenManager == null)
            {
                s_screenManager = Object.Instantiate(Resources.Load<SS.UI.ScreenManager>(screenManagerPath));
            }

            s_sceneManager = Object.FindObjectOfType<SS.UI.SceneManager>();
            if (s_sceneManager == null)
            {
                s_sceneManager = Object.Instantiate(Resources.Load<SS.UI.SceneManager>(sceneManagerPath));
            }

            s_shieldManager = Object.FindObjectOfType<SS.UI.ShieldManager>();
            if (s_shieldManager == null)
            {
                s_shieldManager = Object.Instantiate(Resources.Load<SS.UI.ShieldManager>(shieldManagerPath));
            }

            s_tooltipManager = Object.FindObjectOfType<SS.UI.TooltipManager>();
            if (s_tooltipManager == null)
            {
                s_tooltipManager = Object.Instantiate(Resources.Load<SS.UI.TooltipManager>(tooltipManagerPath));
            }

            s_loadingManager = Object.FindObjectOfType<SS.UI.LoadingManager>();
            if (s_loadingManager == null)
            {
                s_loadingManager = Object.Instantiate(Resources.Load<SS.UI.LoadingManager>(loadingManagerPath));
            }

            s_screenManager.sceneManager = s_sceneManager;
            s_screenManager.shieldManager = s_shieldManager;
            s_screenManager.loadingManager = s_loadingManager;
            s_screenManager.generalManager = s_generalManager;

            s_sceneManager.screenManager = s_screenManager;
            s_sceneManager.generalManager = s_generalManager;

            s_shieldManager.screenManager = s_screenManager;
            s_shieldManager.generalManager = s_generalManager;

            s_tooltipManager.generalManager = s_generalManager;

            s_loadingManager.generalManager = s_generalManager;
        }

        /// <summary>
        /// Set some basic parameters of ScreenManager.
        /// </summary>
        /// <param name="screenShieldColor">The color of screen shield</param>
        /// <param name="screenPath">The path (in Resources folder) of screen's prefabs</param>
        /// <param name="screenAnimationPath">The path (in Resources folder) of screen's animation clips</param>
        /// <param name="sceneLoadingName">The name of the scene loading screen which is put in 'screenPath'. Set it to empty if you do not want to show the loading screen while loading a scene</param>
        /// <param name="loadingName">The name of the loading screen which is put in 'screenPath'. This screen can show/hide on the top of all screens at any time using Loading(bool). Set it to empty if you don't need</param>
        /// <param name="animationSpeed">Screen Animation speed</param>
        /// <param name="tooltipName">Tooltip Name</param>
        /// <param name="showAnimationOneTime">Indicate whether a screen play its show animation again when the screen above it closes</param>
        /// <param name="closeOnTappingShield">Indicate whether close the top screen when users tap the shield</param>
        public static void Set(Color screenShieldColor, string screenPath = "Screens", string screenAnimationPath = "Animations", string sceneLoadingName = "", string loadingName = "", float animationSpeed = 1, string tooltipName = "", bool showAnimationOneTime = false, bool closeOnTappingShield = false)
        {
            if (s_generalManager != null)
                s_generalManager.Setup(animationSpeed);

            if (s_screenManager != null)
                s_screenManager.Setup(screenPath, screenAnimationPath, showAnimationOneTime);

            if (s_sceneManager != null)
                s_sceneManager.Setup(sceneLoadingName, screenPath);

            if (s_shieldManager != null)
                s_shieldManager.Setup(screenShieldColor, closeOnTappingShield);

            if (s_tooltipManager != null)
                s_tooltipManager.Setup(tooltipName, screenPath);

            if (s_loadingManager != null)
                s_loadingManager.Setup(loadingName, screenPath);
        }

        /// <summary>
        /// Set some basic parameters of ScreenManager.
        /// </summary>
        /// <param name="screenPath">The path (in Resources folder) of screen's prefabs</param>
        /// <param name="screenAnimationPath">The path (in Resources folder) of screen's animation clips</param>
        /// <param name="sceneLoadingName">The name of the scene loading screen which is put in 'screenPath'. Set it to empty if you do not want to show the loading screen while loading a scene</param>
        /// <param name="loadingName">The name of the loading screen which is put in 'screenPath'. This screen can show/hide on the top of all screens at any time using Loading(bool). Set it to empty if you don't need</param>
        /// <param name="animationSpeed">Screen Animation speed</param>
        /// <param name="tooltipName">Tooltip Name</param>
        /// <param name="showAnimationOneTime">Indicate whether a screen play its show animation again when the screen above it closes</param>
        /// <param name="closeOnTappingShield">Indicate whether close the top screen when users tap the shield</param>
        public static void Set(string screenPath = "Screens", string screenAnimationPath = "Animations", string sceneLoadingName = "", string loadingName = "", float animationSpeed = 1, string tooltipName = "", bool showAnimationOneTime = false, bool closeOnTappingShield = false)
        {
            if (s_generalManager != null)
                s_generalManager.Setup(animationSpeed);

            if (s_screenManager != null)
                s_screenManager.Setup(screenPath, screenAnimationPath, showAnimationOneTime);

            if (s_sceneManager != null)
                s_sceneManager.Setup(sceneLoadingName, screenPath);

            if (s_shieldManager != null)
                s_shieldManager.Setup(closeOnTappingShield);

            if (s_tooltipManager != null)
                s_tooltipManager.Setup(tooltipName, screenPath);

            if (s_loadingManager != null)
                s_loadingManager.Setup(loadingName, screenPath);
        }

        /// <summary>
        /// Load a scene.
        /// </summary>
        /// <typeparam name="T">The type of a (any) component in the scene</typeparam>
        /// <param name="sceneName">The name of scene</param>
        /// <param name="mode">The load scene mode. Single or Additive</param>
        /// <param name="onSceneLoaded">The callback when the scene is loaded. [IMPORTANT] It is called after the Awake & OnEnable, before the Start.</param>
        /// <param name="clearAllScreens">Clear all screens when the scene is loaded?</param>
        public static void Load<T>(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, SS.UI.SceneManager.OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreens = true) where T : Component
        {
            StopAllAddScreenCoroutines();

            if (s_sceneManager != null)
            {
                s_sceneManager.LoadScene(sceneName, mode, onSceneLoaded, clearAllScreens);
            }
        }

        /// <summary>
        /// Add a screen on top of all screens. [IMPORTANT] The code after the 'Add' method will be called after the Awake & OnEnable, before the Start.
        /// </summary>
        /// <typeparam name="T">The type of a (any) component in the screen</typeparam>
        /// <param name="screenName">The name of screen</param>
        /// <param name="showAnimation">The name of animation clip (which is put in 'screenAnimationPath') is used to animate the screen to show it</param>
        /// <param name="hideAnimation">The name of animation clip (which is put in 'screenAnimationPath') is used to animate the screen to hide it</param>
        /// <param name="animationObjectName">The name of gameobject contains screen's animation. If it is null or empty, the animation gameobject will be the root gameobject</param>
        /// <param name="useExistingScreen">If this is true, check if the screen is existing, bring it to the top. If not found, instantiate a new one</param>
        /// <param name="onScreenLoad">On Screen Loaded callback</param>
        /// <param name="hasShield">Has shield under this screen or not</param>
        /// <param name="manually">This screen is shown by user click or automatically. Just using this for analytics</param>
        /// <param name="addCondition">Only add this screen after this condition return true</param>
        /// <param name="waitUntilNoScreen">Only add this screen when no other screen is showing</param>
        /// <param name="destroyTopScreen">If this is true, destroy the top screen before adding this screen</param>
        /// <param name="hideTopScreen">If this is true, hide the top screen before adding this screen</param>
        /// <returns>The component type T in the screen.</returns>
        public static void Add<T>(string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", bool useExistingScreen = false, SS.UI.ScreenManager.OnScreenLoadDelegate<T> onScreenLoad = null, bool hasShield = true, bool manually = true, SS.UI.ScreenManager.AddConditionDelegate addCondition = null, bool waitUntilNoScreen = false, bool destroyTopScreen = false, bool hideTopScreen = true) where T : Component
        {
            if (s_screenManager != null)
            {
                s_screenManager.pendingScreens++;
                var c = s_screenManager.StartCoroutine(s_screenManager.AddScreen<T>(screenName, showAnimation, hideAnimation, animationObjectName, useExistingScreen, onScreenLoad, hasShield, manually, addCondition, waitUntilNoScreen, destroyTopScreen, hideTopScreen));
                s_screenManager.screenCoroutines.Add(new SS.UI.ScreenManager.ScreenCoroutine(c, screenName));
            }
        }

        /// <summary>
        /// Add a screen on top of all screens. Use ScreenAnimation enum instead of string for animations
        /// </summary>
        public static void Add<T>(string screenName, ScreenAnimation showAnimation, ScreenAnimation hideAnimation, string animationObjectName = "", bool useExistingScreen = false, SS.UI.ScreenManager.OnScreenLoadDelegate<T> onScreenLoad = null, bool hasShield = true, bool manually = true, SS.UI.ScreenManager.AddConditionDelegate addCondition = null, bool waitUntilNoScreen = false, bool destroyTopScreen = false, bool hideTopScreen = true) where T : Component
        {
            Add(screenName, showAnimation.ToString(), hideAnimation.ToString(), animationObjectName, useExistingScreen, onScreenLoad, hasShield, manually, addCondition, waitUntilNoScreen, destroyTopScreen, hideTopScreen);
        }

        /// <summary>
        /// Add a screen to the canvas, on top of all screens. But it's not added to the screen list for managing.
        /// </summary>
        /// <param name="screen">The GameObject of screen</param>
        public static void AddToCanvas(GameObject screen)
        {
            if (s_screenManager != null)
            {
                s_screenManager.AddToContainer(screen, s_screenManager.screenContainer);
            }
        }

        /// <summary>
        /// Destroy immediately the screen which is at the top of all screens, without playing animation.
        /// </summary>
        public static void Destroy()
        {
            if (s_screenManager != null)
            {
                s_screenManager.TryDestroyTopScreen();
            }
        }

        /// <summary>
        /// Destroy immediately the specific screen, without playing animation.
        /// </summary>
        /// <param name="screen">The component in screen which is returned by the Add function.</param>
        public static void Destroy(Component screen)
        {
            if (s_screenManager != null)
            {
                s_screenManager.TryDestroyScreen(screen);
            }
        }

        /// <summary>
        /// Destroy immediately all screens, without playing animation.
        /// </summary>
        public static void DestroyAll()
        {
            if (s_screenManager != null)
            {
                s_screenManager.ClearAllScreens();
            }
        }

        /// <summary>
        /// Close the screen which is at the top of all screens.
        /// </summary>
        /// <param name="onScreenClosed">The callback when the screen is closed. [IMPORTANT] It is called right after the screen is destroyed.</param>
        /// <param name="hideAnimation">The name of animation clip (which is put in 'screenAnimationPath') is used to animate the screen to hide it. If null, the 'hideAnimation' which is declared in the Add function will be used.</param>
        public static void Close(SS.UI.ScreenManager.OnScreenClosedDelegate onScreenClosed = null, string hideAnimation = null)
        {
            if (s_screenManager != null)
            {
                s_screenManager.CloseScreen(onScreenClosed, hideAnimation);
            }
        }

        /// <summary>
        /// Close the screen which is at the top of all screens. Use ScreenAnimation enum instead of string for animations
        /// </summary>\
        public static void Close(SS.UI.ScreenManager.OnScreenClosedDelegate onScreenClosed, ScreenAnimation hideAnimation)
        {
            Close(onScreenClosed, hideAnimation.ToString());
        }

        /// <summary>
        /// Close the screen which is at the top of all screens. Use ScreenAnimation enum instead of string for animations
        /// </summary>\
        public static void Close(ScreenAnimation hideAnimation)
        {
            Close(null, hideAnimation.ToString());
        }

        /// <summary>
        /// Close a specific screen.
        /// </summary>
        /// <param name="screen">The component in screen which is returned by the Add function.</param>
        /// <param name="onScreenClosed">The callback when the screen is closed. [IMPORTANT] It is called right after the screen is destroyed.</param>
        /// <param name="hideAnimation">The name of animation clip (which is put in 'screenAnimationPath') is used to animate the screen to hide it. If null, the 'hideAnimation' which is declared in the Add function will be used.</param>
        public static void Close(Component screen, SS.UI.ScreenManager.OnScreenClosedDelegate onScreenClosed = null, string hideAnimation = null)
        {
            if (s_screenManager != null)
            {
                s_screenManager.CloseScreen(screen, onScreenClosed, hideAnimation);
            }
        }

        /// <summary>
        /// Close a specific screen. Use ScreenAnimation enum instead of string for animations
        /// </summary>
        public static void Close(Component screen, SS.UI.ScreenManager.OnScreenClosedDelegate onScreenClosed, ScreenAnimation hideAnimation)
        {
            Close(screen, onScreenClosed, hideAnimation.ToString());
        }

        /// <summary>
        /// Close a specific screen. Use ScreenAnimation enum instead of string for animations
        /// </summary>
        public static void Close(Component screen, ScreenAnimation hideAnimation)
        {
            Close(screen, null, hideAnimation.ToString());
        }

        /// <summary>
        /// Show/Hide the loading screen (which has the name 'loadingName') on top of all screens.
        /// </summary>
        /// <param name="isShow">True if show, False if hide</param>
        /// <param name="timeout">If timeout == 0, no timeout</param>
        public static void Loading(bool isShow, float timeout = 0)
        {
            if (s_loadingManager != null)
            {
                s_loadingManager.ShowLoading(isShow, timeout);
            }
        }

        /// <summary>
        /// Add OnScreenTransition listener
        /// </summary>
        /// <param name="onScreenAdded"></param>
        public static void AddListener(SS.UI.ScreenManager.OnScreenAddedDelegate onScreenAdded)
        {
            if (s_screenManager != null)
            {
                s_screenManager.OnScreenAdded += onScreenAdded;
            }
        }

        /// <summary>
        /// Remove OnScreenTransition listener
        /// </summary>
        /// <param name="onScreenTransition"></param>
        public static void RemoveListener(SS.UI.ScreenManager.OnScreenAddedDelegate onScreenTransition)
        {
            if (s_screenManager != null)
            {
                s_screenManager.OnScreenAdded -= onScreenTransition;
            }
        }

        /// <summary>
        /// Add OnScreenChanged listener
        /// </summary>
        /// <param name="onScreenChanged"></param>
        public static void AddListener(SS.UI.ScreenManager.OnScreenChangedDelegate onScreenChanged)
        {
            if (s_screenManager != null)
            {
                s_screenManager.OnScreenChanged += onScreenChanged;
            }
        }

        /// <summary>
        /// Remove OnScreenChanged listener
        /// </summary>
        /// <param name="onScreenChanged"></param>
        public static void RemoveListener(SS.UI.ScreenManager.OnScreenChangedDelegate onScreenChanged)
        {
            if (s_screenManager != null)
            {
                s_screenManager.OnScreenChanged -= onScreenChanged;
            }
        }

        /// <summary>
        /// Get The Top RectTransform. UIs here are highest UIs.
        /// </summary>
        public static RectTransform Top
        {
            get
            {
                if (s_screenManager != null)
                {
                    return s_screenManager.topContainer;
                }

                return null;
            }
        }

        /// <summary>
        /// Show the shield
        /// </summary>
        public static void ShowShield()
        {
            if (s_shieldManager != null)
            {
                s_shieldManager.ShowShield();
            }
        }

        /// <summary>
        /// Hide the shield
        /// </summary>
        public static void HideShield()
        {
            if (s_shieldManager != null)
            {
                s_shieldManager.HideShield();
            }
        }

        /// <summary>
        /// Destroy all shield
        /// </summary>
        public static void DestroyShield()
        {
            if (s_shieldManager != null)
            {
                s_shieldManager.DestroyAllShields();
            }
        }

        /// <summary>
        /// Stop All AddScreen Coroutines
        /// </summary>
        public static void StopAllAddScreenCoroutines()
        {
            if (s_screenManager != null)
            {
                s_screenManager.StopAllAddScreenCoroutines();
            }
        }

        /// <summary>
        /// Check if no screen is appearing, no screen is animating, and no screen is pending to be added.
        /// </summary>
        /// <returns></returns>
        public static bool IsNoMoreScreen()
        {
            if (s_screenManager != null)
            {
                return s_screenManager.isNoMoreScreen;
            }

            return true;
        }

        /// <summary>
        /// Show tooltip
        /// </summary>
        /// <param name="text">Tooltip content</param>
        /// <param name="worldPosition">Tooltip position</param>
        /// <param name="tooltipName">Tooltip prefab name</param>
        /// <param name="targetY">Target Y</param>
        public static void ShowTooltip(string text, Vector3 worldPosition, float targetY = 100f)
        {
            if (s_tooltipManager != null)
            {
                s_tooltipManager.LoadAndShowTooltip(text, worldPosition, targetY);
            }
        }

        /// <summary>
        /// Hide Tooltip
        /// </summary>
        public static void HideTooltip()
        {
            if (s_tooltipManager != null)
            {
                s_tooltipManager.HideTooltipImmediately();
            }
        }

        /// <summary>
        /// Pending Screens Count
        /// </summary>
        /// <returns></returns>
        public static int PendingScreensCount()
        {
            if (s_screenManager != null)
            {
                return s_screenManager.pendingScreens;
            }

            return 0;
        }

        /// <summary>
        /// The main canvas of this UI system
        /// </summary>
        public static Canvas Canvas
        {
            get
            {
                if (s_generalManager != null)
                {
                    return s_generalManager.canvas;
                }

                return null;
            }
        }

        /// <summary>
        /// Get the scene loading operation progress.
        /// </summary>
        public static float asyncOperationProgress
        {
            get
            {
                if (s_sceneManager != null)
                {
                    return s_sceneManager.asyncOperationProgress;
                }

                return 0f;
            }
        }
        #endregion
    }
}