// <copyright file="Image_MultiTrack.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    public class Image_MultiTrack : IDisposable
    {
        public Image<Rgb24> SonogramImage { get; private set; }

        private readonly List<ImageTrack> tracks = new List<ImageTrack>();

        public IEnumerable<ImageTrack> Tracks => this.tracks;

        public IEnumerable<AcousticEvent> eventList { get; set; }

        public List<SpectralTrack> spectralTracks { get; set; }

        public double[,] SuperimposedMatrix { get; set; }

        public double[,] SuperimposedRedTransparency { get; set; }

        public double[,] SuperimposedRainbowTransparency { get; set; }

        public int[,] SuperimposedDiscreteColorMatrix { get; set; }

        private double superImposedMaxScore;
        private int[] FreqHits;
        private int nyquistFreq; //sets the frequency scale for drawing events
        private int freqBinCount;
        private double freqBinWidth;
        private double framesPerSecond;

        //private Point[] points;
        public List<PointOfInterest> Points { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image_MultiTrack"/> class.
        /// CONSTRUCTOR
        /// </summary>
        public Image_MultiTrack(Image<Rgb24> image)
        {
            this.SonogramImage = image;
            this.Points = new List<PointOfInterest>();
        }

        public void AddTrack(ImageTrack track)
        {
            this.tracks.Add(track);
        }

        public void AddEvents(IEnumerable<AcousticEvent> _list, int _nyquist, int _freqBinCount, double _framesPerSecond)
        {
            this.eventList = _list;
            this.nyquistFreq = _nyquist;
            this.freqBinCount = _freqBinCount;
            this.framesPerSecond = _framesPerSecond;
            this.freqBinWidth = _nyquist / (double)_freqBinCount;
        }

        public void AddPoints(IEnumerable<PointOfInterest> points)
        {
            // this.points.AddRange(points);

            // scan for preexisting coordinates
            foreach (var pointOfInterest in points)
            {
                // copied to satisfy closure constraint
                PointOfInterest localCopy = pointOfInterest;

                // search current points to see if any share the same coordinates

                var match = this.Points.IndexOf(poi => poi.Point == localCopy.Point);
                if (match >= 0)
                {
                    // if they do share the same coordinates, overwrite the old one
                    this.Points[match] = pointOfInterest;
                }
                else
                {
                    // otherwise, add new point to list
                    this.Points.Add(pointOfInterest);
                }
            }
        }

        public void OverlayRedMatrix(double[,] m, double maxScore)
        {
            //this.SuperimposedMatrix = m; // TODO:  This line does not work !!?? Use next line
            this.SuperimposedRedTransparency = m;
            this.superImposedMaxScore = maxScore;
        }

        public void OverlayRedTransparency(double[,] m)
        {
            this.SuperimposedRedTransparency = m;
        }

        public void OverlayRainbowTransparency(double[,] m)
        {
            this.SuperimposedRainbowTransparency = m;
        }

        public void OverlayDiscreteColorMatrix(int[,] m)
        {
            this.SuperimposedDiscreteColorMatrix = m;
        }

        public void AddFreqHitValues(int[] f, int nyquist)
        {
            this.FreqHits = f;
            this.nyquistFreq = nyquist;
        }

        public void AddTracks(List<SpectralTrack> _tracks, double _framesPerSecond, double _freqBinWidth)
        {
            this.freqBinWidth = _freqBinWidth;
            this.framesPerSecond = _framesPerSecond;
            this.spectralTracks = _tracks;
        }

        /// <summary>
        /// WARNING: This method calls Image_MultiTrack.GetImage().
        /// In some circumstances GetImage() cannot manage images with an area larger than 10,385,000 pixels.
        /// This means it cannot handle recording sonograms longer than 2 minutes.
        /// </summary>
        public void Save(string path)
        {
            var image = this.GetImage();
            if (image == null)
            {
                Log.WriteLine("MultiTrackImage.Save() - WARNING: NULL IMAGE.Cannot save to: " + path);
                return;
            }

            image.Save(path);
        }

        /// <summary>
        /// WARNING: graphics.DrawImage() or GDI cannot draw an image that is too big, typically
        /// with an area larger than 10,385,000 pixels (Jiro estimated > 40000 pixels).
        /// This means it cannot handle recording sonograms longer than 2 minutes.
        /// Therefore call a recursive method to draw the image.
        /// </summary>
        public Image<Rgba32> GetImage()
        {
            // Calculate total height of the bmp
            var height = this.CalculateImageHeight();

            // set up a new image having the correct dimensions
            var imageToReturn = new Image<Rgba32>(this.SonogramImage.Width, height);

            // need to do this before get Graphics because cannot PutPixels into Graphics object.
            if (this.SuperimposedRedTransparency != null)
            {
                this.SonogramImage = this.OverlayRedTransparency((Image<Rgb24>)this.SonogramImage);
            }

            if (this.SuperimposedMatrix != null)
            {
                this.SonogramImage = this.OverlayMatrix((Image<Rgb24>)this.SonogramImage);
            }

            // create new graphics canvas and add in the sonogram image
            imageToReturn.Mutate(g =>
            {
                g.DrawImage(this.SonogramImage, 1f);

                // draw events first because their rectangles can cover other features
                if (this.eventList != null)
                {
                    var hitImage = new Image<Rgba32>(imageToReturn.Width, height);

                    //hitImage.MakeTransparent();
                    foreach (AcousticEvent e in this.eventList)
                    {
                        e.DrawEvent(hitImage, this.framesPerSecond, this.freqBinWidth, this.SonogramImage.Height);
                    }

                    g.DrawImage(hitImage, 0, 0);
                }

                // draw events first because their rectangles can cover other features
                if (this.Points != null)
                {
                    // var stats = new StatDescriptive(this.points.Select(p => p.Item2).ToArray());
                    // stats.Analyze();
                    foreach (PointOfInterest poi in this.Points)
                    {
                        //poi.DrawPoint(g, this.Points, this.sonogramImage.Height);
                        const int radius = 12;
                        poi.DrawBox(g, this.Points, radius);
                    }
                }

                // draw spectral tracks
                if (this.spectralTracks != null)
                {
                    foreach (SpectralTrack t in this.spectralTracks)
                    {
                        t.DrawTrack(g, this.framesPerSecond, this.freqBinWidth, this.SonogramImage.Height);
                    }
                }

                if (this.FreqHits != null)
                {
                    this.DrawFreqHits(g);
                }

                if (this.SuperimposedRainbowTransparency != null)
                {
                    this.OverlayRainbowTransparency(g, (Image<Rgb24>)this.SonogramImage);
                }

                if (this.SuperimposedDiscreteColorMatrix != null)
                {
                    this.OverlayDiscreteColorMatrix(g);
                }
            });

            // now add tracks to the image
            int offset = this.SonogramImage.Height;
            foreach (ImageTrack track in this.tracks)
            {
                track.topOffset = offset;
                track.bottomOffset = offset + track.Height - 1;
                track.DrawTrack(imageToReturn.CloneAs<Rgb24>());
                offset += track.Height;
            }

            return imageToReturn;
        }

        private int CalculateImageHeight()
        {
            int totalHeight = this.SonogramImage.Height;
            foreach (ImageTrack track in this.tracks)
            {
                totalHeight += track.Height;
            }

            return totalHeight;
        }

        public void DrawFreqHits(IImageProcessingContext g)
        {
            int L = this.FreqHits.Length;
            Pen p1 = new Pen(Color.Red, 1);

            for (int x = 0; x < L; x++)
            {
                if (this.FreqHits[x] <= 0)
                {
                    continue;
                }

                int y = (int)(this.SonogramImage.Height * (1 - (this.FreqHits[x] / (double)this.nyquistFreq)));

                //g.DrawRectangle(p1, x, y, x + 1, y + 1);
                g.DrawLine(p1, x, y, x, y + 1);

                // g.DrawText(e.Name, Drawing.Tahoma6, Color.Black, new PointF(x, y - 1));
            }
        }

        /// <summary>
        /// superimposes a matrix of scores on top of a sonogram.
        /// Only draws lines on every second row so that the underling sonogram can be discerned
        /// </summary>
        public void OverlayMatrix(IImageProcessingContext g)
        {
            //int paletteSize = 256;
            var pens = ImageTools.GetRedGradientPalette(); //size = 256

            int rows = this.SuperimposedMatrix.GetLength(0);
            int cols = this.SuperimposedMatrix.GetLength(1);
            int imageHt = this.SonogramImage.Height - 1; //subtract 1 because indices start at zero

            //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(this.SuperimposedMatrix), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\superimposed1.png", false);

            for (int c = 1; c < cols; c++) // traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (this.SuperimposedMatrix[r, c] == 0.0)
                    {
                        continue;
                    }

                    double normScore = this.SuperimposedMatrix[r, c] / this.superImposedMaxScore;

                    //int penID = (int)(paletteSize * normScore);
                    //if (penID >= paletteSize) penID = paletteSize - 1;
                    var brush = new SolidBrush(Color.Red);
                    g.FillRectangle(brush, r, imageHt - c, 1, 1); //THIS DRAWS A PIXEL !!!!
                }

                //c++; //only draw on every second row.
            }
        } //OverlayMatrix()

        /// <summary>
        /// superimposes a matrix of scores on top of a sonogram.
        /// Only draws lines on every second row so that the underling sonogram can be discerned
        /// </summary>
        public Image<Rgb24> OverlayMatrix(Image<Rgb24> bmp)
        {
            Image<Rgb24> newBmp = (Image<Rgb24>)bmp.Clone();

            //int paletteSize = 256;
            var pens = ImageTools.GetRedGradientPalette(); //size = 256

            int rows = this.SuperimposedMatrix.GetLength(0);
            int cols = this.SuperimposedMatrix.GetLength(1);
            int imageHt = this.SonogramImage.Height - 1; //subtract 1 because indices start at zero

            // traverse columns - skip DC column
            for (int c = 1; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (this.SuperimposedMatrix[r, c] == 0.0)
                    {
                        continue;
                    }

                    double normScore = this.SuperimposedMatrix[r, c] / this.superImposedMaxScore;

                    //int penID = (int)(paletteSize * normScore);
                    //if (penID >= paletteSize) penID = paletteSize - 1;
                    //g.DrawLine(pens[penID], r, imageHt - c, r + 1, imageHt - c);
                    //g.DrawLine(new Pen(Color.Red, 1), r, imageHt - c, r + 1, imageHt - c);
                    newBmp[r, imageHt - c] = Color.Red;
                }
            }

            return newBmp;
        } //OverlayMatrix()

        /// <summary>
        /// superimposes a matrix of scores on top of a sonogram.
        /// </summary>
        public Image<Rgb24> OverlayRedTransparency(Image<Rgb24> bmp)
        {
            return Image_MultiTrack.OverlayScoresAsRedTransparency(bmp, this.SuperimposedRedTransparency);
        }

        /// <summary>
        /// superimposes a matrix of scores on top of a sonogram.
        /// TODO: WARNING: THIS METHOD IS YET TO BE DEBUGGED SINCE TRANSITION TO SIX-LABOURS, FEB 2020.
        /// </summary>
        public static Image<Rgb24> OverlayScoresAsRedTransparency(Image<Rgb24> bmp, double[,] hits)
        {
            Image<Rgb24> newBmp = (Image<Rgb24>)bmp.Clone();
            int rows = hits.GetLength(0);
            int cols = hits.GetLength(1);
            int imageHt = bmp.Height - 1; //subtract 1 because indices start at zero

            //traverse columns - skip DC column
            for (int c = 1; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (hits[r, c] == 0.0)
                    {
                        continue;
                    }

                    var pixel = bmp[r, imageHt - c];
                    if (pixel.R == 255)
                    {
                        continue; // white
                    }

                    newBmp[r, imageHt - c] = Color.FromRgb(255, pixel.G, pixel.B);
                }
            }

            return newBmp;
        }

        /// <summary>
        /// superimposes a matrix of scores on top of a sonogram. USES RAINBOW PALLETTE.
        /// ASSUME MATRIX NORMALIZED IN [0,1].
        /// </summary>
        public void OverlayRainbowTransparency(IImageProcessingContext g, Image<Rgb24> bmp)
        {
            Color[] palette = { Color.Crimson, Color.Red, Color.Orange, Color.Yellow, Color.Lime, Color.Green, Color.Blue, Color.Indigo, Color.Violet, Color.Purple };
            int rows = this.SuperimposedRainbowTransparency.GetLength(0);
            int cols = this.SuperimposedRainbowTransparency.GetLength(1);
            int imageHt = this.SonogramImage.Height - 1; //subtract 1 because indices start at zero

            for (int r = 0; r < rows; r++)
            {
                // traverse columns - skip DC column
                for (int c = 1; c < cols; c++)
                {
                    double value = this.SuperimposedRainbowTransparency[r, c];
                    if (value <= 0.0)
                    {
                        continue; //nothing to show
                    }

                    var pixel = bmp[r, imageHt - c];
                    if (pixel.R > 250)
                    {
                        continue; //by-pass white
                    }

                    int index = (int)Math.Floor(value * 9); // get index into pallette
                    if (index > 9)
                    {
                        index = 9;
                    }

                    var newColor = palette[index].ToPixel<Rgb24>();
                    double factor = pixel.R / (255 * 1.2);  // 1.2 is a color intensity adjustment
                    var red = (byte)Math.Floor(newColor.R + ((255 - newColor.R) * factor));
                    var grn = (byte)Math.Floor(newColor.G + ((255 - newColor.G) * factor));
                    var blu = (byte)Math.Floor(newColor.B + ((255 - newColor.B) * factor));
                    g.DrawLine(new Pen(Color.FromRgb(red, grn, blu), 1), r, imageHt - c, r + 1, imageHt - c);
                    c++; //every second column
                }
            }
        } //OverlayRainbowTransparency()

        /// <summary>
        /// superimposes a matrix of scores on top of a sonogram. USES RAINBOW PALLETTE
        /// ASSUME MATRIX consists of integers >=0;
        /// </summary>
        private void OverlayDiscreteColorMatrix(IImageProcessingContext g)
        {
            int rows = this.SuperimposedDiscreteColorMatrix.GetLength(0);
            int cols = this.SuperimposedDiscreteColorMatrix.GetLength(1);
            MatrixTools.MinMax(this.SuperimposedDiscreteColorMatrix, out var min, out var max);
            int palleteLength = ImageTools.DarkColors.Length;

            //Color[] palette = { Color.Crimson, Color.Red, Color.Orange, Color.Yellow, Color.Lime, Color.Green, Color.Blue, Color.Indigo, Color.Violet, Color.Purple };
            int imageHt = this.SonogramImage.Height - 1; //subtract 1 because indices start at zero

            for (int r = 0; r < rows; r++)
            {
                //traverse columns - skip DC column
                for (int c = 1; c < cols; c++)
                {
                    int index = this.SuperimposedDiscreteColorMatrix[r, c];
                    if (index <= 0)
                    {
                        continue; //nothing to show
                    }

                    // Color pixel = bmp.GetPixel(r, imageHt - c);
                    // if (pixel.R > 250) continue; //by-pass white
                    // int index = (int)Math.Floor((value * 9));//get index into pallette
                    if (index >= palleteLength)
                    {
                        index = index % palleteLength;
                    }

                    var newColor = ImageTools.DarkColors[index];

                    //double factor = pixel.R / (double)(255 * 1.2);  //1.2 is a color intensity adjustment
                    //int red = (int)Math.Floor(newColor.R + ((255 - newColor.R) * factor));
                    //int grn = (int)Math.Floor(newColor.G + ((255 - newColor.G) * factor));
                    //int blu = (int)Math.Floor(newColor.B + ((255 - newColor.B) * factor));
                    //g.DrawLine(new Pen(Color.FromRgb(red, grn, blu)), r, imageHt - c, r + 1, imageHt - c);
                    g.DrawLine(new Pen(newColor, 1), r, imageHt - c, r + 1, imageHt - c);
                }
            }
        } //OverlayDiscreteColorMatrix()

        public void Dispose()
        {
            this.eventList = null;
            this.SonogramImage.Dispose();
        }
    } //end class
}