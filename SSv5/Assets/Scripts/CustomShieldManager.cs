using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS.UI;

public class CustomShieldManager : ShieldManager
{
    public override void HideShield(ShieldController shield)
    {
        switch (Scene1.ScreenAnimationType)
        {
            case "Default":
                base.HideShield(shield);
                break;

            case "Custom":
                if (shield.gameObject.activeInHierarchy)
                {
                    _shieldList.Remove(shield);
                    Destroy(shield.gameObject);
                }
                break;
        }
    }

    protected override void ShowShield(ShieldController shield)
    {
        switch (Scene1.ScreenAnimationType)
        {
            case "Default":
                base.ShowShield(shield);
                break;

            case "Custom":
                if (!shield.gameObject.activeInHierarchy)
                {
                    shield.gameObject.SetActive(true);

                    var canvasGroup = shield.GetComponent<CanvasGroup>();
                    canvasGroup.alpha = 1;
                    canvasGroup.blocksRaycasts = true;
                }
                break;
        }
    }
}