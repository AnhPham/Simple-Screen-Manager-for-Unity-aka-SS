/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using SS.UI;

public class ScreenManager
{
    #region Public Static
    /// <summary>
    /// Get the scene loading operation. Can get some values like % progress.
    /// </summary>
    public static AsyncOperation asyncOperation
    {
        get
        {
            return SS.UI.ScreenManager.instance.asyncOperation;
        }
    }

    /// <summary>
    /// Set some basic parameters of ScreenManager.
    /// </summary>
    /// <param name="screenShieldColor">The color of screen shield</param>
    /// <param name="screenPath">The path (in Resources folder) of screen's prefabs</param>
    /// <param name="screenAnimationPath">The path (in Resources folder) of screen's animation clips</param>
    /// <param name="sceneLoadingName">The name of the scene loading screen which is put in 'screenPath'. Set it to empty if you do not want to show the loading screen while loading a scene</param>
    /// <param name="loadingName">The name of the loading screen which is put in 'screenPath'. This screen can show/hide on the top of all screens at any time using Loading(bool). Set it to empty if you don't need</param>
    /// <param name="screenAnimationSpeed">Screen Animation speed</param>
    /// <param name="tooltipName">Tooltip Name</param>
    /// <param name="showAnimationOneTime">Indicate whether a screen play its show animation again when the screen above it closes</param>
    /// <param name="closeOnTappingShield">Indicate whether close the top screen when users tap the shield</param>
    public static void Set(Color screenShieldColor, string screenPath = "Screens", string screenAnimationPath = "Animations", string sceneLoadingName = "", string loadingName = "", float screenAnimationSpeed = 1, string tooltipName = "", bool showAnimationOneTime = false, bool closeOnTappingShield = false)
    {
        SS.UI.ScreenManager.instance.Setup(screenShieldColor, screenPath, screenAnimationPath, sceneLoadingName, loadingName, screenAnimationSpeed, tooltipName, showAnimationOneTime, closeOnTappingShield);
    }

    /// <summary>
    /// Set some basic parameters of ScreenManager.
    /// </summary>
    /// <param name="screenPath">The path (in Resources folder) of screen's prefabs</param>
    /// <param name="screenAnimationPath">The path (in Resources folder) of screen's animation clips</param>
    /// <param name="sceneLoadingName">The name of the scene loading screen which is put in 'screenPath'. Set it to empty if you do not want to show the loading screen while loading a scene</param>
    /// <param name="loadingName">The name of the loading screen which is put in 'screenPath'. This screen can show/hide on the top of all screens at any time using Loading(bool). Set it to empty if you don't need</param>
    /// <param name="screenAnimationSpeed">Screen Animation speed</param>
    /// <param name="tooltipName">Tooltip Name</param>
    /// <param name="showAnimationOneTime">Indicate whether a screen play its show animation again when the screen above it closes</param>
    /// <param name="closeOnTappingShield">Indicate whether close the top screen when users tap the shield</param>
    public static void Set(string screenPath = "Screens", string screenAnimationPath = "Animations", string sceneLoadingName = "", string loadingName = "", float screenAnimationSpeed = 1, string tooltipName = "", bool showAnimationOneTime = false, bool closeOnTappingShield = false)
    {
        SS.UI.ScreenManager.instance.Setup(screenPath, screenAnimationPath, sceneLoadingName, loadingName, screenAnimationSpeed, tooltipName, showAnimationOneTime, closeOnTappingShield);
    }

    /// <summary>
    /// Load a scene.
    /// </summary>
    /// <typeparam name="T">The type of a (any) component in the scene</typeparam>
    /// <param name="sceneName">The name of scene</param>
    /// <param name="mode">The load scene mode. Single or Additive</param>
    /// <param name="onSceneLoaded">The callback when the scene is loaded. [IMPORTANT] It is called after the Awake & OnEnable, before the Start.</param>
    /// <param name="clearAllScreens">Clear all screens when the scene is loaded?</param>
    public static void Load<T>(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, SS.UI.ScreenManager.OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreens = true) where T : Component
    {
        StopAllAddScreenCoroutines();
        SS.UI.ScreenManager.instance.LoadScene(sceneName, mode, onSceneLoaded, clearAllScreens);
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
    public static void Add<T>(string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", bool useExistingScreen = false, SS.UI.ScreenManager.OnScreenLoad<T> onScreenLoad = null, bool hasShield = true, bool manually = true, SS.UI.ScreenManager.AddConditionDelegate addCondition = null, bool waitUntilNoScreen = false, bool destroyTopScreen = false, bool hideTopScreen = true) where T : Component
    {
        SS.UI.ScreenManager.instance.pendingToLoadScreens++;
        var c = SS.UI.ScreenManager.instance.StartCoroutine(SS.UI.ScreenManager.instance.AddScreen<T>(screenName, showAnimation, hideAnimation, animationObjectName, useExistingScreen, onScreenLoad, hasShield, manually, addCondition, waitUntilNoScreen, destroyTopScreen, hideTopScreen));
        SS.UI.ScreenManager.instance.screenCoroutines.Add(new SS.UI.ScreenManager.ScreenCoroutine(c, screenName));
    }

    /// <summary>
    /// Add a screen on top of all screens. Use ScreenAnimation enum instead of string for animations
    /// </summary>
    public static void Add<T>(string screenName, ScreenAnimation showAnimation, ScreenAnimation hideAnimation, string animationObjectName = "", bool useExistingScreen = false, SS.UI.ScreenManager.OnScreenLoad<T> onScreenLoad = null, bool hasShield = true, bool manually = true, SS.UI.ScreenManager.AddConditionDelegate addCondition = null, bool waitUntilNoScreen = false, bool destroyTopScreen = false, bool hideTopScreen = true) where T : Component
    {
        Add(screenName, showAnimation.ToString(), hideAnimation.ToString(), animationObjectName, useExistingScreen, onScreenLoad, hasShield, manually, addCondition, waitUntilNoScreen, destroyTopScreen, hideTopScreen);
    }

    /// <summary>
    /// Add a screen to the canvas, on top of all screens. But it's not added to the screen list for managing.
    /// </summary>
    /// <param name="screen">The GameObject of screen</param>
    public static void AddToCanvas(GameObject screen)
    {
        SS.UI.ScreenManager.instance.AddToContainer(screen, SS.UI.ScreenManager.instance.screenContainer);
    }

    /// <summary>
    /// Destroy immediately the screen which is at the top of all screens, without playing animation.
    /// </summary>
    public static void Destroy()
    {
        SS.UI.ScreenManager.instance.DestroyScreen();
    }

    /// <summary>
    /// Destroy immediately the specific screen, without playing animation.
    /// </summary>
    /// <param name="screen">The component in screen which is returned by the Add function.</param>
    public static void Destroy(Component screen)
    {
        SS.UI.ScreenManager.instance.DestroyScreen(screen);
    }

    /// <summary>
    /// Destroy immediately all screens, without playing animation.
    /// </summary>
    public static void DestroyAll()
    {
        SS.UI.ScreenManager.instance.ClearAllScreen();
    }

    /// <summary>
    /// Close the screen which is at the top of all screens.
    /// </summary>
    /// <param name="onScreenClosed">The callback when the screen is closed. [IMPORTANT] It is called right after the screen is destroyed.</param>
    /// <param name="hideAnimation">The name of animation clip (which is put in 'screenAnimationPath') is used to animate the screen to hide it. If null, the 'hideAnimation' which is declared in the Add function will be used.</param>
    public static void Close(SS.UI.ScreenManager.Callback onScreenClosed = null, string hideAnimation = null)
    {
        SS.UI.ScreenManager.instance.CloseScreen(onScreenClosed, hideAnimation);
    }

    /// <summary>
    /// Close the screen which is at the top of all screens. Use ScreenAnimation enum instead of string for animations
    /// </summary>\
    public static void Close(SS.UI.ScreenManager.Callback onScreenClosed, ScreenAnimation hideAnimation)
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
    public static void Close(Component screen, SS.UI.ScreenManager.Callback onScreenClosed = null, string hideAnimation = null)
    {
        SS.UI.ScreenManager.instance.CloseScreen(screen, onScreenClosed, hideAnimation);
    }

    /// <summary>
    /// Close a specific screen. Use ScreenAnimation enum instead of string for animations
    /// </summary>
    public static void Close(Component screen, SS.UI.ScreenManager.Callback onScreenClosed, ScreenAnimation hideAnimation)
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
        SS.UI.ScreenManager.instance.ShowLoading(isShow, timeout);
    }

    /// <summary>
    /// Remove a specific screen. But don't use this. We use this for an internal purpose.
    /// </summary>
    /// <param name="screen">The component in screen which is returned by Add function.</param>
    public static void RemoveScreen(Component screen)
    {
        SS.UI.ScreenManager.instance.RemoveScreenFromList(screen);
    }

    /// <summary>
    /// Hide Shield Or Show Top Screen
    /// </summary>
    public static void HideShieldOrShowTop(Component screen)
    {
        if (SS.UI.ScreenManager.instance != null && SS.UI.ScreenManager.instance.isActiveAndEnabled)
        {
            SS.UI.ScreenManager.instance.HideScreenShieldOrShowTop(screen);
        }
    }

    /// <summary>
    /// Add OnScreenTransition listener
    /// </summary>
    /// <param name="onScreenAdded"></param>
    public static void AddListener(SS.UI.ScreenManager.OnScreenAddedDelegate onScreenAdded)
    {
        if (SS.UI.ScreenManager.instance != null)
        {
            SS.UI.ScreenManager.instance.OnScreenAdded += onScreenAdded;
        }
    }

    /// <summary>
    /// Remove OnScreenTransition listener
    /// </summary>
    /// <param name="onScreenTransition"></param>
    public static void RemoveListener(SS.UI.ScreenManager.OnScreenAddedDelegate onScreenTransition)
    {
        if (SS.UI.ScreenManager.instance != null)
        {
            SS.UI.ScreenManager.instance.OnScreenAdded -= onScreenTransition;
        }
    }

    /// <summary>
    /// Add OnScreenChanged listener
    /// </summary>
    /// <param name="onScreenChanged"></param>
    public static void AddListener(SS.UI.ScreenManager.OnScreenChangedDelegate onScreenChanged)
    {
        if (SS.UI.ScreenManager.instance != null)
        {
            SS.UI.ScreenManager.instance.OnScreenChanged += onScreenChanged;
        }
    }

    /// <summary>
    /// Remove OnScreenChanged listener
    /// </summary>
    /// <param name="onScreenChanged"></param>
    public static void RemoveListener(SS.UI.ScreenManager.OnScreenChangedDelegate onScreenChanged)
    {
        if (SS.UI.ScreenManager.instance != null)
        {
            SS.UI.ScreenManager.instance.OnScreenChanged -= onScreenChanged;
        }
    }

    /// <summary>
    /// Get The Top RectTransform. UIs here are highest UIs.
    /// </summary>
    public static RectTransform Top
    {
        get
        {
            if (SS.UI.ScreenManager.instance != null)
            {
                return SS.UI.ScreenManager.instance.topContainer;
            }

            return null;
        }
    }

    /// <summary>
    /// Show the shield
    /// </summary>
    public static void ShowShield()
    {
        if (SS.UI.ScreenManager.instance != null)
        {
            for (int i = 0; i < SS.UI.ScreenManager.instance.shieldList.Count; i++)
            {
                var shield = SS.UI.ScreenManager.instance.shieldList[i];

                if (shield != null)
                {
                    if (!shield.gameObject.activeInHierarchy)
                    {
                        shield.gameObject.SetActive(true);
                    }

                    shield.Play("ShieldShow", speed: SS.UI.ScreenManager.instance.screenAnimationSpeed);
                }
            }
        }
    }

    /// <summary>
    /// Hide the shield
    /// </summary>
    public static void HideShield()
    {
        if (SS.UI.ScreenManager.instance != null)
        {
            for (int i = 0; i < SS.UI.ScreenManager.instance.shieldList.Count; i++)
            {
                var shield = SS.UI.ScreenManager.instance.shieldList[i];

                if (shield != null)
                {
                    shield.Play("ShieldHide", speed: SS.UI.ScreenManager.instance.screenAnimationSpeed);
                }
            }
        }
    }

    /// <summary>
    /// Destroy all shield
    /// </summary>
    public static void DestroyShield()
    {
        if (SS.UI.ScreenManager.instance != null)
        {
            for (int i = 0; i < SS.UI.ScreenManager.instance.shieldList.Count; i++)
            {
                var shield = SS.UI.ScreenManager.instance.shieldList[i];
                Object.Destroy(shield.gameObject);
            }

            SS.UI.ScreenManager.instance.shieldList.Clear();
        }
    }

    /// <summary>
    /// Stop All AddScreen Coroutines
    /// </summary>
    public static void StopAllAddScreenCoroutines()
    {
        if (SS.UI.ScreenManager.instance != null)
        {
            for (int i = 0; i < SS.UI.ScreenManager.instance.screenCoroutines.Count; i++)
            {
                var sc = SS.UI.ScreenManager.instance.screenCoroutines[i];
                if (sc != null && sc.coroutine != null)
                {
                    SS.UI.ScreenManager.instance.StopCoroutine(sc.coroutine);
                    sc.coroutine = null;
                    Debug.LogWarning("CM: StopCoroutine " + sc.screenName.ToString());
                }
            }
            SS.UI.ScreenManager.instance.screenCoroutines.Clear();
        }
    }

    /// <summary>
    /// Check if no screen is appearing, no screen is animating, and no screen is pending to be added.
    /// </summary>
    /// <returns></returns>
    public static bool IsNoMoreScreen()
    {
        if (SS.UI.ScreenManager.instance == null)
            return true;

        return (SS.UI.ScreenManager.instance.screenList.Count <= 0 && SS.UI.ScreenManager.instance.loadingScreens <= 0 && SS.UI.ScreenManager.instance.animationPlayingScreens <= 0);
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
        if (SS.UI.ScreenManager.instance == null)
            return;

        SS.UI.ScreenManager.instance.LoadAndShowTooltip(text, worldPosition, targetY);
    }

    /// <summary>
    /// Hide Tooltip
    /// </summary>
    public static void HideTooltip()
    {
        if (SS.UI.ScreenManager.instance == null)
            return;

        SS.UI.ScreenManager.instance.HideTooltipImmediately();
    }

    /// <summary>
    /// Pending Screens Count
    /// </summary>
    /// <returns></returns>
    public static int PendingScreensCount()
    {
        if (SS.UI.ScreenManager.instance == null)
            return 0;

        return SS.UI.ScreenManager.instance.pendingToLoadScreens;
    }
    #endregion
}