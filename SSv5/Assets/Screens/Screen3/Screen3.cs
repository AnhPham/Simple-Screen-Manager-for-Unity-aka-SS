using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SS.UI;

public class Screen3 : MonoBehaviour, IKeyBack
{
    [SerializeField] Text _label;
    public Text Label => _label;

    public void OnKeyBack()
    {
        Core.Close(hideAnimation: ScreenAnimation.FadeHide);
    }
}
