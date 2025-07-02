using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SS.UI;

public class Screen3 : MonoBehaviour, IKeyBack
{
    public Text label;

    public void OnKeyBack()
    {
        Core.Close(hideAnimation: ScreenAnimation.RightHide);
    }
}
