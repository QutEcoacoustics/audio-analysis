namespace QutSensors.UI.Display
{
    using System.IO;
    using System.Text;
    using System.Web.UI;

    /// <summary>
    /// This control will correctly register inline script for use in update panels. Without this the script will not be run on async updates.
    /// </summary>
    public class InlineScript : Control
    {
        protected override void Render(HtmlTextWriter writer)
        {
            var sm = ScriptManager.GetCurrent(Page);
            if (sm != null && sm.IsInAsyncPostBack)
            {
                var sb = new StringBuilder();
                base.Render(new HtmlTextWriter(new StringWriter(sb)));
                ScriptManager.RegisterStartupScript(this, typeof(InlineScript), UniqueID, sb.ToString(), false);
            }
            else
                base.Render(writer);
        }
    }
}