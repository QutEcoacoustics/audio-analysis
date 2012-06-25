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
        public Image sonogramImage { get; private set; }
        List<Image_Track> tracks = new List<Image_Track>();
        public IEnumerable<Image_Track> Tracks { get { return tracks; } }
        public List<AcousticEvent> eventList { get; set; }
        double[,] SuperimposedMatrix { get; set; }
        double[,] SuperimposedRedTransparency { get; set; }
        double[,] SuperimposedRainbowTransparency { get; set; }
        private double superImposedMaxScore;
        private int[] FreqHits;
        private int nyquistFreq; //sets the frequency scale for drawing events
        private int freqBinCount;
        private double framesPerSecond;
        #endregion


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="image"></param>
        public Image_MultiTrack(Image image)
        {
            sonogramImage = image;
        }


        public void AddTrack(Image_Track track)
        {
            tracks.Add(track);
        }

        public void AddEvents(List<AcousticEvent> _list, int _nyquist, int _freqBinCount, double _framesPerSecond)
        {
            this.eventList       = _list;
            this.nyquistFreq     = _nyquist;
            this.freqBinCount    = _freqBinCount;
            this.framesPerSecond = _framesPerSecond;
        }

        public void AddSuperimposedMatrix(Double[,] m, double maxScore)
        {
            this.SuperimposedMatrix = m;
            this.superImposedMaxScore = maxScore;
        }

        public void OverlayRedTransparency(Double[,] m)
        {
            this.SuperimposedRedTransparency = m;
        }
        public void OverlayRainbowTransparency(Double[,] m)
        {
            this.SuperimposedRainbowTransparency = m;
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
            var image2return = new Bitmap(sonogramImage.Width, height, PixelFormat.Format24bppRgb);

            //create new graphics canvas and add in the sonogram image
            using (var g = Graphics.FromImage(image2return))
            {
                ////g.DrawImage(this.SonoImage, 0, 0); // WARNING ### THIS CALL DID NOT WORK THEREFORE
                GraphicsSegmented.Draw(g, this.sonogramImage); // USE THIS CALL INSTEAD.

                if (this.SuperimposedRedTransparency != null)     OverlayRedTransparency(g, (Bitmap)this.sonogramImage);
                if (this.SuperimposedMatrix != null)              OverlayMatrix(g);
                if (this.SuperimposedRainbowTransparency != null) OverlayRainbowTransparency(g, (Bitmap)this.sonogramImage);
                if (this.eventList != null) DrawEvents(g);
                if (this.FreqHits != null) DrawFreqHits(g);
            }

            //now add tracks to the image
            int offset = sonogramImage.Height;
            foreach (Image_Track track in tracks)
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
            int totalHeight = sonogramImage.Height;
            foreach (Image_Track track in tracks)
                totalHeight += track.Height;
            return totalHeight;
        }

        void DrawEvents(Graphics g)
        {
            double freqBinWidth = this.nyquistFreq / this.freqBinCount;
            Pen p1 = new Pen(Color.Crimson);
            Pen p2 = new Pen(Color.Black);
            foreach (AcousticEvent e in this.eventList)
            {
                //if (e.oblong == null) continue;
                //calculate top and bottom freq bins
                int minFreqBin = (int)(e.MinFreq / freqBinWidth);
                int maxFreqBin = (int)(e.MaxFreq / freqBinWidth);
                int height = maxFreqBin - minFreqBin + 1;
                int y = this.sonogramImage.Height - maxFreqBin;

                //calculate start and end time frames
                int t1 = 0;
                int tWidth = 0;
                double duration = e.TimeEnd - e.TimeStart;
                if ((duration != 0.0) && (this.framesPerSecond != 0.0))
                {
                    t1     = (int)Math.Round(e.TimeStart * this.framesPerSecond); //temporal start of event
                    tWidth = (int)Math.Round(duration * this.framesPerSecond);
                }
                else if (e.oblong != null)
                {
                    t1     = e.oblong.r1; //temporal start of event
                    tWidth = e.oblong.r2 - t1 + 1;
                }

                g.DrawRectangle(p1, t1, y, tWidth, height);

                //draw the score bar to indicate relative score
                int scoreHt = (int)Math.Round(height * e.ScoreNormalised);
                int y1 = y + height;
                int y2 = y1 - scoreHt;
                g.DrawLine(p2, t1 + 1, y1, t1 + 1, y2);
                g.DrawLine(p2, t1 + 2, y1, t1 + 2, y2);
                //g.DrawLine(p2, t1 + 3, y1, t1 + 3, y2);
                g.DrawString(e.Name, new Font("Tahoma", 8), Brushes.Black, new PointF(t1, y - 1));
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
                int y = (int)(this.sonogramImage.Height * (1 - (this.FreqHits[x] / (double)this.nyquistFreq)));
                //g.DrawRectangle(p1, x, y, x + 1, y + 1);
                g.DrawLine(p1, x, y, x, y + 1);
                //g.DrawString(e.Name, new Font("Tahoma", 6), Brushes.Black, new PointF(x, y - 1));
            }
        }

        /// <summary>
        /// superimposes a matrix of scores on top of a sonogram.
        /// </summary>
        /// <param name="g"></param>
        void OverlayRedTransparency(Graphics g, Bitmap bmp)
        {
            int rows = this.SuperimposedRedTransparency.GetLength(0);
            int cols = this.SuperimposedRedTransparency.GetLength(1);
            int imageHt = this.sonogramImage.Height - 1; //subtract 1 because indices start at zero
            //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(this.SuperimposedRedTransparency), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\superimposed1.png", false);

            for (int c = 1; c < cols; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (this.SuperimposedRedTransparency[r, c] == 0.0) continue;
                    Color pixel = bmp.GetPixel(r, imageHt - c);
                    if(pixel.R == 255) continue; //white
                    g.DrawLine(new Pen(Color.FromArgb(255, pixel.G, pixel.B)), r, imageHt - c, r + 1, imageHt - c);
                }
            }
        } //OverlayRedTransparency()

        /// <summary>
        /// superimposes a matrix of scores on top of a sonogram. USES RAINBOW PALLETTE
        /// ASSUME MATRIX NORMALIZED IN [0,1]
        /// </summary>
        /// <param name="g"></param>
        void OverlayRainbowTransparency(Graphics g, Bitmap bmp)
        {
            Color[] palette = { Color.Crimson, Color.Red, Color.Orange, Color.Yellow, Color.Lime, Color.Green, Color.Blue, Color.Indigo, Color.Violet, Color.Purple };
            int rows = this.SuperimposedRainbowTransparency.GetLength(0);
            int cols = this.SuperimposedRainbowTransparency.GetLength(1);
            int imageHt = this.sonogramImage.Height - 1; //subtract 1 because indices start at zero

            for (int r = 0; r < rows; r++)
            {
                for (int c = 1; c < cols; c++)//traverse columns - skip DC column
                {
                    double value = this.SuperimposedRainbowTransparency[r, c];
                    if (value <= 0.0) continue; //nothing to show
                    Color pixel = bmp.GetPixel(r, imageHt - c);
                    if (pixel.R > 250) continue; //by-pass white
                    int index = (int)Math.Floor((value * 9));//get index into pallette
                    if (index > 9) index = 9;
                    Color newColor = palette[index];
                    double factor = pixel.R / (double)(255 * 1.2);  //1.2 is a color intensity adjustment
                    int red = (int)Math.Floor(newColor.R + ((255 - newColor.R) * factor));
                    int grn = (int)Math.Floor(newColor.G + ((255 - newColor.G) * factor));
                    int blu = (int)Math.Floor(newColor.B + ((255 - newColor.B) * factor));
                    g.DrawLine(new Pen(Color.FromArgb(red, grn, blu)), r, imageHt - c, r + 1, imageHt - c);
                }
            }
        } //OverlayRainbowTransparency()

        ///// <summary>
        ///// superimposes a matrix of scores on top of a sonogram.
        ///// Only draws lines on every second row so that the underling sonogram can be discerned
        ///// </summary>
        ///// <param name="g"></param>
        void OverlayMatrix(Graphics g)
        {
            int paletteSize = 256;
            var pens = ImageTools.GetRedGradientPalette(); //size = 256

            int rows = this.SuperimposedMatrix.GetLength(0);
            int cols = this.SuperimposedMatrix.GetLength(1);
            int imageHt = this.sonogramImage.Height - 1; //subtract 1 because indices start at zero
            //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(this.SuperimposedMatrix), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\superimposed1.png", false);

            for (int c = 1; c < cols; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (this.SuperimposedMatrix[r, c] == 0.0) continue;
                    double normScore = this.SuperimposedMatrix[r, c] / (double)this.superImposedMaxScore;
                    int penID = (int)(paletteSize * normScore);
                    if (penID >= paletteSize) penID = paletteSize - 1;
                    g.DrawLine(pens[penID], r, imageHt - c, r + 1, imageHt - c);
                }
                c++; //only draw on every second row.
            }
        } //DrawSuperimposedMatrix()

        #region IDisposable Members

        public void Dispose()
        {
            this.eventList = null;
            this.sonogramImage.Dispose();
        }

        #endregion
    } //end class
}