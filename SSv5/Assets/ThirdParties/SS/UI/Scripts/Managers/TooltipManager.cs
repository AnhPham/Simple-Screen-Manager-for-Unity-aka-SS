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
    public class TooltipManager : MonoBehaviour
    {
        #region Serialize Fields
        [SerializeField] protected string _tooltipName;
        [SerializeField] protected string _tooltipPath;
        [SerializeField] protected GeneralManager _generalManager;
        #endregion

        #region Protected Members
        protected TooltipBaseController _tooltip;
        #endregion

        #region Public Properties
        public GeneralManager GeneralManager { get => _generalManager; set => _generalManager = value; }
        #endregion

        #region Protected Properties
        protected RectTransform TopContainer => _generalManager.TopContainer;
        #endregion

        #region Unity Cycle
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);

            GeneralManager = FindObjectOfType<GeneralManager>();
        }
        #endregion

        #region Public Functions
        public virtual void Setup(string tooltipName = "", string tooltipPath = "")
        {
            _tooltipName = tooltipName;
            _tooltipPath = tooltipPath;
        }

        public virtual void LoadAndShowTooltip(string text, Vector3 worldPosition, float targetY = 100f)
        {
            if (string.IsNullOrEmpty(_tooltipName))
                return;

            if (_tooltip != null)
            {
                _tooltip.transform.SetParent(TopContainer, true);
                _tooltip.ShowTooltip(text, worldPosition, targetY);
                return;
            }

            var tooltipPrefab = Resources.Load<ScreenReference>(Path.Combine(_tooltipPath, _tooltipName)).ScreenPrefab;
            CreateAndShowTooltip(tooltipPrefab, text, worldPosition, targetY);
        }

        public virtual void HideTooltipImmediately()
        {
            if (_tooltip != null)
            {
                _tooltip.HideToolTip();
            }
        }
        #endregion

        #region Protected Functions
        protected virtual void CreateAndShowTooltip(GameObject tooltipPrefab, string text, Vector3 worldPosition, float targetY)
        {
            var tooltip = Instantiate(tooltipPrefab, TopContainer);
            _tooltip = tooltip.GetComponent<TooltipBaseController>();

            if (_tooltip != null)
            {
                _tooltip.ShowTooltip(text, worldPosition, targetY);
            }
        }
        #endregion
    }
}