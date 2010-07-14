using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using TowseyLib;

namespace AudioAnalysisTools
{
    using QutSensors.Shared;

    public class Image_MultiTrack : IDisposable
    {

        #region Properties
        public Image SonoImage { get; private set; }
        List<Image_Track> tracks = new List<Image_Track>();
        public IEnumerable<Image_Track> Tracks { get { return tracks; } }
        public List<AcousticEvent> EventList { get; set; }
        double[,] SuperimposedMatrix { get; set; }
        private double superImposedMaxScore;
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

        public void AddSuperimposedMatrix(Double[,] m, double maxScore)
        {
            this.SuperimposedMatrix = m;
            this.superImposedMaxScore = maxScore;
        }

        /// <summary>
        /// WARNING: This method calls Image_MultiTrack.GetImage().
        /// In some circumstances GetImage() cannot manage images with an area larger than 10,385,000 pixels.
        /// This means it cannot handle recording sonograms longer than 2 minutes.
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            Image image = GetImage();
            if (image == null)
            {
                Log.WriteLine("MultiTrackImage.Save() - WARNING: NULL IMAGE.Cannot save to: " + path);
                return;
            }
            image.Save(path, ImageFormat.Png);
            //image.Save(path, ImageFormat.Jpeg);
        }

        /// <summary>
        /// WARNING: graphics.DrawImage() or GDI cannot draw an image that is too big, typically 
        /// with an area larger than 10,385,000 pixels (Jiro estimated > 40000 pixels).
        /// This means it cannot handle recording sonograms longer than 2 minutes.
        /// Therefore call a recursive method to draw the image.
        /// </summary>
        /// <returns></returns>
        public Image GetImage()
        {
            // Calculate total height of the bmp
            var height = CalculateImageHeight();

            //set up a new image having the correct dimensions
            var image2return = new Bitmap(SonoImage.Width, height, PixelFormat.Format24bppRgb);

            //create new graphics canvas and add in the sonogram image
            using (var g = Graphics.FromImage(image2return))
            {
                ////g.DrawImage(this.SonoImage, 0, 0); // WARNING ### THIS CALL DID NOT WORK THEREFORE
                GraphicsSegmented.Draw(g, this.SonoImage); // USE THIS CALL INSTEAD.

                if (this.SuperimposedMatrix != null) Superimpose(g);
                if (this.EventList != null) DrawEvents(g);
            }

            //now add tracks to the image
            int offset = SonoImage.Height;
            foreach (var track in Tracks)
            {
                track.topOffset = offset;
                track.bottomOffset = offset + track.Height - 1;
                track.DrawTrack(image2return);
                offset += track.Height;
            }
            return image2return;
        }




        private int CalculateImageHeight()
        {
            int totalHeight = SonoImage.Height;
            foreach (Image_Track track in tracks)
                totalHeight += track.Height;
            return totalHeight;
        }

        void DrawEvents(Graphics g)
        {
            Pen p1 = new Pen(Color.Red);
            Pen p2 = new Pen(Color.Yellow);
            foreach (AcousticEvent e in this.EventList)
            {
                //double start = e.StartTime;
                //double duration = e.Duration;
                //int minF = e.MinFreq;
                //int maxF = e.MaxFreq;
                int x = e.oblong.r1;
                int y = 256 - e.oblong.c2;
                int width = e.oblong.r2 - x + 1;
                int height = e.oblong.c2 - e.oblong.c1 + 1;
                g.DrawRectangle(p1, x, y, width, height);
                int scoreHt = (int)Math.Round(height * e.ScoreNormalised);
                int y1 = y + height;
                int y2 = y1 - scoreHt;
                g.DrawLine(p2, x, y1, x, y2);
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
            int imageHt = this.SonoImage.Height - 1; //subtract 1 because indices start at zero
            double[] bounds = { 0.0, 0.14, 0.28, 0.42, 0.56, 0.70, 0.85, 1.0 }; //for normalised score

            for (int c = 1; c < cols; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (this.SuperimposedMatrix[r, c] == 0.0) continue;

                    double normScore = this.SuperimposedMatrix[r, c] / this.superImposedMaxScore;
                    if ((normScore > bounds[0]) && (normScore <= bounds[1])) pen = pens[0]; //red
                    else
                        if ((normScore > bounds[1]) && (normScore <= bounds[2])) pen = pens[1]; //orange
                        else
                            if ((normScore > bounds[2]) && (normScore <= bounds[3])) pen = pens[2]; //yellow
                            else
                                if ((normScore > bounds[3]) && (normScore <= bounds[4])) pen = pens[3]; //green
                                else
                                    if ((normScore > bounds[4]) && (normScore <= bounds[5])) pen = pens[4]; //blue
                                    else
                                        if ((normScore > bounds[5]) && (normScore <= bounds[6])) pen = pens[5]; //indigo
                                        else
                                            if ((normScore > bounds[6]) && (normScore <= bounds[7])) pen = pens[6]; //violet
                                            else pen = new Pen(Color.Brown);                                        //brown
                    g.DrawLine(pen, r, imageHt - c, r, imageHt - c + 1);
                    r += 1;
                }
                c += 1;
            }


        }


        #region IDisposable Members

        public void Dispose()
        {
            this.EventList = null;
            this.SonoImage.Dispose();

        }

        #endregion
    } //end class
}