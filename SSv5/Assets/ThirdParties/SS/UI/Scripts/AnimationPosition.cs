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
        [SerializeField] protected float _baseWidth = 720;
        [SerializeField] protected float _baseHeight = 1600;

        protected float _width;
        protected float _height;
        protected RectTransform _rect;
        protected UnscaledAnimation _animation;

        protected virtual void Awake()
        {
            _animation = GetComponent<UnscaledAnimation>();
            _rect = GetComponent<RectTransform>();
            _height = _baseHeight;
            _width = _height * Screen.width / Screen.height;
        }

        protected virtual void LateUpdate()
        {
            if (_animation != null && _animation.IsPlaying)
            {
                Reposition();
            }
        }

        public virtual void Reposition()
        {
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x * _width / _baseWidth, _rect.anchoredPosition.y * _height / _baseHeight);
        }
    }
}