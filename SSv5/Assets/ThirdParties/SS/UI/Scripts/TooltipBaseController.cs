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
        public float padding = 10f;

        RectTransform tooltipRect;
        UnscaledAnimation m_Animation;
        Vector2 m_StartPosition;
        float m_TargetY;

        protected virtual void Awake()
        {
            tooltipRect = GetComponent<RectTransform>();
            m_Animation = GetComponent<UnscaledAnimation>();
        }

        private void LateUpdate()
        {
            if (m_Animation != null && m_Animation.isPlaying)
            {
                Reposition();
            }
        }

        private void Reposition()
        {
            tooltipRect.anchoredPosition = new Vector2(tooltipRect.anchoredPosition.x + m_StartPosition.x, tooltipRect.anchoredPosition.y * m_TargetY + m_StartPosition.y); ;
        }

        public void ShowTooltip(string text, Vector2 anchoredPosition, float targetY = 100f)
        {
            // Canvas
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }
            var canvasRect = canvas.GetComponent<RectTransform>();

            // Target Y
            m_TargetY = targetY;

            // Text
            SetText(text);
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

            // Size
            float halfWidth = tooltipRect.rect.width * 0.5f;
            float canvasHalfWidth = canvasRect.rect.width * 0.5f;

            // Update position to avoid overflow screen
            if (anchoredPosition.x - halfWidth < -canvasHalfWidth + padding)
            {
                anchoredPosition.x = -canvasHalfWidth + halfWidth + padding;
            }
            else if (anchoredPosition.x + halfWidth > canvasHalfWidth - padding)
            {
                anchoredPosition.x = canvasHalfWidth - halfWidth - padding;
            }
            tooltipRect.anchoredPosition = anchoredPosition;

            // Start position
            m_StartPosition = anchoredPosition;

            // Activate
            gameObject.SetActive(true);

            m_Animation.Play("Tooltip", OnAnimationEnd);
        }

        public void ShowTooltip(string text, Vector3 worldPosition, float targetY = 100f)
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

        public void HideToolTip()
        {
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }

        public void OnAnimationEnd(string clipName)
        {
            HideToolTip();
        }
    }
}