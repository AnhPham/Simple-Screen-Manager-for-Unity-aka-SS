using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SS.UI;

public class Scene1 : MonoBehaviour
{
    public static string ScreenAnimationType = "Default";

    [SerializeField] private Text _label;
    [SerializeField] private Text _animButtonLabel;

    public string Data { get; set; }

    private void Start()
    {
        _label.text = Data;

        _animButtonLabel.text = (ScreenAnimationType == "Default" ? "Custom" : "Default") + " Animation";

        Core.Load<Scene2>(sceneName: "Scene2", mode: LoadSceneMode.Additive, onSceneLoaded: (scene2) =>
        {
            scene2.Cube.localScale = new Vector3(2, 1, 1);
        });
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

    public void OnAnimationButtonTap()
    {
        _animButtonLabel.text = ScreenAnimationType + " Animation";

        switch (ScreenAnimationType)
        {
            case "Default":
                ScreenAnimationType = "Custom";
                Core.Set(sceneLoadingName: "SceneLoading", loadingName: "Loading", tooltipName: "Tooltip", screenAnimationPath: "Custom/Animations", closeOnTappingShield: true);
                break;

            case "Custom":
                ScreenAnimationType = "Default";
                Core.Set(loadingName: "Loading", tooltipName: "Tooltip");
                break;
        }
    }
}
