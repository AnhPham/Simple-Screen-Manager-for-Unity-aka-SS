using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS.UI;

public class CustomScreenManager : ScreenManager
{
    public override void Close(Component screen, OnScreenClosedDelegate onScreenClosed = null, string hideAnimation = null)
    {
        if (IsAnyScreenActive())
        {
            TryDestroyScreen(screen);
            onScreenClosed?.Invoke();
        }
    }
}