using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS.UI;

public class CustomScreenManager : ScreenManager
{
    public override void Close(Component screen, OnScreenClosedDelegate onScreenClosed = null, string hideAnimation = null)
    {
        switch (Scene1.ScreenAnimationType)
        {
            case "Default":
                base.Close(screen, onScreenClosed, hideAnimation);
                break;

            case "Custom":
                if (IsAnyScreenActive())
                {
                    TryDestroyScreen(screen);
                    onScreenClosed?.Invoke();
                }
                break;
        }
    }

    protected override int FramesDelayBeforeShowAnimation()
    {
        switch (Scene1.ScreenAnimationType)
        {
            case "Custom":
                return 0;
        }

        return base.FramesDelayBeforeShowAnimation();
    }
}