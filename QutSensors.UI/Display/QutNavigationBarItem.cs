// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QutNavigationBarItem.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the QutNavigationBarItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.UI;

    public class QutNavigationBarItem
    {
        public string Text { get; set; }

        public string HRef { get; set; }

        public bool Selected { get; set; }

        public bool RightAlign { get; set; }

        public bool NewWindow { get; set; }

        public QutNavigationBarItem()
        {
        }

        public QutNavigationBarItem(string text, string href)
        {
            this.Text = text;
            this.HRef = this.ProcessUrl(href);

            this.Selected = QutNavigationBar.IsSelected(this);
        }

        public QutNavigationBarItem(string text, string href, bool rightAlign, bool newWindow)
        {
            this.Text = text;
            this.HRef = this.ProcessUrl(href);

            string hrefPath = this.RemoveExtension(this.HRef).ToLower();
            string requestPath = this.RemoveExtension(HttpContext.Current.Request.FilePath).ToLower();

            this.RightAlign = rightAlign;
            this.NewWindow = newWindow;
        }

        public QutNavigationBarItem(string text, string href, bool rightAlign)
            : this(text, href, rightAlign, false)
        {
        }

        public void Render(HtmlTextWriter writer)
        {
            writer.WriteLine(
@"<span class=""topnav_button {3}""><span class=""{2}""><a href=""{0}""{4}>{1}</a></span></span>",
this.HRef,
this.Text,
this.Selected ? "topnav_button_selected" : string.Empty,
this.RightAlign ? "topnav_button_right" : string.Empty,
this.NewWindow ? "target=\"_blank\"" : string.Empty);
        }



        string RemoveExtension(string url)
        {
            return url.Substring(0, url.Length - Path.GetExtension(url).Length);
        }

        protected string ProcessUrl(string url)
        {
            if (url != null && url.StartsWith("~"))
            {
                return HttpRuntime.AppDomainAppVirtualPath + url.Substring(1);
            }

            return url;
        }


    }
}