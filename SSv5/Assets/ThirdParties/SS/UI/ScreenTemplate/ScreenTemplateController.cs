using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS.UI;

public class ScreenTemplateController : MonoBehaviour, IKeyBack
{
    public const string NAME = "ScreenTemplate";

    public void OnKeyBack()
    {
        Core.Close();
    }
}