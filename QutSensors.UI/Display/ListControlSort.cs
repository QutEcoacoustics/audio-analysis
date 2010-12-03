namespace QutSensors.UI.Display
{
    using System;
    using System.Collections;
    using System.Web.UI.WebControls;

    /// <summary>
    /// 
    /// </summary>
    /// <remarks> from: http://forums.aspfree.com/code-bank-54/asp-net-using-c--sorting-dropdownlist-by-text-or-value-79339.html </remarks>
    public static class ListControlSort
    {

        public enum ListItemSortOptions { Text, Value }
        public enum SortOrder { Ascending = 1, Descending = -1, Unspecified = 0 }

        public static void Sort(this ListControl combo, SortOrder order, ListItemSortOptions options)
        {
            if (options == ListItemSortOptions.Text)
            {
                ListControlSort.SortCombo(combo, new ComboTextComparer(order));
            }
            else
            {
                ListControlSort.SortCombo(combo, new ComboValueComparer(order));

            }

        }

        public static void SortByValue(ListControl combo)
        {
            SortCombo(combo, new ComboValueComparer());
        }

        public static void SortByText(ListControl combo)
        {
            SortCombo(combo, new ComboTextComparer());
        }

        private static void SortCombo(ListControl combo, IComparer comparer)
        {
            int i;
            if (combo.Items.Count <= 1)
                return;
            ArrayList arrItems = new ArrayList();
            for (i = 0; i < combo.Items.Count; i++)
            {
                ListItem item = combo.Items[i];
                arrItems.Add(item);
            }
            arrItems.Sort(comparer);
            combo.Items.Clear();
            for (i = 0; i < arrItems.Count; i++)
            {
                combo.Items.Add((ListItem)arrItems[i]);
            }
        }


        /// <summary>
        /// compare list items by their value
        /// </summary>
        private class ComboValueComparer : IComparer
        {
            private int _modifier;

            public ComboValueComparer()
            {
                _modifier = (int)SortOrder.Ascending;
            }

            public ComboValueComparer(SortOrder order)
            {
                _modifier = (int)order;
            }

            //sort by value
            public int Compare(Object o1, Object o2)
            {
                ListItem cb1 = (ListItem)o1;
                ListItem cb2 = (ListItem)o2;
                return cb1.Value.CompareTo(cb2.Value) * _modifier;
            }
        }

        /// <summary>
        /// compare list items by their text.
        /// </summary>
        private class ComboTextComparer : IComparer
        {
            private int _modifier;

            public ComboTextComparer()
            {
                _modifier = (int)SortOrder.Ascending;
            }

            public ComboTextComparer(SortOrder order)
            {
                _modifier = (int)order;
            }

            //sort by value
            public int Compare(Object o1, Object o2)
            {
                ListItem cb1 = (ListItem)o1;
                ListItem cb2 = (ListItem)o2;
                return cb1.Text.CompareTo(cb2.Text) * _modifier;
            }
        }





    }
}
