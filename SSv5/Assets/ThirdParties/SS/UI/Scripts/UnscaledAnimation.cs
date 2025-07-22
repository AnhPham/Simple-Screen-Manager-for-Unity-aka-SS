/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEngine;

namespace SS.UI
{
    public class UnscaledAnimation : MonoBehaviour
    {
        public delegate void OnAnimationEndDelegate(string clipName);
        protected OnAnimationEndDelegate _onAnimationEnd;

        protected float _accumTime = 0F;
        protected float _speed = 1f;
        protected bool _isPlaying = false;
        protected bool _isEndAnim = false;
        protected string _curClipName;
        protected AnimationState _curState;
        protected Animation _animation;

        Animation Animation
        {
            get
            {
                if (_animation == null)
                {
                    _animation = GetComponent<Animation>();
                }
                return _animation;
            }
        }

        public bool IsPlaying => _isPlaying;

        public string CurrentClipName => _curClipName;

        public virtual void Play(string clip, OnAnimationEndDelegate onAnimationEnd = null, float speed = 1)
        {
            if (_curState != null)
            {
                _curState.enabled = false;
            }

            _accumTime = 0F;
            _curClipName = clip;
            _curState = Animation[clip];
            _curState.weight = 1;
            _curState.blendMode = AnimationBlendMode.Blend;
            _curState.normalizedTime = 0;
            _curState.enabled = true;
            _isPlaying = true;
            _isEndAnim = false;
            _onAnimationEnd = onAnimationEnd;
            _speed = speed;
        }

        public virtual void PauseAtBeginning(string animationName)
        {
            Animation.Play(animationName);
            Animation[animationName].time = 0;
            Animation.Sample();
            Animation.Stop();
        }

        public virtual float GetLength(string animationName)
        {
            return Animation[animationName].length;
        }

        protected virtual void Start()
        {
            if (Animation.playAutomatically)
            {
                Animation.Stop();
                Play(Animation.clip.name);
            }
        }

        protected virtual void Update()
        {
            if (_isPlaying)
            {
                if (_isEndAnim == true)
                {
                    _curState.enabled = false;
                    _isPlaying = false;

                    if (_onAnimationEnd != null)
                    {
                        _onAnimationEnd(_curClipName);
                    }

                    return;
                }

                _accumTime += Time.unscaledDeltaTime * _speed;
                if (_accumTime >= _curState.length)
                {
                    if (_curState.wrapMode == WrapMode.Loop)
                    {
                        _accumTime = 0;
                    }
                    else
                    {
                        _accumTime = _curState.length;
                        _isEndAnim = true;
                    }
                }
                _curState.normalizedTime = _accumTime / _curState.length;
            }
        }
    }
}