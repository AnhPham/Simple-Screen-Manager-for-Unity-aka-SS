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
        public ScreenManager Screen { get => _screenManager; set => _screenManager = value; }
        public GeneralManager General { get => _generalManager; set => _generalManager = value; }
        #endregion

        #region Protected Properties
        protected Camera BackgroundCamera => _generalManager.backgroundCamera;
        protected UnscaledAnimation SceneShield => _generalManager.sceneShield;
        protected RectTransform SceneLoadingContainer => _generalManager.sceneLoadingContainer;
        protected float AnimationSpeed => _generalManager.animationSpeed;
        #endregion

        #region Unity Cycle
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;

            General = FindObjectOfType<GeneralManager>();

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

        public virtual void LoadScene<T>(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreen = true) where T : Component
        {
            StartCoroutine(CoLoadScene(sceneName, mode, onSceneLoaded, clearAllScreen));
        }
        #endregion

        #region protected virtual Functions
        protected virtual IEnumerator CoLoadScene<T>(string sceneName, LoadSceneMode mode, OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreen = true) where T : Component
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

                    // If there is a component in the custom loading UI which implements ISceneLoading, play its show-animation
                    if (_sceneLoadingInterface != null)
                    {
                        _sceneLoadingInterface.Show();
                        yield return new WaitForSecondsRealtime(_sceneLoadingInterface.ShowDuration());
                    }
                }

                // By default, clear all exist screens while loading a scene in the single mode.
                if (clearAllScreen)
                {
                    Screen.ClearAllScreens();
                }

                // Load scene
                _asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
                _asyncOperation.allowSceneActivation = isDefaultLoading ? true : false;
                _asyncOperation.completed += (asyncOp) =>
                {
                    onSceneLoaded?.Invoke(GetSceneComponent<T>(_lastLoadedScene));
                };

                // While loading
                while (!_asyncOperation.isDone)
                {
                    if (isDefaultLoading)
                    {
                        // For default loading, update the real progress each frame
                        AsyncOperationProgress = _asyncOperation.progress;
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
                var asyncOperationAdditive = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
                asyncOperationAdditive.completed += (asyncOp) =>
                {
                    onSceneLoaded?.Invoke(GetSceneComponent<T>(_lastLoadedScene));
                };
            }
        }

        protected virtual void TryCreateAndShowSceneLoading()
        {
            if (_sceneLoading == null)
            {
#if ADDRESSABLE
                var async = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(_sceneLoadingName);
                async.Completed += (a => {
                    if (a.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        CreateSceneLoading(async.Result);
                        ShowSceneLoading();
                    }
                });

                while (_sceneLoading == null)
                {
                    yield return 0;
                }
#else
                var prefab = Resources.Load<GameObject>(Path.Combine(_sceneLoadingPath, _sceneLoadingName));
                CreateSceneLoading(prefab);
                ShowSceneLoading();
#endif
            }
            else
            {
                ShowSceneLoading();
            }
        }

        protected virtual void UpdateProgressForSceneLoading()
        {
            if (_asyncOperation.progress < 0.9f || _loadingTime < _loadingMinDuration)
            {
                _loadingTime += Time.deltaTime;
                AsyncOperationProgress = _loadingTime / _loadingMinDuration < _asyncOperation.progress ? _loadingTime / _loadingMinDuration : _asyncOperation.progress;
            }
            else
            {
                _asyncOperation.allowSceneActivation = true;
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
        #endregion
    }
}