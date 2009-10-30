using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TagCloud
{
    public partial class TagCloud : UserControl
    {
        ObservableCollection<TagItem> _items;

        public TagCloud()
        {
            InitializeComponent();

            
            _items = new ObservableCollection <TagItem>();
            this.DataContext = _items;
        }
    }
}
