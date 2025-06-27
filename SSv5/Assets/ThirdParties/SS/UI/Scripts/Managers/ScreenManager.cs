/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
    public enum ScreenAnimation
    {
        BottomHide, // The screen slides from the center to the bottom when hiding.
        BottomShow, // The screen slides from the bottom to the center when showing.
        FadeHide,   // The screen fades out when hiding.
        FadeShow,   // The screen fades in when showing.
        LeftHide,   // The screen slides from the center to the left when hiding.
        LeftShow,   // The screen slides from the left to the center when showing.
        RightHide,  // The screen slides from the center to the right when hiding.
        RightShow,  // The screen slides from the right to the center when showing.
        RotateHide, // The screen rotates clockwise when hiding.
        RotateShow, // The screen rotates counterclockwise when showing.
        ScaleHide,  // The screen scales down to 0 when hiding.
        ScaleShow,  // The screen scales up to 1 when showing.
        TopHide,    // The screen slides from the center to the top when hiding.
        TopShow     // The screen slides from the top to the center when showing.
    }

    public class ScreenManager : MonoBehaviour
    {
        #region Sub Class
        public class ScreenCoroutine
        {
            public Coroutine coroutine;
            public string screenName;

            public ScreenCoroutine(Coroutine coroutine, string screenName)
            {
                this.coroutine = coroutine;
                this.screenName = screenName;
            }
        }
        #endregion

        #region Serialize Fields
        [SerializeField] string m_ScreenPath = "Screens";
        [SerializeField] string m_ScreenAnimationPath = "Animations";
        [SerializeField] bool m_ShowAnimationOneTime = false;
        [SerializeField] SceneManager m_SceneManager;
        [SerializeField] ShieldManager m_ShieldManager;
        [SerializeField] LoadingManager m_LoadingManager;
        [SerializeField] GeneralManager m_GeneralManager;
        #endregion

        #region Delegates & Events
        public delegate void OnScreenLoadDelegate<T>(T t);
        public delegate void OnScreenClosedDelegate();
        public delegate void OnAnimationEndedDelegate();
        public delegate void OnScreenAddedDelegate(string toScreen, string fromScreen, bool manually);
        public delegate void OnScreenChangedDelegate(int screenCount);
        public delegate bool AddConditionDelegate();

        public OnScreenAddedDelegate OnScreenAdded;
        public OnScreenChangedDelegate OnScreenChanged;
        #endregion

        #region Private Fields
        private List<Component> m_ScreenList = new List<Component>();
        private List<ScreenCoroutine> m_ScreenCoroutines = new List<ScreenCoroutine>();
        private int m_PendingScreens = 0;   // Number of screens is pending to load
        private int m_LoadingScreens = 0;   // Number of screens is being loaded
        private int m_AnimatingScreens = 0; // Number of screens is being animated (show/hide)
        #endregion

        #region Public Properties
        public SceneManager sceneManager { get => m_SceneManager; set => m_SceneManager = value; }
        public ShieldManager shieldManager { get => m_ShieldManager; set => m_ShieldManager = value; }
        public LoadingManager loadingManager { get => m_LoadingManager; set => m_LoadingManager = value; }
        public GeneralManager generalManager { get => m_GeneralManager; set => m_GeneralManager = value; }
        public int pendingScreens { get => m_PendingScreens; set => m_PendingScreens = value; }
        public bool isNoMoreScreen { get => m_ScreenList.Count <= 0 && m_LoadingScreens <= 0 && m_AnimatingScreens <= 0; }
        public List<ScreenCoroutine> screenCoroutines => m_ScreenCoroutines;
        public Canvas canvas => m_GeneralManager.canvas;
        public RectTransform screenContainer => m_GeneralManager.screenContainer;
        public RectTransform topContainer => m_GeneralManager.topContainer;
        public float animationSpeed => m_GeneralManager.animationSpeed;
        #endregion

        #region Private Short Function
        private bool IsLoadingVisible() => loadingManager.loadingObject != null && loadingManager.loadingObject.activeInHierarchy;
        private bool IsAnyScreenActive() => m_ScreenList.Count > 0;
        private bool IsAnyScreenLoading() => m_LoadingScreens > 0;
        private bool IsAnyScreenAnimating() => m_AnimatingScreens > 0;
        private bool IsAnyScreenPending() => m_PendingScreens > 0;
        private bool IsScreen(Transform t) => t.GetComponent<ScreenController>() != null;
        private bool IsShield(Transform t) => t.GetComponent<ShieldController>() != null;
        private Component GetTopScreen() => m_ScreenList[m_ScreenList.Count - 1];
        #endregion

        #region Unity Cycle
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            generalManager = FindObjectOfType<GeneralManager>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscapeKey();
            }
        }
        #endregion

        #region Escape Key
        private void HandleEscapeKey()
        {
            if (!IsLoadingVisible() && IsAnyScreenActive())
            {
                Component topScreen = GetTopScreen();
                if (TryHandleKeyBack(topScreen)) return;
                CloseScreen();
            }
        }

        private bool TryHandleKeyBack(Component screen)
        {
            if (screen.TryGetComponent(out IKeyBack keyBack))
            {
                keyBack.OnKeyBack();
                return true;
            }
            return false;
        }
        #endregion

        #region Init
        public void Setup(string screenPath = "Screens", string screenAnimationPath = "Animations", bool showAnimationOneTime = false)
        {
            m_ScreenPath = screenPath;
            m_ScreenAnimationPath = screenAnimationPath;
            m_ShowAnimationOneTime = showAnimationOneTime;
        }
        #endregion

        #region Close & Destroy
        public void CloseScreen(OnScreenClosedDelegate onScreenClosed = null, string hideAnimation = null)
        {
            if (IsAnyScreenActive())
            {
                var topScreen = GetTopScreen();

                if (topScreen != null)
                {
                    CloseScreen(topScreen, onScreenClosed, hideAnimation);
                }
            }

            // if show animation one time, activate the underlying screen right after close top screen is called, before its hide animation is started 
            if (m_ShowAnimationOneTime && IsAnyScreenActive())
            {
                ActivateTopScreen();
            }
        }

        private void ActivateTopScreen()
        {
            if (IsAnyScreenActive())
            {
                var topScreen = GetTopScreen();

                if (topScreen != null)
                {
                    topScreen.gameObject.SetActive(true);
                }
            }
        }

        public void CloseScreen(Component screen, OnScreenClosedDelegate onScreenClosed = null, string hideAnimation = null)
        {
            if (IsAnyScreenActive())
            {
                hideAnimation = (hideAnimation != null) ? hideAnimation : screen.GetComponent<ScreenController>().hideAnimation;
                PlayAnimation(screen, hideAnimation, 0, true, () => { onScreenClosed?.Invoke(); });
            }
        }

        public void ClearAllScreens()
        {
            // Destroy all screens
            while (IsAnyScreenActive())
            {
                var topScreen = GetTopScreen();

                // Remove from list before destroying will not triggered OnScreenDestroy
                RemoveTopScreenFromListInternal();

                DestroyScreenInternal(topScreen);
            }

            // Destroy all shields
            shieldManager.DestroyAllShields();

            // Reset count variables
            m_LoadingScreens = 0;
            m_AnimatingScreens = 0;
            m_PendingScreens = 0;
        }

        public void TryDestroyTopScreen()
        {
            if (IsAnyScreenActive())
            {
                var topScreen = GetTopScreen();
                DestroyScreenInternal(topScreen);
            }
        }

        public void TryDestroyScreen(Component screen)
        {
            if (screen != null && screen.gameObject != null)
            {
                DestroyScreenInternal(screen);
            }
        }

        private void DestroyScreenInternal(Component screen)
        {
            Destroy(screen.gameObject);
        }
        #endregion

        #region Add Screen
        public void StopAllAddScreenCoroutines()
        {
            for (int i = 0; i < screenCoroutines.Count; i++)
            {
                var sc = screenCoroutines[i];
                if (sc != null && sc.coroutine != null)
                {
                    StopCoroutine(sc.coroutine);
                    sc.coroutine = null;
                    Debug.LogWarning("CM: StopCoroutine " + sc.screenName.ToString());
                }
            }
            screenCoroutines.Clear();
        }

        public IEnumerator AddScreen<T>(string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", bool useExistingScreen = false, OnScreenLoadDelegate<T> onScreenLoad = null, bool hasShield = true, bool manually = true, AddConditionDelegate addCondition = null, bool waitUntilNoScreen = false, bool destroyTopScreen = false, bool hideTopScreen = true) where T : Component
        {
            // Wait conditions
            yield return WaitForAddConditions(addCondition, waitUntilNoScreen);

            // Update loading screen count
            m_LoadingScreens++;

            // Create Shield if no any screen active
            if (!IsAnyScreenActive() && hasShield)
            {
                CreateShield(true);
            }

            // Set fromScreen is the last loaded scene name (then will set it again after check the top screen)
            var fromScreen = GetLastLoadedSceneName();

            // Try find existing screen
            var hasExistingScreen = false; T existingScreen = null; int existingScreenIndex = 0;
            if (useExistingScreen)
            {
                hasExistingScreen = TryFindExistingScreen<T>(out existingScreen, out existingScreenIndex);
            }

            // Handle for destroyTopScreen, hideTopScreen, hasShield
            if (IsAnyScreenActive())
            {
                if (!useExistingScreen || !hasExistingScreen)
                {
                    var topScreen = GetTopScreen();
                    if (topScreen != null)
                    {
                        if (destroyTopScreen)
                        {
                            HandleDestroyTopScreen(topScreen, hasShield);
                        }
                        else
                        {
                            HandleHideTopScreen(topScreen, hasShield, hideTopScreen);
                        }
                        // Set fromScreen is the top screen name
                        fromScreen = topScreen.name;
                    }
                }
            }

            // Handle existing/new screen
            if (hasExistingScreen)
            {
                HandleExistingScreen(existingScreen, existingScreenIndex, onScreenLoad, screenName, fromScreen, manually);
            }
            else
            {
                HandleNewScreen(fromScreen, screenName, showAnimation, hideAnimation, animationObjectName, onScreenLoad, hasShield, manually);
            }
        }

        private IEnumerator WaitForAddConditions(AddConditionDelegate addCondition, bool waitUntilNoScreen)
        {
            // Wait until the addCondition() return true. This is a custom condition.
            while (addCondition != null && !addCondition()) yield return null;

            // Wait until no more screen is being loaded or animated
            while (IsAnyScreenLoading() || IsAnyScreenAnimating()) yield return null;

            // If waitUntilNoScreen is true, wait until no more screen is active or loading
            while (waitUntilNoScreen && (IsAnyScreenLoading() || IsAnyScreenActive())) yield return null;
        }

        private string GetLastLoadedSceneName()
        {
            return sceneManager.lastLoadedScene != null ? sceneManager.lastLoadedScene.name : string.Empty;
        }

        private bool TryFindExistingScreen<T>(out T existingScreen, out int index) where T : Component
        {
            for (int i = 0; i < m_ScreenList.Count; i++)
            {
                existingScreen = m_ScreenList[i].GetComponentInChildren<T>();
                if (existingScreen != null)
                {
                    index = i;
                    return true;
                }
            }
            existingScreen = null;
            index = -1;
            return false;
        }

        private void HandleDestroyTopScreen(Component topScreen, bool hasShield)
        {
            if (hasShield)
            {
                CreateShield(true);
            }
            DestroyScreenInternal(topScreen);
        }

        private void HandleHideTopScreen(Component topScreen, bool hasShield, bool hideTopScreen)
        {
            if (hideTopScreen)
            {
                topScreen.gameObject.SetActive(false);
            }
            else
            {
                if (hasShield)
                {
                    CreateShield(true);
                }
            }
        }

        private void HandleExistingScreen<T>(T screen, int index, OnScreenLoadDelegate<T> onScreenLoad, string screenName, string fromScreen, bool manually) where T : Component
        {
            // Find Screen's child index
            var screenChildIndex = FindChildIndex(screenContainer, screen.transform);

            // If found
            if (screenChildIndex >= 0)
            {
                // If not the lowest one
                if (screenChildIndex > 0)
                {
                    // Underlying object
                    Transform underlying = screenContainer.GetChild(screenChildIndex - 1);

                    // Overlying object
                    Transform overlying = null;
                    if (screenChildIndex + 1 < screenContainer.childCount)
                    {
                        overlying = screenContainer.GetChild(screenChildIndex + 1);
                    }

                    // If underlying object is a shield
                    if (IsShield(underlying))
                    {
                        // If no overlying object or it also is a shield
                        if (overlying == null || IsShield(overlying))
                        {
                            // Move the underlying shield to the highest position
                            underlying.transform.SetAsLastSibling();
                        }
                        else
                        {
                            // If overlying object is a screen, deactivate it
                            overlying.gameObject.SetActive(false);
                        }
                    }
                }

                // Move this screen to the highest position, play its show animation.
                screen.transform.SetAsLastSibling();
                screen.gameObject.SetActive(true);
                PlayAnimation(screen, screen.GetComponent<ScreenController>().showAnimation, 4);

                // Update the loading count
                m_LoadingScreens--;

                // Swap it with the top screen in the screen list
                var temp = m_ScreenList[index];
                m_ScreenList[index] = m_ScreenList[m_ScreenList.Count - 1];
                m_ScreenList[m_ScreenList.Count - 1] = temp;

                // Send OnScreenLoad event
                onScreenLoad?.Invoke(screen);

                // Update the pending count
                if (m_PendingScreens > 0)
                {
                    m_PendingScreens--;
                }

                // Send OnScreenAdded event
                OnScreenAdded?.Invoke(screenName, fromScreen, manually);
            }
        }

        private void HandleNewScreen<T>(string fromScreen, string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", OnScreenLoadDelegate<T> onScreenLoad = null, bool hasShield = true, bool manually = true) where T : Component
        {
#if ADDRESSABLE
            var async = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(screenName);
            async.Completed += (a => {
                if (a.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    CreateScreen<T>(async.Result, screenName, showAnimation, hideAnimation, animationObjectName, onScreenLoad, hasShield);
                    OnScreenAdded?.Invoke(screenName, fromScreen, manually);
                }
            });
#else
            var prefab = Resources.Load<GameObject>(Path.Combine(m_ScreenPath, screenName));
            CreateScreen<T>(prefab, screenName, showAnimation, hideAnimation, animationObjectName, onScreenLoad, hasShield);
            OnScreenAdded?.Invoke(screenName, fromScreen, manually);
#endif
        }
        #endregion

        #region On Screen Destroy
        public void OnScreenDestroy(Component screen)
        {
            // Only reveal underlying objects if the screen is in the screen list.
            // In ClearAllScreens function, we remove screens from the screen list first, then destroy them, then this reveal function will not be called. 
            if (TryRemoveScreenFromList(screen))
            {
                RevealUnderlyingScreenOrShield(screen);
            }
        }

        public void RevealUnderlyingScreenOrShield(Component screen)
        {
            var childCount = screenContainer.childCount;

            if (childCount > 0)
            {
                var childIndex = FindChildIndex(screenContainer, screen.transform);

                if (childIndex < 0)
                {
                    return;
                }

                // Check overlying screen
                var needProcessUnderlying = false;

                if (childIndex + 1 < childCount)
                {
                    var overlying = screenContainer.GetChild(childIndex + 1);
                    var overlyingScreen = overlying.GetComponent<ScreenController>();
                    if (overlyingScreen == null || !overlyingScreen.hasShield)
                    {
                        needProcessUnderlying = true;
                    }
                }
                else
                {
                    needProcessUnderlying = true;
                }

                if (needProcessUnderlying)
                {
                    ProcessUnderlyingRecursive(childCount, childIndex - 1);
                }
            }
        }

        private void ProcessUnderlyingRecursive(int childCount, int childIndex)
        {
            if (childIndex >= 0 && childIndex < childCount)
            {
                var top = screenContainer.GetChild(childIndex);

                var topScreen = top.GetComponent<ScreenController>();

                if (topScreen != null && !topScreen.beingDestroyed)
                {
                    if (!m_ShowAnimationOneTime)
                    {
                        if (topScreen.gameObject != null && !topScreen.gameObject.activeInHierarchy)
                        {
                            topScreen.gameObject.SetActive(true);

                            var topController = topScreen.GetComponent<ScreenController>();

                            PlayAnimation(topScreen, topController.showAnimation);
                        }
                    }
                }
                else
                {
                    var shield = top.GetComponent<ShieldController>();

                    if (shield != null && !shield.beingDestroyed)
                    {
                        HideScreenShield(shield);

                        ProcessUnderlyingRecursive(childCount, childIndex - 1);
                    }
                }
            }
        }
        #endregion

        #region Create Screen
        private void CreateScreen<T>(GameObject prefab, string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", OnScreenLoadDelegate<T> onScreenLoad = null, bool hasShield = true) where T : Component
        {
            T screen = Instantiate(prefab.GetComponent<T>(), screenContainer);

            screen.name = screenName;
            AddToContainer(screen.gameObject, screenContainer);

            var controller = AddScreenController(screen);
            controller.screen = screen;
            controller.showAnimation = showAnimation;
            controller.hideAnimation = hideAnimation;
            controller.animationObjectName = animationObjectName;
            controller.hasShield = hasShield;
            controller.screenManager = this;

            AddAnimations(screen, animationObjectName, showAnimation, hideAnimation);
            PlayAnimation(screen, showAnimation, 4);

            AddScreenToList(screen);

            onScreenLoad?.Invoke(screen);

            if (m_PendingScreens > 0)
            {
                m_PendingScreens--;
            }
        }

        public void AddToContainer(GameObject screen, RectTransform container)
        {
            screen.transform.SetParent(container);
            screen.transform.localPosition = Vector3.zero;
            screen.transform.localScale = Vector3.one;
            screen.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }

        private ScreenController AddScreenController(Component screen)
        {
            var controller = screen.GetComponent<ScreenController>();

            if (controller == null)
            {
                controller = screen.gameObject.AddComponent<ScreenController>();
            }

            return controller;
        }
        #endregion

        #region Shield
        private ShieldController CreateShield(bool showAfterCreate = false)
        {
            return shieldManager.CreateShield(showAfterCreate);
        }

        private void HideScreenShield(ShieldController shield)
        {
            shieldManager.HideScreenShield(shield);
        }
        #endregion

        #region Animation
        private Animation AddAnimations(Component screen, string animationObjectName = "", params string[] animationNames)
        {
            GameObject animObject = screen.gameObject;

            if (!string.IsNullOrEmpty(animationObjectName))
            {
                animObject = FindChildBFS(screen.gameObject, animationObjectName);

                if (animObject == null)
                {
                    animObject = screen.gameObject;
                }
            }

            var anim = animObject.GetComponent<Animation>();
            if (anim == null)
            {
                anim = animObject.AddComponent<Animation>();
            }

            var unscaledAnim = animObject.GetComponent<UnscaledAnimation>();
            if (unscaledAnim == null)
            {
                animObject.AddComponent<UnscaledAnimation>();
            }

            anim.playAutomatically = false;

            for (int i = 0; i < animationNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(animationNames[i]))
                {
                    if (anim.GetClip(animationNames[i]) == null)
                    {
                        var path = Path.Combine(m_ScreenAnimationPath, animationNames[i]);
                        var clip = Resources.Load<AnimationClip>(path);

                        if (clip == null)
                        {
                            var defaultPath = Path.Combine("Animations", animationNames[i]);
                            clip = Resources.Load<AnimationClip>(defaultPath);
                        }

                        if (clip != null)
                        {
                            anim.AddClip(clip, animationNames[i]);
                        }
                        else
                        {
                            Debug.LogWarning("Animation Clip not found: " + path);
                        }
                    }

                    if (animObject.GetComponent<CanvasGroup>() == null)
                    {
                        animObject.AddComponent<CanvasGroup>();
                    }

                    switch (animationNames[i])
                    {
                        case "RightShow":
                        case "LeftShow":
                        case "TopShow":
                        case "BottomShow":
                        case "RightHide":
                        case "LeftHide":
                        case "TopHide":
                        case "BottomHide":
                            if (animObject.GetComponent<AnimationPosition>() == null)
                            {
                                animObject.AddComponent<AnimationPosition>();
                            }
                            break;
                    }
                }
            }

            return anim;
        }

        private void PlayAnimation(Component screen, string animationName, int delayFrames = 0, bool destroyScreenAtAnimationEnd = false, OnAnimationEndedDelegate onAnimationEnd = null)
        {
            m_AnimatingScreens++;

            var anim = AddAnimations(screen, screen.GetComponent<ScreenController>().animationObjectName, animationName);

            StartCoroutine(CoPlayAnimation(anim, animationName, delayFrames, onAnimationEnd, destroyScreenAtAnimationEnd ? screen : null));
        }

        private IEnumerator CoPlayAnimation(Animation anim, string animationName, int delayFrames, OnAnimationEndedDelegate onAnimationEnd = null, Component screenToBeDestroyed = null)
        {
            if (anim.GetClip(animationName) != null)
            {
                // Show screen shield before playing animation
                shieldManager.transparentTopShield.SetActive(true);

                // Unscaled anim
                var unscaledAnim = anim.GetComponent<UnscaledAnimation>();
                unscaledAnim.PauseAtBeginning(animationName);

                // Reposition by screen width / height
                var animRepos = anim.GetComponent<AnimationPosition>();
                if (animRepos != null)
                {
                    animRepos.Reposition();
                }

                // Wating some frames for smooth
                for (int i = 0; i < delayFrames; i++)
                {
                    yield return 0;
                }

                // Play animation
                unscaledAnim.Play(animationName, speed: animationSpeed);

                // Wait animation
                yield return new WaitForSecondsRealtime(anim[animationName].length / animationSpeed);

                // Turn off screen shield after animation end
                shieldManager.transparentTopShield.SetActive(false);
            }

            if (screenToBeDestroyed != null)
            {
                DestroyScreenInternal(screenToBeDestroyed);
            }

            onAnimationEnd?.Invoke();

            m_AnimatingScreens--;
        }
        #endregion

        #region Find Algorithms
        private int FindChildIndex(Transform parent, Transform t)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (child == t)
                {
                    return i;
                }
            }

            return -1;
        }

        private GameObject FindChildBFS(GameObject parent, string name)
        {
            Queue<Transform> queue = new Queue<Transform>();

            queue.Enqueue(parent.transform);

            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();

                foreach (Transform child in current)
                {
                    if (child.name == name)
                    {
                        return child.gameObject;
                    }

                    queue.Enqueue(child);
                }
            }

            return null;
        }
        #endregion

        #region Screen List Operations
        private void AddScreenToList(Component screen)
        {
            m_LoadingScreens--;

            m_ScreenList.Add(screen);

            OnScreenChanged?.Invoke(m_ScreenList.Count);
        }

        private bool TryRemoveScreenFromList(Component screen)
        {
            if (screen != null && m_ScreenList.Contains(screen))
            {
                m_ScreenList.Remove(screen);

                OnScreenChanged?.Invoke(m_ScreenList.Count);

                return true;
            }

            return false;
        }

        private void RemoveTopScreenFromListInternal()
        {
            m_ScreenList.RemoveAt(m_ScreenList.Count - 1);
        }
        #endregion
    }
}