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
        OnAnimationEndDelegate m_OnAnimationEnd;

        float m_AccumTime = 0F;
        float m_Speed = 1f;
        AnimationState m_CurState;
        bool m_IsPlayingAnim = false;
        bool m_IsEndAnim = false;
        string m_CurClipName;

        Animation m_Animation;
        Animation Animation
        {
            get
            {
                if (m_Animation == null)
                {
                    m_Animation = GetComponent<Animation>();
                }
                return m_Animation;
            }
        }

        public bool isPlaying
        {
            get
            {
                return m_IsPlayingAnim;
            }
        }

        public string currentClipName
        {
            get
            {
                return m_CurClipName;
            }
        }

        public void Play(string clip, OnAnimationEndDelegate onAnimationEnd = null, float speed = 1)
        {
            m_AccumTime = 0F;
            m_CurClipName = clip;
            m_CurState = Animation[clip];
            m_CurState.weight = 1;
            m_CurState.blendMode = AnimationBlendMode.Blend;
            m_CurState.normalizedTime = 0;
            m_CurState.enabled = true;
            m_IsPlayingAnim = true;
            m_IsEndAnim = false;
            m_OnAnimationEnd = onAnimationEnd;
            m_Speed = speed;
        }

        public void PauseAtBeginning(string animationName)
        {
            Animation.Play(animationName);
            Animation[animationName].time = 0;
            Animation.Sample();
            Animation.Stop();
        }

        public float GetLength(string animationName)
        {
            return Animation[animationName].length;
        }

        private void Start()
        {
            if (Animation.playAutomatically)
            {
                Animation.Stop();
                Play(Animation.clip.name);
            }
        }

        private void Update()
        {
            if (m_IsPlayingAnim)
            {
                if (m_IsEndAnim == true)
                {
                    m_CurState.enabled = false;
                    m_IsPlayingAnim = false;

                    if (m_OnAnimationEnd != null)
                    {
                        m_OnAnimationEnd(m_CurClipName);
                    }

                    return;
                }

                m_AccumTime += Time.unscaledDeltaTime * m_Speed;
                if (m_AccumTime >= m_CurState.length)
                {
                    if (m_CurState.wrapMode == WrapMode.Loop)
                    {
                        m_AccumTime = 0;
                    }
                    else
                    {
                        m_AccumTime = m_CurState.length;
                        m_IsEndAnim = true;
                    }
                }
                m_CurState.normalizedTime = m_AccumTime / m_CurState.length;
            }
        }
    }
}