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
        double[,] SuperimposedMatrix {get; set;}
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

        public void AddSuperimposedMatrix(Double[,] m)
        {
            this.SuperimposedMatrix = m;
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
                if (this.SuperimposedMatrix != null) Superimpose(g);
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
            Pen p1 = new Pen(Color.LightGreen);
            Pen p2 = new Pen(Color.Red);
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
                g.DrawRectangle(p1, x, y, width, height);
                int scoreHt = (int)Math.Round(height * e.NormalisedScore);
                int y1 = y + height;
                int y2 = y1 - scoreHt;
                g.DrawLine(p2, x,     y1, x,     y2);
                g.DrawLine(p2, x + 1, y1, x + 1, y2);
                g.DrawLine(p2, x + 2, y1, x + 2, y2);
                g.DrawLine(p2, x + 3, y1, x + 3, y2);
            }
        }

        void Superimpose(Graphics g)
        {
            Pen p1 = new Pen(Color.Red);
            Pen p2 = new Pen(Color.Orange);
            Pen p3 = new Pen(Color.Yellow);
            Pen p4 = new Pen(Color.Green);
            Pen p5 = new Pen(Color.Blue);
            Pen p6 = new Pen(Color.Indigo);
            Pen p7 = new Pen(Color.Violet);
            var pens = new List<Pen>(); 
            pens.Add(p1);
            pens.Add(p2);
            pens.Add(p3);
            pens.Add(p4);
            pens.Add(p5);
            pens.Add(p6);
            pens.Add(p7);

            int rows = this.SuperimposedMatrix.GetLength(0);
            int cols = this.SuperimposedMatrix.GetLength(1);
            int imageHt = this.Image.Height;
            //int maxValue = 64;

            for (int c = 0; c < cols; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (this.SuperimposedMatrix[r, c] == 0.0) continue;

                    if ((this.SuperimposedMatrix[r, c] > 0.0) && (this.SuperimposedMatrix[r, c] <= 8))
                        g.DrawLine(pens[0], r, imageHt - c, r + 1, imageHt - c);
                    else
                        if ((this.SuperimposedMatrix[r, c] > 8.0) && (this.SuperimposedMatrix[r, c] < 12))
                            g.DrawLine(pens[1], r, imageHt - c, r + 1, imageHt - c);
                        else
                            if ((this.SuperimposedMatrix[r, c] > 12.0) && (this.SuperimposedMatrix[r, c] < 16))
                                g.DrawLine(pens[2], r, imageHt - c, r + 1, imageHt - c);
                            else
                                if ((this.SuperimposedMatrix[r, c] > 16.0) && (this.SuperimposedMatrix[r, c] < 20))
                                    g.DrawLine(pens[3], r, imageHt - c, r + 1, imageHt - c);
                                else
                                    if ((this.SuperimposedMatrix[r, c] > 20.0) && (this.SuperimposedMatrix[r, c] < 24))
                                        g.DrawLine(pens[4], r, imageHt - c, r + 1, imageHt - c);
                                    else
                                        if ((this.SuperimposedMatrix[r, c] > 24.0) && (this.SuperimposedMatrix[r, c] < 28))
                                            g.DrawLine(pens[5], r, imageHt - c, r + 1, imageHt - c);
                                        else
                                            if ((this.SuperimposedMatrix[r, c] > 28.0) && (this.SuperimposedMatrix[r, c] < 32))
                                                g.DrawLine(pens[6], r, imageHt - c, r + 1, imageHt - c);
                }
            }


        }

	} //end class
}