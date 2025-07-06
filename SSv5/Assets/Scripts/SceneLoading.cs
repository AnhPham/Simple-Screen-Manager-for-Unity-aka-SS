using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS.UI;

public class SceneLoading : MonoBehaviour, ISceneLoading
{
    [SerializeField] RectTransform _progress;
    [SerializeField] RectTransform _progressBG;
    [SerializeField] Animation _animation;
    [SerializeField] CanvasGroup _canvasGroup;

    public void Show()
    {
        _animation.Play("SceneLoadingShow");
    }

    public void Hide()
    {
        _animation.Play("SceneLoadingHide");
    }

    public float ShowDuration()
    {
        return _animation["SceneLoadingShow"].length;
    }

    public float HideDuration()
    {
        return _animation["SceneLoadingHide"].length;
    }

    private void OnEnable()
    {
        _canvasGroup.alpha = 0;
        _progress.sizeDelta = new Vector2(0, _progressBG.sizeDelta.y);
    }

    private void Update()
    {
        _progress.sizeDelta = new Vector2(Core.asyncOperationProgress * _progressBG.sizeDelta.x, _progressBG.sizeDelta.y);
    }
}
