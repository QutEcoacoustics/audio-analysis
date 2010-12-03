// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QutFrontpageLinks.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the QutFrontpageLinks type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class QutFrontpageLinks : HierarchicalDataBoundControl
    {
        #region Properties
        private List<Node> nodes = new List<Node>();
        public List<Node> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
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
        #endregion

        protected override void PerformDataBinding()
        {
            base.PerformDataBinding();

            if (!IsBoundUsingDataSourceID && (DataSource == null))
                return;

            HierarchicalDataSourceView view = GetData(null);
            if (view == null)
                throw new InvalidOperationException("No view returned by data source control.");

            IHierarchicalEnumerable enumerable = view.Select();
            if (enumerable != null)
            {
                Nodes.Clear();

                RecurseDataBindInternal(null, enumerable, 1);
            }
        }

        int totalDepth = 0;
        private void RecurseDataBindInternal(Node node, IHierarchicalEnumerable enumerable, int depth)
        {
            foreach (object item in enumerable)
            {
                IHierarchyData data = enumerable.GetHierarchyData(item);

                if (null != data)
                {
                    // Create an object that represents the bound data to the control.
                    Node newNode = new Node(depth);
                    if (node == null)
                        nodes.Add(newNode);
                    else
                        node.Nodes.Add(newNode);

                    if (DataTextField.Length > 0)
                        newNode.Text = DataBinder.GetPropertyValue(data, DataTextField, null);
                    if (DataHRefField.Length > 0)
                        newNode.HRef = DataBinder.GetPropertyValue(data, DataHRefField, null);
                    newNode.Selected = newNode.HRef.ToLower() == HttpContext.Current.Request.FilePath.ToLower();
                    if (newNode.Selected)
                        totalDepth = depth + 1;

                    if (data.HasChildren)
                    {
                        IHierarchicalEnumerable newEnumerable = data.GetChildren();
                        if (newEnumerable != null)
                            RecurseDataBindInternal(newNode, newEnumerable, depth + 1);
                    }
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(@"<div class=""links"">");
            foreach (Node node in nodes)
                node.Render(writer);
            writer.Write("</div>");
        }

        public class Node
        {
            public string Text;
            public string HRef;
            public string ImageUrl;
            public int Depth;
            public bool Selected = false;
            public List<Node> Nodes = new List<Node>();

            public Node(int depth)
            {
                this.Depth = depth;
            }

            public void Render(HtmlTextWriter writer)
            {
                writer.WriteLine(@"<table cellspacing=""0"" cellpadding=""0"" width=""100%"" border=""0"">");

                // Actual top link
                writer.WriteLine(@"<tr><td>");
                writer.WriteLine(@"<h2><a href=""{0}"">{1}</a></h2>", HRef, Text);
                writer.WriteLine(@"</td>");
                writer.WriteLine(@"<td style=""vertical-align: middle"" width=""80"" rowspan=""2"">");
                if (!string.IsNullOrEmpty(ImageUrl))
                    writer.WriteLine(@"<a href=""{1}""><img height=""70"" alt=""{2}"" src=""{0}{3}"" width=""70"" border=""0""></a>", HttpRuntime.AppDomainAppVirtualPath, HRef, Text, ImageUrl);
                writer.WriteLine(@"</td></tr>");

                // Sub-links
                writer.WriteLine(@"<tr><td>");
                int currentLink = 0;
                int nodesPerColumn = (Nodes.Count / 3) + 1;
                currentLink = RenderColumn(writer, "rowOne", currentLink, nodesPerColumn);
                currentLink = RenderColumn(writer, "rowTwo", currentLink, 2 * nodesPerColumn);
                currentLink = RenderColumn(writer, "rowThree", currentLink, 3 * nodesPerColumn);
                writer.WriteLine("</td></tr>");

                writer.WriteLine("</table>");
            }

            private int RenderColumn(HtmlTextWriter writer, string cssClass, int currentLink, int nodesPerColumn)
            {
                writer.WriteLine(@"<div class=""{0}""><ul>", cssClass);
                for (; currentLink < nodesPerColumn; currentLink++)
                {
                    if (Nodes.Count <= currentLink)
                        break;
                    Node node = Nodes[currentLink];
                    writer.WriteLine(@"<li><a href=""{1}""><img src=""{0}/graphics/bulletImg.gif"" width=""12"" height=""12"" border=""0"" class=""linksBullet"" alt="""" />{2}</a></li>", HttpRuntime.AppDomainAppVirtualPath, node.HRef, node.Text);
                }
                writer.WriteLine("</ul></div>");
                return currentLink;
            }
        }
    }
}