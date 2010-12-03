namespace QutSensors.UI.Display
{
    using System;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;

    public class QutLeftMenu : HierarchicalDataBoundControl
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

        bool autoPopulate = true;
        [DefaultValue(true)]
        public bool AutoPopulate
        {
            get { return autoPopulate; }
            set { autoPopulate = value; }
        }
        #endregion

        int totalDepth = 0;

        protected override void PerformDataBinding()
        {
            base.PerformDataBinding();

            if (autoPopulate)
            {
                Node rootNode = new Node(SiteMap.RootNode, 1);
                nodes.Add(rootNode);

                bool found = false;
                foreach (SiteMapNode node in SiteMap.RootNode.ChildNodes)
                    // was: if( true ||
                    if (SiteMap.CurrentNode != null && (node == SiteMap.CurrentNode || SiteMap.CurrentNode.IsDescendantOf(node)))
                    {
                        found = true;
                        rootNode.Nodes.Add(AddNode(node, rootNode));
                    }

                if (!found)
                {
                    foreach (SiteMapNode node in SiteMap.RootNode.ChildNodes)
                        rootNode.Nodes.Add(AddNode(node, rootNode));
                }
            }
            else
            {
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
        }

        private Node AddNode(SiteMapNode node, Node parent)
        {
            if (totalDepth <= parent.Depth)
                totalDepth = parent.Depth + 1;
            Node newNode = new Node(node, parent.Depth + 1);
            //if (SiteMap.CurrentNode != null && (node == SiteMap.CurrentNode || SiteMap.CurrentNode.IsDescendantOf(node)))
            foreach (SiteMapNode childNode in node.ChildNodes)
                newNode.Nodes.Add(AddNode(childNode, newNode));
            return newNode;
        }

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
                    if (item is SiteMapNode)
                        newNode.Selected = item == SiteMap.CurrentNode;
                    else
                        newNode.Selected = IsSelected(newNode); // newNode.HRef.ToLower() == HttpContext.Current.Request.FilePath.ToLower();
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

        private static bool IsSelected(Node node)
        {
            string currentPath = HttpContext.Current.Request.FilePath.ToLower();
            string itemPath = node.HRef.ToLower();
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
            string appPath = HttpRuntime.AppDomainAppVirtualPath;
            if (appPath == "/")
                appPath = "";

            writer.WriteLine("<!-- noindex -->");
            writer.WriteLine(@"<table width=""220"" border=""0"" cellspacing=""0"" cellpadding=""0"">");


            // Header row
            writer.WriteLine("<tr>");
            for (int i = 0; i < totalDepth - 1; i++)
                writer.WriteLine(@"<td width=""10"" height=""1""><img width=""10"" height=""1"" src=""{0}/graphics/blank.gif"" alt=""""></td>", appPath);
            writer.WriteLine(@"</tr>");

            foreach (Node node in Nodes)
                node.Render(this, writer, totalDepth, false, false, false);

            writer.WriteLine("</table>");
            writer.WriteLine("<!-- endnoindex -->");
        }


        public class Node
        {
            public string Text;
            public string HRef;
            public int Depth;
            public bool Selected = false;
            public List<Node> Nodes = new List<Node>();

            public Node(int depth)
            {
                Depth = depth;
            }

            public Node(SiteMapNode siteMapNode, int depth)
            {
                Depth = depth;
                Text = siteMapNode.Title;
                HRef = siteMapNode.Url;
                Selected = siteMapNode == SiteMap.CurrentNode;
            }

            static int nodeCount = 0;
            public void Render(Control c, HtmlTextWriter writer, int totalDepth, bool parentSelected, bool isLast, bool siblingSelected)
            {
                string appPath = HttpRuntime.AppDomainAppVirtualPath;
                if (appPath == "/")
                    appPath = "";

                nodeCount++;

                if (Text == "Separator")
                    writer.WriteLine(@"
<tr>
<td colspan=""{1}"" width=""220""><img alt=""- - - - -"" src=""{0}/graphics/divider.gif"" width=""220"" height=""15""></td>
</tr>", appPath, totalDepth);
                else
                    switch (Depth)
                    {
                        case 1:
                            writer.WriteLine(@"<tr><td colspan=""{3}"" width=""220""><a href=""{2}"" class=""level1{1}"">{0}</a></td></tr>", Text, Selected ? "current" : "menu", c.ResolveUrl(HRef), totalDepth);
                            break;
                        case 2:
                            {
                                string spanClass = Selected ? "level2current" : "level2menu";
                                string firstImage = SelectedOrChildSelected() ? "/graphics/mid_bracket.gif" : "/graphics/menuline.gif";
                                string secondImage = SelectedOrChildSelected() && Nodes.Count > 0 ? "/graphics/top_bracket.gif" : "/graphics/arrow_01.gif";
                                string mouseOverImage = SelectedOrChildSelected() && Nodes.Count > 0 ? "/graphics/top_bracket.gif" : "/graphics/arrow_01_f3.gif";
                                writer.WriteLine(@"<tr>");
                                writer.WriteLine(@"<td width=""10"" valign=""top"" background=""{0}{1}""><img alt="""" src=""{0}{2}"" width=""10"" height=""15"" name=""node{3}""></td>", appPath, firstImage, secondImage, nodeCount);
                                writer.WriteLine(@"<td colspan=""{1}"" width=""210""><a href=""{2}"" class=""{3}"" onMouseOut=""MM_swapImgRestore()"" onMouseOver=""MM_swapImage('node{4}','','{0}{5}',1)"">{6}</span></td>", appPath, totalDepth - 1, c.ResolveUrl(HRef), spanClass, nodeCount, mouseOverImage, Text);
                                writer.WriteLine(@"</tr>");
                                break;
                            }
                        default:
                            {
                                string spanClass = Selected ? "level{0}current" : "level{0}menu";
                                spanClass = string.Format(spanClass, Depth);
                                string firstImage = parentSelected || siblingSelected ? "/graphics/mid_bracket.gif" : "/graphics/menuline.gif";
                                string thirdImage = Selected ? "/graphics/square.gif" : "/graphics/blank.gif";
                                string secondImage = isLast && Depth == 3 && (parentSelected || siblingSelected) ? "/graphics/bot_bracket.gif" : "/graphics/blank.gif";
                                string align = isLast && Depth == 3 && (parentSelected || siblingSelected) ? "bottom" : "top";
                                string mouseOverImage = Selected ? "/graphics/square.gif" : "/graphics/arrow.gif";

                                writer.WriteLine(@"<tr>");
                                writer.WriteLine(@"<td width=""10"" valign=""{1}"" background=""{0}{2}""><img alt="""" src=""{0}{3}"" width=""10"" height=""15""></td>", appPath, align, firstImage, secondImage);
                                for (int i = 1; i < Depth - 2; i++)
                                    writer.WriteLine(@"<td width=""10"" height=""1""><img width=""10"" height=""1"" src=""{0}/graphics/blank.gif"" alt=""""></td>", appPath);
                                writer.WriteLine(@"<td width=""10"" valign=""top""><img alt="""" width=""10"" height=""15"" border=""0"" src=""{0}{1}"" name=""node{2}""></td>", appPath, thirdImage, nodeCount);
                                writer.WriteLine(@"<td colspan=""{1}"" width=""{2}""><a href=""{3}"" class=""{4}"" onMouseOut=""MM_swapImgRestore()"" onMouseOver=""MM_swapImage('node{5}','','{0}{6}',1)"">{7}</span></td>", appPath, totalDepth - Depth + 1, 220 - (10 * (Depth - 1)), c.ResolveUrl(HRef), spanClass, nodeCount, mouseOverImage, Text);
                                writer.WriteLine(@"</tr>");
                                break;
                            }
                    }

                if (SelectedOrChildSelected() || Depth < 3)
                    foreach (Node node in Nodes)
                        node.Render(c, writer, totalDepth, Selected, node == Nodes[Nodes.Count - 1], ChildSelected());
            }

            bool SelectedOrChildSelected()
            {
                if (Selected)
                    return true;
                return ChildSelected();
            }

            bool ChildSelected()
            {
                foreach (Node node in Nodes)
                    if (node.SelectedOrChildSelected())
                        return true;
                return false;
            }
        }
    }
}