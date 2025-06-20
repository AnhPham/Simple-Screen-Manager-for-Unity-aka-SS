using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SS.UI;

public class Scene1 : MonoBehaviour
{
    [SerializeField] private Text label;

    public string data { get; set; }

    private void Start()
    {
        label.text = data;

        ScreenManager.Load<Scene2>(sceneName: "Scene2", mode: LoadSceneMode.Additive, onSceneLoaded: (scene2) =>
        {
            scene2.cube.localScale = new Vector3(2, 1, 1);
        });

        AddScreen1();
    }

    public void OnScreen1ButtonTap()
    {
        AddScreen1();
    }

    private void AddScreen1()
    {
        ScreenManager.Add<Screen1>(screenName: "Screen1", showAnimation: ScreenAnimation.RightShow, hideAnimation: ScreenAnimation.RightHide, onScreenLoad: (screen) => {
            screen.label.text = "Screen1";
        });
    }
}
