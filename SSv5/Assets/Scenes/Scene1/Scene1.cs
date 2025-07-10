using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SS.UI;

public class Scene1 : MonoBehaviour
{
    [SerializeField] private Text _label;

    public string Data { get; set; }

    private void Start()
    {
        _label.text = Data;

        Core.Load<Scene2>(sceneName: "Scene2", mode: LoadSceneMode.Additive, onSceneLoaded: (scene2) =>
        {
            scene2.Cube.localScale = new Vector3(2, 1, 1);
        });

        AddScreen1();
    }

    public void OnScreen1ButtonTap()
    {
        AddScreen1();
    }

    private void AddScreen1()
    {
        Core.Add<Screen1>(screenName: "Screen1", showAnimation: ScreenAnimation.ScaleShow, hideAnimation: ScreenAnimation.RightHide, onScreenLoad: (screen) => {
            screen.Label.text = "Screen1";
        });
    }
}
