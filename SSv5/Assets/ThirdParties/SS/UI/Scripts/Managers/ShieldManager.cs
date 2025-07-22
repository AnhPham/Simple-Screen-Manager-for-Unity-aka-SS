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

        public virtual ShieldController CreateShield(bool showAfterCreate = false, float shieldAlpha = -1)
        {
            var shield = Instantiate(Resources.Load<GameObject>(ShieldPrefabPath()), ScreenContainer).GetComponent<ShieldController>();
            shield.name = "Screen Shield";
            shield.transform.SetAsLastSibling();
            shield.gameObject.SetActive(false);

            UpdateShieldColor(shield, shieldAlpha);
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

        public virtual void MoveShieldToTop(ShieldController shield)
        {
            if (shield != null && _shieldList.Contains(shield))
            {
                shield.transform.SetAsLastSibling();

                _shieldList.Remove(shield);
                _shieldList.Add(shield);
            }
        }
        #endregion

        #region Protected Functions
        public virtual void UpdateShield(ShieldController shield, GameObject screen)
        {
            if (shield == null)
                return;

            if (screen == null)
                return;

            UpdateShieldEvents(shield, screen);

            var screenController = screen.GetComponent<ScreenController>();
            if (screenController != null)
            {
                UpdateShieldColor(shield, screenController.ShieldAlpha);
            }
        }

        protected virtual void OnShieldTap()
        {
            ScreenManager.Close();
        }

        protected virtual void UpdateShieldColor(ShieldController shield, float shieldAlpha = -1)
        {
            var image = shield.GetComponent<Image>();
            image.color = shieldAlpha < 0 ? _screenShieldColor : new Color(_screenShieldColor.r, _screenShieldColor.g, _screenShieldColor.b, shieldAlpha);
        }

        protected virtual void ShowShield(ShieldController shield)
        {
            if (!shield.gameObject.activeInHierarchy || (shield.UnscaledAnimation.IsPlaying && shield.UnscaledAnimation.CurrentClipName == "ShieldHide"))
            {
                shield.gameObject.SetActive(true);
                shield.UnscaledAnimation.Play("ShieldShow", speed: AnimationSpeed);
            }
        }

        protected virtual void UpdateShieldEvents(ShieldController shield, GameObject screen)
        {
            // Get or Add EventTrigger
            var eventTrigger = shield.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = shield.gameObject.AddComponent<EventTrigger>();
            }

            // Clear
            eventTrigger.triggers.Clear();

            // Find IShieldBehavior
            screen.TryGetComponent(out IShieldBehavior shieldBehavior);

            if (shieldBehavior != null)
            {
                // Tap
                var tap = CreateShieldTapEntry(shieldBehavior);
                eventTrigger.triggers.Add(tap);

                // Hold
                var hold = CreateShieldHoldEntry(shieldBehavior);
                eventTrigger.triggers.Add(hold);

                // Release
                var release = CreateShieldReleaseEntry(shieldBehavior);
                eventTrigger.triggers.Add(release);
            }
            else
            {
                if (_closeOnTappingShield)
                {
                    // Find IKeyBack
                    screen.TryGetComponent(out IKeyBack keyBack);

                    // Priority OnKeyBack if found IKeyBack
                    var tap = keyBack != null ? CreateShieldTapEntry(keyBack) : CreateShieldTapEntry();

                    // Add trigger
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

        protected virtual EventTrigger.Entry CreateShieldTapEntry(IKeyBack keyBack)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => { keyBack.OnKeyBack(); });

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