using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS.UI;

public class CustomShieldManager : ShieldManager
{
    public override void HideShield(ShieldController shield)
    {
        if (shield.gameObject.activeInHierarchy)
        {
            _shieldList.Remove(shield);
            Destroy(shield.gameObject);
        }
    }

    protected override void ShowShield(ShieldController shield)
    {
        if (!shield.gameObject.activeInHierarchy)
        {
            shield.gameObject.SetActive(true);

            var canvasGroup = shield.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }
    }
}