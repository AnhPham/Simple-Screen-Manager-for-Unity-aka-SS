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
        private List<UnscaledAnimation> m_ShieldList = new List<UnscaledAnimation>();
        private GameObject m_TransparentTopShield;
        #endregion

        #region Get/Set
        public GameObject transparentTopShield
        {
            get
            {
                return m_TransparentTopShield;
            }
        }

        public List<UnscaledAnimation> shieldList
        {
            get
            {
                return m_ShieldList;
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

        public RectTransform screenContainer
        {
            get
            {
                return m_GeneralManager.screenContainer;
            }
        }

        public RectTransform topShieldContainer
        {
            get
            {
                return m_GeneralManager.topShieldContainer;
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

        public void DestroyShield()
        {
            for (int i = 0; i < m_ShieldList.Count; i++)
            {
                var shield = m_ShieldList[i];
                Destroy(shield.gameObject);
            }

            m_ShieldList.Clear();
        }

        public UnscaledAnimation CreateShield(bool showAfterCreate = false)
        {
            var shield = Instantiate(Resources.Load<GameObject>("Prefabs/Shield"), screenContainer).GetComponent<UnscaledAnimation>();
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

        public void HideScreenShield(UnscaledAnimation shield)
        {
            if (shield.gameObject.activeInHierarchy)
            {
                shield.Play("ShieldHide", (anim) => {
                    m_ShieldList.Remove(shield);
                    Destroy(shield.gameObject);
                }, speed: animationSpeed);
            }
        }

        public void AddShieldTapEvent(UnscaledAnimation shield)
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

                    shield.Play("ShieldShow", speed: animationSpeed);
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
                    shield.Play("ShieldHide", speed: animationSpeed);
                }
            }
        }
        #endregion

        #region Private Functions
        private void OnShieldTap()
        {
            screenManager.CloseScreen();
        }

        private void UpdateScreenShieldColor(UnscaledAnimation shield)
        {
            var image = shield.GetComponent<Image>();
            image.color = m_ScreenShieldColor;
        }

        private void ShowScreenShield(UnscaledAnimation shield)
        {
            if (!shield.gameObject.activeInHierarchy || (shield.isPlaying && shield.currentClipName == "ShieldHide"))
            {
                shield.gameObject.SetActive(true);
                shield.Play("ShieldShow", speed: animationSpeed);
            }
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