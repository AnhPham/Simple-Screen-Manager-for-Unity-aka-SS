using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SS.UI;

public class Screen1 : MonoBehaviour, IKeyBack, IShieldBehavior
{
    [SerializeField] Text _label;
    public Text Label => _label;

    private bool _pressedSpaceKey;

    public void OnKeyBack()
    {
        Core.Close();
    }

    public void OnAddScreen2ButtonTap()
    {
        Core.Add<Screen2>(screenName: "Screen2", animationObjectName: "AnimationRoot", useExistingScreen: true, onScreenLoad: (screen) => {
            screen.Label.text = "Screen2";
        });
    }

    public void OnAddScreen2UntilNoScreenButtonTap()
    {
        Core.Add<Screen2>(screenName: "Screen2", animationObjectName: "AnimationRoot", onScreenLoad: (screen) => {
            screen.Label.text = "Screen2";
        }, waitUntilNoScreen: true);
    }

    public void OnAddScreen2UntilSpacePressedButtonTap()
    {
        Core.Add<Screen2>(screenName: "Screen2", animationObjectName: "AnimationRoot", onScreenLoad: (screen) => {
            screen.Label.text = "Screen2";
            _pressedSpaceKey = false;
        }, addCondition: WaitSpaceKey);
    }

    public void OnAddScreen2ButNotHideMeButtonTap()
    {
        Core.Add<Screen2>(screenName: "Screen2", animationObjectName: "AnimationRoot", onScreenLoad: (screen) => {
            screen.Label.text = "Screen2";
        }, hideTopScreen: false, shieldAlpha: 0.9f);
    }

    public void OnAddScreen3AndDestroyMeButtonTap()
    {
        Core.Add<Screen3>(screenName: "Screen3", onScreenLoad: (screen) => {
            screen.Label.text = "Screen3";
        }, destroyTopScreen: true);
    }

    public void OnDestroyMeThenAddScreen3ButtonTap()
    {
        Core.Destroy();

        Core.Add<Screen3>(screenName: "Screen3", onScreenLoad: (screen) => {
            screen.Label.text = "Screen3";
        }, onScreenPreLoad: Wait1Seconds);

        IEnumerator Wait1Seconds()
        {
            Core.Loading(true);
            yield return new WaitForSeconds(1);
            Core.Loading(false);
        }
    }

    private bool WaitSpaceKey()
    {
        return _pressedSpaceKey;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _pressedSpaceKey = true;
        }
    }

    public void OnShieldTap()
    {
        Core.Close();
    }

    public void OnShieldHold()
    {
    }

    public void OnShieldRelease()
    {
    }
}
