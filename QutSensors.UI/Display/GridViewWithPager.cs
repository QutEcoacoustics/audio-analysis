namespace QutSensors.UI.Display
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using QutSensors.Shared;

    public class GridViewWithPager : GridView
    {
        public bool UseCustomPager
        {
            get { return (bool?)ViewState["UseCustomPager"] ?? false; }
            set { ViewState["UseCustomPager"] = value; }
        }

        protected override void OnRowCreated(GridViewRowEventArgs e)
        {
            base.OnRowCreated(e);

            if (e.Row.RowType == DataControlRowType.Header && !string.IsNullOrEmpty(SortExpression))
                AddSortImage(e.Row);
        }
        void AddSortImage(GridViewRow headerRow)
        {
            Int32 iCol = GetSortColumnIndex(SortExpression);
            if (-1 == iCol)
                return;

            // Create the sorting image based on the sort direction.
            Image sortImage = new Image();
            if (SortDirection.Ascending == SortDirection)
            {
                sortImage.ImageUrl = Page.ResolveClientUrl("~/graphics/collapse.jpg");
                sortImage.AlternateText = "Ascending Order";
            }
            else
            {
                sortImage.ImageUrl = Page.ResolveClientUrl("~/graphics/expand.jpg");
                sortImage.AlternateText = "Descending Order";
            }

            // Add the image to the appropriate header cell.
            headerRow.Cells[iCol].Controls.Add(sortImage);
        }

        private int GetSortColumnIndex(string sortExpression)
        {
            if (sortExpression.EndsWith("desc"))
                sortExpression = sortExpression.Substring(0, sortExpression.Length - "desc".Length + 1);
            return Columns.Cast<DataControlField>().IndexOf(c => c.SortExpression == sortExpression);
        }

        protected override void InitializePager(GridViewRow row, int columnSpan, PagedDataSource pagedDataSource)
        {
            if (UseCustomPager)
                CreateCustomPager(row, columnSpan, pagedDataSource);
            else
                base.InitializePager(row, columnSpan, pagedDataSource);
        }

        protected virtual void CreateCustomPager(GridViewRow row, int columnSpan, PagedDataSource pagedDataSource)
        {
            int pageCount = pagedDataSource.PageCount;
            int pageIndex = pagedDataSource.CurrentPageIndex + 1;
            int pageButtonCount = PagerSettings.PageButtonCount;

            TableCell cell = new TableCell();
            row.Cells.Add(cell);
            if (columnSpan > 1) cell.ColumnSpan = columnSpan;

            if (pageCount > 1)
            {
                HtmlGenericControl pager = new HtmlGenericControl("div");
                pager.Attributes["class"] = "pagination";
                cell.Controls.Add(pager);

                int min = pageIndex - pageButtonCount;
                int max = pageIndex + pageButtonCount;

                if (max > pageCount)
                    min -= max - pageCount;
                else if (min < 1)
                    max += 1 - min;

                // Create "previous" button
                Control page = pageIndex > 1
                                ? BuildLinkButton(pageIndex - 2, PagerSettings.PreviousPageText, "Page", "Prev")
                                : BuildSpan(PagerSettings.PreviousPageText, "disabled");
                pager.Controls.Add(page);

                // Create page buttons
                bool needDiv = false;
                for (int i = 1; i <= pageCount; i++)
                {
                    if (i <= 2 || i > pageCount - 2 || (min <= i && i <= max))
                    {
                        string text = i.ToString(NumberFormatInfo.InvariantInfo);
                        page = i == pageIndex
                                ? BuildSpan(text, "current")
                                : BuildLinkButton(i - 1, text, "Page", text);
                        pager.Controls.Add(page);
                        needDiv = true;
                    }
                    else if (needDiv)
                    {
                        page = BuildSpan("&hellip;", null);
                        pager.Controls.Add(page);
                        needDiv = false;
                    }
                }

                // Create "next" button
                page = pageIndex < pageCount
                        ? BuildLinkButton(pageIndex, PagerSettings.NextPageText, "Page", "Next")
                        : BuildSpan(PagerSettings.NextPageText, "disabled");
                pager.Controls.Add(page);
            }
        }

        private Control BuildLinkButton(int pageIndex, string text, string commandName, string commandArgument)
        {
            PagerLinkButton link = new PagerLinkButton(this);
            link.Text = text;
            link.EnableCallback(ParentBuildCallbackArgument(pageIndex));
            link.CommandName = commandName;
            link.CommandArgument = commandArgument;
            return link;
        }

        private Control BuildSpan(string text, string cssClass)
        {
            HtmlGenericControl span = new HtmlGenericControl("span");
            if (!String.IsNullOrEmpty(cssClass)) span.Attributes["class"] = cssClass;
            span.InnerHtml = text;
            return span;
        }

        private string ParentBuildCallbackArgument(int pageIndex)
        {
            MethodInfo m =
                typeof(GridView).GetMethod("BuildCallbackArgument", BindingFlags.NonPublic | BindingFlags.Instance, null,
                                            new Type[] { typeof(int) }, null);
            return (string)m.Invoke(this, new object[] { pageIndex });
        }

        public class PagerLinkButton : LinkButton
        {
            public PagerLinkButton(IPostBackContainer container)
            {
                _container = container;
            }

            public void EnableCallback(string argument)
            {
                _enableCallback = true;
                _callbackArgument = argument;
            }

            public override bool CausesValidation
            {
                get { return false; }
                set { throw new ApplicationException("Cannot set validation on pager buttons"); }
            }

            protected override void Render(HtmlTextWriter writer)
            {
                SetCallbackProperties();
                base.Render(writer);
            }

            private void SetCallbackProperties()
            {
                if (_enableCallback)
                {
                    ICallbackContainer container = _container as ICallbackContainer;
                    if (container != null)
                    {
                        string callbackScript = container.GetCallbackScript(this, _callbackArgument);
                        if (!string.IsNullOrEmpty(callbackScript)) OnClientClick = callbackScript;
                    }
                }
            }

            #region Private fields
            private readonly IPostBackContainer _container;
            private bool _enableCallback;
            private string _callbackArgument;
            #endregion
        }
    }
}