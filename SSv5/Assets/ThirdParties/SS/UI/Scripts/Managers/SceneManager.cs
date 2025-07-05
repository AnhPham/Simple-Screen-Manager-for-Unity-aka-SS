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
        #region SerializeField
        [SerializeField] string m_SceneLoadingPath;
        [SerializeField] string m_SceneLoadingName;
        [SerializeField] float m_LoadingMinDuration = 0.5f;
        [SerializeField] ScreenManager m_ScreenManager;
        [SerializeField] GeneralManager m_GeneralManager;
        #endregion

        #region Delegate
        public delegate void OnSceneLoad<T>(T t);
        #endregion

        #region Private Member
        private Scene m_LastLoadedScene;
        private GameObject m_SceneLoading;
        private ISceneLoading m_SceneLoadingInterface;
        private AsyncOperation m_AsyncOperation;
        private float m_Time;
        #endregion

        #region Public Properties
        public float asyncOperationProgress { get; protected set; }
        public Scene lastLoadedScene => m_LastLoadedScene;
        public ScreenManager screenManager { get => m_ScreenManager; set => m_ScreenManager = value; }
        public GeneralManager generalManager { get => m_GeneralManager; set => m_GeneralManager = value; }
        public Camera backgroundCamera => m_GeneralManager.backgroundCamera;
        public UnscaledAnimation sceneShield => m_GeneralManager.sceneShield;
        public RectTransform sceneLoadingContainer => m_GeneralManager.sceneLoadingContainer;
        public float animationSpeed => m_GeneralManager.animationSpeed;
        #endregion

        #region Unity Cycle
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;

            generalManager = FindObjectOfType<GeneralManager>();

            SetupCameras();
            SetupCanvases();
        }
        #endregion

        #region Events
        private void SceneManager_sceneUnloaded(Scene scene)
        {
            backgroundCamera.gameObject.SetActive(true);
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
        public void Setup(string sceneLoadingName = "", string sceneLoadingPath = "")
        {
            m_SceneLoadingName = sceneLoadingName;
            m_SceneLoadingPath = sceneLoadingPath;
        }

        public void LoadScene<T>(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreen = true) where T : Component
        {
            StartCoroutine(CoLoadScene(sceneName, mode, onSceneLoaded, clearAllScreen));
        }
        #endregion

        #region Private Functions
        private IEnumerator CoLoadScene<T>(string sceneName, LoadSceneMode mode, OnSceneLoad<T> onSceneLoaded = null, bool clearAllScreen = true) where T : Component
        {
            var isDefaultLoading = string.IsNullOrEmpty(m_SceneLoadingName);

            if (mode == LoadSceneMode.Single)
            {
                m_AsyncOperation = null;
                m_Time = 0;
                asyncOperationProgress = 0;

                if (isDefaultLoading)
                {
                    sceneShield.transform.SetAsLastSibling();
                    sceneShield.Play("ShieldShow", speed: animationSpeed);

                    yield return new WaitForSecondsRealtime(sceneShield.GetLength("ShieldShow") / animationSpeed);
                }
                else
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

                        while (m_SceneLoading == null)
                        {
                            yield return 0;
                        }
#else
                        var prefab = Resources.Load<GameObject>(Path.Combine(m_SceneLoadingPath, m_SceneLoadingName));
                        CreateSceneLoading(prefab);
                        ShowSceneLoading();
#endif
                    }
                    else
                    {
                        ShowSceneLoading();
                    }

                    if (m_SceneLoadingInterface != null)
                    {
                        m_SceneLoadingInterface.Show();
                        yield return new WaitForSecondsRealtime(m_SceneLoadingInterface.ShowDuration());
                    }
                }

                if (clearAllScreen)
                {
                    screenManager.ClearAllScreens();
                }

                m_AsyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
                m_AsyncOperation.allowSceneActivation = isDefaultLoading ? true : false;
                m_AsyncOperation.completed += (asyncOp) =>
                {
                    onSceneLoaded?.Invoke(GetSceneComponent<T>(m_LastLoadedScene));
                };

                while (!m_AsyncOperation.isDone)
                {
                    if (!isDefaultLoading)
                    {
                        if (m_AsyncOperation.progress < 0.9f || m_Time < m_LoadingMinDuration)
                        {
                            m_Time += Time.deltaTime;
                            asyncOperationProgress = m_Time / m_LoadingMinDuration < m_AsyncOperation.progress ? m_Time / m_LoadingMinDuration : m_AsyncOperation.progress;
                        }
                        else
                        {
                            m_AsyncOperation.allowSceneActivation = true;
                            asyncOperationProgress = 1f;
                        }
                    }
                    else
                    {
                        asyncOperationProgress = m_AsyncOperation.progress;
                    }
                    yield return null;
                }

                asyncOperationProgress = 1f;

                if (isDefaultLoading)
                {
                    sceneShield.Play("ShieldHide", speed: animationSpeed);
                }
                else
                {
                    if (m_SceneLoadingInterface != null)
                    {
                        m_SceneLoadingInterface.Hide();
                        yield return new WaitForSecondsRealtime(m_SceneLoadingInterface.HideDuration());
                    }
                    m_SceneLoading.SetActive(false);
                }
            }
            else
            {
                var asyncOperation2 = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
                asyncOperation2.completed += (asyncOp) =>
                {
                    onSceneLoaded?.Invoke(GetSceneComponent<T>(m_LastLoadedScene));
                };
            }
        }

        private void CreateSceneLoading(GameObject prefab)
        {
            m_SceneLoading = Instantiate(prefab);
            m_SceneLoading.name = m_SceneLoadingName;
            AddToContainer(m_SceneLoading, sceneLoadingContainer);
            m_SceneLoading.TryGetComponent(out m_SceneLoadingInterface);
        }

        private void ShowSceneLoading()
        {
            m_SceneLoading.SetActive(true);
        }

        private void SetupCameras()
        {
            var cameras = FindObjectsOfType<Camera>();

            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != backgroundCamera)
                {
                    if (cameras[i].clearFlags == CameraClearFlags.Skybox || cameras[i].clearFlags == CameraClearFlags.SolidColor)
                    {
                        backgroundCamera.gameObject.SetActive(false);
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

        private void AddToContainer(GameObject screen, RectTransform container)
        {
            screen.transform.SetParent(container);
            screen.transform.localPosition = Vector3.zero;
            screen.transform.localScale = Vector3.one;
            screen.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }
        #endregion
    }
}