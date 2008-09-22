using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using SilverlightControls;

namespace ControlConsumer
{
	[ScriptableType]
	public partial class Page : Canvas
	{
		public void Page_Loaded(object o, EventArgs e)
		{
			// Required to initialize variables
			InitializeComponent();

			HtmlPage.RegisterScriptableObject("page", this);

            SilverlightControls.MediaPlayerX mediaPlayerX = new MediaPlayerX();
            mediaPlayerX.Loaded += mediaControl_Loaded;

            this.parentCanvas.Children.Add(mediaPlayerX);
		}

		void mediaControl_Loaded(object o, EventArgs e)
		{
			HtmlPage.RegisterScriptableObject("mediaControl", o);
		}

		[ScriptableMember]
		public void Test()
		{
			Background = new SolidColorBrush(Color.FromArgb(255,122, 230, 13));
		}

		void mediaControl_MediaOpened(object o, EventArgs e)
		{
			MediaPlayerX mediaPlayer = FindName("mediaPlayer") as MediaPlayerX;
			mediaPlayer.StartMarker = 48000;
			mediaPlayer.MarkerDuration = 6000;
			mediaPlayer.PlayFrom(48000);
		}
	}
}