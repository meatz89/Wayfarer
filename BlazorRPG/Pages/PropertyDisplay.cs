namespace BlazorRPG.Pages;

public class PropertyDisplay
{
    public PropertyDisplay()
    {
        
    }

    public PropertyDisplay(string v1, string v2, string v3, string v4, string v5)
    {
        this.Text = v1;
        this.Icon = v2;
        this.TooltipText = v3;
        this.CssClass = v4;
        this.TagName = v5;
    }

    public string Text { get; set; }
    public string Icon { get; set; }
    public string TooltipText { get; set; }
    public string CssClass { get; set; }
    public string TagName { get; set; } 
}