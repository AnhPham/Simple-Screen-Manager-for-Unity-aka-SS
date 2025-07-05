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
    public class LoadingManager : MonoBehaviour
    {
        #region SerializeField
        [SerializeField] string m_LoadingName;
        [SerializeField] string m_LoadingPath;
        [SerializeField] GeneralManager m_GeneralManager;
        #endregion

        #region Delegate
        #endregion

        #region Private Member
        private GameObject m_LoadingObject;
        private Coroutine m_LoadingCoroutine;
        private bool m_IsLoading;
        #endregion

        #region Public Properties
        public GeneralManager generalManager { get => m_GeneralManager; set => m_GeneralManager = value; }
        public RectTransform screenLoadingContainer => m_GeneralManager.screenLoadingContainer;
        public GameObject loadingObject => m_LoadingObject;
        #endregion

        #region Unity Cycle
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            generalManager = FindObjectOfType<GeneralManager>();
        }
        #endregion

        #region Events
        #endregion

        #region Public Functions
        public void Setup(string loadingName = "", string loadingPath = "")
        {
            m_LoadingName = loadingName;
            m_LoadingPath = loadingPath;
        }

        public void ShowLoading(bool isShow, float timeout = 0)
        {
            m_IsLoading = isShow;
            if (isShow)
            {
                if (!string.IsNullOrEmpty(m_LoadingName))
                {
                    if (m_LoadingObject == null)
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
                        var prefab = Resources.Load<GameObject>(Path.Combine(m_LoadingPath, m_LoadingName));
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
        #endregion

        #region Private Functions
        private void CreateLoading(GameObject prefab)
        {
            m_LoadingObject = Instantiate(prefab);
            m_LoadingObject.name = m_LoadingName;
            m_LoadingObject.SetActive(false);
            AddToContainer(m_LoadingObject, screenLoadingContainer);
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
                    m_LoadingObject.SetActive(true);
                }
            }
        }

        private void HideLoading()
        {
            StopLoadingCoroutine();

            if (m_LoadingObject != null)
            {
                m_LoadingObject.SetActive(false);
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
            m_LoadingObject.SetActive(true);

            yield return new WaitForSecondsRealtime(timeout);

            m_LoadingObject.SetActive(false);
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