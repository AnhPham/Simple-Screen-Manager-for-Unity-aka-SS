/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

namespace SS.UI
{
    public class SceneManager : MonoBehaviour
    {
        #region Serialize Fields
        [SerializeField] protected string _sceneLoadingPath;
        [SerializeField] protected string _sceneLoadingName;
        [SerializeField] protected float _loadingMinDuration = 0.5f;
        [SerializeField] protected ScreenManager _screenManager;
        [SerializeField] protected GeneralManager _generalManager;
        #endregion

        #region Delegate
        public delegate void OnSceneLoad<T>(T t);
        public delegate IEnumerator OnScenePreLoadDelegate();
        #endregion

        #region Protected Member
        protected Scene _lastLoadedScene;
        protected GameObject _sceneLoading;
        protected ISceneLoading _sceneLoadingInterface;
        protected AsyncOperation _asyncOperation;
        protected float _loadingTime;
        #endregion

        #region Public Properties
        public float AsyncOperationProgress { get; protected set; }
        public string LastLoadedSceneName => _lastLoadedScene != null ? _lastLoadedScene.name : string.Empty;
        public ScreenManager ScreenManager { get => _screenManager; set => _screenManager = value; }
        public GeneralManager GeneralManager { get => _generalManager; set => _generalManager = value; }
        #endregion

        #region Protected Properties
        protected Camera BackgroundCamera => _generalManager.BackgroundCamera;
        protected UnscaledAnimation SceneShield => _generalManager.SceneShield;
        protected RectTransform SceneLoadingContainer => _generalManager.SceneLoadingContainer;
        protected float AnimationSpeed => _generalManager.AnimationSpeed;
        #endregion

        #region Unity Cycle
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;

            GeneralManager = FindObjectOfType<GeneralManager>();

            SetupCameras();
            SetupCanvases();
        }
        #endregion

        #region Events
        protected virtual void OnSceneUnloaded(Scene scene)
        {
            BackgroundCamera.gameObject.SetActive(true);
        }

        protected virtual void OnActiveSceneChanged(Scene scene1, Scene scene2)
        {
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetupCameras();
            SetupCanvases();

            _lastLoadedScene = scene;
        }
        #endregion

        #region Public Functions
        public virtual void Setup(string sceneLoadingName = "", string sceneLoadingPath = "")
        {
            _sceneLoadingName = sceneLoadingName;
            this._sceneLoadingPath = sceneLoadingPath;
        }

        public virtual void LoadScene<T>(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreen = true, OnScenePreLoadDelegate onScenePreLoad = null) where T : Component
        {
            StartCoroutine(CoLoadScene(sceneName, mode, onSceneLoaded, clearAllScreen, onScenePreLoad));
        }
        #endregion

        #region protected virtual Functions
        protected virtual IEnumerator CoLoadScene<T>(string sceneName, LoadSceneMode mode, OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreen = true, OnScenePreLoadDelegate onScenePreLoad = null) where T : Component
        {
            // If _sceneLoadingName is null, use a default fading shield. 
            var isDefaultLoading = string.IsNullOrEmpty(_sceneLoadingName);

            // For single mode
            if (mode == LoadSceneMode.Single)
            {
                // Reset variables
                _asyncOperation = null;  
                _loadingTime = 0;
                AsyncOperationProgress = 0;

                if (isDefaultLoading)
                {
                    // For default loading, fade in the shield
                    SceneShield.Play("ShieldShow", speed: AnimationSpeed);

                    yield return new WaitForSecondsRealtime(SceneShield.GetLength("ShieldShow") / AnimationSpeed);
                }
                else
                {
                    // For custom loading UI
                    TryCreateAndShowSceneLoading();

                    // Wait until _sceneLoading is not null
                    while (_sceneLoading == null)
                    {
                        yield return 0;
                    }

                    // If there is a component in the custom loading UI which implements ISceneLoading, wait until its show-animation end
                    if (_sceneLoadingInterface != null)
                    {
                        yield return new WaitForSecondsRealtime(_sceneLoadingInterface.ShowDuration());
                    }
                }

                // By default, clear all exist screens while loading a scene in the single mode.
                if (clearAllScreen)
                {
                    ScreenManager.ClearAllScreens();
                }

                if (onScenePreLoad != null)
                {
                    yield return onScenePreLoad();
                }

                // Load scene
                LoadAsyncOperationScene(sceneName, mode, isDefaultLoading, onSceneLoaded);

                // While loading
                while (!IsAsyncOperationDone())
                {
                    if (isDefaultLoading)
                    {
                        // For default loading, update the real progress each frame
                        AsyncOperationProgress = GetAsyncOperationProgress();
                    }
                    else
                    {
                        // For custom loading UI, update the progress each frame by real or fake loading progress depends on the loading speed
                        UpdateProgressForSceneLoading();
                    }
                    yield return null;
                }

                // Loading done, 100%
                AsyncOperationProgress = 1f;

                // Free memory
                Resources.UnloadUnusedAssets();
                System.GC.Collect();

                // Wait before hide loading
                var delay = TimeBeforeHideLoading(sceneName);
                if (delay > 0)
                {
                    yield return new WaitForSecondsRealtime(delay);
                }

                if (isDefaultLoading)
                {
                    // For default loading, fade out the shield
                    SceneShield.Play("ShieldHide", speed: AnimationSpeed);
                }
                else
                {
                    // For custom loading UI, if there is a component which implements ISceneLoading, play its hide-animation
                    if (_sceneLoadingInterface != null)
                    {
                        _sceneLoadingInterface.Hide();
                        yield return new WaitForSecondsRealtime(_sceneLoadingInterface.HideDuration());
                    }

                    // Deactivate the custom scene loading UI
                    _sceneLoading.SetActive(false);
                }
            }
            else
            {
                // For addtive mode
                LoadAsyncOperationScene(sceneName, mode, true, onSceneLoaded);
            }
        }

        protected virtual void TryCreateAndShowSceneLoading()
        {
            if (_sceneLoading == null)
            {
                var prefab = Resources.Load<ScreenReference>(Path.Combine(_sceneLoadingPath, _sceneLoadingName)).ScreenPrefab;
                CreateSceneLoading(prefab);
                ShowSceneLoading();
            }
            else
            {
                ShowSceneLoading();
            }
        }

        protected virtual void UpdateProgressForSceneLoading()
        {
            if (GetAsyncOperationProgress() < 0.9f || _loadingTime < _loadingMinDuration)
            {
                _loadingTime += Time.deltaTime;
                AsyncOperationProgress = _loadingTime / _loadingMinDuration < GetAsyncOperationProgress() ? _loadingTime / _loadingMinDuration : GetAsyncOperationProgress();
            }
            else
            {
                ActivateAsyncOperationScene();
                AsyncOperationProgress = 1f;
            }
        }

        protected virtual void CreateSceneLoading(GameObject prefab)
        {
            _sceneLoading = Instantiate(prefab);
            _sceneLoading.name = _sceneLoadingName;
            _sceneLoading.TryGetComponent(out _sceneLoadingInterface);
            AddToContainer(_sceneLoading, SceneLoadingContainer);
        }

        protected virtual void ShowSceneLoading()
        {
            _sceneLoading.SetActive(true);

            // If there is a component in the custom loading UI which implements ISceneLoading, play its show-animation
            if (_sceneLoadingInterface != null)
            {
                _sceneLoadingInterface.Show();
            }
        }

        protected virtual void SetupCameras()
        {
            var cameras = FindObjectsOfType<Camera>();

            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != BackgroundCamera)
                {
                    if (cameras[i].clearFlags == CameraClearFlags.Skybox || cameras[i].clearFlags == CameraClearFlags.SolidColor)
                    {
                        BackgroundCamera.gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }

        protected virtual void SetupCanvases()
        {
            var screenRatio = (float)UnityEngine.Screen.width / UnityEngine.Screen.height;

            var canvasScalers = FindObjectsOfType<CanvasScaler>(true);
            for (int i = 0; i < canvasScalers.Length; i++)
            {
                SetupCanvasScaler(canvasScalers[i], screenRatio);
            }
        }

        protected virtual void SetupCanvasScaler(CanvasScaler canvasScaler, float screenRatio)
        {
            canvasScaler.matchWidthOrHeight = screenRatio > 0.44f ? 1f : 0f;
        }

        protected virtual T GetSceneComponent<T>(Scene scene) where T : Component
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

        protected virtual void AddToContainer(GameObject screen, RectTransform container)
        {
            screen.transform.SetParent(container);
            screen.transform.localPosition = Vector3.zero;
            screen.transform.localScale = Vector3.one;
            screen.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }

        protected virtual float TimeBeforeHideLoading(string sceneName)
        {
            return 0;
        }

        protected virtual void LoadAsyncOperationScene<T>(string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad, OnSceneLoad<T> onSceneLoaded = null) where T : Component
        {
            _asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            _asyncOperation.allowSceneActivation = activateOnLoad;
            _asyncOperation.completed += (asyncOp) =>
            {
                onSceneLoaded?.Invoke(GetSceneComponent<T>(_lastLoadedScene));
            };  
        }

        protected virtual bool IsAsyncOperationDone()
        {
            return _asyncOperation.isDone;         
        }

        protected virtual float GetAsyncOperationProgress()
        {
            return _asyncOperation.progress;       
        }

        protected virtual void ActivateAsyncOperationScene()
        {
            _asyncOperation.allowSceneActivation = true;    
        }
        #endregion
    }
}