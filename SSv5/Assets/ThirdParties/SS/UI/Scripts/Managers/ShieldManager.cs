/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
    public class ShieldManager : MonoBehaviour
    {
        #region SerializeField
        [SerializeField] Color m_ScreenShieldColor = new Color(0, 0, 0, 0.8f);
        [SerializeField] bool m_CloseOnTappingShield = false;
        [SerializeField] ScreenManager m_ScreenManager;
        [SerializeField] GeneralManager m_GeneralManager;
        #endregion

        #region Delegate
        #endregion

        #region Private Member
        private List<ShieldController> m_ShieldList = new List<ShieldController>();
        private GameObject m_TransparentTopShield;
        #endregion

        #region Public Properties
        public GameObject transparentTopShield => m_TransparentTopShield;
        public List<ShieldController> shieldList => m_ShieldList;
        public ScreenManager screenManager { get => m_ScreenManager; set => m_ScreenManager = value; }
        public GeneralManager generalManager { get => m_GeneralManager; set => m_GeneralManager = value; }
        public RectTransform screenContainer => m_GeneralManager.ScreenContainer;
        public RectTransform topShieldContainer => m_GeneralManager.TopShieldContainer;
        public float animationSpeed => m_GeneralManager.AnimationSpeed;
        #endregion

        #region Unity Cycle
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            generalManager = FindObjectOfType<GeneralManager>();
            m_TransparentTopShield = CreateTransparentTopShield();
        }
        #endregion

        #region Events
        #endregion

        #region Public Functions
        public void Setup(Color screenShieldColor, bool closeOnTappingShield = false)
        {
            m_ScreenShieldColor = screenShieldColor;
            Setup(closeOnTappingShield);
        }

        public void Setup(bool closeOnTappingShield = false)
        {
            m_CloseOnTappingShield = closeOnTappingShield;
        }

        public void DestroyAllShields()
        {
            for (int i = 0; i < m_ShieldList.Count; i++)
            {
                var shield = m_ShieldList[i];
                Destroy(shield.gameObject);
            }

            m_ShieldList.Clear();
        }

        public ShieldController CreateShield(bool showAfterCreate = false)
        {
            var shield = Instantiate(Resources.Load<GameObject>("Prefabs/Shield"), screenContainer).GetComponent<ShieldController>();
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

        public void HideScreenShield(ShieldController shield)
        {
            if (shield.gameObject.activeInHierarchy)
            {
                shield.unscaledAnimation.Play("ShieldHide", (anim) => {
                    m_ShieldList.Remove(shield);
                    Destroy(shield.gameObject);
                }, speed: animationSpeed);
            }
        }

        public void AddShieldTapEvent(ShieldController shield)
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

        public GameObject CreateTransparentTopShield()
        {
            var shield = Instantiate(Resources.Load<GameObject>("Prefabs/TransparentShield"), topShieldContainer.transform);
            shield.name = "Transparent Shield";

            var image = shield.GetComponent<Image>();
            image.color = new Color(0, 0, 0, 0);

            shield.SetActive(false);

            return shield;
        }

        public void ShowShield()
        {
            for (int i = 0; i < shieldList.Count; i++)
            {
                var shield = shieldList[i];

                if (shield != null)
                {
                    if (!shield.gameObject.activeInHierarchy)
                    {
                        shield.gameObject.SetActive(true);
                    }

                    shield.unscaledAnimation.Play("ShieldShow", speed: animationSpeed);
                }
            }
        }

        public void HideShield()
        {
            for (int i = 0; i < shieldList.Count; i++)
            {
                var shield = shieldList[i];

                if (shield != null)
                {
                    shield.unscaledAnimation.Play("ShieldHide", speed: animationSpeed);
                }
            }
        }
        #endregion

        #region Private Functions
        private void OnShieldTap()
        {
            screenManager.CloseScreen();
        }

        private void UpdateScreenShieldColor(ShieldController shield)
        {
            var image = shield.GetComponent<Image>();
            image.color = m_ScreenShieldColor;
        }

        private void ShowScreenShield(ShieldController shield)
        {
            if (!shield.gameObject.activeInHierarchy || (shield.unscaledAnimation.isPlaying && shield.unscaledAnimation.currentClipName == "ShieldHide"))
            {
                shield.gameObject.SetActive(true);
                shield.unscaledAnimation.Play("ShieldShow", speed: animationSpeed);
            }
        }
        #endregion
    }
}