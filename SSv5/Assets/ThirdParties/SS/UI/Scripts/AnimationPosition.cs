/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEngine;

namespace SS.UI
{
    /// <summary>
    /// Unity animation fixes position in its timeline, but we want it runs dynamically in some cases.
    /// </summary>
    public class AnimationPosition : MonoBehaviour
    {
        [SerializeField] float _baseWidth = 720;
        [SerializeField] float _baseHeight = 1600;

        float _width;
        float _height;
        RectTransform _rect;
        UnscaledAnimation _animation;

        private void Awake()
        {
            _animation = GetComponent<UnscaledAnimation>();
            _rect = GetComponent<RectTransform>();
            _height = _baseHeight;
            _width = _height * Screen.width / Screen.height;
        }

        private void LateUpdate()
        {
            if (_animation != null && _animation.IsPlaying)
            {
                Reposition();
            }
        }

        public void Reposition()
        {
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x * _width / _baseWidth, _rect.anchoredPosition.y * _height / _baseHeight);
        }
    }
}