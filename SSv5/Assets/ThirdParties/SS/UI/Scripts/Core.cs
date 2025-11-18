/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SS.UI
{
    public class Core
    {
        protected static GeneralManager _generalManager;
        protected static ScreenManager _screenManager;
        protected static SceneManager _sceneManager;
        protected static ShieldManager _shieldManager;
        protected static TooltipManager _tooltipManager;
        protected static LoadingManager _loadingManager;

        protected static bool _initialized = false;

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
            if (_initialized)
                return;

            _initialized = true;

            _generalManager = Object.FindObjectOfType<SS.UI.GeneralManager>();
            if (_generalManager == null)
            {
                _generalManager = Object.Instantiate(Resources.Load<SS.UI.GeneralManager>(generalManagerPath));
            }

            _screenManager = Object.FindObjectOfType<SS.UI.ScreenManager>();
            if (_screenManager == null)
            {
                _screenManager = Object.Instantiate(Resources.Load<SS.UI.ScreenManager>(screenManagerPath));
            }

            _sceneManager = Object.FindObjectOfType<SS.UI.SceneManager>();
            if (_sceneManager == null)
            {
                _sceneManager = Object.Instantiate(Resources.Load<SS.UI.SceneManager>(sceneManagerPath));
            }

            _shieldManager = Object.FindObjectOfType<SS.UI.ShieldManager>();
            if (_shieldManager == null)
            {
                _shieldManager = Object.Instantiate(Resources.Load<SS.UI.ShieldManager>(shieldManagerPath));
            }

            _tooltipManager = Object.FindObjectOfType<SS.UI.TooltipManager>();
            if (_tooltipManager == null)
            {
                _tooltipManager = Object.Instantiate(Resources.Load<SS.UI.TooltipManager>(tooltipManagerPath));
            }

            _loadingManager = Object.FindObjectOfType<SS.UI.LoadingManager>();
            if (_loadingManager == null)
            {
                _loadingManager = Object.Instantiate(Resources.Load<SS.UI.LoadingManager>(loadingManagerPath));
            }

            _screenManager.SceneManager = _sceneManager;
            _screenManager.ShieldManager = _shieldManager;
            _screenManager.LoadingManager = _loadingManager;
            _screenManager.GeneralManager = _generalManager;

            _sceneManager.ScreenManager = _screenManager;
            _sceneManager.GeneralManager = _generalManager;

            _shieldManager.ScreenManager = _screenManager;
            _shieldManager.GeneralManager = _generalManager;

            _tooltipManager.GeneralManager = _generalManager;

            _loadingManager.GeneralManager = _generalManager;
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
            if (_generalManager != null)
                _generalManager.Setup(animationSpeed);

            if (_screenManager != null)
                _screenManager.Setup(screenPath, screenAnimationPath, showAnimationOneTime);

            if (_sceneManager != null)
                _sceneManager.Setup(sceneLoadingName, screenPath);

            if (_shieldManager != null)
                _shieldManager.Setup(screenShieldColor, closeOnTappingShield);

            if (_tooltipManager != null)
                _tooltipManager.Setup(tooltipName, screenPath);

            if (_loadingManager != null)
                _loadingManager.Setup(loadingName, screenPath);
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
            if (_generalManager != null)
                _generalManager.Setup(animationSpeed);

            if (_screenManager != null)
                _screenManager.Setup(screenPath, screenAnimationPath, showAnimationOneTime);

            if (_sceneManager != null)
                _sceneManager.Setup(sceneLoadingName, screenPath);

            if (_shieldManager != null)
                _shieldManager.Setup(closeOnTappingShield);

            if (_tooltipManager != null)
                _tooltipManager.Setup(tooltipName, screenPath);

            if (_loadingManager != null)
                _loadingManager.Setup(loadingName, screenPath);
        }

        /// <summary>
        /// Load a scene.
        /// </summary>
        /// <typeparam name="T">The type of a (any) component in the scene</typeparam>
        /// <param name="sceneName">The name of scene</param>
        /// <param name="mode">The load scene mode. Single or Additive</param>
        /// <param name="onSceneLoaded">The callback when the scene is loaded. [IMPORTANT] It is called after the Awake & OnEnable, before the Start.</param>
        /// <param name="clearAllScreens">Clear all screens when the scene is loaded?</param>
        /// <param name="onScenePreLoad">On Scene PreLoad callback. It is an iterator method, useful for loading addressables before showing scene</param>
        public static void Load<T>(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, SS.UI.SceneManager.OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreens = true, SS.UI.SceneManager.OnScenePreLoadDelegate onScenePreLoad = null) where T : Component
        {
            StopAllAddScreenCoroutines();

            if (_sceneManager != null)
            {
                _sceneManager.LoadScene(sceneName, mode, onSceneLoaded, clearAllScreens, onScenePreLoad);
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
        /// <param name="shieldAlpha">Alpha of the shield. By default it is -1, means no change</param>
        /// <param name="ignoreOnScreenAdded">Ignore OnScreenAdded after screen is loaded, by default it is false</param>
        /// <param name="onScreenPreLoad">On Screen PreLoad callback. It is an iterator method, useful for loading addressables before showing screen</param>
        /// <returns>The component type T in the screen.</returns>
        public static void Add<T>(string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", bool useExistingScreen = false, SS.UI.ScreenManager.OnScreenLoadDelegate<T> onScreenLoad = null, bool hasShield = true, bool manually = true, SS.UI.ScreenManager.AddConditionDelegate addCondition = null, bool waitUntilNoScreen = false, bool destroyTopScreen = false, bool hideTopScreen = true, float shieldAlpha = -1, bool ignoreOnScreenAdded = false, SS.UI.ScreenManager.OnScreenPreLoadDelegate onScreenPreLoad = null) where T : Component
        {
            if (_screenManager != null)
            {
                _screenManager.PendingScreens++;
                var c = _screenManager.StartCoroutine(_screenManager.AddScreen<T>(screenName, showAnimation, hideAnimation, animationObjectName, useExistingScreen, onScreenLoad, hasShield, manually, addCondition, waitUntilNoScreen, destroyTopScreen, hideTopScreen, shieldAlpha, ignoreOnScreenAdded, onScreenPreLoad));
                _screenManager.ScreenCoroutines.Add(new SS.UI.ScreenManager.ScreenCoroutine(c, screenName));
            }
        }

        /// <summary>
        /// Add a screen on top of all screens. Use ScreenAnimation enum instead of string for animations
        /// </summary>
        public static void Add<T>(string screenName, ScreenAnimation showAnimation, ScreenAnimation hideAnimation, string animationObjectName = "", bool useExistingScreen = false, SS.UI.ScreenManager.OnScreenLoadDelegate<T> onScreenLoad = null, bool hasShield = true, bool manually = true, SS.UI.ScreenManager.AddConditionDelegate addCondition = null, bool waitUntilNoScreen = false, bool destroyTopScreen = false, bool hideTopScreen = true, float shieldAlpha = -1, bool ignoreOnScreenAdded = false, SS.UI.ScreenManager.OnScreenPreLoadDelegate onScreenPreLoad = null) where T : Component
        {
            Add(screenName, showAnimation.ToString(), hideAnimation.ToString(), animationObjectName, useExistingScreen, onScreenLoad, hasShield, manually, addCondition, waitUntilNoScreen, destroyTopScreen, hideTopScreen, shieldAlpha, ignoreOnScreenAdded, onScreenPreLoad);
        }

        /// <summary>
        /// Add a screen to the canvas, on top of all screens. But it's not added to the screen list for managing.
        /// </summary>
        /// <param name="screen">The GameObject of screen</param>
        public static void AddToCanvas(GameObject screen)
        {
            if (_screenManager != null)
            {
                _screenManager.AddToContainer(screen, _screenManager.ScreenContainer);
            }
        }

        /// <summary>
        /// Destroy immediately the screen which is at the top of all screens, without playing animation.
        /// </summary>
        public static void Destroy()
        {
            if (_screenManager != null)
            {
                _screenManager.TryDestroyTopScreen();
            }
        }

        /// <summary>
        /// Destroy immediately the specific screen, without playing animation.
        /// </summary>
        /// <param name="screen">The component in screen which is returned by the Add function.</param>
        public static void Destroy(Component screen)
        {
            if (_screenManager != null)
            {
                _screenManager.TryDestroyScreen(screen);
            }
        }

        /// <summary>
        /// Destroy immediately all screens, without playing animation.
        /// </summary>
        public static void DestroyAll()
        {
            if (_screenManager != null)
            {
                _screenManager.ClearAllScreens();
            }
        }

        /// <summary>
        /// Close the screen which is at the top of all screens.
        /// </summary>
        /// <param name="onScreenClosed">The callback when the screen is closed. [IMPORTANT] It is called right after the screen is destroyed.</param>
        /// <param name="hideAnimation">The name of animation clip (which is put in 'screenAnimationPath') is used to animate the screen to hide it. If null, the 'hideAnimation' which is declared in the Add function will be used.</param>
        public static void Close(SS.UI.ScreenManager.OnScreenClosedDelegate onScreenClosed = null, string hideAnimation = null)
        {
            if (_screenManager != null)
            {
                _screenManager.Close(onScreenClosed, hideAnimation);
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
            if (_screenManager != null)
            {
                _screenManager.Close(screen, onScreenClosed, hideAnimation);
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
            if (_loadingManager != null)
            {
                _loadingManager.ShowLoading(isShow, timeout);
            }
        }

        /// <summary>
        /// Add OnScreenTransition listener
        /// </summary>
        /// <param name="onScreenAdded"></param>
        public static void AddListener(SS.UI.ScreenManager.OnScreenAddedDelegate onScreenAdded)
        {
            if (_screenManager != null)
            {
                _screenManager.OnScreenAdded += onScreenAdded;
            }
        }

        /// <summary>
        /// Remove OnScreenTransition listener
        /// </summary>
        /// <param name="onScreenTransition"></param>
        public static void RemoveListener(SS.UI.ScreenManager.OnScreenAddedDelegate onScreenTransition)
        {
            if (_screenManager != null)
            {
                _screenManager.OnScreenAdded -= onScreenTransition;
            }
        }

        /// <summary>
        /// Add OnScreenChanged listener
        /// </summary>
        /// <param name="onScreenChanged"></param>
        public static void AddListener(SS.UI.ScreenManager.OnScreenChangedDelegate onScreenChanged)
        {
            if (_screenManager != null)
            {
                _screenManager.OnScreenChanged += onScreenChanged;
            }
        }

        /// <summary>
        /// Remove OnScreenChanged listener
        /// </summary>
        /// <param name="onScreenChanged"></param>
        public static void RemoveListener(SS.UI.ScreenManager.OnScreenChangedDelegate onScreenChanged)
        {
            if (_screenManager != null)
            {
                _screenManager.OnScreenChanged -= onScreenChanged;
            }
        }

        /// <summary>
        /// Get The Top RectTransform. UIs here are highest UIs.
        /// </summary>
        public static RectTransform Top
        {
            get
            {
                if (_screenManager != null)
                {
                    return _screenManager.TopContainer;
                }

                return null;
            }
        }

        /// <summary>
        /// Show the shield
        /// </summary>
        public static void ShowShield()
        {
            if (_shieldManager != null)
            {
                _shieldManager.ShowAllShields();
            }
        }

        /// <summary>
        /// Hide the shield
        /// </summary>
        public static void HideShield()
        {
            if (_shieldManager != null)
            {
                _shieldManager.HideAllShields();
            }
        }

        /// <summary>
        /// Destroy all shield
        /// </summary>
        public static void DestroyShield()
        {
            if (_shieldManager != null)
            {
                _shieldManager.DestroyAllShields();
            }
        }

        /// <summary>
        /// Stop All AddScreen Coroutines
        /// </summary>
        public static void StopAllAddScreenCoroutines()
        {
            if (_screenManager != null)
            {
                _screenManager.StopAllAddScreenCoroutines();
            }
        }

        /// <summary>
        /// Check if no screen is appearing, no screen is animating, and no screen is pending to be added.
        /// </summary>
        /// <returns></returns>
        public static bool IsNoMoreScreen()
        {
            if (_screenManager != null)
            {
                return _screenManager.IsNoMoreScreen;
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
            if (_tooltipManager != null)
            {
                _tooltipManager.LoadAndShowTooltip(text, worldPosition, targetY);
            }
        }

        /// <summary>
        /// Hide Tooltip
        /// </summary>
        public static void HideTooltip()
        {
            if (_tooltipManager != null)
            {
                _tooltipManager.HideTooltipImmediately();
            }
        }

        /// <summary>
        /// Pending Screens Count
        /// </summary>
        /// <returns></returns>
        public static int PendingScreensCount()
        {
            if (_screenManager != null)
            {
                return _screenManager.PendingScreens;
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
                if (_generalManager != null)
                {
                    return _generalManager.MainCanvas;
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
                if (_sceneManager != null)
                {
                    return _sceneManager.AsyncOperationProgress;
                }

                return 0f;
            }
        }
        #endregion
    }
}