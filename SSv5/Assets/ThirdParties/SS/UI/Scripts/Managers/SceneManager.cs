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
        [SerializeField] ScreenManager m_ScreenManager;
        [SerializeField] ShieldManager m_ShieldManager;
        [SerializeField] GeneralManager m_GeneralManager;
        #endregion

        #region Delegate
        public delegate void OnSceneLoad<T>(T t);
        #endregion

        #region Private Member
        private Scene m_LastLoadedScene;
        private GameObject m_SceneLoading;
        #endregion

        #region Get/Set
        public AsyncOperation asyncOperation
        {
            get;
            protected set;
        }

        public Scene lastLoadedScene
        {
            get
            {
                return m_LastLoadedScene;
            }
        }

        public ScreenManager screenManager
        {
            get
            {
                return m_ScreenManager;
            }

            set
            {
                m_ScreenManager = value;
            }
        }

        public ShieldManager shieldManager
        {
            get
            {
                return m_ShieldManager;
            }

            set
            {
                m_ShieldManager = value;
            }
        }

        public GeneralManager generalManager
        {
            get
            {
                return m_GeneralManager;
            }

            set
            {
                m_GeneralManager = value;
            }
        }

        public Camera backgroundCamera
        {
            get
            {
                return m_GeneralManager.backgroundCamera;
            }
        }
        public UnscaledAnimation sceneShield
        {
            get
            {
                return m_GeneralManager.sceneShield;
            }
        }

        public RectTransform sceneLoadingContainer
        {
            get
            {
                return m_GeneralManager.sceneLoadingContainer;
            }
        }

        public float animationSpeed
        {
            get
            {
                return m_GeneralManager.animationSpeed;
            }
        }
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
            sceneShield.transform.SetAsLastSibling();

            if (mode == LoadSceneMode.Single)
            {
                sceneShield.Play("ShieldShow", speed: animationSpeed);

                yield return new WaitForSecondsRealtime(sceneShield.GetLength("ShieldShow") / animationSpeed);

                if (clearAllScreen)
                {
                    shieldManager.DestroyShield();

                    screenManager.ClearAllScreen();
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
                    var prefab = Resources.Load<GameObject>(Path.Combine(m_SceneLoadingPath, m_SceneLoadingName));
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
                onSceneLoaded?.Invoke(GetSceneComponent<T>(m_LastLoadedScene));
            };

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            if (mode == LoadSceneMode.Single)
            {
                sceneShield.Play("ShieldHide", speed: animationSpeed);
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

        private void CreateSceneLoading(GameObject prefab)
        {
            m_SceneLoading = Instantiate(prefab);
            m_SceneLoading.name = m_SceneLoadingName;
            AddToContainer(m_SceneLoading, sceneLoadingContainer);
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