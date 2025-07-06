/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
    public class TooltipBaseController : MonoBehaviour
    {
        [SerializeField] protected float _padding = 10f;

        protected RectTransform _tooltipRect;
        protected UnscaledAnimation _animation;
        protected Vector2 _startPosition;
        protected float _targetY;

        protected virtual void Awake()
        {
            _tooltipRect = GetComponent<RectTransform>();
            _animation = GetComponent<UnscaledAnimation>();
        }

        protected virtual void LateUpdate()
        {
            if (_animation != null && _animation.IsPlaying)
            {
                Reposition();
            }
        }

        protected virtual void Reposition()
        {
            _tooltipRect.anchoredPosition = new Vector2(_tooltipRect.anchoredPosition.x + _startPosition.x, _tooltipRect.anchoredPosition.y * _targetY + _startPosition.y); ;
        }

        public virtual void ShowTooltip(string text, Vector2 anchoredPosition, float targetY = 100f)
        {
            // Canvas
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }
            var canvasRect = canvas.GetComponent<RectTransform>();

            // Target Y
            _targetY = targetY;

            // Text
            SetText(text);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);

            // Size
            float halfWidth = _tooltipRect.rect.width * 0.5f;
            float canvasHalfWidth = canvasRect.rect.width * 0.5f;

            // Update position to avoid overflow screen
            if (anchoredPosition.x - halfWidth < -canvasHalfWidth + _padding)
            {
                anchoredPosition.x = -canvasHalfWidth + halfWidth + _padding;
            }
            else if (anchoredPosition.x + halfWidth > canvasHalfWidth - _padding)
            {
                anchoredPosition.x = canvasHalfWidth - halfWidth - _padding;
            }
            _tooltipRect.anchoredPosition = anchoredPosition;

            // Start position
            _startPosition = anchoredPosition;

            // Activate
            gameObject.SetActive(true);

            _animation.Play("Tooltip", OnAnimationEnd);
        }

        public virtual void ShowTooltip(string text, Vector3 worldPosition, float targetY = 100f)
        {
            // Canvas
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            var localPosition = canvas.transform.InverseTransformPoint(worldPosition);

            ShowTooltip(text, new Vector2(localPosition.x, localPosition.y), targetY);
        }

        protected virtual void SetText(string text)
        {
        }

        public virtual void HideToolTip()
        {
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }

        public virtual void OnAnimationEnd(string clipName)
        {
            HideToolTip();
        }
    }
}