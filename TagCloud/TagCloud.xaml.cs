using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TagCloud.TagService;

namespace TagCloud
{

    public partial class TagCloud : UserControl
    {
        #region Items (DependencyProperty)

        /// <summary>
        /// The TagItems in this TagCloud
        /// </summary>
        public ObservableCollection<TagItem> Items
        {
            get { return (ObservableCollection<TagItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<TagItem>), typeof(TagCloud),
            new PropertyMetadata( new ObservableCollection <TagItem>(), OnItemsChanged));

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TagCloud)d).OnItemsChanged(e);
        }

        protected virtual void OnItemsChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion


        public TagCloud()
        {
            InitializeComponent();
            HtmlPage.RegisterScriptableObject("TagCloud", this);
            HtmlPage.RegisterCreateableType("TagItem", typeof(TagItem));
            
            this.DataContext = Items;
        }

        #region  interface

        public void GetDataAutomatically()
        {
            TagCloudServicesvcClient client;
            if (Application.Current.Host.Source.Host.Contains("localhost"))
            {
                client = new TagCloudServicesvcClient("CustomBinding_ITagCloudServicesvc_Debug");
            }
            else // its a live site...
            {
                EndpointAddress address =
                    new EndpointAddress(
                        (new Uri(Application.Current.Host.Source, "/TagCloudService.svc").AbsoluteUri));

                client = new TagCloudServicesvcClient("CustomBinding_ITagCloudServicesvc", address);//lookin ServiceRefernces.ClientConfig

            }
            client.GetTagsCompleted += client_GetTagsCompleted;

            client.GetTagsAsync();

        }

        private void client_GetTagsCompleted(object sender, GetTagsCompletedEventArgs e)
        {
            foreach (var item in e.Result)
            {
                    Items.Add(new TagItem() {IsSelected = item.IsSelected, Name = item.Name ?? String.Empty, Weighting = item.Weight});
            }
        }

        [ScriptableMember()]
        public TagItem[] GetSelected()
        {
            return new TagItem[0];
        }

        [ScriptableMember]
        public bool SetSelected(string[] names, bool keepOthersSelected)
        {
            return true;
        }

        [ScriptableMember]
        public void RemoveItem(string tagName)
        {
            
        }

        [ScriptableMember]
        public void RemoveItem(TagItem tagItem)
        {

        }

        [ScriptableMember]
        public void AddItem(string tagName, int weight, bool selected)
        {

        }

        [ScriptableMember]
        public void AddItem(TagItem tagItem)
        {

        }


        [ScriptableMember]
        public event EventHandler<EventArgs> SelectedChanged;
        private void OnSelectedChanged()
        {
            if (SelectedChanged != null)
                SelectedChanged(this, EventArgs.Empty);

        }

        #endregion


    }
}
