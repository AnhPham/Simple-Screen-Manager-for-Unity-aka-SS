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
        #region SerializeField
        [SerializeField] string m_TooltipName;
        [SerializeField] string m_TooltipPath;
        [SerializeField] GeneralManager m_GeneralManager;
        #endregion

        #region Delegate
        #endregion

        #region Private Member
        private TooltipBaseController m_Tooltip;
        #endregion

        #region Public Properties
        public GeneralManager generalManager { get => m_GeneralManager; set => m_GeneralManager = value; }
        public RectTransform topContainer => m_GeneralManager.TopContainer;
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
        public void Setup(string tooltipName = "", string tooltipPath = "")
        {
            m_TooltipName = tooltipName;
            m_TooltipPath = tooltipPath;
        }

        public void LoadAndShowTooltip(string text, Vector3 worldPosition, float targetY = 100f)
        {
            if (string.IsNullOrEmpty(m_TooltipName))
                return;

            if (m_Tooltip != null)
            {
                m_Tooltip.transform.SetParent(topContainer, true);
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
            var tooltipPrefab = Resources.Load<GameObject>(Path.Combine(m_TooltipPath, m_TooltipName));
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
        #endregion

        #region Private Functions
        private void CreateAndShowTooltip(GameObject tooltipPrefab, string text, Vector3 worldPosition, float targetY)
        {
            var tooltip = Instantiate(tooltipPrefab, topContainer);
            m_Tooltip = tooltip.GetComponent<TooltipBaseController>();

            if (m_Tooltip != null)
            {
                m_Tooltip.ShowTooltip(text, worldPosition, targetY);
            }
        }
        #endregion
    }
}