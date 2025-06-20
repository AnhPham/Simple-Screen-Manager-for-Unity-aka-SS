using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS.UI;

public class SceneLoading : MonoBehaviour
{
    const float PROGRESS_WIDTH = 500;
    const float PROGRESS_HEIGHT = 100;

    [SerializeField] RectTransform m_Progress;

    private void Update()
    {
        m_Progress.sizeDelta = new Vector2(ScreenManager.asyncOperation.progress * PROGRESS_WIDTH, PROGRESS_HEIGHT);
    }
}
