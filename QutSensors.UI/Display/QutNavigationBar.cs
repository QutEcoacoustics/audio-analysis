// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QutNavigationBar.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the QutNavigationBar type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class QutNavigationBar : DataBoundControl
    {
        #region Properties
        List<QutNavigationBarItem> items = new List<QutNavigationBarItem>();
        public List<QutNavigationBarItem> Items
        {
            get { return items; }
        }

        public string DataTextField
        {
            get
            {
                object o = ViewState["DataTextField"];
                return ((o == null) ? string.Empty : (string)o);
            }
            set
            {
                ViewState["DataTextField"] = value;
                if (Initialized)
                    OnDataPropertyChanged();
            }
        }

        public string DataHRefField
        {
            get
            {
                object o = ViewState["DataHRefField"];
                return ((o == null) ? string.Empty : (string)o);
            }
            set
            {
                ViewState["DataHRefField"] = value;
                if (Initialized)
                    OnDataPropertyChanged();
            }
        }

        bool autoPopulate = true;
        [DefaultValue(true)]
        public bool AutoPopulate
        {
            get { return autoPopulate; }
            set { autoPopulate = value; }
        }
        #endregion

        protected override void PerformDataBinding(System.Collections.IEnumerable data)
        {
            base.PerformDataBinding(data);

            if (autoPopulate)
            {
                foreach (SiteMapNode node in SiteMap.RootNode.ChildNodes)
                    AddItem(node);
            }
            else if (data != null)
            {
                QutNavigationBarItem qutNavigationBarItem = new QutNavigationBarItem();

                if (DataTextField.Length > 0)
                    qutNavigationBarItem.Text = DataBinder.GetPropertyValue(data, DataTextField, null);
                if (DataHRefField.Length > 0)
                    qutNavigationBarItem.HRef = DataBinder.GetPropertyValue(data, DataHRefField, null);
                qutNavigationBarItem.Selected = IsSelected(qutNavigationBarItem);

                if (qutNavigationBarItem.Text.ToLower() != "Separator")
                    items.Add(qutNavigationBarItem);
            }
        }

        /// <summary>
        /// The setup navigation bar.
        /// </summary>
        /// <param name="navigationBar">
        /// The navigation bar.
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        public static void SetupNavigationBar(QutNavigationBar navigationBar, Page page)
        {
            navigationBar.Items.Add(new QutNavigationBarItem("Welcome", page.ResolveUrl("~/")));
            navigationBar.Items.Add(
                new QutNavigationBarItem("Listen to Audio", page.ResolveUrl("~/UI/AudioReading/AudioReadingData.aspx")));
            navigationBar.Items.Add(new QutNavigationBarItem("Projects", page.ResolveUrl("~/UI/Projects.aspx")));
            navigationBar.Items.Add(new QutNavigationBarItem("Sensor Map", page.ResolveUrl("~/SensorMap.aspx")));
            navigationBar.Items.Add(new QutNavigationBarItem("Forum", page.ResolveUrl("~/Forum")));
            navigationBar.Items.Add(new QutNavigationBarItem("Reference Tags", page.ResolveUrl("~/UI/Tag/ReferenceTagList.aspx"), false, true));
            navigationBar.Items.Add(new QutNavigationBarItem("Contact Us", page.ResolveUrl("~/UI/General/ContactUs.aspx")));

            navigationBar.Items.Add(new QutNavigationBarItem("Admin", page.ResolveUrl("~/UI/Management.aspx"), true));
        }

        public void AddItem(SiteMapNode node)
        {
            if (node.Title.ToLower() != "separator")
                items.Add(new QutNavigationBarItem(node.Title, node.Url));
        }

        public static bool IsSelected(QutNavigationBarItem qutNavigationBarItem)
        {
            string currentPath = HttpContext.Current.Request.FilePath.ToLower();
            string itemPath = qutNavigationBarItem.HRef.ToLower();
            if (currentPath == itemPath)
                return true;
            if (currentPath.EndsWith("default.aspx"))
            {
                currentPath = currentPath.Substring(0, currentPath.Length - "default.aspx".Length);
                if (currentPath == itemPath)
                    return true;
                currentPath = currentPath.Substring(0, currentPath.Length - 1); // Remove trailing slash
                if (currentPath == itemPath)
                    return true;
            }
            currentPath = Path.GetFileNameWithoutExtension(currentPath);
            if (currentPath == itemPath)
                return true;
            currentPath = currentPath + "/";
            if (currentPath == itemPath)
                return true;
            return false;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            writer.WriteLine(@"<div class=""topnav"">");
            foreach (QutNavigationBarItem item in items)
                item.Render(writer);
            writer.WriteLine(@" &nbsp; </div>");
        }
    }
}