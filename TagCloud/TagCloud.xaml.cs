using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        protected ObservableCollection<TagItem> Items
        {
            get { return (ObservableCollection<TagItem>)GetValue(ItemsProperty); }
            private set { SetValue(ItemsProperty, value); }
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


        #region AllowMultipleSelections (DependencyProperty)

        /// <summary>
        /// If true, more than one item can be selected at a time
        /// </summary>
        public bool AllowMultipleSelections
        {
            get { return (bool)GetValue(AllowMultipleSelectionsProperty); }
            set { SetValue(AllowMultipleSelectionsProperty, value); }
        }
        public static readonly DependencyProperty AllowMultipleSelectionsProperty =
            DependencyProperty.Register("AllowMultipleSelections", typeof(bool), typeof(TagCloud),
            new PropertyMetadata(true, new PropertyChangedCallback(OnAllowMultipleSelectionsChanged)));

        private static void OnAllowMultipleSelectionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TagCloud)d).OnAllowMultipleSelectionsChanged(e);
        }

        protected virtual void OnAllowMultipleSelectionsChanged(DependencyPropertyChangedEventArgs e)
        {
            //if single selection mode, deselect all but last selected
            if (! (bool)e.NewValue)
            {
                foreach (var item in Items)
                {
                    if (item != _lastSelected[0])
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }

        #endregion

        readonly TagItem[] _lastSelected;

        public TagCloud()
        {
            InitializeComponent();
            try
            {
                HtmlPage.RegisterScriptableObject("TagCloud", this);
                HtmlPage.RegisterCreateableType("TagItem", typeof(TagItem));
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Scriptable object registration failed! " + ex.Message);
            }

            _lastSelected = new TagItem[] {null};
            
            Items.CollectionChanged += Items_CollectionChanged;
            
            this.DataContext = Items;
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            object[] oldItems = (object[]) e.OldItems ?? new object[0];
            object[] newItems = (object[]) e.NewItems ?? new object[0];
            if (oldItems.Any((x)=> ((TagItem)x).IsSelected) || newItems.Any((x)=> ((TagItem)x).IsSelected))
            {
                OnSelectedChanged();
            }
            OnItemsChangedPrivate();
        }

        void newTagItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                if (((TagItem)sender).IsSelected)//don't raise event if something is deselected
                {
                    OnSelectedChanged();
                }
            }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
          if (! AllowMultipleSelections)
          {
              if (_lastSelected[0] != null)
              {
                  _lastSelected[0].IsSelected = false;
              }
              _lastSelected[0] = (TagItem) ((FrameworkElement) sender).DataContext;
                  _lastSelected[0].IsSelected = true;
              
          }
          else
          {
              ((TagItem) ((FrameworkElement) sender).DataContext).IsSelected = true;
          }
        }

        #region interface
        ///<summary>
        /// This method will query the webservice and populate the cloud with data
        ///</summary>
        [ScriptableMember]
        public void GetDataAutomatically()
        {
            TagCloudServicesvcClient client;
            try
            {
                if (Application.Current.Host.Source.Host.Contains("localhost"))
                {
                    client = new TagCloudServicesvcClient("CustomBinding_ITagCloudServicesvc_Debug");
                }
                else // its a live site...
                {
                    EndpointAddress address =
                        new EndpointAddress(
                            (new Uri(Application.Current.Host.Source, "/TagCloudService.svc").AbsoluteUri));

                    client = new TagCloudServicesvcClient("CustomBinding_ITagCloudServicesvc", address);
                        //lookin ServiceRefernces.ClientConfig

                }
            }
            catch (Exception)
            {
                client = new TagCloudServicesvcClient("CustomBinding_ITagCloudServicesvc_Debug");
            }

            client.GetTagsCompleted += client_GetTagsCompleted;

            client.GetTagsAsync();

        }

        private void client_GetTagsCompleted(object sender, GetTagsCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                foreach (var item in e.Result)
                {
                    TagItem newTagItem = new TagItem()
                                             {
                                                 IsSelected = item.IsSelected,
                                                 Name = item.Name ?? String.Empty,
                                                 Weighting = item.Weight
                                             };
                    newTagItem.PropertyChanged += newTagItem_PropertyChanged;
                    Items.Add(newTagItem);
                }
                if (!AllowMultipleSelections)
                {
                    SetSelected(new[] { e.Result.Last().Name }, false);
                }
            }

        }
        
        [ScriptableMember()]
        public TagItem[] GetSelected()
        {
            if (AllowMultipleSelections)
            {
                var result = from t in Items
                             where t.IsSelected
                             select t;
                return result.ToArray();
            }
            else
            {
                return  _lastSelected;
            }
        }

        [ScriptableMember]
        public TagItem[] GetAll()
        {
            return Items.ToArray();
        }

        [ScriptableMember]
        public string[] GetAllNames()
        {
            return Items.Select((x)=>x.Name).ToArray();
        }

        ///<summary>
        /// Sets the given named tags to seleected
        ///</summary>
        ///<param name="names">An array of strings that contain tag names. If the name is not found nothing is done.</param>
        ///<param name="keepOthersSelected">If true, does nothing. If false, this method will deslect all other elements that are currently selected</param>
        ///<returns>true, if changes were made. false if something went wrong</returns>
        [ScriptableMember]
        public bool SetSelected(string[] names, bool keepOthersSelected)
        {
            if (! AllowMultipleSelections && (names.Count() > 1 || keepOthersSelected))
            {
                return false;
            }
            List<string> fastNames = new List<string>(names);
            for (int i = 0; i < fastNames.Count; i++)
            {
                var item = Items[i];
                if (fastNames.Contains(item.Name))
                {
                    item.IsSelected = true;
                    if (i == Items.Count - 1)
                    {
                        _lastSelected[0] = item;
                    }
                    fastNames.Remove(item.Name);
                }
                else
                {
                    if (! keepOthersSelected)
                    {
                        item.IsSelected = false;
                    }
                }
            }

            return true;
        }

        [ScriptableMember]
        public bool RemoveItem(string tagName)
        {
            return Items.Remove(new TagItem() {Name = tagName});
        }

        [ScriptableMember]
        public bool RemoveItem(TagItem tagItem)
        {
            return Items.Remove(tagItem);
        }

        [ScriptableMember]
        public void AddItem(string tagName, int weight, bool selected)
        {
            TagItem newTagItem = new TagItem() { Name = tagName, Weighting = weight, IsSelected = selected };
            Items.Add(newTagItem);
            if (!AllowMultipleSelections)
            {
                SetSelected(new[] { tagName }, false);
            }
            newTagItem.PropertyChanged += newTagItem_PropertyChanged;
        }

        [ScriptableMember]
        public void AddItem(TagItem tagItem)
        {
            Items.Add(tagItem);
            if (! AllowMultipleSelections)
            {
                SetSelected(new[] {tagItem.Name}, false);
            }
            tagItem.PropertyChanged += newTagItem_PropertyChanged;
        }


        [ScriptableMember]
        public event EventHandler<EventArgs> SelectedChanged;
        private void OnSelectedChanged()
        {
            if (SelectedChanged != null)
                SelectedChanged(this, EventArgs.Empty);

        }

        [ScriptableMember]
        public event EventHandler<EventArgs> ItemsChanged;
        private void OnItemsChangedPrivate()
        {
            if (ItemsChanged != null)
                ItemsChanged(this, EventArgs.Empty);

        }

        #endregion


    }
}
