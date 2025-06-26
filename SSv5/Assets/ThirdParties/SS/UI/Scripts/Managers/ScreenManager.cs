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
        public delegate void OnScreenLoad<T>(T t);
        public delegate void Callback();
        public delegate void OnScreenAddedDelegate(string toScreen, string fromScreen, bool manually);
        public delegate void OnScreenChangedDelegate(int screenCount);
        public delegate bool AddConditionDelegate();

        public OnScreenAddedDelegate OnScreenAdded;
        public OnScreenChangedDelegate OnScreenChanged;
        #endregion

        #region Private Fields
        private List<Component> m_ScreenList = new List<Component>();
        private List<ScreenCoroutine> m_ScreenCoroutines = new List<ScreenCoroutine>();
        private int m_PendingToLoadScreens = 0;
        private int m_LoadingScreens = 0;
        private int m_AnimationPlayingScreens = 0;
        #endregion

        #region Public Properties
        public SceneManager sceneManager { get => m_SceneManager; set => m_SceneManager = value; }
        public ShieldManager shieldManager { get => m_ShieldManager; set => m_ShieldManager = value; }
        public LoadingManager loadingManager { get => m_LoadingManager; set => m_LoadingManager = value; }
        public GeneralManager generalManager { get => m_GeneralManager; set => m_GeneralManager = value; }
        public int pendingToLoadScreens { get => m_PendingToLoadScreens; set => m_PendingToLoadScreens = value; }
        public List<ScreenCoroutine> screenCoroutines => m_ScreenCoroutines;
        public Canvas canvas => m_GeneralManager.canvas;
        public RectTransform screenContainer => m_GeneralManager.screenContainer;
        public RectTransform topContainer => m_GeneralManager.topContainer;
        public float animationSpeed => m_GeneralManager.animationSpeed;
        #endregion

        #region Private Properties
        private bool IsLoadingVisible() => loadingManager.loadingObject != null && loadingManager.loadingObject.activeInHierarchy;
        private bool IsAnyScreenActive() => m_ScreenList.Count > 0;
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

        #region Escape Key Logic
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

        #region Public Functions
        public void Setup(string screenPath = "Screens", string screenAnimationPath = "Animations", bool showAnimationOneTime = false)
        {
            m_ScreenPath = screenPath;
            m_ScreenAnimationPath = screenAnimationPath;
            m_ShowAnimationOneTime = showAnimationOneTime;
        }

        public void CloseScreen(Callback onScreenClosed = null, string hideAnimation = null)
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

        public void CloseScreen(Component screen, Callback onScreenClosed = null, string hideAnimation = null)
        {
            if (IsAnyScreenActive())
            {
                hideAnimation = (hideAnimation != null) ? hideAnimation : screen.GetComponent<ScreenController>().hideAnimation;
                PlayAnimation(screen, hideAnimation, 0, true, onScreenClosed);
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
            m_AnimationPlayingScreens = 0;
            m_PendingToLoadScreens = 0;
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

        public IEnumerator AddScreen<T>(string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", bool useExistingScreen = false, OnScreenLoad<T> onScreenLoad = null, bool hasShield = true, bool manually = true, AddConditionDelegate addCondition = null, bool waitUntilNoScreen = false, bool destroyTopScreen = false, bool hideTopScreen = true) where T : Component
        {
            // Wait conditions
            while (addCondition != null && !addCondition())
            {
                yield return 0;
            }

            while (m_LoadingScreens > 0 || m_AnimationPlayingScreens > 0)
            {
                yield return 0;
            }

            while (waitUntilNoScreen && (m_LoadingScreens > 0 || m_ScreenList.Count > 0))
            {
                yield return 0;
            }

            m_LoadingScreens++;

            // Create Shield
            if (m_ScreenList.Count == 0 && hasShield)
            {
                CreateShield(true);
            }

            // From screen
            var fromScreen = sceneManager.lastLoadedScene != null ? sceneManager.lastLoadedScene.name : string.Empty;

            // Check Exist Screen
            var hasExistingScreen = false;
            T existingScreen = null;
            int existingScreenIndex = 0;

            if (useExistingScreen)
            {
                for (int i = 0; i < m_ScreenList.Count; i++)
                {
                    existingScreen = m_ScreenList[i].GetComponentInChildren<T>();

                    if (existingScreen != null)
                    {
                        hasExistingScreen = true;
                        existingScreenIndex = i;

                        break;
                    }
                }
            }

            // Check destroyTopScreen, hideTopScreen, hasShield
            if (m_ScreenList.Count > 0)
            {
                if (!useExistingScreen || !hasExistingScreen)
                {
                    var topScreen = m_ScreenList[m_ScreenList.Count - 1];

                    if (topScreen != null)
                    {
                        if (destroyTopScreen)
                        {
                            if (hasShield)
                            {
                                CreateShield(true);
                            }
                            DestroyScreenInternal(topScreen);
                        }
                        else
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

                        fromScreen = topScreen.name;
                    }
                }
            }

            // Process Existing Screen
            T screen = null;
            if (hasExistingScreen)
            {
                screen = existingScreen;

                var screenChildIndex = FindChildIndex(screenContainer, screen.transform);
                if (screenChildIndex >= 0)
                {
                    if (screenChildIndex > 0)
                    {
                        Transform underlying = screenContainer.GetChild(screenChildIndex - 1);
                        Transform overlying = null;

                        if (screenChildIndex + 1 < screenContainer.childCount)
                        {
                            overlying = screenContainer.GetChild(screenChildIndex + 1);
                        }

                        // Check underlying, overlying screen / shield
                        if (underlying.GetComponent<ScreenController>() == null)
                        {
                            if (overlying == null || overlying.GetComponent<ScreenController>() == null)
                            {
                                underlying.transform.SetAsLastSibling();
                            }
                            else
                            {
                                overlying.gameObject.SetActive(false);
                            }
                        }
                    }

                    screen.transform.SetAsLastSibling();
                    screen.gameObject.SetActive(true);
                    PlayAnimation(screen, screen.GetComponent<ScreenController>().showAnimation, 4);

                    m_LoadingScreens--;

                    var temp = m_ScreenList[existingScreenIndex];
                    m_ScreenList[existingScreenIndex] = m_ScreenList[m_ScreenList.Count - 1];
                    m_ScreenList[m_ScreenList.Count - 1] = temp;

                    onScreenLoad?.Invoke(screen);

                    if (m_PendingToLoadScreens > 0)
                    {
                        m_PendingToLoadScreens--;
                    }
                }
            }

            if (!hasExistingScreen)
            {
#if ADDRESSABLE
                var async = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(screenName);
                async.Completed += (a => {
                    if (a.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        CreateScreen<T>(async.Result, screenName, showAnimation, hideAnimation, animationObjectName, onScreenLoad, hasShield);
                    }
                });
#else
                var prefab = Resources.Load<GameObject>(Path.Combine(m_ScreenPath, screenName));
                CreateScreen<T>(prefab, screenName, showAnimation, hideAnimation, animationObjectName, onScreenLoad, hasShield);
#endif
            }

            OnScreenAdded?.Invoke(screenName, fromScreen, manually);
        }

        public void AddToContainer(GameObject screen, RectTransform container)
        {
            screen.transform.SetParent(container);
            screen.transform.localPosition = Vector3.zero;
            screen.transform.localScale = Vector3.one;
            screen.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }

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

        public void OnScreenDestroy(Component screen)
        {
            if (TryRemoveScreenFromList(screen))
            {
                RevealUnderlyingScreenOrShield(screen);
            }
        }

        public bool IsNoMoreScreen()
        {
            return (m_ScreenList.Count <= 0 && m_LoadingScreens <= 0 && m_AnimationPlayingScreens <= 0);
        }
        #endregion

        #region Private Functions
        private void CreateScreen<T>(GameObject prefab, string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", OnScreenLoad<T> onScreenLoad = null, bool hasShield = true) where T : Component
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

            if (m_PendingToLoadScreens > 0)
            {
                m_PendingToLoadScreens--;
            }
        }

        private ShieldController CreateShield(bool showAfterCreate = false)
        {
            return shieldManager.CreateShield(showAfterCreate);
        }

        private void HideScreenShield(ShieldController shield)
        {
            shieldManager.HideScreenShield(shield);
        }

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

        private void PlayAnimation(Component screen, string animationName, int delayFrames = 0, bool destroyScreenAtAnimationEnd = false, Callback onAnimationEnd = null)
        {
            m_AnimationPlayingScreens++;

            var anim = AddAnimations(screen, screen.GetComponent<ScreenController>().animationObjectName, animationName);

            StartCoroutine(CoPlayAnimation(anim, animationName, delayFrames, onAnimationEnd, destroyScreenAtAnimationEnd ? screen : null));
        }

        private IEnumerator CoPlayAnimation(Animation anim, string animationName, int delayFrames, Callback onAnimationEnd = null, Component screenToBeDestroyed = null)
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

            m_AnimationPlayingScreens--;
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
                            Debug.Log("topScreen: " + topScreen.name);
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

        private void DestroyScreenInternal(Component screen)
        {
            Destroy(screen.gameObject);
        }
        #endregion
    }
}