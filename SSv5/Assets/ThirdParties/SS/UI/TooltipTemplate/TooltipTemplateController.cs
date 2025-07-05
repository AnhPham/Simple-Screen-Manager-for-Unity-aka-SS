using SS.UI;

public class TooltipTemplateController : TooltipBaseController
{
    UnityEngine.UI.Text contentText;

    protected override void Awake()
    {
        base.Awake();

        this.contentText = GetComponentInChildren<UnityEngine.UI.Text>();
    }

    protected override void SetText(string text)
    {
        if (this.contentText != null)
        {
            this.contentText.text = text;
        }
    }
}