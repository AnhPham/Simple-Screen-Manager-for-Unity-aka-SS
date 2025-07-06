using SS.UI;

public class TooltipTemplateController : TooltipBaseController
{
    UnityEngine.UI.Text _contentText;

    protected override void Awake()
    {
        base.Awake();

        this._contentText = GetComponentInChildren<UnityEngine.UI.Text>();
    }

    protected override void SetText(string text)
    {
        if (this._contentText != null)
        {
            this._contentText.text = text;
        }
    }
}