using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using System.Windows.Threading;
using System.Windows.Browser;
using System.Windows.Media.Imaging;

namespace SilverlightControls
{
    [ScriptableType]
    public partial class MediaPlayerX : UserControl
    {
		const bool EnableDebugOutput = false;

        const string SCRIPTABLE_OBJECT_KEY = "mediaControl";

		FrameworkElement actualControl;
		Image image;

		MediaElement media;
		//System.Windows.Browser.HtmlTimer timer;
        DispatcherTimer timer;
		Canvas startMarkerLine, stopMarkerLine, markerRegion;

		#region Initialise
		public MediaPlayerX()
		{
            InitializeComponent();

            //System.IO.Stream s = GetType().Assembly.GetManifestResourceStream("SilverlightControls.MediaPlayerX.xaml");
            //actualControl =  (FrameworkElement)XamlReader.Load(new System.IO.StreamReader(s).ReadToEnd());

            actualControl = this;

            (actualControl.FindName("DebugCanvas") as FrameworkElement).Visibility = EnableDebugOutput ? Visibility.Visible : Visibility.Collapsed;

            image = actualControl.FindName("SpectrogramImage") as Image;
            media = actualControl.FindName("VideoWindow") as MediaElement;
            media.MediaEnded += new RoutedEventHandler(media_MediaEnded);
            media.MediaFailed += media_MediaFailed;

            startMarkerLine = (Canvas)actualControl.FindName("StartMarkerCanvas");
            stopMarkerLine = (Canvas)actualControl.FindName("StopMarker");
            markerRegion = (Canvas)actualControl.FindName("MarkerRegion");
            InitialiseTimeline();
            InitialiseVolume();
            ConnectButtons();

            media.MediaOpened += media_MediaOpened;
            media.CurrentStateChanged += media_CurrentStateChanged;

            //timer = new System.Windows.Browser.HtmlTimer();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(40);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            HtmlPage.RegisterScriptableObject(SCRIPTABLE_OBJECT_KEY, this);


            //image.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(image_ImageFailed);

            //BitmapImage bitmap = new BitmapImage(new Uri(GetURLBase() + "/Images/Sample.jpg"));

            //image.Source = bitmap;
		}

        protected string GetURLBase()
        {
            string str = System.Windows.Application.Current.Host.Source.OriginalString;
            return str.Substring(0, str.LastIndexOf("/")).Replace("/ClientBin", string.Empty);
        }

		void media_MediaEnded(object sender, EventArgs e)
		{
			OnFinished();
		}

		[ScriptableMember]
		public event EventHandler<EventArgs> MediaOpened;
		void media_MediaOpened(object sender, EventArgs e)
		{
			media.Volume = Volume;
			media.IsMuted = isMute;

			if (MediaOpened != null)
				MediaOpened(this, e);
		}

		[ScriptableMember]
		public event EventHandler<ExceptionRoutedEventArgs> MediaFailed;
        void media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
		{
			if (MediaFailed != null)
				MediaFailed(this, e);
		}

		private void ConnectButtons()
		{
			Canvas playPauseButton = actualControl.FindName("PlayPauseButton") as Canvas;
			playPauseButton.MouseLeftButtonDown += delegate { PlayPause(); };
			playPauseButton.MouseEnter += delegate { BeginStoryboard("PlayPauseButton_MouseEnter"); };
			playPauseButton.MouseLeave += delegate { BeginStoryboard("PlayPauseButton_MouseLeave"); };
			playPauseButton.MouseLeftButtonDown += delegate { BeginStoryboard("PlayPauseButton_MouseDown"); };
			playPauseButton.MouseLeftButtonUp += delegate { BeginStoryboard("PlayPauseButton_MouseUp"); };

			Canvas stopButton = actualControl.FindName("StopButton") as Canvas;
			stopButton.MouseLeftButtonDown += delegate { Stop(); };
			stopButton.MouseEnter += delegate { BeginStoryboard("StopButton_MouseEnter"); };
			stopButton.MouseLeave += delegate { BeginStoryboard("StopButton_MouseLeave"); };
			stopButton.MouseLeftButtonDown += delegate { BeginStoryboard("StopButton_MouseDown"); };
			stopButton.MouseLeftButtonUp += delegate { BeginStoryboard("StopButton_MouseUp"); };

			Canvas nextButton = actualControl.FindName("NextButton") as Canvas;
			nextButton.MouseLeftButtonDown += delegate { Stop(); };
			nextButton.MouseEnter += delegate { BeginStoryboard("NextButton_MouseEnter"); };
			nextButton.MouseLeave += delegate { BeginStoryboard("NextButton_MouseLeave"); };
			nextButton.MouseLeftButtonDown += delegate { BeginStoryboard("NextButton_MouseDown"); };
			nextButton.MouseLeftButtonUp += delegate { BeginStoryboard("NextButton_MouseUp"); };

			Canvas previousButton = actualControl.FindName("PreviousButton") as Canvas;
			previousButton.MouseLeftButtonDown += delegate { Stop(); };
			previousButton.MouseEnter += delegate { BeginStoryboard("PreviousButton_MouseEnter"); };
			previousButton.MouseLeave += delegate { BeginStoryboard("PreviousButton_MouseLeave"); };
			previousButton.MouseLeftButtonDown += delegate { BeginStoryboard("PreviousButton_MouseDown"); };
			previousButton.MouseLeftButtonUp += delegate { BeginStoryboard("PreviousButton_MouseUp"); };

			Canvas muteButton = actualControl.FindName("MuteButton") as Canvas;
			muteButton.MouseLeftButtonDown += delegate { ToggleMute(); };
			muteButton.MouseEnter += delegate { BeginStoryboard("MuteButton_MouseEnter"); };
			muteButton.MouseLeave += delegate { BeginStoryboard("MuteButton_MouseLeave"); };
			muteButton.MouseLeftButtonDown += delegate { BeginStoryboard("MuteButton_MouseDown"); };
			muteButton.MouseLeftButtonUp += delegate { BeginStoryboard("MuteButton_MouseUp"); };
		}
		#endregion

		#region Properties
		TimeSpan _startMarker;
		public TimeSpan InternalStartMarker
		{
			get { return _startMarker; }
			set
			{
				_startMarker = value;
				OnStartMarkerChanged();
			}
		}

		[ScriptableMember]
		public event EventHandler<EventArgs> StartMarkerChanged;
		private void OnStartMarkerChanged()
		{
			if (StartMarkerChanged != null)
				StartMarkerChanged(this, EventArgs.Empty);

			if (media.CurrentState != MediaElementState.Opening)
			{
				double position = _startMarker.TotalMilliseconds / media.NaturalDuration.TimeSpan.TotalMilliseconds;
				startMarkerLine.SetValue(Canvas.LeftProperty, position * spectrogramWidth);
			}
		}

		[ScriptableMember]
		public event EventHandler<EventArgs> Finished;
		private void OnFinished()
		{
			if (Finished != null)
				Finished(this, EventArgs.Empty);
		}

		TimeSpan? stopMarker;
		public TimeSpan? InternalStopMarker
		{
			get { return stopMarker; }
			set
			{
				stopMarker = value;
				OnStopMarkerChanged();
			}
		}

		[ScriptableMember]
		public event EventHandler<EventArgs> StopMarkerChanged;
		private void OnStopMarkerChanged()
		{
			if (StopMarkerChanged != null)
				StopMarkerChanged(this, EventArgs.Empty);

			if (stopMarker == null)
			{
				markerRegion.Visibility = stopMarkerLine.Visibility = Visibility.Collapsed;
			}
			else if (media.CurrentState != MediaElementState.Opening)
			{
				markerRegion.Visibility = stopMarkerLine.Visibility = Visibility.Visible;

				double position = stopMarker.Value.TotalMilliseconds / media.NaturalDuration.TimeSpan.TotalMilliseconds;
				stopMarkerLine.SetValue(Canvas.LeftProperty, position * spectrogramWidth);

				double startPosition = _startMarker.TotalMilliseconds / media.NaturalDuration.TimeSpan.TotalMilliseconds;
				markerRegion.SetValue(Canvas.LeftProperty, startPosition * spectrogramWidth);

				markerRegion.Width = (position - startPosition) * spectrogramWidth;
			}
		}

		[ScriptableMember]
		public double StartMarker
		{
			get
			{
				if (stopMarker == null)
					return _startMarker.TotalMilliseconds;
				else if (_startMarker > stopMarker)
					return stopMarker.Value.TotalMilliseconds;
				else
					return _startMarker.TotalMilliseconds;
			}

			set
			{
				media.Position = InternalStartMarker = TimeSpan.FromMilliseconds(value);
			}
		}

		[ScriptableMember]
		public double MarkerDuration
		{
			get
			{
				if (stopMarker == null)
					return 0;
				else if (_startMarker > stopMarker)
					return (_startMarker - stopMarker.Value).TotalMilliseconds;
				else
					return (stopMarker.Value - _startMarker).TotalMilliseconds;
			}

			set
			{
				if (value <= 0.1)
					InternalStopMarker = null;
				else
					InternalStopMarker = _startMarker.Add(TimeSpan.FromMilliseconds(value));
			}
		}

		[ScriptableMember]
		public string ImageSource
		{
			get {
                BitmapImage bitmap = image.Source as BitmapImage;
                if (bitmap != null)
                {
                    return bitmap.UriSource.ToString();
                }
                return image.Source.ToString(); 
            }
			set 
            {
                //image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(value, UriKind.RelativeOrAbsolute));

                // no idea why the above doesn't work. I tried handling the ImageFailed exception, and found that
                // I was getting network errors. The Uri is not being correctly resolved.
                BitmapImage bitmap = new BitmapImage(new Uri(GetURLBase() + value));
                image.Source = bitmap;
            }
		}

		[ScriptableMember]
		public string AudioSource
		{
			get { return media.Source.ToString(); }
			set { media.Source = new Uri(value, UriKind.RelativeOrAbsolute); }
		}

		double volume;
		[ScriptableMember]
		public double Volume
		{
			get { return volume; }
			set
			{
				media.Volume = volume = value;
				if (isMute)
					IsMute = false.ToString();
			}
		}

		bool isMute = false;
		[ScriptableMember]
		public string IsMute
		{
			get { return isMute.ToString(); }
			set
			{
				bool newValue = Convert.ToBoolean(value);
				if (isMute != newValue)
				{
					isMute = newValue;
					media.IsMuted = isMute;
					if (isMute)
						BeginStoryboard("MuteOnSymbol_Show");
					else
						BeginStoryboard("MuteOnSymbol_Hide");
				}
			}
		}
		#endregion

		#region Timeline Methods
		Path timeThumb;
		Canvas timeline, spectrogram, spectrogramSweeper;
		double timeLineWidth, spectrogramWidth;
		bool positionDragging;

		private void InitialiseTimeline()
		{
			timeThumb = actualControl.FindName("TimeThumb") as Path;
			timeline = actualControl.FindName("Timeline") as Canvas;
			timeline.MouseLeftButtonDown += timeline_MouseLeftButtonDown;
			timeline.MouseLeftButtonUp += timeline_MouseLeftButtonUp;
			timeline.MouseMove += timeline_MouseMove;
			timeLineWidth = (actualControl.FindName("TimeSliderDecoration") as Canvas).Width;

			spectrogramSweeper = actualControl.FindName("SpectrogramSweeper") as Canvas;
			spectrogram = actualControl.FindName("Spectrogram") as Canvas;
			spectrogramWidth = (actualControl.FindName("SpectrogramImage") as Image).Width;
			spectrogram.MouseLeftButtonDown += spectrogram_MouseLeftButtonDown;
			spectrogram.MouseLeftButtonUp += spectrogram_MouseLeftButtonUp;
			spectrogram.MouseLeave += spectrogram_MouseLeave;
			spectrogram.MouseMove += spectrogram_MouseMove;
		}

		void timeline_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			positionDragging = true;
			timeline.CaptureMouse();
			UpdatePositionFromMouse(timeline, timeLineWidth, e);
			InternalStartMarker = media.Position;
			InternalStopMarker = null;
		}

		void timeline_MouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			positionDragging = false;
			timeline.ReleaseMouseCapture();
		}

		void timeline_MouseMove(object sender, MouseEventArgs e)
		{
			if (positionDragging)
				InternalStopMarker = TimeSpan.FromMilliseconds(CalculatePositionFromMouse(timeline, timeLineWidth, e));
		}

		void spectrogram_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			positionDragging = true;
			spectrogram.CaptureMouse();
			UpdatePositionFromMouse(spectrogram, spectrogramWidth, e);
			InternalStartMarker = media.Position;
			InternalStopMarker = null;
		}

		void spectrogram_MouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			positionDragging = false;
			spectrogram.ReleaseMouseCapture();
		}

		void spectrogram_MouseLeave(object sender, EventArgs e)
		{
			positionDragging = false;
			spectrogram.ReleaseMouseCapture();
		}

		void spectrogram_MouseMove(object sender, MouseEventArgs e)
		{
			if (positionDragging)
				InternalStopMarker = TimeSpan.FromMilliseconds(CalculatePositionFromMouse(spectrogram, spectrogramWidth, e));
		}

		private void UpdatePositionFromMouse(Canvas control, double width, MouseEventArgs e)
		{
			double milliseconds = CalculatePositionFromMouse(control, width, e);
			media.Position = TimeSpan.FromMilliseconds(milliseconds);
			UpdatePosition();
		}

		private double CalculatePositionFromMouse(Canvas control, double width, MouseEventArgs e)
		{
			double milliseconds = media.NaturalDuration.TimeSpan.TotalMilliseconds * (e.GetPosition(control).X / width);
			if (milliseconds < 0)
				milliseconds = 0;
			return milliseconds;
		}

		private void UpdatePosition()
		{
			double position = media.Position.TotalMilliseconds / media.NaturalDuration.TimeSpan.TotalMilliseconds;
			timeThumb.SetValue(Canvas.LeftProperty, position * timeLineWidth);
			spectrogramSweeper.SetValue(Canvas.LeftProperty, position * spectrogramWidth);
		}
		#endregion

		#region Volume Methods
		Path volumeThumb;
		double volumeWidth;
		Canvas volumeCanvas, volumeSliderDecoration;
		void InitialiseVolume()
		{
			volumeSliderDecoration = actualControl.FindName("VolumeSliderDecoration") as Canvas;
			volumeWidth = volumeSliderDecoration.Width;

			volumeThumb = actualControl.FindName("VolumeThumb") as Path;

			volumeCanvas = actualControl.FindName("VolumeCanvas") as Canvas;
			volumeCanvas.MouseLeftButtonDown += volume_MouseLeftButtonDown;
			volumeCanvas.MouseLeftButtonUp += volume_MouseLeftButtonUp;
			volumeCanvas.MouseMove += volume_MouseMove;
		}

		bool volumeDragging = false;
		void volume_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			volumeDragging = true;
			volumeCanvas.CaptureMouse();
			UpdateVolumeFromMouse(e);
		}

		void volume_MouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			volumeDragging = false;
			volumeCanvas.ReleaseMouseCapture();
			OnVolumeChanged();
		}

		void volume_MouseMove(object sender, MouseEventArgs e)
		{
			if (volumeDragging)
				UpdateVolumeFromMouse(e);
		}

		private void UpdateVolumeFromMouse(MouseEventArgs e)
		{
			double pos = e.GetPosition(volumeCanvas).X;
			if (pos < 0) pos = 0;
			if (pos > volumeWidth) pos = volumeWidth;
			volume = media.Volume = (pos / volumeWidth);
			if (isMute)
				IsMute = false.ToString();
			UpdateVolumePosition();
		}

		void UpdateVolumePosition()
		{
			volumeThumb.SetValue(Canvas.LeftProperty, (media.Volume * volumeWidth) - 2.0); // 2.0 Just seems to fix up a slight offset. Not sure why
		}

		[ScriptableMember]
		public event EventHandler<EventArgs> VolumeChanged;
		private void OnVolumeChanged()
		{
			if (VolumeChanged != null)
				VolumeChanged(this, EventArgs.Empty);
		}
		#endregion

		bool playing;
		void timer_Tick(object sender, EventArgs e)
		{
			if (playing)
			{
				if (stopMarker != null)
				{
					if (_startMarker < stopMarker)
					{
						if (stopMarker.Value < media.Position || _startMarker > media.Position)
							media.Position = InternalStartMarker;
					}
					else
					{
						if (_startMarker < media.Position || stopMarker.Value > media.Position)
							media.Position = InternalStopMarker.Value;
					}
					startMarkerLine.Visibility = Visibility.Visible;
				}
				else
				{
					InternalStartMarker = media.Position;
					if (startMarkerLine.Visibility == Visibility.Visible)
						startMarkerLine.Visibility = Visibility.Collapsed;
				}
				UpdatePosition();
			}
			else if (startMarkerLine.Visibility == Visibility.Collapsed)
				startMarkerLine.Visibility = Visibility.Visible;
			UpdateVolumePosition();
		}

		void media_CurrentStateChanged(object sender, EventArgs e)
		{
			switch (media.CurrentState)
			{
				case MediaElementState.Playing :
					playing = true;
					BeginStoryboard("PlaySymbol_Hide");
					BeginStoryboard("PauseSymbol_Show");
					break;
				case MediaElementState.Stopped:
				case MediaElementState.Paused:
					playing = false;
					BeginStoryboard("PlaySymbol_Show");
					BeginStoryboard("PauseSymbol_Hide");
					UpdatePosition();
					break;
				case MediaElementState.Opening:
					break;
			}
		}

		#region Media Control Methods
		[ScriptableMember]
		public void PlayFrom(double position)
		{
			media.Position = TimeSpan.FromMilliseconds(position);
			media.Play();
		}

		[ScriptableMember]
		public void PlayPause()
		{
			switch (media.CurrentState)
			{
				case MediaElementState.Playing:
					media.Pause();
					break;
				case MediaElementState.Paused:
				case MediaElementState.Stopped:
					media.Play();
					break;
			}
		}

		[ScriptableMember]
		public void Stop()
		{
			media.Stop();
		}

		[ScriptableMember]
		public void ToggleMute()
		{
			if (isMute)
				BeginStoryboard("MuteOnSymbol_Hide");
			else
				BeginStoryboard("MuteOnSymbol_Show");
			media.IsMuted = isMute = !isMute;
			OnMuteChanged();
		}

		[ScriptableMember]
		public event EventHandler<EventArgs> MuteChanged;
		private void OnMuteChanged()
		{
			if (MuteChanged != null)
				MuteChanged(this, EventArgs.Empty);
		}
		#endregion

		private void BeginStoryboard(string name)
		{
			((Storyboard)actualControl.FindName(name)).Begin();
		}

		void DebugWrite(object o)
		{
			(actualControl.FindName("Debug") as TextBlock).Text = o.ToString();
		}
	}
}
