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

        ScreenManager.Set(sceneLoadingName: "SceneLoading", loadingName: "Loading", tooltipName: "Tooltip");

        ScreenManager.Load<Scene1>(sceneName: "Scene1", onSceneLoaded: (scene1) =>
        {
            scene1.data = "Scene1";
        });
    }
}
