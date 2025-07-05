using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS.UI;

public class SceneLoading : MonoBehaviour, ISceneLoading
{
    [SerializeField] RectTransform m_Progress;
    [SerializeField] RectTransform m_ProgressBG;
    [SerializeField] Animation m_Animation;

    public void Show()
    {
        m_Animation.Play("SceneLoadingShow");
    }

    public void Hide()
    {
        m_Animation.Play("SceneLoadingHide");
    }

    public float ShowDuration()
    {
        return m_Animation["SceneLoadingShow"].length;
    }

    public float HideDuration()
    {
        return m_Animation["SceneLoadingHide"].length;
    }

    private void OnEnable()
    {
        m_Progress.sizeDelta = new Vector2(0, m_ProgressBG.sizeDelta.y);
    }

    private void Update()
    {
        m_Progress.sizeDelta = new Vector2(Core.asyncOperationProgress * m_ProgressBG.sizeDelta.x, m_ProgressBG.sizeDelta.y);
    }
}
