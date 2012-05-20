using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using TowseyLib;

namespace AudioAnalysisTools
{
    using Acoustics.Shared;

    public class Image_MultiTrack : IDisposable
    {

        #region Properties
        public Image SonoImage { get; private set; }
        List<Image_Track> tracks = new List<Image_Track>();
        public IEnumerable<Image_Track> Tracks { get { return tracks; } }
        public List<AcousticEvent> eventList { get; set; }
        double[,] SuperimposedMatrix { get; set; }
        private double superImposedMaxScore;
        private int[] FreqHits;
        private int nyquistFreq;
        private int freqBinCount;
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

        public void AddEvents(List<AcousticEvent> list, int nyquist, int freqBinCount)
        {
            this.eventList = list;
            this.nyquistFreq = nyquist;
            this.freqBinCount = freqBinCount;
        }

        public void AddSuperimposedMatrix(Double[,] m, double maxScore)
        {
            this.SuperimposedMatrix = m;
            this.superImposedMaxScore = maxScore;
        }

        public void AddFreqHitValues(int[] f, int nyquist)
        {
            this.FreqHits = f;
            this.nyquistFreq = nyquist;
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

                if (this.SuperimposedMatrix != null) DrawSuperimposedMatrix(g);
                if (this.eventList != null) DrawEvents(g);
                if (this.FreqHits != null) DrawFreqHits(g);
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
            Pen p2 = new Pen(Color.Black);
            foreach (AcousticEvent e in this.eventList)
            {
                //double start = e.StartTime;
                //double duration = e.Duration;
                //int minF = e.MinFreq;
                //int maxF = e.MaxFreq;
                int fBin1 = (int)(this.freqBinCount * (e.MinFreq / (double)this.nyquistFreq));
                int fBin2 = (int)(this.freqBinCount * (e.MaxFreq / (double)this.nyquistFreq));
                int height = fBin2 - fBin1 + 1;
                int t1 = e.oblong.r1; //temporal start of event
                //int y = e.FreqBinCount - e.oblong.c2;
                int y = this.freqBinCount - fBin2;
                int tWidth = e.oblong.r2 - t1 + 1;
                //int height = e.oblong.c2 - e.oblong.c1 + 1;
                g.DrawRectangle(p1, t1, y, tWidth, height);
                //draw the score bar to indicate relative score
                int scoreHt = (int)Math.Round(height * e.ScoreNormalised);
                int y1 = y + height;
                int y2 = y1 - scoreHt;
                //g.DrawLine(p2, x, y1, x, y2);
                g.DrawLine(p2, t1 + 1, y1, t1 + 1, y2);
                g.DrawLine(p2, t1 + 2, y1, t1 + 2, y2);
                g.DrawLine(p2, t1 + 3, y1, t1 + 3, y2);
                g.DrawString(e.Name, new Font("Tahoma", 10), Brushes.Black, new PointF(t1, y - 1));
            }
        }


        void DrawFreqHits(Graphics g)
        {
            int L = this.FreqHits.Length;
            Pen p1 = new Pen(Color.Red);
            //Pen p2 = new Pen(Color.Black);
            for (int x = 0; x < L; x++)
            {
                if (this.FreqHits[x] <= 0) continue;
                int y = (int)(this.SonoImage.Height * (1 - (this.FreqHits[x] / (double)this.nyquistFreq)));
                //g.DrawRectangle(p1, x, y, x + 1, y + 1);
                g.DrawLine(p1, x, y, x, y + 1);
                //g.DrawString(e.Name, new Font("Tahoma", 6), Brushes.Black, new PointF(x, y - 1));
            }
        }

        /// <summary>
        /// superimposes a matrix of scores on top of a sonogram.
        /// Only draws lines on every second row so that the underling sonogram can be discerned
        /// </summary>
        /// <param name="g"></param>
        void DrawSuperimposedMatrix(Graphics g)
        {
            int paletteSize = 256;
            var pens = ImageTools.GetRedGradientPalette(); //size = 256

            int rows = this.SuperimposedMatrix.GetLength(0);
            int cols = this.SuperimposedMatrix.GetLength(1);
            int imageHt = this.SonoImage.Height - 1; //subtract 1 because indices start at zero
            //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(this.SuperimposedMatrix), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\superimposed1.png", false);

            for (int c = 1; c < cols; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (this.SuperimposedMatrix[r, c] == 0.0) continue;
                    double normScore = this.SuperimposedMatrix[r, c] / (double)this.superImposedMaxScore;
                    int penID = (int)(paletteSize * normScore);
                    if (penID >= paletteSize) penID = paletteSize - 1;
                    Pen pen = pens[penID];
                    g.DrawLine(pen, r, imageHt - c, r + 1, imageHt - c);
                }
                c++; //only draw on every second row.
            }
        } //Superimpose()


        #region IDisposable Members

        public void Dispose()
        {
            this.eventList = null;
            this.SonoImage.Dispose();
        }

        #endregion
    } //end class
}