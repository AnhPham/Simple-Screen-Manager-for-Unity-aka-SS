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
        #region Serialize Fields
        [SerializeField] protected string _loadingName;
        [SerializeField] protected string _loadingPath;
        [SerializeField] protected GeneralManager _generalManager;
        #endregion

        #region Protected Member
        protected GameObject _loadingObject;
        protected Coroutine _loadingCoroutine;
        protected bool _isLoading;
        #endregion

        #region Public Properties
        public GeneralManager GeneralManager { get => _generalManager; set => _generalManager = value; }
        public GameObject LoadingObject => _loadingObject;
        #endregion

        #region Protected Properties
        protected RectTransform ScreenLoadingContainer => _generalManager.ScreenLoadingContainer;
        #endregion

        #region Unity Cycle
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);

            GeneralManager = FindObjectOfType<GeneralManager>();
        }
        #endregion

        #region Public Functions
        public virtual void Setup(string loadingName = "", string loadingPath = "")
        {
            _loadingName = loadingName;
            _loadingPath = loadingPath;
        }

        public virtual void ShowLoading(bool isShow, float timeout = 0)
        {
            _isLoading = isShow;
            if (isShow)
            {
                if (!string.IsNullOrEmpty(_loadingName))
                {
                    if (_loadingObject == null)
                    {
                        var prefab = Resources.Load<ScreenReference>(Path.Combine(_loadingPath, _loadingName)).ScreenPrefab;
                        CreateLoading(prefab);
                        ShowLoading(timeout);
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

        #region protected Functions
        protected virtual void CreateLoading(GameObject prefab)
        {
            _loadingObject = Instantiate(prefab);
            _loadingObject.name = _loadingName;
            _loadingObject.SetActive(false);
            AddToContainer(_loadingObject, ScreenLoadingContainer);
        }

        protected virtual void ShowLoading(float timeout = 0)
        {
            if (_isLoading)
            {
                StopLoadingCoroutine();

                if (timeout > 0)
                {
                    _loadingCoroutine = StartCoroutine(CoShowLoading(timeout));
                }
                else
                {
                    _loadingObject.SetActive(true);
                }
            }
        }

        protected virtual void HideLoading()
        {
            StopLoadingCoroutine();

            if (_loadingObject != null)
            {
                _loadingObject.SetActive(false);
            }
        }

        protected virtual void StopLoadingCoroutine()
        {
            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
                _loadingCoroutine = null;
            }
        }

        protected virtual IEnumerator CoShowLoading(float timeout)
        {
            _loadingObject.SetActive(true);

            yield return new WaitForSecondsRealtime(timeout);

            _loadingObject.SetActive(false);
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