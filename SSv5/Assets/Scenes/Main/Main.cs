using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SS.UI;

public class Main : MonoBehaviour
{
    private void Start()
    {
        Application.targetFrameRate = 60;

        Core.Init();
        Core.Set(loadingName: "Loading", tooltipName: "Tooltip");
        //Core.Init(screenManagerPath: "Custom/Managers/ScreenManager", shieldManagerPath: "Custom/Managers/ShieldManager");
        //Core.Set(sceneLoadingName: "SceneLoading", loadingName: "Loading", tooltipName: "Tooltip", screenAnimationPath: "Custom/Animations");

        Core.Load<Scene1>(sceneName: "Scene1", onSceneLoaded: (scene1) =>
        {
            scene1.Data = "Scene1";
        });
    }
}
