using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace AudioStuff
{
	public class MultiTrackImage
	{
		#region Statics
		const int ScaleHeight = 10;
		#endregion

		public MultiTrackImage(Image image)
		{
			Image = image;
		}

		#region Properties
		public Image Image { get; private set; }
		List<Track> tracks = new List<Track>();
		public IEnumerable<Track> Tracks { get { return tracks; } }

		public bool AddGrid { get; set; }
		#endregion

		public void AddTrack(Track track)
		{
			tracks.Add(track);
		}

		public void Save(string path)
		{
			GetImage().Save(path, ImageFormat.Png);
		}

		public Image GetImage()
		{
			// Calculate total height of the bmp
			var height = CalculateImageHeight();

			var retVal = new Bitmap(Image.Width, height, PixelFormat.Format24bppRgb);
			using (var g = Graphics.FromImage(retVal))
				g.DrawImage(Image, 0, 0);
			int offset = Image.Height;
			foreach (var track in Tracks)
			{
				track.Offset = offset;
				track.DrawTrack(retVal);
				offset += track.Height;
			}
			return retVal;
		}

		int CalculateImageHeight()
		{
			int totalHeight = Image.Height;
			if (AddGrid)
				totalHeight += 2 * ScaleHeight;
			foreach (Track track in tracks)
				totalHeight += track.Height;
			return totalHeight;
		}
	}
}