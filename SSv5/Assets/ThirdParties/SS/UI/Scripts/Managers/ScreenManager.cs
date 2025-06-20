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
using UnityEngine.SceneManagement;

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

        #region Static
        private static ScreenManager m_Instance;

        public static ScreenManager instance
        {
            get
            {
                if (m_Instance == null)
                {
                    Instantiate(Resources.Load<ScreenManager>("Prefabs/ScreenManager"));
                }

                return m_Instance;
            }
        }
        #endregion

        #region SerializeField
        [SerializeField] string m_ScreenPath = "Screens";
        [SerializeField] string m_ScreenAnimationPath = "Animations";
        [SerializeField] string m_SceneLoadingName;
        [SerializeField] string m_LoadingName;
        [SerializeField] string m_TooltipName;
        [SerializeField] Color m_ScreenShieldColor = new Color(0, 0, 0, 0.8f);
        [SerializeField] float m_ScreenAnimationSpeed = 1;
        [SerializeField] bool m_ShowAnimationOneTime = false;
        [SerializeField] bool m_CloseOnTappingShield = false;
        [SerializeField] Camera m_BackgroundCamera;
        [SerializeField] Canvas m_Canvas;
        [SerializeField] UnscaledAnimation m_SceneShield;
        [SerializeField] RectTransform m_ScreenContainer;
        [SerializeField] RectTransform m_TopContainer;
        [SerializeField] RectTransform m_ScreenLoadingContainer;
        [SerializeField] RectTransform m_ScreenShieldTopContainer;
        [SerializeField] RectTransform m_SceneLoadingContainer;
        #endregion

        #region Delegate
        public delegate void OnSceneLoad<T>(T t);
        public delegate void OnScreenLoad<T>(T t);
        public delegate void Callback();
        public delegate void OnScreenAddedDelegate(string toScreen, string fromScreen, bool manually);
        public delegate void OnScreenChangedDelegate(int screenCount);
        public delegate bool AddConditionDelegate();
        #endregion

        #region Events
        public OnScreenAddedDelegate OnScreenAdded;
        public OnScreenChangedDelegate OnScreenChanged;
        #endregion

        #region Private Member
        private Scene m_LastLoadedScene;
        private List<Component> m_ScreenList = new List<Component>();
        private GameObject m_SceneLoading;
        private GameObject m_Loading;
        private TooltipBaseController m_Tooltip;
        private List<UnscaledAnimation> m_ShieldList = new List<UnscaledAnimation>();
        private GameObject m_ScreenShieldTop;
        private int m_PendingToLoadScreens = 0;
        private int m_LoadingScreens = 0;
        private int m_AnimationPlayingScreens = 0;
        private List<ScreenCoroutine> m_ScreenCoroutines = new List<ScreenCoroutine>();
        private Coroutine m_LoadingCoroutine;
        private bool m_IsLoading;
        #endregion

        #region Public Get/Set
        public AsyncOperation asyncOperation
        {
            get;
            protected set;
        }

        public int pendingToLoadScreens
        {
            get
            {
                return m_PendingToLoadScreens;
            }

            set
            {
                m_PendingToLoadScreens = value;
            }
        }

        public List<ScreenCoroutine> screenCoroutines
        {
            get
            {
                return m_ScreenCoroutines;
            }
        }

        public RectTransform screenContainer
        {
            get
            {
                return m_ScreenContainer;
            }
        }

        public RectTransform topContainer
        {
            get
            {
                return m_TopContainer;
            }
        }

        public List<UnscaledAnimation> shieldList
        {
            get
            {
                return m_ShieldList;
            }
        }

        public float screenAnimationSpeed
        {
            get
            {
                return m_ScreenAnimationSpeed;
            }
        }

        public List<Component> screenList
        {
            get
            {
                return m_ScreenList;
            }
        }

        public int loadingScreens
        {
            get
            {
                return m_LoadingScreens;
            }
        }

        public int animationPlayingScreens
        {
            get
            {
                return m_AnimationPlayingScreens;
            }
        }
        #endregion

        #region Unity Functions
        private void Awake()
        {
            if (m_Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                m_Instance = this;
                DontDestroyOnLoad(gameObject);

                name = "ScreenManager";

                Application.targetFrameRate = 60;

                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
                UnityEngine.SceneManagement.SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;

                SetupCameras();
                SetupCanvases();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (m_Loading == null || !m_Loading.activeInHierarchy)
                {
                    if (m_ScreenList.Count > 0)
                    {
                        var screen = m_ScreenList[m_ScreenList.Count - 1];

                        if (screen.TryGetComponent(out IKeyBack keyback))
                        {
                            keyback.OnKeyBack();
                        }
                        else
                        {
                            CloseScreen();
                        }
                    }
                }
            }
        }

        private void SceneManager_sceneUnloaded(Scene scene)
        {
            m_BackgroundCamera.gameObject.SetActive(true);
        }

        private void SceneManager_activeSceneChanged(Scene scene1, Scene scene2)
        {
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetupCameras();
            SetupCanvases();

            m_LastLoadedScene = scene;
        }
        #endregion

        #region Public Functions
        public void Setup(Color screenShieldColor, string screenPath = "Screens", string screenAnimationPath = "Animations", string sceneLoadingName = "", string loadingName = "", float screenAnimationSpeed = 1, string tooltipName = "", bool showAnimationOneTime = false, bool closeOnTappingShield = false)
        {
            m_ScreenShieldColor = screenShieldColor;
            Setup(screenPath, screenAnimationPath, sceneLoadingName, loadingName, screenAnimationSpeed, tooltipName, showAnimationOneTime, closeOnTappingShield);
        }

        public void Setup(string screenPath = "Screens", string screenAnimationPath = "Animations", string sceneLoadingName = "", string loadingName = "", float screenAnimationSpeed = 1, string tooltipName = "", bool showAnimationOneTime = false, bool closeOnTappingShield = false)
        {
            m_ScreenPath = screenPath;
            m_ScreenAnimationPath = screenAnimationPath;
            m_SceneLoadingName = sceneLoadingName;
            m_LoadingName = loadingName;
            m_TooltipName = tooltipName;
            m_ShowAnimationOneTime = showAnimationOneTime;
            m_CloseOnTappingShield = closeOnTappingShield;

            if (screenAnimationSpeed > 0)
            {
                m_ScreenAnimationSpeed = screenAnimationSpeed;
            }
        }

        public void LoadScene<T>(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreen = true) where T : Component
        {
            StartCoroutine(CoLoadScene(sceneName, mode, onSceneLoaded, clearAllScreen));
        }
        #endregion

        #region Private Functions

        private void OnShieldTap()
        {
            CloseScreen();
        }

        private IEnumerator CoLoadScene<T>(string sceneName, LoadSceneMode mode, OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreen = true) where T : Component
        {
            m_SceneShield.transform.SetAsLastSibling();

            if (mode == LoadSceneMode.Single)
            {
                m_SceneShield.Play("ShieldShow", speed: m_ScreenAnimationSpeed);

                yield return new WaitForSecondsRealtime(m_SceneShield.GetLength("ShieldShow") / m_ScreenAnimationSpeed);

                if (clearAllScreen)
                {
                    DestroyShield();

                    ClearAllScreen();
                }
            }

            if (mode == LoadSceneMode.Single && !string.IsNullOrEmpty(m_SceneLoadingName))
            {
                if (m_SceneLoading == null)
                {
#if ADDRESSABLE
                var async = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(m_SceneLoadingName);
                async.Completed += (a => {
                    if (a.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        CreateSceneLoading(async.Result);
                        ShowSceneLoading();
                    }
                });
#else
                    var prefab = Resources.Load<GameObject>(Path.Combine(m_ScreenPath, m_SceneLoadingName));
                    CreateSceneLoading(prefab);
                    ShowSceneLoading();
#endif
                }
                else
                {
                    ShowSceneLoading();
                }
            }

            asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
            asyncOperation.completed += (asyncOp) =>
            {
                onSceneLoaded?.Invoke(GetSceneComponent<T>(instance.m_LastLoadedScene));
            };

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            if (mode == LoadSceneMode.Single)
            {
                m_SceneShield.Play("ShieldHide", speed: m_ScreenAnimationSpeed);
            }

            if (mode == LoadSceneMode.Single && !string.IsNullOrEmpty(m_SceneLoadingName))
            {
                while (m_SceneLoading == null)
                {
                    yield return 0;
                }
            }

            if (m_SceneLoading != null)
            {
                yield return 0;

                m_SceneLoading.SetActive(false);
            }
        }

        public void DestroyShield()
        {
            for (int i = 0; i < m_ShieldList.Count; i++)
            {
                var shield = m_ShieldList[i];
                Destroy(shield.gameObject);
            }

            m_ShieldList.Clear();
        }

        private void CreateSceneLoading(GameObject prefab)
        {
            m_SceneLoading = Instantiate(prefab);
            m_SceneLoading.name = m_SceneLoadingName;
            AddToContainer(m_SceneLoading, m_SceneLoadingContainer);
        }

        private void ShowSceneLoading()
        {
            m_SceneLoading.SetActive(true);
        }

        public IEnumerator AddScreen<T>(string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", bool useExistingScreen = false, OnScreenLoad<T> onScreenLoad = null, bool hasShield = true, bool manually = true, AddConditionDelegate addCondition = null, bool waitUntilNoScreen = false, bool destroyTopScreen = false, bool hideTopScreen = true) where T : Component
        {
            // Wait
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

            // Create Shield Top
            if (m_ScreenShieldTop == null)
            {
                m_ScreenShieldTop = CreateTransparentShield();
                m_ScreenShieldTop.SetActive(false);
            }

            var fromScreen = m_LastLoadedScene != null ? m_LastLoadedScene.name : string.Empty;

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
                            DestroyScreen(topScreen);
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
                    else
                    {

                    }
                }
            }

            // Process Existing Screen
            T screen = null;
            if (hasExistingScreen)
            {
                screen = existingScreen;

                var screenChildIndex = FindChildIndex(m_ScreenContainer, screen.transform);
                if (screenChildIndex >= 0)
                {
                    if (screenChildIndex > 0)
                    {
                        Transform below = m_ScreenContainer.GetChild(screenChildIndex - 1);
                        Transform above = null;

                        if (screenChildIndex + 1 < m_ScreenContainer.childCount)
                        {
                            above = m_ScreenContainer.GetChild(screenChildIndex + 1);
                        }

                        // Check Below, Above
                        if (below.GetComponent<ScreenController>() == null)
                        {
                            if (above == null || above.GetComponent<ScreenController>() == null)
                            {
                                below.transform.SetAsLastSibling();
                            }
                            else
                            {
                                above.gameObject.SetActive(false);
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

        private void CreateScreen<T>(GameObject prefab, string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", OnScreenLoad<T> onScreenLoad = null, bool hasShield = true) where T : Component
        {
            T screen = Instantiate(prefab.GetComponent<T>(), m_ScreenContainer);

            screen.name = screenName;
            AddToContainer(screen.gameObject, m_ScreenContainer);

            var controller = AddScreenController(screen);
            controller.screen = screen;
            controller.showAnimation = showAnimation;
            controller.hideAnimation = hideAnimation;
            controller.animationObjectName = animationObjectName;
            controller.hasShield = hasShield;

            AddAnimations(screen, animationObjectName, showAnimation, hideAnimation);
            PlayAnimation(screen, showAnimation, 4);

            AddScreenToList(screen);

            onScreenLoad?.Invoke(screen);

            if (m_PendingToLoadScreens > 0)
            {
                m_PendingToLoadScreens--;
            }
        }

        public void CloseScreen(Callback onScreenClosed = null, string hideAnimation = null)
        {
            if (m_ScreenList.Count > 0)
            {
                var screen = m_ScreenList[m_ScreenList.Count - 1];
                CloseScreen(screen, onScreenClosed, hideAnimation);
            }

            if (m_ShowAnimationOneTime && m_ScreenList.Count > 0)
            {
                var topScreen = m_ScreenList[m_ScreenList.Count - 1];

                if (topScreen != null)
                {
                    topScreen.gameObject.SetActive(true);
                }
            }
        }

        public void CloseScreen(Component screen, Callback onScreenClosed = null, string hideAnimation = null)
        {
            if (m_ScreenList.Count > 0)
            {
                RemoveScreenFromList(screen);

                hideAnimation = (hideAnimation != null) ? hideAnimation : screen.GetComponent<ScreenController>().hideAnimation;
                PlayAnimation(screen, hideAnimation, 0, true, onScreenClosed);
            }
        }

        public void ClearAllScreen()
        {
            while (m_ScreenList.Count > 0)
            {
                var screen = m_ScreenList[0];
                m_ScreenList.RemoveAt(0);

                DestroyScreen(screen);
            }

            m_LoadingScreens = 0;
            m_AnimationPlayingScreens = 0;
            m_PendingToLoadScreens = 0;
        }

        public void DestroyScreen()
        {
            if (m_ScreenList.Count > 0)
            {
                var screen = m_ScreenList[m_ScreenList.Count - 1];

                DestroyScreen(screen);
            }
        }

        public void DestroyScreen(Component screen)
        {
            if (screen != null && screen.gameObject != null)
            {
                Destroy(screen.gameObject);
            }
        }

        public void ShowLoading(bool isShow, float timeout = 0)
        {
            m_IsLoading = isShow;
            if (isShow)
            {
                if (!string.IsNullOrEmpty(m_LoadingName))
                {
                    if (m_Loading == null)
                    {
#if ADDRESSABLE
                    var async = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(m_LoadingName);
                    async.Completed += (a => {
                        if (a.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                        {
                            CreateLoading(async.Result);
                            ShowLoading(timeout);
                        }
                    });
#else
                        var prefab = Resources.Load<GameObject>(Path.Combine(m_ScreenPath, m_LoadingName));
                        CreateLoading(prefab);
                        ShowLoading(timeout);
#endif
                    }
                    else
                    {
                        ShowLoading(timeout);
                    }
                }
            }
            else
            {
                HideLoading();
            }
        }

        private void CreateLoading(GameObject prefab)
        {
            m_Loading = Instantiate(prefab);
            m_Loading.name = m_LoadingName;
            m_Loading.SetActive(false);
            AddToContainer(m_Loading, m_ScreenLoadingContainer);
        }

        private void ShowLoading(float timeout = 0)
        {
            if (m_IsLoading)
            {
                StopLoadingCoroutine();

                if (timeout > 0)
                {
                    m_LoadingCoroutine = StartCoroutine(CoShowLoading(timeout));
                }
                else
                {
                    m_Loading.SetActive(true);
                }
            }
        }

        private void HideLoading()
        {
            StopLoadingCoroutine();

            if (m_Loading != null)
            {
                m_Loading.SetActive(false);
            }
        }

        private void StopLoadingCoroutine()
        {
            if (m_LoadingCoroutine != null)
            {
                StopCoroutine(m_LoadingCoroutine);
                m_LoadingCoroutine = null;
            }
        }

        private IEnumerator CoShowLoading(float timeout)
        {
            m_Loading.SetActive(true);

            yield return new WaitForSecondsRealtime(timeout);

            m_Loading.SetActive(false);
        }

        public void AddToContainer(GameObject screen, RectTransform container)
        {
            screen.transform.SetParent(container);
            screen.transform.localPosition = Vector3.zero;
            screen.transform.localScale = Vector3.one;
            screen.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }

        private UnscaledAnimation CreateShield(bool showAfterCreate = false)
        {
            var shield = Instantiate(Resources.Load<GameObject>("Prefabs/Shield"), m_ScreenContainer).GetComponent<UnscaledAnimation>();
            shield.name = "Screen Shield";
            shield.transform.SetAsLastSibling();
            shield.gameObject.SetActive(false);

            UpdateScreenShieldColor(shield);
            AddShieldTapEvent(shield);
            m_ShieldList.Add(shield);

            if (showAfterCreate)
            {
                ShowScreenShield(shield);
            }

            return shield;
        }

        private void UpdateScreenShieldColor(UnscaledAnimation shield)
        {
            var image = shield.GetComponent<Image>();
            image.color = instance.m_ScreenShieldColor;
        }

        private void ShowScreenShield(UnscaledAnimation shield)
        {
            if (!shield.gameObject.activeInHierarchy || (shield.isPlaying && shield.currentClipName == "ShieldHide"))
            {
                shield.gameObject.SetActive(true);
                shield.Play("ShieldShow", speed: m_ScreenAnimationSpeed);
            }
        }

        private void HideScreenShield(UnscaledAnimation shield)
        {
            if (shield.gameObject.activeInHierarchy)
            {
                shield.Play("ShieldHide", (anim) => {
                    m_ShieldList.Remove(shield);
                    Destroy(shield.gameObject);
                }, speed: m_ScreenAnimationSpeed);
            }
        }

        private void AddShieldTapEvent(UnscaledAnimation shield)
        {
            if (m_CloseOnTappingShield)
            {
                var eventTrigger = shield.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
                entry.callback.AddListener((eventData) => { OnShieldTap(); });

                eventTrigger.triggers.Add(entry);
            }
        }

        private GameObject CreateTransparentShield()
        {
            var shield = Instantiate(Resources.Load<GameObject>("Prefabs/TransparentShield"), m_ScreenShieldTopContainer.transform);
            shield.name = "Transparent Shield";

            var image = shield.GetComponent<Image>();
            image.color = new Color(0, 0, 0, 0);

            return shield;
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
                m_ScreenShieldTop.SetActive(true);

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
                unscaledAnim.Play(animationName, speed: m_ScreenAnimationSpeed);

                // Wait animation
                yield return new WaitForSecondsRealtime(anim[animationName].length / m_ScreenAnimationSpeed);

                // Turn off screen shield after animation end
                m_ScreenShieldTop.SetActive(false);
            }

            if (screenToBeDestroyed != null)
            {
                DestroyScreen(screenToBeDestroyed);
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

        private void SetupCameras()
        {
            var cameras = FindObjectsOfType<Camera>();

            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != m_BackgroundCamera)
                {
                    if (cameras[i].clearFlags == CameraClearFlags.Skybox || cameras[i].clearFlags == CameraClearFlags.SolidColor)
                    {
                        m_BackgroundCamera.gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }

        private void SetupCanvases()
        {
            var screenRatio = (float)Screen.width / Screen.height;

            var canvasScalers = FindObjectsOfType<CanvasScaler>(true);
            for (int i = 0; i < canvasScalers.Length; i++)
            {
                SetupCanvasScaler(canvasScalers[i], screenRatio);
            }
        }

        private void SetupCanvasScaler(CanvasScaler canvasScaler, float screenRatio)
        {
            canvasScaler.matchWidthOrHeight = screenRatio > 0.44f ? 1f : 0f;
        }

        private T GetSceneComponent<T>(Scene scene) where T : Component
        {
            var objects = scene.GetRootGameObjects();

            for (int i = 0; i < objects.Length; i++)
            {
                var t = objects[i].GetComponentInChildren<T>();

                if (t != null)
                {
                    return t;
                }
            }

            return null;
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

        public void HideScreenShieldOrShowTop(Component screen)
        {
            var childCount = m_ScreenContainer.childCount;

            if (childCount > 0)
            {
                var childIndex = FindChildIndex(m_ScreenContainer, screen.transform);

                if (childIndex < 0)
                {
                    return;
                }

                // Check higher screen
                var needProcessBelow = false;

                if (childIndex + 1 < childCount)
                {
                    var higher = m_ScreenContainer.GetChild(childIndex + 1);
                    var higherScreen = higher.GetComponent<ScreenController>();
                    if (higherScreen == null || !higherScreen.hasShield)
                    {
                        needProcessBelow = true;
                    }
                }
                else
                {
                    needProcessBelow = true;
                }

                if (needProcessBelow)
                {
                    ProcessBelowRecursive(childCount, childIndex - 1);
                }
            }
        }

        private void ProcessBelowRecursive(int childCount, int childIndex)
        {
            if (childIndex >= 0 && childIndex < childCount)
            {
                var top = m_ScreenContainer.GetChild(childIndex);
                var topScreen = top.GetComponent<ScreenController>();

                if (topScreen != null)
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
                    var shield = top.GetComponent<UnscaledAnimation>();

                    if (shield != null)
                    {
                        HideScreenShield(shield);

                        ProcessBelowRecursive(childCount, childIndex - 1);
                    }
                }
            }
        }

        public void AddScreenToList(Component screen)
        {
            m_LoadingScreens--;

            m_ScreenList.Add(screen);

            OnScreenChanged?.Invoke(m_ScreenList.Count);
        }

        public void RemoveScreenFromList(Component screen)
        {
            if (m_ScreenList.Contains(screen))
            {
                m_ScreenList.Remove(screen);

                OnScreenChanged?.Invoke(m_ScreenList.Count);
            }
        }

        private void CreateAndShowTooltip(GameObject tooltipPrefab, string text, Vector3 worldPosition, float targetY)
        {
            var tooltip = Instantiate(tooltipPrefab, m_TopContainer);
            m_Tooltip = tooltip.GetComponent<TooltipBaseController>();

            if (m_Tooltip != null)
            {
                m_Tooltip.ShowTooltip(text, worldPosition, targetY);
            }
        }

        public void LoadAndShowTooltip(string text, Vector3 worldPosition, float targetY = 100f)
        {
            if (string.IsNullOrEmpty(m_TooltipName))
                return;

            if (m_Tooltip != null)
            {
                m_Tooltip.transform.SetParent(m_TopContainer, true);
                m_Tooltip.ShowTooltip(text, worldPosition, targetY);
                return;
            }

#if ADDRESSABLE
            var async = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(m_TooltipName);
            async.Completed += (a => {
                if (a.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    CreateAndShowTooltip(async.Result, text, worldPosition, targetY);
                }
            });
#else
            var tooltipPrefab = Resources.Load<GameObject>(Path.Combine(m_ScreenPath, m_TooltipName));
            CreateAndShowTooltip(tooltipPrefab, text, worldPosition, targetY);
#endif
        }

        public void HideTooltipImmediately()
        {
            if (m_Tooltip != null)
            {
                m_Tooltip.HideToolTip();
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
        #endregion
    }
}