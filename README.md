<h1>Simple & Scalable UI Manager for Unity (aka SS)</h1>

<h2>Who is this for?</h2>

* Developers looking for a **very simple UI Manager / Navigator for Unity** with enough features to build any type of game, even mid-core or more complex projects.
* Those who want to **learn the basics in 10 minutes**.
* Those who want to **master advanced features in 30 minutes**.

<h2>Concept</h2>

* Regardless of fullscreen page or modal/modeless window, they are all considered **Screen**.
* At any given time, **only one Screen is visible** to optimize performance. If a Screen is shown on top of another, the underlying Screen will be temporarily hidden, and it will reappear when the top Screen is closed.
* A Screen is a freeform prefab that does **not require any specific scripts**, allowing it to be used as a child object anywhere.
* A **Scene** is freeform and contains any object, including its own UI canvas. The Scene’s canvas will not be hidden when a Screen is displayed on top.
* The package size is only 38 KB and does **not depend** on any external libraries.

<p align="center">
  <img width="200px" src="/learn/unity/ss/demo.gif?raw=true" alt="Demo">
  <p align="center">In this demo, even though they share the same prefab, the Store can be displayed as a modal window or as the content of the Store Tab.</p>
</p>

<h2>Basic Usage</h2>

<h3>1. Screen Settings</h3>
Set this to change the main canvas scaler and the game window size

```cs
From Menu: SS / Screen Settings / Input Screen Width & Height / Save
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/screen-settings.gif?raw=true" alt="Demo">
</p>

<h3>2. Create a screen</h3>

```cs
From Menu: SS / Screen Generator / Input Screen Name / Generate
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/screen-generator.gif?raw=true" alt="Demo">
</p>

<h3>3. Drag screen prefab to Resources/Screens folder</h3>In case of not using Addressables

<p align="center">
  <img width="500px" src="/learn/unity/ss/drag-screen.gif?raw=true" alt="Demo">
</p>

<h3>4. Add a screen on top with default animation</h3>

```cs
using SS.UI;
```

```cs
Core.Add<Screen1Controller>(screenName: "Screen1");
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/add-screen.gif?raw=true" alt="Demo">
</p>

<h3>5. Close a screen</h3>

```cs
Core.Close();
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/close-screen.gif?raw=true" alt="Demo">
</p>

<h3>6. Load a scene with automatic fade</h3>

```cs
Core.Load<Scene1Controller>(sceneName: "Scene1");
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/load-scene.gif?raw=true" alt="Demo">
</p>

<h3>Basic Usage Tutorial Video</h3>

<a href="https://youtu.be/mzCAjf7hye4" target="_blank" rel="noopener noreferrer">https://youtu.be/mzCAjf7hye4</a>

<br>
<h2>Advance Usage</h2>

<h3>1. Addressables</h3>
If you want to use Addressables, you do not need to drag screen prefabs to Resources/Screens

<h4>1.1. Install Addressables package: </h4>

```cs
From Menu: Window / Package Manager / Unity Registry / Addressables / Install
```

<h4>1.2. Add this Scripting Define Symbol: ADDRESSABLE</h4>

```cs
From Menu: Edit / Project Settings / Player / Other Settings / Scripting Define Symbols / + / ADDRESSABLE / Apply
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/addressable-symbol.png?raw=true" alt="Demo">
</p>

<h4>1.3. Add screen prefab to addressables groups  </h4>

Make sure the addressable name is the same as the screen name, not a path to the prefab.

<p align="center">
  <img width="500px" src="learn/unity/ss/advance/addressable-groups.gif?raw=true" alt="Demo">
</p>

<h3>2. Screen Animations</h3>

<h4>2.1. Default Screen Animations: </h4>

```cs
public enum ScreenAnimation
{
    BottomHide, // The screen slides from the center to the bottom when hiding.
    BottomShow, // The screen slides from the bottom to the center when showing.
    FadeHide,   // The screen fades out when hiding.
    FadeShow,   // The screen fades in when showing.
    LeftHide,   // The screen slides from the center to the left when hiding.
    LeftShow,   // The screen slides from the left to the center when showing.
    RightHide,  // The screen slides from the center to the right when hiding.
    RightShow,  // The screen slides from the right to the center when showing.
    RotateHide, // The screen rotates clockwise when hiding.
    RotateShow, // The screen rotates counterclockwise when showing.
    ScaleHide,  // The screen scales down to 0 when hiding.
    ScaleShow,  // The screen scales up to 1 when showing.
    TopHide,    // The screen slides from the center to the top when hiding.
    TopShow     // The screen slides from the top to the center when showing.
}
```

<h4>2.2. Set Animations when adding screen: </h4>

The screen slides from the left to the center when showing.
```cs
Core.Add<Screen1Controller>(screenName: "Screen1", showAnimation: ScreenAnimation.LeftShow, hideAnimation: ScreenAnimation.LeftHide);
```

The screen slides from the center to the left when hiding. The 'hideAnimation' which is declared in the Add function will be used
```cs
Core.Close();
```

The screen fades out when hiding.
```cs
Core.Close(hideAnimation: ScreenAnimation.FadeHide);
```

<h4>2.3. Custom Screen Animations: </h4>

Put your custom animations (Unity legacy animations) in Resources/Animations
<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/custom-screen-animations.png?raw=true" alt="Demo">
</p>

Add screen with custom animations
```cs
Core.Add<Screen1Controller>(screenName: "Screen1", showAnimation: "Custom1Show", hideAnimation: "Custom1Hide");
```

<h4>2.4. Custom Animation Object: </h4>

In default, animations will be added to the root object of screen

```cs
Core.Add<Screen1Controller>(screenName: "Screen1");
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/default-animation-object.png?raw=true" alt="Demo">
</p>

In case you only want to animate a few objects on the screen, the rest are static and not animated

```cs
Core.Add<Screen1Controller>(screenName: "Screen1", animationObjectName: "Animation");
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/custom-animation-object.png?raw=true" alt="Demo">
</p>

<h4>2.5. Screen Animation Speed </h4>

In default, Screen Animation Speed is 1. You can change it.

```cs
Core.Set(screenAnimationSpeed: 1.5f);
```

<h4>2.6. Show Animation One Time </h4>

Indicate whether a screen play its show animation again when the screen above it closes. By default, showAnimationOneTime is false.

```cs
Core.Set(showAnimationOneTime: true);
```

<h4>2.7. None Screen Animation </h4>

For None Screen Animation, you can use an empty string for showAnimation & hideAnimation like this:

```cs
Core.Add<Screen1Controller>(screenName: "Screen1", showAnimation: "", hideAnimation: "");
```

<h3>3. Events</h3>

<h4>3.1. On Screen Loaded </h4>

Note: onScreenLoaded is called after Awake & OnEnable, before Start of scripts in screen

```cs
Core.Add<Screen1Controller>(screenName: "Screen1", onScreenLoad: (screen) => {
    // screen.Init();
});
```


<h4>3.2. On Scene Loaded </h4>

Note: onSceneLoaded is called after Awake & OnEnable, before Start of scripts in scene

```cs
Core.Load<Scene1Controller>(sceneName: "Scene1", onSceneLoaded: (scene1) =>
{
    // scene1.Init();
});
```

<h4>3.3. On Screen Closed </h4>

Note: onScreenClosed is called after the hideAnimation is ended (right after the screen is destroyed)

```cs
Core.Close(() =>
{
    // Code after closing this screen
});
```

<h4>3.4. On Key Back </h4>

If you create a screen by the Screen Generator, the screen controller will implement OnKeyBack of the IKeyBack interface by default. It means when players press the physics back button on Android (or ESC key on PC), the screen will be closed. If you don't want that, just remove IKeyBack in the script.

```cs
public class Screen1Controller : MonoBehaviour, IKeyBack
{
    public void OnKeyBack()
    {
        Core.Close();
    }
}
```

<h4>3.5. On Screen Added </h4>

Some projects require sending logs for analytics, indicating which screen is added, from which screen, and whether it was added manually (user click) or automatically.

```cs
// On Start of Main
Core.AddListener(onScreenAdded: (toScreen, fromScreen, manually) => {
    Debug.Log(string.Format("Add screen {0} from screen {1} ") + (manually ? "manually" : "automatically"));
});
Core.Load<Scene1Controller>(sceneName: "Scene1");
```

```cs
// On Screen1 Button Tap
Core.Add<Screen1Controller>(screenName: "Screen1", manually:true);
```

```cs
// On Start of Screen1Controller
Core.Add<Screen2Controller>(screenName: "Screen2", manually:false);
```

Output:
```cs
Added Screen1 from Scene1 manually
Added Screen2 from Screen1 automatically
```


<h4>3.6. On Screen Changed </h4>

Some projects require displaying an ads banner only when no screens are being shown.

```cs
void OnEnable()
{
    Core.AddListener(OnScreenChanged);
}
```

```cs
void OnDisable()
{
    Core.RemoveListener(OnScreenChanged);
}
```

```cs
void OnScreenChanged(int screenCount)
{
    if (screenCount > 0)
    {
        // Banner.Hide();
    }
    else
    {
        // Banner.Show();
    }
}
```

<h3>4. Set conditions to display screen</h3>

<h4>4.1. Wait Until No Screen </h4>

In this example, Screen2 will be shown when user closes Screen1, then Screen3 will be shown when user closes Screen2. 

```cs
Core.Add<Screen1Controller>(screenName: "Screen1");
Core.Add<Screen2Controller>(screenName: "Screen2",  waitUntilNoScreen: true);
Core.Add<Screen3Controller>(screenName: "Screen3",  waitUntilNoScreen: true);
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/wait-until-no-screen.gif?raw=true" alt="Demo">
</p>

This example does not use waitUntilNoScreen, 3 Screens will appear consecutively.

```cs
Core.Add<Screen1Controller>(screenName: "Screen1");
Core.Add<Screen2Controller>(screenName: "Screen2");
Core.Add<Screen3Controller>(screenName: "Screen3");
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/no-wait-until-no-screen.gif?raw=true" alt="Demo">
</p>

<h4>4.2. Custom Add-Condition </h4>

In this example, Screen1 will be shown when the bool something variable becomes true

```cs
bool something = false;
```

```cs
Core.Add<Screen1Controller>(screenName: "Screen1", addCondition: WaitSomething);
```

```cs
bool WaitSomething()
{
    return something;
}
```

<h4>4.3. Wait until no screen to do other things </h4>

In some cases, you have to wait until there is no more Screen displayed before doing something to avoid being covered by a Screen.

```cs
IEnumerator WaitUntilNoScreenToDoSomething()
{
    while (!Core.IsNoMoreScreen())
    {
        yield return 0;
    }

    // Do somethings
}
```

<h3>5. Screen Shield</h3>

The Screen shield is an image with customizable color and transparency, located between the current Scene (with its UI canvas) and the top Screen. By default, the Screen shield will be shown when a Screen is displayed.

<h4>5.1. Set Screen Shield Color </h4>

```cs
Core.Set(screenShieldColor: new Color(0, 0, 0, 0.8f));
```

<h4>5.2. Display a Screen with/without a Shield </h4>

```cs
Core.Add<Screen1Controller>(screenName:"Screen1");
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/with-shield.png?raw=true" alt="Demo">
</p>

```cs
Core.Add<Screen1Controller>(screenName:"Screen1", hasShield: false);
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/without-shield.png?raw=true" alt="Demo">
</p>

<h4>5.3. Show/Hide the Screen shield manually (with fade animation) </h4>

```cs
Core.ShowShield();
```

```cs
Core.HideShield();
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/show-hide-shield.gif?raw=true" alt="Demo">
</p>

<h4>5.4. Close on tapping Shield </h4>

Indicate whether close the top screen when users tap the shield. By default, closeOnTappingShield is false.

```cs
Core.Set(closeOnTappingShield: true);
```

<h3>6. Other parameters of adding a screen</h3>

<h4>6.1. Use Existing Screen </h4>

If this parameter is true, check if the screen is existing, bring it to the top. If not found, instantiate a new one

```cs
Core.Add<Screen2Controller>(screenName: "Screen2", useExistingScreen: true);
```

```cs
Core.Add<Screen1Controller>(screenName: "Screen1", useExistingScreen: true);
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/use-existing-screen.gif?raw=true" alt="Demo">
</p>


By default, this parameter is false, instantiate a new screen whenever Add is called

```cs
Core.Add<Screen2Controller>(screenName: "Screen2");
```

```cs
Core.Add<Screen1Controller>(screenName: "Screen1");
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/not-use-existing-screen.gif?raw=true" alt="Demo">
</p>

<h4>6.2. Destroy Top Screen</h4>

If this parameter is true, destroy the top screen before adding a screen

```cs
Core.Add<Screen2Controller>(screenName: "Screen2", destroyTopScreen: true);
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/destroy-top-screen.gif?raw=true" alt="Demo">
</p>

By default, this parameter is false, temporary hide the top screen when add the Screen2, and show it again after closing the Screen2

```cs
Core.Add<Screen2Controller>(screenName: "Screen2");
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/not-destroy-top-screen.gif?raw=true" alt="Demo">
</p>

<h3>7. Loading</h3>

<h4>7.1. Scene Loading </h4>

Show a loading UI while loading a Scene.

From Menu, SS / Screen Generator, create a Screen named SceneLoading. Use *Core.asyncOperation.progress* to get progress of scene loading, like below example

```cs
public class SceneLoadingController : MonoBehaviour
{
    const float PROGRESS_WIDTH = 500;
    const float PROGRESS_HEIGHT = 50;

    [SerializeField] RectTransform m_Progress;

    private void Update()
    {
        m_Progress.sizeDelta = new Vector2(Core.asyncOperation.progress * PROGRESS_WIDTH, PROGRESS_HEIGHT);
    }
}
```

Do not forget to set the Scene Loading name on App Launch 

```cs
Core.Set(sceneLoadingName: "SceneLoading");
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/scene-loading.gif?raw=true" alt="Demo">
</p>

<h4>7.2. Loading on Top</h4>

Show a loading UI on top of all screens

From Menu, SS / Screen Generator, create a Screen named *Loading*. You should add a loop loading animation to it.

Do not forget to set the Loading name on App Launch 

```cs
Core.Set(loadingName: "Loading");
```


Show Loading

```cs
Core.Loading(true);
```

Hide Loading

```cs
Core.Loading(false);
```

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/loading.gif?raw=true" alt="Demo">
</p>

<h3>8. Tooltip</h3>

Show a tooltip with automatic screen padding.

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/tooltip-demo.gif?raw=true" alt="Demo">
</p>

From Menu, SS / Tooltip Generator, input Tooltip name, select Text type (Default or TextMeshPro), click Generate

<p align="center">
  <img width="500px" src="/learn/unity/ss/advance/tooltip-generator.png?raw=true" alt="Demo">
</p>

Edit the Tooltip prefab as you want, drag it to Resources/Screens folder (or drag to Addressable Group in case of using Addressables). You can also edit the Tooltip showing animation

Do not forget to set the Tooltip name on App Launch 

```cs
Core.Set(tooltipName: "Tooltip");
```

Show the tooltip

```cs
public Transform button;
```

```cs
// targetY is the distance from the start position along the Y axis
Core.ShowTooltip(text: "Tooltip Text", worldPosition: button.position, targetY: 100f);
```

Hide the tooltip

```cs
Core.HideTooltip();
```

<h3>9. Other useful methods of Core</h3>

<h4>9.1. Top </h4>

In some cases, you want to add somethings to the top of all Screens (like some flying coins)

```cs
// Coin.cs
transform.SetParent(Core.Top);
```

<h4>9.2. Destroy </h4>

Destroy immediately the screen which is at the top of all screens, without playing animation.

```cs
Core.Destroy();
```

<h4>9.3. DestroyAll </h4>

Destroy immediately all screens, without playing animation.

```cs
Core.DestroyAll();
```

<h4>9.4. Destroy or Close a specific screen </h4>

```cs
var screen1 = FindObjectOfType<Screen1Controller>(true);
```

```cs
Core.Destroy(screen: screen1);
```

```cs
Core.Close(screen: screen1);
```

<h2>Render pipeline compatibility</h2>

:white_check_mark: Built-in<br>
:white_check_mark: URP<br>
:white_check_mark: HDRP<br>

<h2>Unity Version</h2>

2022.3.30 or higher.
In fact this package can work on most versions of Unity because it is very compact.

<h2>License</h2>

This software is released under the MIT License.  
You are free to use it within the scope of the license.  
However, the following copyright and license notices are required for use.

https://github.com/AnhPham/Simple-Screen-Manager-for-Unity-aka-SS?tab=MIT-1-ov-file
