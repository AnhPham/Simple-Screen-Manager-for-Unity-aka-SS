using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SS.UI;

public class Screen1 : MonoBehaviour, IKeyBack
{
    public Text label;

    private bool pressedSpaceKey;

    public void OnKeyBack()
    {
        Core.Close();
    }

    public void OnAddScreen2ButtonTap()
    {
        Core.Add<Screen2>(screenName: "Screen2", animationObjectName: "AnimationRoot", useExistingScreen: true, onScreenLoad: (screen) => {
            screen.label.text = "Screen2";
        });
    }

    public void OnAddScreen2UntilNoScreenButtonTap()
    {
        Core.Add<Screen2>(screenName: "Screen2", animationObjectName: "AnimationRoot", onScreenLoad: (screen) => {
            screen.label.text = "Screen2";
        }, waitUntilNoScreen: true);
    }

    public void OnAddScreen2UntilSpacePressedButtonTap()
    {
        Core.Add<Screen2>(screenName: "Screen2", animationObjectName: "AnimationRoot", onScreenLoad: (screen) => {
            screen.label.text = "Screen2";
            pressedSpaceKey = false;
        }, addCondition: WaitSpaceKey);
    }

    public void OnAddScreen2ButNotHideMeButtonTap()
    {
        Core.Add<Screen2>(screenName: "Screen2", animationObjectName: "AnimationRoot", onScreenLoad: (screen) => {
            screen.label.text = "Screen2";
        }, hideTopScreen: false);
    }

    public void OnAddScreen3AndDestroyMeButtonTap()
    {
        Core.Add<Screen3>(screenName: "Screen3", animationObjectName: "AnimationRoot", onScreenLoad: (screen) => {
            screen.label.text = "Screen3";
        }, destroyTopScreen: true);
    }

    private bool WaitSpaceKey()
    {
        return pressedSpaceKey;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            pressedSpaceKey = true;
        }
    }
}
