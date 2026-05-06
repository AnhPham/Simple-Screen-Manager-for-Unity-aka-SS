/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SS.UI
{
    public enum ScreenAnimation
    {
        None,       // No animation
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
            public Coroutine Coroutine { get; set; }
            public string ScreenName { get; set; }

            public ScreenCoroutine(Coroutine coroutine, string screenName)
            {
                this.Coroutine = coroutine;
                this.ScreenName = screenName;
            }
        }
        #endregion

        #region Serialize Fields
        [SerializeField] string _screenPath = "Screens";
        [SerializeField] string _screenAnimationPath = "Animations";
        [SerializeField] bool _showAnimationOneTime = false;
        [SerializeField] SceneManager _sceneManager;
        [SerializeField] ShieldManager _shieldManager;
        [SerializeField] LoadingManager _loadingManager;
        [SerializeField] GeneralManager _generalManager;
        #endregion

        #region Delegates & Events
        public delegate void OnScreenLoadDelegate<T>(T t);
        public delegate void OnScreenClosedDelegate();
        public delegate void OnAnimationEndedDelegate();
        public delegate void OnScreenAddedDelegate(string toScreen, string fromScreen, bool manually);
        public delegate void OnScreenChangedDelegate(int screenCount);
        public delegate bool AddConditionDelegate();
        public delegate IEnumerator OnScreenPreLoadDelegate();

        public OnScreenAddedDelegate OnScreenAdded;
        public OnScreenChangedDelegate OnScreenChanged;
        #endregion

        #region Protected Fields
        protected List<Component> _screenList = new List<Component>();
        protected List<GameObject> _screenshieldList = new List<GameObject>();
        protected List<ScreenCoroutine> _screenCoroutines = new List<ScreenCoroutine>();
        protected int _pendingScreens = 0;   // Number of screens is pending to load
        protected int _loadingScreens = 0;   // Number of screens is being loaded
        protected int _animatingScreens = 0; // Number of screens is being animated (show/hide)
        #endregion

        #region Public Properties
        public SceneManager SceneManager { get => _sceneManager; set => _sceneManager = value; }
        public ShieldManager ShieldManager { get => _shieldManager; set => _shieldManager = value; }
        public LoadingManager LoadingManager { get => _loadingManager; set => _loadingManager = value; }
        public GeneralManager GeneralManager { get => _generalManager; set => _generalManager = value; }
        public int PendingScreens { get => _pendingScreens; set => _pendingScreens = value; }
        public bool IsNoMoreScreen => _screenList.Count <= 0 && _loadingScreens <= 0 && _animatingScreens <= 0;
        public List<ScreenCoroutine> ScreenCoroutines => _screenCoroutines;
        public RectTransform ScreenContainer => _generalManager.ScreenContainer;
        public RectTransform TopContainer => _generalManager.TopContainer;
        #endregion

        #region Protected Properties
        protected float AnimationSpeed => _generalManager.AnimationSpeed;
        #endregion

        #region protected Short Function
        protected bool IsLoadingVisible() => LoadingManager.LoadingObject != null && LoadingManager.LoadingObject.activeInHierarchy;
        protected bool IsAnyScreenActive() => _screenList.Count > 0;
        protected bool IsAnyScreenLoading() => _loadingScreens > 0;
        protected bool IsAnyScreenAnimating() => _animatingScreens > 0;
        protected bool IsAnyScreenPending() => _pendingScreens > 0;
        protected bool IsScreen(Transform t) => t.GetComponent<ScreenController>() != null;
        protected bool IsShield(Transform t) => t.GetComponent<ShieldController>() != null;
        protected Component GetTopScreen() => _screenList[_screenList.Count - 1];
        protected Component Get2ndScreen() => _screenList[_screenList.Count - 2];
        #endregion

        #region Unity Cycle
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            GeneralManager = FindAnyObjectByType<GeneralManager>();
        }

        protected virtual void Update()
        {
            if (IsEscapePressedThisFrame())
            {
                HandleEscapeKey();
            }
        }
        #endregion

        #region Escape Key
        protected virtual bool IsEscapePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        protected virtual void HandleEscapeKey()
        {
            if (!IsLoadingVisible() && IsAnyScreenActive())
            {
                Component topScreen = GetTopScreen();
                if (TryHandleKeyBack(topScreen)) return;
                Close();
            }
        }

        protected virtual bool TryHandleKeyBack(Component screen)
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
        public virtual void Setup(string screenPath = "Screens", string screenAnimationPath = "Animations", bool showAnimationOneTime = false)
        {
            _screenPath = screenPath;
            _screenAnimationPath = screenAnimationPath;
            _showAnimationOneTime = showAnimationOneTime;
        }
        #endregion

        #region Close & Destroy
        public virtual void Close(OnScreenClosedDelegate onScreenClosed = null, string hideAnimation = null)
        {
            if (IsAnyScreenActive())
            {
                var topScreen = GetTopScreen();

                if (topScreen != null)
                {
                    Close(topScreen, onScreenClosed, hideAnimation);
                }
            }

            // if show animation one time, activate the underlying screen right after close top screen is called, before its hide animation is started 
            if (_showAnimationOneTime && IsAnyScreenActive())
            {
                ActivateTopScreen();
            }
        }

        protected virtual void ActivateTopScreen()
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

        public virtual void Close(Component screen, OnScreenClosedDelegate onScreenClosed = null, string hideAnimation = null)
        {
            if (IsAnyScreenActive())
            {
                // Remove from the list, handle underlying objects before destroying it
                OnScreenClosed(screen);

                hideAnimation = (hideAnimation != null) ? hideAnimation : screen.GetComponent<ScreenController>().HideAnimation;
                PlayAnimation(screen, hideAnimation, 0, true, () => { onScreenClosed?.Invoke(); });
            }
        }

        public virtual void ClearAllScreens()
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
            ShieldManager.DestroyAllShields();

            // Reset count variables
            _loadingScreens = 0;
            _animatingScreens = 0;
            _pendingScreens = 0;
            _screenshieldList.Clear();
        }

        public virtual void TryDestroyTopScreen()
        {
            if (IsAnyScreenActive())
            {
                var topScreen = GetTopScreen();

                // Remove from the list, handle underlying objects before destroying it
                OnScreenClosed(topScreen);

                DestroyScreenInternal(topScreen);
            }
        }

        public virtual void TryDestroyScreen(Component screen)
        {
            if (screen != null && screen.gameObject != null)
            {
                // Remove from the list, handle underlying objects before destroying it
                OnScreenClosed(screen);

                DestroyScreenInternal(screen);
            }
        }

        protected virtual void DestroyScreenInternal(Component screen)
        {
            Destroy(screen.gameObject);
        }
        #endregion

        #region Add Screen
        public virtual void StopAllAddScreenCoroutines()
        {
            for (int i = 0; i < ScreenCoroutines.Count; i++)
            {
                var sc = ScreenCoroutines[i];
                if (sc != null && sc.Coroutine != null)
                {
                    StopCoroutine(sc.Coroutine);
                    sc.Coroutine = null;
                }
            }
            ScreenCoroutines.Clear();
        }

        public virtual IEnumerator AddScreen<T>(string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", bool useExistingScreen = false, OnScreenLoadDelegate<T> onScreenLoad = null, bool hasShield = true, bool manually = true, AddConditionDelegate addCondition = null, bool waitUntilNoScreen = false, bool destroyTopScreen = false, bool hideTopScreen = true, float shieldAlpha = -1, bool ignoreOnScreenAdded = false, OnScreenPreLoadDelegate onScreenPreLoad = null) where T : Component
        {
            // Wait until the addCondition() return true. This is a custom condition.
            while (addCondition != null && !addCondition()) yield return null;

            // Wait until no more screen is being loaded or animated
            while (IsAnyScreenLoading() || IsAnyScreenAnimating()) yield return null;

            // If waitUntilNoScreen is true, wait until no more screen is active or loading
            while (waitUntilNoScreen && (IsAnyScreenLoading() || IsAnyScreenActive())) yield return null;

            if (onScreenPreLoad != null)
            {
                yield return onScreenPreLoad();
            }

            // Update loading screen count
            _loadingScreens++;

            // Show transparent top shield
            ShieldManager.TransparentTopShield.SetActive(true);

            // Shield
            ShieldController shield = null;

            // Create Shield if no any screen active
            if (!IsAnyScreenActive() && hasShield)
            {
                shield = CreateShield(true, shieldAlpha);
            }

            // Try find existing screen
            var hasExistingScreen = false; T existingScreen = null; int existingScreenIndex = 0;
            if (useExistingScreen)
            {
                hasExistingScreen = TryFindExistingScreen<T>(out existingScreen, out existingScreenIndex);
            }

            // Set fromScreen
            var fromScreen = SceneManager.LastLoadedSceneName;
            if (IsAnyScreenActive())
            {
                var topScreen = GetTopScreen();
                if (topScreen != null)
                {
                    // Handle hideTopScreen
                    if (!hasExistingScreen && !destroyTopScreen)
                    {
                        shield = HandleHideTopScreen(topScreen, hasShield, hideTopScreen, shieldAlpha);
                    }

                    // Set fromScreen
                    fromScreen = topScreen.name;
                }
            }

            // Handle existing/new screen
            if (hasExistingScreen)
            {
                HandleExistingScreen(existingScreen, existingScreenIndex, onScreenLoad, screenName, fromScreen, manually, ignoreOnScreenAdded);
            }
            else
            {
                HandleNewScreen(fromScreen, screenName, showAnimation, hideAnimation, animationObjectName, onScreenLoad, hasShield, manually, destroyTopScreen, shield, shieldAlpha, ignoreOnScreenAdded);
            }
        }

        protected virtual bool TryFindExistingScreen<T>(out T existingScreen, out int index) where T : Component
        {
            for (int i = 0; i < _screenList.Count; i++)
            {
                existingScreen = _screenList[i].GetComponent<T>();
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

        protected virtual void HandleOnScreenLoaded(string screenName, string fromScreen, bool manually, bool destroyTopScreen, bool hasShield, Component screen, ShieldController shield, bool ignoreOnScreenAdded)
        {
            // Shield
            var nearestShield = shield;

            // Invoke OnScreenLoaded
            if (!ignoreOnScreenAdded)
            {
                OnScreenAdded?.Invoke(screenName, fromScreen, manually);
            }

            // Handle the 2nd screen
            if (destroyTopScreen && _screenList.Count > 1)
            {
                var secondScreen = Get2ndScreen();

                nearestShield = HandleDestroyTopScreen(secondScreen, hasShield);
            }

            // Shield Event
            if (nearestShield != null)
            {
                ShieldManager.UpdateShield(nearestShield, screen.gameObject);
            }
        }

        protected virtual ShieldController HandleDestroyTopScreen(Component topScreen, bool hasShield)
        {
            ShieldController shield = null;

            if (hasShield)
            {
                // Find the shield before removing screen from the list
                shield = FindNearestShieldUnderScreen(topScreen.gameObject);

                // Remove from the list and the current shield will not be destroyed
                RemoveScreenFromListInternal(topScreen);
            }
            else
            {
                // Remove from the list and destroy the current shield
                OnScreenClosed(topScreen);
            }

            // Destroy it
            DestroyScreenInternal(topScreen);

            return shield;
        }

        protected virtual ShieldController HandleHideTopScreen(Component topScreen, bool hasShield, bool hideTopScreen, float shieldAlpha)
        {
            if (hideTopScreen)
            {
                topScreen.gameObject.SetActive(false);
            }
            else
            {
                if (hasShield)
                {
                    return CreateShield(true, shieldAlpha);
                }
            }

            return ShieldManager.GetTopShield;
        }

        protected virtual void HandleExistingScreen<T>(T screen, int index, OnScreenLoadDelegate<T> onScreenLoad, string screenName, string fromScreen, bool manually, bool ignoreOnScreenAdded) where T : Component
        {
            // Find Screen's child index
            var screenChildIndex = _screenshieldList.IndexOf(screen.gameObject);

            // If found
            if (screenChildIndex >= 0)
            {
                // If not the lowest one
                if (screenChildIndex > 0)
                {
                    // Underlying object
                    Transform underlying = _screenshieldList[screenChildIndex - 1].transform;

                    // Overlying object
                    Transform overlying = null;
                    if (screenChildIndex + 1 < _screenshieldList.Count)
                    {
                        overlying = _screenshieldList[screenChildIndex + 1].transform;
                    }

                    // If underlying object is a shield
                    var shield = underlying.GetComponent<ShieldController>();
                    if (shield != null)
                    {
                        // If no overlying object or it also is a shield
                        if (overlying == null || IsShield(overlying))
                        {
                            // Move the underlying shield to the highest position
                            ShieldManager.MoveShieldToTop(shield);

                            // Move shield to the top of _screenshieldList
                            _screenshieldList.Remove(shield.gameObject);
                            _screenshieldList.Add(shield.gameObject);
                        }
                        else
                        {
                            // If overlying object is a screen and it is the current top
                            if (screenChildIndex + 2 >= _screenshieldList.Count)
                            {
                                // Deactivate it
                                overlying.gameObject.SetActive(false);

                                // Update events and color of the shield
                                ShieldManager.UpdateShield(shield, screen.gameObject);
                            }
                        }
                    }
                }

                // Update the loading count
                OnScreenLoadEnd();

                // Move this screen to the highest position, play its show animation.
                screen.transform.SetAsLastSibling();
                screen.gameObject.SetActive(true);
                PlayAnimation(screen, screen.GetComponent<ScreenController>().ShowAnimation, FramesDelayBeforeShowAnimation());

                // Move this screen to top in the screen list
                _screenList.RemoveAt(index);
                _screenList.Add(screen);

                // Move this screen to top in the screen shield list
                _screenshieldList.Remove(screen.gameObject);
                _screenshieldList.Add(screen.gameObject);

                // Send OnScreenLoad event
                onScreenLoad?.Invoke(screen);

                // Update the pending count
                if (_pendingScreens > 0)
                {
                    _pendingScreens--;
                }

                // Send OnScreenAdded event (do not need to HandleOnScreenLoaded)
                if (!ignoreOnScreenAdded)
                {
                    OnScreenAdded?.Invoke(screenName, fromScreen, manually);
                }
            }
        }

        protected virtual void HandleNewScreen<T>(string fromScreen, string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", OnScreenLoadDelegate<T> onScreenLoad = null, bool hasShield = true, bool manually = true, bool destroyTopScreen = false, ShieldController shield = null, float shieldAlpha = -1, bool ignoreOnScreenAdded = false) where T : Component
        {
            var screenRef = Resources.Load<ScreenReference>(Path.Combine(_screenPath, screenName));
            var screen = CreateScreen<T>(screenRef.ScreenPrefab, screenName, showAnimation, hideAnimation, animationObjectName, onScreenLoad, hasShield, shieldAlpha);
            HandleOnScreenLoaded(screenName, fromScreen, manually, destroyTopScreen, hasShield, screen, shield, ignoreOnScreenAdded);
        }

        protected virtual void OnScreenLoadEnd()
        {
            _loadingScreens--;

            if (_loadingScreens <= 0)
            {
                ShieldManager.TransparentTopShield.SetActive(false);
            }
        }
        #endregion

        #region On Screen Destroy
        public virtual void OnScreenClosed(Component screen)
        {
            // Only reveal underlying objects if the screen is in the screen list.
            // In ClearAllScreens function, we remove screens from the screen list first, then destroy them, then this reveal function will not be called.
            var index = TryRemoveScreenFromList(screen);
            if (index >= 0)
            {
                RevealUnderlyingScreenOrShield(index);
            }
        }

        // Reveal underlying objects only if the overlying object is a shield, or a screen without shield, or no overlying object
        public virtual void RevealUnderlyingScreenOrShield(int index)
        {
            var childCount = _screenshieldList.Count;

            if (childCount > 0)
            {
                var needHandleUnderlying = false;

                // If this screen has overlying object
                if (index < childCount)
                {
                    var overlyingObject = _screenshieldList[index];
                    var overlyingScreen = overlyingObject.GetComponent<ScreenController>();

                    // If overlying object is a shield, or it is a screen without shield
                    if (overlyingScreen == null || !overlyingScreen.HasShield)
                    {
                        needHandleUnderlying = true;
                    }
                }
                else // If this screen has no overlying object
                {
                    needHandleUnderlying = true;
                }

                if (needHandleUnderlying)
                {
                    HandleUnderlyingRecursive(childCount, index - 1);
                }
            }
        }

        // If underlying object is a shield, hide it then continue check its underlying object recursively. If it is a screen, show it and stop recursive.
        protected virtual void HandleUnderlyingRecursive(int childCount, int index)
        {
            // If childIndex is in valid range
            if (index >= 0 && index < childCount)
            {
                // Get the object by childIndex
                var obj = _screenshieldList[index];

                // Get its screen controller
                var screen = obj.GetComponent<ScreenController>();

                // If it is a screen
                if (screen != null && !screen.BeingDestroyed)
                {
                    if (!_showAnimationOneTime)
                    {
                        if (screen.gameObject != null && !screen.gameObject.activeInHierarchy)
                        {
                            // Show it
                            screen.gameObject.SetActive(true);
                            PlayAnimation(screen, screen.ShowAnimation);

                            // Update shield events
                            var shield = FindNearestShieldUnderScreen(index);
                            if (shield != null)
                            {
                                ShieldManager.UpdateShield(shield, screen.gameObject);
                            }
                        }
                    }
                }
                else
                {
                    // Get its shield controller
                    var shield = obj.GetComponent<ShieldController>();

                    // If it is a shield
                    if (shield != null && !shield.BeingDestroyed)
                    {
                        // Hide it
                        HideScreenShield(shield);

                        // Continue to handle its underlying object
                        HandleUnderlyingRecursive(childCount, index - 1);
                    }
                }
            }
        }

        protected virtual ShieldController FindNearestShieldUnderScreen(GameObject screen)
        {
            var index = _screenshieldList.IndexOf(screen);

            return FindNearestShieldUnderScreen(index);
        }

        protected virtual ShieldController FindNearestShieldUnderScreen(int index)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                var obj = _screenshieldList[i];

                var shield = obj.GetComponent<ShieldController>();

                if (shield != null)
                {
                    return shield;
                }
            }

            return null;
        }
        #endregion

        #region Create Screen
        protected virtual T CreateScreen<T>(GameObject prefab, string screenName, string showAnimation = "ScaleShow", string hideAnimation = "ScaleHide", string animationObjectName = "", OnScreenLoadDelegate<T> onScreenLoad = null, bool hasShield = true, float shieldAlpha = -1) where T : Component
        {
            T screen = Instantiate(prefab.GetComponent<T>(), ScreenContainer);

            screen.name = screenName;
            AddToContainer(screen.gameObject, ScreenContainer);

            var controller = AddScreenController(screen);
            controller.Screen = screen;
            controller.ShowAnimation = showAnimation;
            controller.HideAnimation = hideAnimation;
            controller.AnimationObjectName = animationObjectName;
            controller.HasShield = hasShield;
            controller.ShieldAlpha = shieldAlpha;
            controller.Manager = this;

            AddScreenToList(screen);

            AddAnimations(screen, animationObjectName, showAnimation, hideAnimation);
            PlayAnimation(screen, showAnimation, FramesDelayBeforeShowAnimation());

            onScreenLoad?.Invoke(screen);

            if (_pendingScreens > 0)
            {
                _pendingScreens--;
            }

            return screen;
        }

        public virtual void AddToContainer(GameObject screen, RectTransform container)
        {
            screen.transform.SetParent(container);
            screen.transform.localPosition = Vector3.zero;
            screen.transform.localScale = Vector3.one;
            screen.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }

        protected virtual ScreenController AddScreenController(Component screen)
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
        protected virtual ShieldController CreateShield(bool showAfterCreate = false, float shieldAlpha = -1)
        {
            var shield = ShieldManager.CreateShield(showAfterCreate, shieldAlpha);
            _screenshieldList.Add(shield.gameObject);

            return shield;
        }

        protected virtual void HideScreenShield(ShieldController shield)
        {
            ShieldManager.HideShield(shield);
            _screenshieldList.Remove(shield.gameObject);
        }
        #endregion

        #region Animation
        protected virtual Animation AddAnimations(Component screen, string animationObjectName = "", params string[] animationNames)
        {
            // By defause, animation object is screen object
            GameObject animObject = screen.gameObject;

            // If animationObjectName is not null or empty, find it
            if (!string.IsNullOrEmpty(animationObjectName))
            {
                animObject = FindChildBFS(screen.gameObject, animationObjectName);

                if (animObject == null)
                {
                    animObject = screen.gameObject;
                }
            }

            // If Unity Animation is not added, add it.
            var anim = animObject.GetComponent<Animation>();
            if (anim == null)
            {
                anim = animObject.AddComponent<Animation>();
            }

            // If UnscaledAnimation is not added, add it. This one is for play Unity Animations without affection of time scale.
            var unscaledAnim = animObject.GetComponent<UnscaledAnimation>();
            if (unscaledAnim == null)
            {
                animObject.AddComponent<UnscaledAnimation>();
            }

            // Set not play automatically
            anim.playAutomatically = false;

            // Loop all animation names
            for (int i = 0; i < animationNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(animationNames[i]) && string.Compare(animationNames[i], "None") != 0)
                {
                    // If has no Animation Clip
                    if (anim.GetClip(animationNames[i]) == null)
                    {
                        // Load the Animation Clip from screen animation path
                        var path = Path.Combine(_screenAnimationPath, animationNames[i]);
                        var clip = Resources.Load<AnimationClip>(path);

                        // If not found, load it from the default path
                        if (clip == null)
                        {
                            var defaultPath = Path.Combine("Animations", animationNames[i]);
                            clip = Resources.Load<AnimationClip>(defaultPath);
                        }

                        // If found, add the Animation Clip to the Animation
                        if (clip != null)
                        {
                            anim.AddClip(clip, animationNames[i]);
                        }
                        else
                        {
                            Debug.LogWarning("Animation Clip not found: " + path);
                        }
                    }

                    // Add canvas group to control alpha of entire objects in the screen
                    if (animObject.GetComponent<CanvasGroup>() == null)
                    {
                        animObject.AddComponent<CanvasGroup>();
                    }

                    // Add AnimationPosition component to play the screen animation dynamically. By default, Unity Animation fixes object positions throughout its timeline.
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

        protected virtual void PlayAnimation(Component screen, string animationName, int delayFrames = 0, bool destroyScreenAtAnimationEnd = false, OnAnimationEndedDelegate onAnimationEnd = null)
        {
            _animatingScreens++;

            var anim = AddAnimations(screen, screen.GetComponent<ScreenController>().AnimationObjectName, animationName);

            StartCoroutine(CoPlayAnimation(anim, animationName, delayFrames, onAnimationEnd, destroyScreenAtAnimationEnd ? screen : null));
        }

        protected virtual IEnumerator CoPlayAnimation(Animation anim, string animationName, int delayFrames, OnAnimationEndedDelegate onAnimationEnd = null, Component screenToBeDestroyed = null)
        {
            if (anim.GetClip(animationName) != null)
            {
                // Show the transparent top shield before playing any screen animation, to prevent any touch
                ShieldManager.TransparentTopShield.SetActive(true);

                // Get Unscaled anim and pause the animation at frame 0.
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

                if (unscaledAnim != null)
                {
                    // Play animation
                    unscaledAnim.Play(animationName, speed: AnimationSpeed);

                    // Wait animation end
                    yield return new WaitForSecondsRealtime(anim[animationName].length / AnimationSpeed);
                }

                // Turn off transparent top shield after animation end
                ShieldManager.TransparentTopShield.SetActive(false);
            }

            if (screenToBeDestroyed != null)
            {
                DestroyScreenInternal(screenToBeDestroyed);
            }

            onAnimationEnd?.Invoke();

            _animatingScreens--;
        }

        protected virtual int FramesDelayBeforeShowAnimation()
        {
            return 4;
        }
        #endregion

        #region Find Algorithms
        protected virtual int FindChildIndex(Transform parent, Transform t)
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

        protected virtual GameObject FindChildBFS(GameObject parent, string name)
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
        protected virtual void AddScreenToList(Component screen)
        {
            OnScreenLoadEnd();

            _screenList.Add(screen);
            _screenshieldList.Add(screen.gameObject);

            OnScreenChanged?.Invoke(_screenList.Count);
        }

        protected virtual int TryRemoveScreenFromList(Component screen)
        {
            if (screen != null && _screenList.Contains(screen))
            {
                var index = RemoveScreenFromListInternal(screen);

                OnScreenChanged?.Invoke(_screenList.Count);

                return index;
            }

            return -1;
        }

        protected virtual void RemoveTopScreenFromListInternal()
        {
            _screenList.RemoveAt(_screenList.Count - 1);
            _screenshieldList.RemoveAt(_screenshieldList.Count - 1);
        }

        protected virtual int RemoveScreenFromListInternal(Component screen)
        {
            var index = _screenshieldList.IndexOf(screen.gameObject);

            _screenList.Remove(screen);
            _screenshieldList.Remove(screen.gameObject);

            return index;
        }
        #endregion
    }
}