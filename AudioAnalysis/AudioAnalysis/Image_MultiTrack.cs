using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using TowseyLib;

namespace AudioAnalysis
{
	public class Image_MultiTrack
	{

		#region Properties
		public Image Image { get; private set; }
		List<Image_Track> tracks = new List<Image_Track>();
		public IEnumerable<Image_Track> Tracks { get { return tracks; } }
        List<AcousticEvent> EventList { get; set; }
		#endregion

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="image"></param>
        public Image_MultiTrack(Image image)
        {
            Image = image;
        }
        
        
        public void AddTrack(Image_Track track)
		{
			tracks.Add(track);
		}

        public void AddEvents(List<AcousticEvent> list)
        {
            this.EventList = list;
        }

		public void Save(string path)
		{
            Image image = GetImage();
            if (image == null) 
                Log.WriteLine("MultiTrackImage.Save() - WARNING: NULL IMAGE.Cannot save to: "+path);
			image.Save(path, ImageFormat.Png);
		}

		public Image GetImage()
		{
			// Calculate total height of the bmp
			var height = CalculateImageHeight();

            //set up a new image having the correct dimensions
			var returnImage = new Bitmap(Image.Width, height, PixelFormat.Format24bppRgb);
            //create new graphics canvas and add in the sonogram image
            using (var g = Graphics.FromImage(returnImage))
            {
                g.DrawImage(Image, 0, 0);
                if (this.EventList != null) DrawEvents(g);
            }

            //now add tracks to the image
			int offset = Image.Height;
			foreach (var track in Tracks)
			{
				track.topOffset = offset;
                track.bottomOffset = offset + track.Height-1;
                track.DrawTrack(returnImage);
				offset += track.Height;
			}
			return returnImage;
		}

		int CalculateImageHeight()
		{
			int totalHeight = Image.Height;
            foreach (Image_Track track in tracks)
                totalHeight += track.Height;
			return totalHeight;
		}
        
        void DrawEvents(Graphics g)
        {
            Pen p = new Pen(Color.Red);
            foreach(AcousticEvent e in this.EventList)
            {
                //double start = e.StartTime;
                //double duration = e.Duration;
                //int minF = e.MinFreq;
                //int maxF = e.MaxFreq;
                int x = e.oblong.r1;
                int y = 256 - e.oblong.c2;
                int width  = e.oblong.r2 - x + 1;
                int height = e.oblong.c2 - e.oblong.c1 + 1;
                g.DrawRectangle(p, x, y, width, height);
            }
        }

	} //end class
}