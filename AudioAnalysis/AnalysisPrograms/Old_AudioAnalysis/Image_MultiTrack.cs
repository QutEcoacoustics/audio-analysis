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
		public Image SonoImage { get; private set; }
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
            SonoImage = image;
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
			var image2return = new Bitmap(SonoImage.Width, height, PixelFormat.Format24bppRgb);

            //create new graphics canvas and add in the sonogram image
            using (var g = Graphics.FromImage(image2return))
            {                
                g.DrawImage(this.SonoImage, 0, 0);
                if (this.EventList != null) DrawEvents(g);
                if (this.SuperimposedMatrix != null) Superimpose(g);
            }

            //now add tracks to the image
			int offset = SonoImage.Height;
			foreach (var track in Tracks)
			{
				track.topOffset = offset;
                track.bottomOffset = offset + track.Height-1;
                track.DrawTrack(image2return);
				offset += track.Height;
			}
			return image2return;
		}

		int CalculateImageHeight()
		{
			int totalHeight = SonoImage.Height;
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
            Pen pen = null;

            int rows = this.SuperimposedMatrix.GetLength(0);
            int cols = this.SuperimposedMatrix.GetLength(1);
            int imageHt = this.SonoImage.Height-1; //subtract 1 because indices start at zero
            //int[] bounds = {0,7,14,21,28,35,42,49}; //for max value around 50
            int[] bounds = { 0, 6, 12, 18, 24, 30, 34, 44 }; //for max value = 44

            for (int c = 0; c < cols; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (this.SuperimposedMatrix[r, c] == 0.0) continue;

                    if ((this.SuperimposedMatrix[r, c] > bounds[0]) && (this.SuperimposedMatrix[r, c] <= bounds[1])) pen = pens[0];
                    else
                    if ((this.SuperimposedMatrix[r, c] > bounds[1]) && (this.SuperimposedMatrix[r, c] <= bounds[2])) pen = pens[1];
                    else
                    if ((this.SuperimposedMatrix[r, c] > bounds[2]) && (this.SuperimposedMatrix[r, c] <= bounds[3])) pen = pens[2];
                    else
                    if ((this.SuperimposedMatrix[r, c] > bounds[3]) && (this.SuperimposedMatrix[r, c] <= bounds[4])) pen = pens[3];
                    else
                    if ((this.SuperimposedMatrix[r, c] > bounds[4]) && (this.SuperimposedMatrix[r, c] <= bounds[5])) pen = pens[4];
                    else
                    if ((this.SuperimposedMatrix[r, c] > bounds[5]) && (this.SuperimposedMatrix[r, c] <= bounds[6])) pen = pens[5];
                    else
                    if ((this.SuperimposedMatrix[r, c] > bounds[6]) && (this.SuperimposedMatrix[r, c] <= bounds[7])) pen = pens[6];
                    else pen = new Pen(Color.Brown);

                    g.DrawLine(pen, r, imageHt - c, r + 1, imageHt - c);
                }
            }


        }

	} //end class
}