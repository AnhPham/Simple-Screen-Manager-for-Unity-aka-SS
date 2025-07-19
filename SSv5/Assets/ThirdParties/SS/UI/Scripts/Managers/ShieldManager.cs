/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        public ScreenManager ScreenManager { get => _screenManager; set => _screenManager = value; }
        public GeneralManager GeneralManager { get => _generalManager; set => _generalManager = value; }
        public ShieldController GetTopShield => _shieldList.Count > 0 ? _shieldList[_shieldList.Count - 1] : null;
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

            GeneralManager = FindObjectOfType<GeneralManager>();
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

        public virtual ShieldController CreateShield(bool showAfterCreate = false)
        {
            var shield = Instantiate(Resources.Load<GameObject>(ShieldPrefabPath()), ScreenContainer).GetComponent<ShieldController>();
            shield.name = "Screen Shield";
            shield.transform.SetAsLastSibling();
            shield.gameObject.SetActive(false);

            UpdateShieldColor(shield);
            _shieldList.Add(shield);

            if (showAfterCreate)
            {
                ShowShield(shield);
            }

            return shield;
        }

        public virtual void HideShield(ShieldController shield)
        {
            if (shield.gameObject.activeInHierarchy)
            {
                shield.UnscaledAnimation.Play("ShieldHide", (anim) => {
                    _shieldList.Remove(shield);
                    Destroy(shield.gameObject);
                }, speed: AnimationSpeed);
            }
        }

        public virtual void ShowAllShields()
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

                    shield.UnscaledAnimation.Play("ShieldShow", speed: AnimationSpeed);
                }
            }
        }

        public virtual void HideAllShields()
        {
            for (int i = 0; i < ShieldList.Count; i++)
            {
                var shield = ShieldList[i];

                if (shield != null)
                {
                    shield.UnscaledAnimation.Play("ShieldHide", speed: AnimationSpeed);
                }
            }
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
        #endregion

        #region Protected Functions
        protected virtual void OnShieldTap()
        {
            ScreenManager.Close();
        }

        protected virtual void UpdateShieldColor(ShieldController shield)
        {
            var image = shield.GetComponent<Image>();
            image.color = _screenShieldColor;
        }

        protected virtual void ShowShield(ShieldController shield)
        {
            if (!shield.gameObject.activeInHierarchy || (shield.UnscaledAnimation.IsPlaying && shield.UnscaledAnimation.CurrentClipName == "ShieldHide"))
            {
                shield.gameObject.SetActive(true);
                shield.UnscaledAnimation.Play("ShieldShow", speed: AnimationSpeed);
            }
        }

        public virtual void UpdateShieldEvents(ShieldController shield, GameObject screen)
        {
            if (shield == null)
                return;

            if (screen == null)
                return;

            // Get or Add EventTrigger
            var eventTrigger = shield.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = shield.gameObject.AddComponent<EventTrigger>();
            }

            // Clear
            eventTrigger.triggers.Clear();

            screen.TryGetComponent(out IShieldBehavior shieldBehavior);

            if (shieldBehavior != null)
            {
                var tap = CreateShieldTapEntry(shieldBehavior);
                eventTrigger.triggers.Add(tap);

                var hold = CreateShieldHoldEntry(shieldBehavior);
                eventTrigger.triggers.Add(hold);

                var release = CreateShieldReleaseEntry(shieldBehavior);
                eventTrigger.triggers.Add(release);
            }
            else
            {
                if (_closeOnTappingShield)
                {
                    var tap = CreateShieldTapEntry();
                    eventTrigger.triggers.Add(tap);
                }
            }
        }

        protected virtual EventTrigger.Entry CreateShieldTapEntry()
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => { OnShieldTap(); });

            return entry;
        }

        protected virtual EventTrigger.Entry CreateShieldTapEntry(IShieldBehavior shieldBehavior)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => { shieldBehavior.OnShieldTap(); });

            return entry;
        }

        protected virtual EventTrigger.Entry CreateShieldHoldEntry(IShieldBehavior shieldBehavior)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((eventData) => { shieldBehavior.OnShieldHold (); });

            return entry;
        }

        protected virtual EventTrigger.Entry CreateShieldReleaseEntry(IShieldBehavior shieldBehavior)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((eventData) => { shieldBehavior.OnShieldRelease(); });

            return entry;
        }

        protected virtual EventTrigger.Entry CreateShieldHoldEntry()
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((eventData) => { OnShieldTap(); });

            return entry;
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

        protected virtual string ShieldPrefabPath()
        {
            return "Prefabs/Shield";
        }
        #endregion
    }
}