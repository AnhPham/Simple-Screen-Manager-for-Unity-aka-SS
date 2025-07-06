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
        #region Serialize Fields
        [SerializeField] protected Color _screenShieldColor = new Color(0, 0, 0, 0.8f);
        [SerializeField] protected bool _closeOnTappingShield = false;
        [SerializeField] protected ScreenManager _screenManager;
        [SerializeField] protected GeneralManager _generalManager;
        #endregion

        #region Protected Member
        protected List<ShieldController> _shieldList = new List<ShieldController>();
        protected GameObject _transparentTopShield;
        #endregion

        #region Public Properties
        public GameObject TransparentTopShield => _transparentTopShield;
        public ScreenManager Screen { get => _screenManager; set => _screenManager = value; }
        public GeneralManager General { get => _generalManager; set => _generalManager = value; }
        #endregion

        #region Protected Properties
        protected List<ShieldController> ShieldList => _shieldList;
        protected RectTransform ScreenContainer => _generalManager.ScreenContainer;
        protected RectTransform TopShieldContainer => _generalManager.TopShieldContainer;
        protected float AnimationSpeed => _generalManager.AnimationSpeed;
        #endregion

        #region Unity Cycle
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);

            General = FindObjectOfType<GeneralManager>();
            _transparentTopShield = CreateTransparentTopShield();
        }
        #endregion

        #region Public Functions
        public virtual void Setup(Color screenShieldColor, bool closeOnTappingShield = false)
        {
            _screenShieldColor = screenShieldColor;
            Setup(closeOnTappingShield);
        }

        public virtual void Setup(bool closeOnTappingShield = false)
        {
            _closeOnTappingShield = closeOnTappingShield;
        }

        public virtual void DestroyAllShields()
        {
            for (int i = 0; i < _shieldList.Count; i++)
            {
                var shield = _shieldList[i];
                Destroy(shield.gameObject);
            }

            _shieldList.Clear();
        }

        public virtual ShieldController CreateShield(bool showAfterCreate = false)
        {
            var shield = Instantiate(Resources.Load<GameObject>("Prefabs/Shield"), ScreenContainer).GetComponent<ShieldController>();
            shield.name = "Screen Shield";
            shield.transform.SetAsLastSibling();
            shield.gameObject.SetActive(false);

            UpdateScreenShieldColor(shield);
            AddShieldTapEvent(shield);
            _shieldList.Add(shield);

            if (showAfterCreate)
            {
                ShowScreenShield(shield);
            }

            return shield;
        }

        public virtual void HideScreenShield(ShieldController shield)
        {
            if (shield.gameObject.activeInHierarchy)
            {
                shield.unscaledAnimation.Play("ShieldHide", (anim) => {
                    _shieldList.Remove(shield);
                    Destroy(shield.gameObject);
                }, speed: AnimationSpeed);
            }
        }

        public virtual void ShowShield()
        {
            for (int i = 0; i < ShieldList.Count; i++)
            {
                var shield = ShieldList[i];

                if (shield != null)
                {
                    if (!shield.gameObject.activeInHierarchy)
                    {
                        shield.gameObject.SetActive(true);
                    }

                    shield.unscaledAnimation.Play("ShieldShow", speed: AnimationSpeed);
                }
            }
        }

        public virtual void HideShield()
        {
            for (int i = 0; i < ShieldList.Count; i++)
            {
                var shield = ShieldList[i];

                if (shield != null)
                {
                    shield.unscaledAnimation.Play("ShieldHide", speed: AnimationSpeed);
                }
            }
        }
        #endregion

        #region Protected Functions
        protected virtual void OnShieldTap()
        {
            Screen.CloseScreen();
        }

        protected virtual void UpdateScreenShieldColor(ShieldController shield)
        {
            var image = shield.GetComponent<Image>();
            image.color = _screenShieldColor;
        }

        protected virtual void ShowScreenShield(ShieldController shield)
        {
            if (!shield.gameObject.activeInHierarchy || (shield.unscaledAnimation.isPlaying && shield.unscaledAnimation.currentClipName == "ShieldHide"))
            {
                shield.gameObject.SetActive(true);
                shield.unscaledAnimation.Play("ShieldShow", speed: AnimationSpeed);
            }
        }

        protected virtual void AddShieldTapEvent(ShieldController shield)
        {
            if (_closeOnTappingShield)
            {
                var eventTrigger = shield.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
                entry.callback.AddListener((eventData) => { OnShieldTap(); });

                eventTrigger.triggers.Add(entry);
            }
        }

        protected virtual GameObject CreateTransparentTopShield()
        {
            var shield = Instantiate(Resources.Load<GameObject>("Prefabs/TransparentShield"), TopShieldContainer.transform);
            shield.name = "Transparent Shield";

            var image = shield.GetComponent<Image>();
            image.color = new Color(0, 0, 0, 0);

            shield.SetActive(false);

            return shield;
        }
        #endregion
    }
}