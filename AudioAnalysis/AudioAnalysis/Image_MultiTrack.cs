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
			var retVal = new Bitmap(Image.Width, height, PixelFormat.Format24bppRgb);
            //create new graphics canvas and add in the sonogram image
			using (var g = Graphics.FromImage(retVal))
				g.DrawImage(Image, 0, 0);
            //now add tracks to the image
			int offset = Image.Height;
			foreach (var track in Tracks)
			{
				track.topOffset = offset;
                track.bottomOffset = offset + track.Height-1;
                track.DrawTrack(retVal);
				offset += track.Height;
			}
			return retVal;
		}

		int CalculateImageHeight()
		{
			int totalHeight = Image.Height;
            foreach (Image_Track track in tracks)
                totalHeight += track.Height;
			return totalHeight;
		}

	} //end class
}