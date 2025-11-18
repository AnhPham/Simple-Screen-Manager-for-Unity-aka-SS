using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SS.UI;

public class Screen2 : MonoBehaviour, IKeyBack, IShieldBehavior
{
    [SerializeField] Text _label;
    public Text Label => _label;

    public void OnLoadScene1ButtonTap()
    {
        Core.Load<Scene1>(sceneName: "Scene1", mode: LoadSceneMode.Single, onSceneLoaded: (scene1) =>
        {
            scene1.Data = "Scene1...";
        }, onScenePreLoad: Wait1Seconds);

        IEnumerator Wait1Seconds()
        {
            yield return new WaitForSeconds(1);
        }
    }

    public void OnKeyBack()
    {
        Core.Close();
    }

    public void OnCloseScreen1ButtonTap()
    {
        var screen1 = FindObjectOfType<Screen1>(true);

        if (screen1 != null)
        {
            Core.Destroy(screen: screen1);
        }
    }

    public void OnCloseAllScreensButtonTap()
    {
        Core.DestroyAll();
    }

    public void OnShowLoadingButtonTap()
    {
        StartCoroutine(ShowLoadingASecond());
    }

    public void OnAddScreen1ButtonTap()
    {
        Core.Add<Screen1>(screenName: "Screen1", showAnimation: ScreenAnimation.RightShow, hideAnimation: ScreenAnimation.RightHide, useExistingScreen: true, onScreenLoad: (screen) => {
            screen.Label.text = "Screen1";
        });
    }

    public void OnShowTooltipButtonTap(Button button)
    {
        Core.ShowTooltip(text: "This is a long tooltip to test overflowing the screen", worldPosition: button.transform.position, targetY:Random.Range(100f, 300f));
    }

    private IEnumerator ShowLoadingASecond()
    {
        Core.Loading(true);

        yield return new WaitForSecondsRealtime(1);

        Core.Loading(false);
    }

    public void OnShieldTap()
    {
    }

    public void OnShieldHold()
    {
        SetAllScreensAlpha(0);
        Core.HideShield();
    }

    public void OnShieldRelease()
    {
        SetAllScreensAlpha(1);
        Core.ShowShield();
    }

    private void SetAllScreensAlpha(float alpha)
    {
        var screens = FindObjectsOfType<ScreenController>();
        for (int i = 0; i < screens.Length; i++)
        {
            screens[i].GetComponent<CanvasGroup>().alpha = alpha;
        }
    }
}
