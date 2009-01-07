using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using TowseyLib;
using System.Collections.Generic;

/// <summary>
/// Summary description for BitMaps
/// </summary>
namespace AudioStuff
{
    public enum TrackType { none, energy, syllables, scoreArray, scoreMatrix, zeroCrossings, hits }

    public sealed class SonoImage
    {
        public const int    scaleHt   = 10;   //pixel height of the top and bottom time scales

        private int sf; //sample rate or sampling frequency
        private int NyquistF = 0;  //max frequency = Nyquist freq = sf/2
        private int minFreq;
        private int maxFreq;
        private double recordingLength; //length in seconds 
        private double scoreThreshold;
        private bool addGrid;
        private int topScanBin;  //top Scan Bin i.e. the index of the bin
        private int bottomScanBin; //bottom Scan Bin

        private SonogramType sonogramType;
        private bool doMelScale;
		private List<Track> tracks;

        //colors for tracks
        public static Color[] TrackColors = {Color.White, Color.Red, Color.Orange, Color.Cyan, Color.OrangeRed, Color.Pink, Color.Salmon, Color.Tomato, Color.DarkRed, Color.Purple, 
                                          /*Color.AliceBlue,*/ /*Color.Aqua, Color.Aquamarine, Color.Azure, Color.Bisque,*/
                             Color.Blue, Color.BlueViolet, /*Color.Brown, Color.BurlyWood,*/ Color.CadetBlue, /*Color.Chartreuse,*/ 
                             Color.Chocolate, /*Color.Coral,*/ /*Color.CornflowerBlue,*/ /*Color.Cornsilk,*/ Color.Crimson, Color.DarkBlue, 
                             Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta, 
                             Color.DarkOliveGreen, Color.DarkOrange, Color.DarkOrchid, Color.DarkSalmon, Color.DarkSeaGreen, 
                             Color.DarkSlateBlue, Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink, Color.DeepSkyBlue, 
                             Color.DimGray, Color.DodgerBlue, Color.Firebrick, Color.ForestGreen, Color.Fuchsia, 
                             Color.Gainsboro, Color.Gold, Color.Goldenrod, /*Color.Gray,*/ Color.Green, /*Color.GreenYellow,*/ 
                             Color.Honeydew, Color.HotPink, Color.IndianRed, Color.Indigo, /*Color.Khaki,*/ Color.Lavender, 
                             /*Color.LavenderBlush,*/ Color.LawnGreen, /*Color.LemonChiffon,*/ Color.Lime, 
                             Color.LimeGreen, /*Color.Linen,*/ Color.Magenta, Color.Maroon, Color.MediumAquamarine, Color.MediumBlue, 
                             /*Color.MediumOrchid,*/ Color.MediumPurple, /*Color.MediumSeaGreen,*/ Color.MediumSlateBlue, Color.MediumSpringGreen, 
                             Color.MediumTurquoise, Color.MediumVioletRed, Color.MidnightBlue, /*Color.MistyRose,*/ /*Color.Moccasin,*/ 
                             Color.Navy, /*Color.OldLace,*/ Color.Olive, /*Color.OliveDrab,*/ 
                             /*Color.Orchid, Color.PaleVioletRed, Color.PapayaWhip, */
                             /*Color.PeachPuff,*/ /*Color.Peru,*/ Color.Plum, /*Color.PowderBlue,*/ Color.RosyBrown, 
                             Color.RoyalBlue, Color.SaddleBrown, /*Color.SandyBrown,*/ Color.SeaGreen, /*Color.Sienna,*/ 
                             /*Color.Silver,*/ Color.SkyBlue, Color.SlateBlue, /*Color.SlateGray,*/ Color.SpringGreen, Color.SteelBlue, 
                             /*Color.Tan,*/ Color.Teal, Color.Thistle, Color.Turquoise, Color.Violet, /*Color.Wheat,*/ 
                             /*Color.Yellow,*/ Color.YellowGreen,  Color.Black};

		public SonoImage(Sonogram sonogram)
        {
            Log.WriteIfVerbose("\n\nCREATING SONOGRAM IMAGE.");
            SonoConfig state = sonogram.State;
            this.sf = state.SampleRate;
            this.NyquistF = this.sf / 2; //max frequency
            this.minFreq = state.FreqBand_Min;
            this.maxFreq = state.FreqBand_Max;
            this.bottomScanBin = state.MinTemplateFreq;
            this.recordingLength = state.TimeDuration;
            this.addGrid = state.AddGrid;
            this.topScanBin = state.MaxTemplateFreq;       //only used when scanning with a template
            this.bottomScanBin = state.MinTemplateFreq;
            this.scoreThreshold = state.ZScoreThreshold;

            this.sonogramType = state.SonogramType;
            this.doMelScale = state.DoMelScale;
        }
        /// <summary>
        /// This constructor called when drawing images of feature vectors
        /// </summary>
        public SonoImage(SonoConfig state, SonogramType sonogramType)
        {
            this.sf = state.SampleRate;
            this.NyquistF = this.sf / 2; //max frequency
            this.minFreq = state.FreqBand_Min;
            this.maxFreq = state.FreqBand_Max;
            this.recordingLength = state.TimeDuration;
            this.addGrid = state.AddGrid;
            this.topScanBin = state.MaxTemplateFreq;       //only used when scanning with a template
            this.bottomScanBin = state.MinTemplateFreq;
            this.scoreThreshold = state.ZScoreThreshold;

            this.sonogramType = sonogramType;
        }

        public void AddTrack(Track track)
        {
            if (!addGrid)
				Log.WriteLine("??????? WARNING !! SonoImage.AddTrack(): Must set ADD_GRID=true in .ini file in order to add tracks to image of sonogram.");
            if (tracks == null)
				tracks = new List<Track>();
            tracks.Add(track);
        }

		public void SetTracks(List<Track> tracks)
        {
            if (!this.addGrid)
                Log.WriteLine("??????? WARNING !! SonoImage.AddTrack(): Must set ADD_GRID=true in .ini file in order to add tracks to image of sonogram.");
            this.tracks = tracks;
        }

        /// <summary>
        /// converts matrix coordinates to image coordinates through 90 degree rotation
        /// </summary>
        public void TransformCoordinates(int r1, int c1, int r2, int c2, out int x1, out int y1, out int x2, out int y2, int mWidth)
        {
            x1 = r1;
            x2 = r2;
            y1 = mWidth - c2;
            y2 = mWidth - c1;
            if (this.addGrid)
            {
                y1 -= SonoImage.scaleHt;
                y2 -= SonoImage.scaleHt;
            }
        }

        public Bitmap CreateBitMapOfTemplate(double[,] matrix)
        {
            this.addGrid = false;
            return CreateBitmap(matrix);
        }

		public Bitmap CreateBitMapOfTemplate(double[] featureVector)
		{
			this.addGrid = false;
			int fVLength = featureVector.Length;
			int avLength = fVLength / 3; //assume that feature vector is composed of three parts.
			int rowWidth = 15;
			this.sonogramType = SonogramType.acousticVectors;

			//create a matrix of the required image
			double[,] M = new double[rowWidth * 3, avLength];
			for (int r = 0; r < rowWidth; r++)
			{
				for (int c = 0; c < avLength; c++)
					M[r, c] = featureVector[c];
			}
			for (int r = rowWidth; r < 2 * rowWidth; r++)
			{
				for (int c = 0; c < avLength; c++)
					M[r, c] = featureVector[avLength + c];
			}
			for (int r = (2 * rowWidth); r < (3 * rowWidth); r++)
			{
				for (int c = 0; c < avLength; c++)
					M[r, c] = featureVector[(2 * avLength) + c];
			}

			return CreateBitmap(M);
		}

        public Bitmap CreateBitmap(double[,] matrix)
        {
            int width   = matrix.GetLength(0); //number of spectra in sonogram
            int sHeight = matrix.GetLength(1); //number of freq bins in sonogram

			int binHeight;
			switch (sonogramType)
			{
				case SonogramType.cepstral:
				case SonogramType.acousticVectors:
					binHeight = 256 / sHeight; // several pixels per cepstral coefficient
					break;
				default:
					binHeight = 1;
					break;
			}

            int imageHt = sHeight * binHeight; // image ht = sonogram ht. Later include grid and score scales
            double hzBin = NyquistF / (double)sHeight;

            if (doMelScale) //do mel scale conversions
            {
                double melBin = Speech.Mel(this.NyquistF) / (double)sHeight;
                double topMel = Speech.Mel(this.topScanBin * hzBin);
                double botMel = Speech.Mel(this.bottomScanBin*hzBin);
                this.topScanBin    = (int)(topMel / melBin);
                this.bottomScanBin = (int)(botMel / melBin);
            }

            //calculate total height of the bmp
            int totalHt = imageHt;
            if (this.addGrid) totalHt = scaleHt + imageHt + scaleHt;
            if (tracks != null)
				foreach (Track track in tracks)
					totalHt += track.Height;

            Bitmap bmp = new Bitmap(width, totalHt, PixelFormat.Format24bppRgb);
            AddSonogram(bmp, matrix, binHeight, addGrid, topScanBin, bottomScanBin);

            int offset = scaleHt + imageHt;
            if (addGrid) bmp = Add1kHzLines(bmp);
            if (addGrid) bmp = AddXaxis(bmp, offset);
            if (Log.Verbosity > 0 && tracks != null) Log.WriteLine("\tNumber of tracks=" + this.tracks.Count);
            offset = scaleHt + imageHt + scaleHt;
            if (this.tracks != null) foreach (Track track in this.tracks)
                {
                    track.Offset = offset;
                    offset += track.Height;
                    //Console.WriteLine("offset=" + track.Offset);
                    track.DrawTrack(bmp);
                }
            return bmp;
        }

        public Bitmap AddSonogram(Bitmap bmp, double[,] sonogram, int binHt, int unsafePixels)
        {
            int width = sonogram.GetLength(0);
            int sHeight = sonogram.GetLength(1);
            int imageHt = sHeight * binHt;     //image ht = sonogram ht. Later include grid and score scales
            int totalHt = imageHt;
            if (this.addGrid) totalHt = scaleHt + imageHt + scaleHt;

            double min;
            double max;
            DataTools.MinMax(sonogram, out min, out max);
            double range = max - min;
          
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, totalHt), ImageLockMode.WriteOnly, bmp.PixelFormat);
            if (bmpData.Stride < 0) throw new NotSupportedException("Bottum-up bitmaps");

            unsafe
            {
                byte* p0 = (byte*)bmpData.Scan0;

                if (addGrid) //add spacer track for top time tics
                {
                    byte* p1 = p0;
                    for (int h = 0; h < scaleHt; h++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            *p1++ = 0; //b          
                            *p1++ = 0; //g          
                            *p1++ = 0; //r           
                        }
                        p0 += bmpData.Stride;  //add in the scale
                    }
                } //end of adding time grid

                for (int y = sHeight; y > 0; y--) //over all freq bins
                {
                    byte* p1 = p0;
                    for (int r = 0; r < binHt; r++) //repeat this bin if ceptral image
                    {
                        for (int x = 0; x < width; x++) //for pixels in the line
                        {
                            //normalise and bound the value - use min bound, max and 255 image intensity range
                            double value = (sonogram[x, y - 1] - min) / (double)range;
                            int c = (int)Math.Floor(255.0 * value); //original version
                            //int c = (int)Math.Floor(255.0 * normal * normal); //take square of normalised value to enhance features
                            if (c < 0) c = 0;      //truncate if < 0
                            else
                                if (c >= 256) c = 255; //truncate if >255
                            //c /= 2; //half the image intensity

                            c = 255 - c; //reverse the gray scale

                            int g = c + 40; //green tinge used in the template scan band 
                            if (g >= 256) g = 255;

                            if ((y > topScanBin) && (y < bottomScanBin))
                            {
                                *p1++ = (byte)c; //b           c 
                                *p1++ = (byte)g; //g          (c/2)
                                *p1++ = (byte)c; //r           0
                            }
                            else
                            {
                                *p1++ = (byte)c; //b           c 
                                *p1++ = (byte)c; //g          (c/2)
                                *p1++ = (byte)c; //r           0
                            }
                        }
                        p0 += bmpData.Stride;
                    } //end repeats over one track
                }//end over all freq bins
            } //end unsafe  
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public static void AddSonogram(Bitmap bmp, double[,] sonogram, int binHt, bool addGrid, int topScanBin, int bottomScanBin)
        {
            int width = sonogram.GetLength(0);
            int sHeight = sonogram.GetLength(1);
            int imageHt = sHeight * binHt;     //image ht = sonogram ht. Later include grid and score scales
            int totalHt = imageHt;
            if (addGrid) totalHt = scaleHt + imageHt + scaleHt;
            int yOffset = imageHt;
            if (addGrid) yOffset = scaleHt + imageHt;

            double min;
            double max;
            DataTools.MinMax(sonogram, out min, out max);
            double range = max - min;

            Color[] grayScale = ImageTools.GrayScale();

            for (int y = 0; y < sHeight; y++) //over all freq bins
            {
                for (int r = 0; r < binHt; r++) //repeat this bin if ceptral image
                {
                    for (int x = 0; x < width; x++) //for pixels in the line
                    {
                        //normalise and bound the value - use min bound, max and 255 image intensity range
                        double value = (sonogram[x, y] - min) / (double)range;
                        int c = (int)Math.Floor(255.0 * value); //original version
                        //int c = (int)Math.Floor(255.0 * normal * normal); //take square of normalised value to enhance features
                        if (c < 0) c = 0;      //truncate if < 0
                        else
                        if (c >= 256) c = 255; //truncate if >255
                        //c /= 2; //half the image intensity

                        c = 255 - c; //reverse the gray scale

                        int g = c + 40; //green tinge used in the template scan band 
                        if (g >= 256) g = 255;
                        //Color col = Color.FromArgb(c,c,c);
                        Color col = grayScale[c];
                        if ((y > topScanBin) && (y < bottomScanBin))col = Color.FromArgb(c,g,c);
                        //int row = sOffset - y;
                        bmp.SetPixel(x, yOffset-1, col);
                    }//for all pixels in line

                    yOffset--;
                } //end repeats over one track
            }//end over all freq bins
        }

        public int[] CreateLinearYaxis(int herzInterval, int imageHt)
        {
            int freqRange = this.maxFreq - this.minFreq + 1;
            double pixelPerHz = imageHt / (double)freqRange; 
            int[] vScale = new int[imageHt];
            //Console.WriteLine("freqRange=" + freqRange + " herzInterval=" + herzInterval + " imageHt=" + imageHt + " pixelPerHz=" + pixelPerHz);

            for (int f = this.minFreq + 1; f < this.maxFreq; f++)
            {
                if (f % 1000 == 0)  //convert freq value to pixel id
                {
                    int hzOffset = f - this.minFreq;
                    int pixelID = (int)(hzOffset * pixelPerHz) + 1;
                    if (pixelID >= imageHt) pixelID = imageHt - 1;
                    //Console.WriteLine("f=" + f + " hzOffset=" + hzOffset + " pixelID=" + pixelID);
                    vScale[pixelID] = 1;  
                }
            }
            return vScale;
        }

        /// <summary>
        /// use this method to generate grid lines for mel scale image
        /// </summary>
        public int[] CreateMelYaxis(int herzInterval, int imageHt)
        {
            //int freqRange = this.maxFreq - this.minFreq + 1;
            double minMel = Speech.Mel(this.minFreq);
            int melRange = (int)(Speech.Mel(this.maxFreq) - minMel + 1);
            //double pixelPerHz = imageHt / (double)freqRange;
            double pixelPerMel = imageHt / (double)melRange;
            int[] vScale = new int[imageHt];
            //Console.WriteLine("minMel=" + minMel.ToString("F1") + " melRange=" + melRange + " herzInterval=" + herzInterval + " imageHt=" + imageHt + " pixelPerMel=" + pixelPerMel);

            for (int f = this.minFreq + 1; f < this.maxFreq; f++)
            {
                if (f % 1000 == 0)  //convert freq value to pixel id
                {
                    //int hzOffset  = f - this.minFreq;
                    int melOffset = (int)(Speech.Mel(f) - minMel);
                    int pixelID = (int)(melOffset * pixelPerMel) + 1;
                    if (pixelID >= imageHt) pixelID = imageHt - 1;
                    //Console.WriteLine("f=" + f + " melOffset=" + melOffset + " pixelID=" + pixelID);
                    vScale[pixelID] = 1;
                }
            }
            return vScale;
        }

        public byte[] CreateXaxis(int width, double timeDuration)
        {
            byte[] ba = new byte[width]; //byte array
            double period = this.recordingLength/width;

            for (int x = 0; x < width; x++) ba[x] = (byte)230; //set background pale gray colour
            for (int x = 0; x < width-1; x++)
            {
                double elapsedTime = x * timeDuration / (double)width;
                double mod1sec = elapsedTime % 1.0000;
                double mod10sec = elapsedTime % 10.0000;
                if (mod10sec < period)//double black line
                {
                    //Console.WriteLine("time=" + time + " mod=" + mod + " samplesPerSec=" + samplesPerSec);
                    ba[x]   = (byte)0;          
                    ba[x+1] = (byte)0;            
                }
                else
                    if (mod1sec <= period)
                    {
                        //Console.WriteLine("time=" + time + " mod=" + mod + " samplesPerSec=" + samplesPerSec);
                        ba[x] = (byte)50;          
                    }
            }
            return ba; //byte array
        }
        public Bitmap AddXaxis(Bitmap bmp, int offset)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            byte[] hScale = CreateXaxis(width, this.recordingLength);
            Color black = Color.Black;
            Color gray  = Color.Gray;
            Color white = Color.White;
            Color c = white;
            offset += (SonoImage.scaleHt - 1);//shift offset to bottom of scale

            for (int x = 0; x < width; x++)
            {
                c = white;
                if (hScale[x] == 50) c = gray;
                else
                if (hScale[x] == 0)  c = black;

                //top axis
                for (int h = 0; h < scaleHt; h++) bmp.SetPixel(x, h, c);
                bmp.SetPixel(x, scaleHt-1, black);
                //bottom axis tick marks
                for (int h = 0; h < scaleHt; h++) bmp.SetPixel(x, offset-h, c);
                bmp.SetPixel(x, offset, black);            // top line of scale
                bmp.SetPixel(x, offset - scaleHt + 1, black);  // bottom line of scale
            } //end of adding time grid
            return bmp;
        }

        public Bitmap Add1kHzLines(Bitmap bmp)
        {
            if (this.sonogramType == SonogramType.cepstral) return bmp; //do not add to cepstral image
            else
            if (this.sonogramType == SonogramType.acousticVectors) return bmp; //do not add to cepstral image
 
            const int kHz = 1000;
            int width  = bmp.Width;
            int height = bmp.Height;

            //calculate height of the sonogram
            int sHeight = height;
            if(addGrid) sHeight -= (SonoImage.scaleHt + SonoImage.scaleHt);
            if (this.tracks.Count != 0) sHeight -= (Track.DefaultHeight);

            int[] vScale = CreateLinearYaxis(kHz, sHeight); //calculate location of 1000Hz grid lines
            if (this.doMelScale) vScale = CreateMelYaxis(kHz, sHeight); 

            Color c = Color.LightGray;
            for (int p = 0; p < vScale.Length; p++) //over all Y-axis pixels
            {
                if (vScale[p]==0) continue;
                int y = scaleHt + sHeight - p;  
                for (int x = 0; x < width; x++) bmp.SetPixel(x, y, c);
            }
            return bmp;
        }//end method Add1kHzLines()

        public Bitmap AddShapeBoundaries(Bitmap bmp, ArrayList shapes, Color col)
        {
            int x1; int y1; int x2; int y2;
            int mWidth = bmp.Height;
            Color shapeColor;

            foreach (Shape shape in shapes)
            {
                int r1 = shape.r1;
                int c1 = shape.c1;
                int r2 = shape.r2;
                int c2 = shape.c2;
                int colorCount = ImageTools.darkColors.Length;
                int colorID = shape.category % colorCount;
                if (shape.category == -1) shapeColor = col;
                else                      shapeColor = ImageTools.darkColors[colorID];
                //Console.WriteLine("category=" + shape.category);
                
                TransformCoordinates(r1,c1,r2,c2,out x1, out y1, out x2, out y2, mWidth);
                for (int i = x1; i <= x2; i++) bmp.SetPixel(i, y1, shapeColor);
                for (int i = x1; i <= x2; i++) bmp.SetPixel(i, y2, shapeColor);
                for (int i = y1; i <= y2; i++) bmp.SetPixel(x1, i, shapeColor);
                for (int i = y1; i <= y2; i++) bmp.SetPixel(x2, i, shapeColor);
            }
            return bmp;
        }

        public Bitmap AddShapeSolids(Bitmap bmp, ArrayList shapes, Color col)
        {
            int x1; int y1; int x2; int y2;
            int mWidth = bmp.Height;
            Color shapeColor = col;
            //int categoryToView = 14;

            foreach (Shape shape in shapes)
            {
                int r1 = shape.r1;
                int c1 = shape.c1;
                int r2 = shape.r2;
                int c2 = shape.c2;
                //if (shape.category != categoryToView) continue;
                int colorCount = ImageTools.darkColors.Length;
                int colorID = shape.category % colorCount;
                //if (shape.category == -1) shapeColor = col;
                //else shapeColor = ImageTools.darkColors[colorID];

                TransformCoordinates(r1, c1, r2, c2, out x1, out y1, out x2, out y2, mWidth);
                for (int i = x1; i <= x2; i++) 
                for (int j = y1; j <= y2; j++) bmp.SetPixel(i, j, shapeColor);
            }
            return bmp;
        }

        public Bitmap AddCentroidBoundaries(Bitmap bmp, ArrayList shapes, Color col)
        {
            int x1; int y1; int x2; int y2;
            int mWidth = bmp.Height;
            Color shapeColor = col;
            int r1 = 0;

            foreach (Shape shape in shapes)
            {
                r1 += 10;
                int c1 = shape.c1;
                int r2 = r1 + shape.RowWidth;
                int c2 = shape.c2;
                int colorCount = ImageTools.darkColors.Length;
                int colorID = shape.category % colorCount;
                //if (shape.category == -1) shapeColor = col;
                //else shapeColor = ImageTools.darkColors[colorID];

                TransformCoordinates(r1, c1, r2, c2, out x1, out y1, out x2, out y2, mWidth);
                for (int i = x1; i <= x2; i++)
                    for (int j = y1; j <= y2; j++) bmp.SetPixel(i, j, shapeColor);
                r1 += shape.RowWidth;
            }
            return bmp;
        }
    }// end class SonoImage

    public sealed class Track
    {
        public const double ScoreMax = 8.0; //max score displayed in score track of image
        public const int DefaultHeight = 30;   //pixel height of a track
        // none, energy, syllables, scoreArray, scoreMatrix, zeroCrossings, hits 
        public const int syllablesTrackHeight = 10;   //pixel height of a track
        public const int scoreTrackHeight = 20;   //pixel height of a track

		public TrackType TrackType { get; set; }

		public int Offset { get; set; }
        private int height = DefaultHeight;
        public int Height { get { return height; } set { height = value; } }
        private int[] intData = null;
        private double[] doubleData = null;
        private double[,] doubleMatrix = null;

        public double MinDecibelReference { set; get; }
        public double MaxDecibelReference { set; get; }
        public double SegmentationThreshold_k1 { set; get; }
        public double SegmentationThreshold_k2 { set; get; }

        public double ScoreThreshold { set; get; }
        private int garbageID = 0;
        public int GarbageID { get { return garbageID; } set { garbageID = value; } }

        public Track(TrackType type, int[] data)
        {
            this.TrackType = type;
            this.intData = data;
            this.height = SetTrackHeight();
            //if(SonoImage.Verbose)Console.WriteLine("\tTrack CONSTRUCTOR: trackType = " + type + "  Data = " + data.ToString());
        }
        public Track(TrackType type, double[] data)
        {
            this.TrackType = type;
            this.doubleData = data;
            //if (SonoImage.Verbose) Console.WriteLine("\tTrack CONSTRUCTOR: trackType = " + type + "  Data = " + data.ToString());
        }

        public Track(TrackType type, double[,] data)
        {
            this.TrackType = type;
            this.doubleMatrix = data;
            //if (SonoImage.Verbose) Console.WriteLine("\tTrack CONSTRUCTOR: trackType = " + type + "  Data = " + data.ToString());
        }

        public void SetIntArray(int[] data)
        {
            this.intData = data;
        }

        private int SetTrackHeight()
        {
			switch (TrackType)
			{
				case TrackType.syllables:
					return syllablesTrackHeight;
				case TrackType.scoreArray:
					return scoreTrackHeight;
				case TrackType.energy:
				case TrackType.scoreMatrix:
				default:
					return DefaultHeight;
			}
        }

        public void DrawTrack(Bitmap bmp)
        {
            Log.WriteIfVerbose("\tDrawing track type =" + TrackType);
			switch (TrackType)
			{
				case TrackType.energy:
					AddDecibelTrack(bmp); //frame energy track
					break;
				case TrackType.syllables:
					AddSyllablesTrack(bmp);
					break;
				case TrackType.scoreArray:
					AddScoreArrayTrack(bmp);  //add a score track
					break;
				case TrackType.scoreMatrix:
					AddScoreMatrixTrack(bmp);  //add a score track
					break;
			}
            // none, energy, syllables, scoreArray, scoreMatrix, zeroCrossings, hits 
        }
        
        /// <summary>
        /// paints a track of symbol colours derived from symbol ID
        /// </summary>
        public Bitmap AddSyllablesTrack(Bitmap bmp)
        {
            int bmpWidth = bmp.Width;
            //int bmpHt = bmp.Height;
            Color gray = Color.LightGray;
            Color white = Color.White;
            //Color red = Color.Red;
            if ((intData == null) || (intData.Length == 0))
            {
                Console.WriteLine("#####WARNING!! AddScoreArrayTrack(Bitmap bmp):- Integer data does not exists!");
                return bmp;
            }

            //Console.WriteLine("offset=" + this.offset);
            int bottom = Offset + this.height - 1;
            for (int x = 0; x < Math.Min(bmp.Width, intData.Length); x++)
            {
                Color col = SonoImage.TrackColors[intData[x]];
                if (intData[x] == 0) col = white;
                if (intData[x] == this.garbageID) col = gray;
                for (int z = 0; z < this.height; z++) bmp.SetPixel(x, Offset + z, col);  //add in hits
                bmp.SetPixel(x, bottom, Color.Black);
            }
            return bmp;
        }

        /// <summary>
        /// This method assumes that the passed data array is of values, min=0.0, max = approx 8-16.
        /// </summary>
        public Bitmap AddScoreArrayTrack(Bitmap bmp)
        {
            //Console.WriteLine("arrayLength=" + scoreArray.Length + "  imageLength=" + width);
            //Console.WriteLine("height=" + height);
        //    int offset  = bmpHt - Track.DefaultHeight;
            Color gray  = Color.LightGray;
            Color white = Color.White;
         //   Color red   = Color.Red;
           // bool intDataExists = ((intData != null) && (intData.Length != 0));
          //  if ((!intDataExists)) Console.WriteLine("#####WARNING!! AddScoreArrayTrack(Bitmap bmp):- Integer data does not exists!");

            int bmpWidth = bmp.Width;
            int bmpHt = bmp.Height;
			for (int x = 0; x < Math.Min(bmp.Width, doubleData.Length); x++)
            {
                int id = this.Height - 1 - (int)(this.Height * doubleData[x] / Track.ScoreMax);
                if (id < 0) id = 0;
                else if (id > this.Height) id = this.Height;
                //paint white and leave a black vertical histogram bar
                for (int z = 0; z < id; z++) bmp.SetPixel(x, Offset + z, white);
            }

            //add in horizontal threshold significance line
            double max = 2 * this.ScoreThreshold;
            if (max < Track.ScoreMax) max = Track.ScoreMax;
            int lineID = (int)(this.Height * (1 - (this.ScoreThreshold / max)));
            for (int x = 0; x < bmpWidth; x++) bmp.SetPixel(x, Offset + lineID, gray);

            return bmp;
        }

        /// <summary>
        /// This method assumes that the passed data array is of values, min=0.0, max = approx 8-16.
        /// </summary>
        public Bitmap AddScoreMatrixTrack(Bitmap bmp)
        {
            int bmpWidth = bmp.Width;
            int bmpHt = bmp.Height;
            //Console.WriteLine("arrayLength=" + scoreArray.Length + "  imageLength=" + width);
            //Console.WriteLine("height=" + height);
            int offset = bmpHt - Track.DefaultHeight;
            Color gray = Color.LightGray;
            Color white = Color.White;
            Color red = Color.Red;
          //  bool intDataExists = ((intData != null) && (intData.Length != 0));
          //  if ((!intDataExists)) Console.WriteLine("#####WARNING!! AddScoreMatrixTrack(Bitmap bmp):- Integer data does not exists!");

            int numberOfScoreTracks = this.doubleMatrix.GetLength(0);
            double[] scores = new double[numberOfScoreTracks];
            for (int x = 0; x < bmpWidth; x++)
            {
                //transfer scores
                for (int y = 0; y < numberOfScoreTracks; y++) scores[y] = this.doubleMatrix[y, x];
                int maxIndex = DataTools.GetMaxIndex(scores);

                int id = this.Height - 1 - (int)(this.Height * scores[maxIndex] / Track.ScoreMax);
                if (id < 0) id = 0;
                else if (id > this.Height) id = this.Height;
                //paint white and leave a black vertical histogram bar
                for (int z = 0; z < id; z++) bmp.SetPixel(x, offset + z, white);
                for (int z = id; z < this.Height; z++) bmp.SetPixel(x, offset + z, SonoImage.TrackColors[maxIndex+15]);
            }

            //add in horizontal threshold significance line
            double max = 2 * this.ScoreThreshold;
            if (max < Track.ScoreMax) max = Track.ScoreMax;
            int lineID = (int)(this.Height * (1 - (this.ScoreThreshold / max)));
            for (int x = 0; x < bmpWidth; x++) bmp.SetPixel(x, offset + lineID, gray);

            return bmp;
        }

        /// <summary>
        /// This method assumes that the passed decibel array has zero minimum bounds determined by constants
        /// in the Sonogram class i.e. Sonogram.minLogEnergy and Sonogram.maxLogEnergy.
        /// </summary>
        public Bitmap AddDecibelTrack(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height; //height = 10 + 513 + 10 + 50 = 583 

            int offset = height - Track.DefaultHeight; //row id offset for placing track pixels
            Color white = Color.White;
            double range = this.MaxDecibelReference - this.MinDecibelReference;
            //Console.WriteLine("range=" + range + "  minRef=" + this.MinDecibelReference + "  maxRef=" + this.MaxDecibelReference);

            Color[] stateColors = { Color.White, Color.Green, Color.Red };

            for (int x = 0; x < width; x++)
            {
                double norm = (doubleData[x] - this.MinDecibelReference) / range;
                int id = Track.DefaultHeight - 1 - (int)(Track.DefaultHeight * norm);
                if (id < 0) id = 0;
                else if (id > Track.DefaultHeight) id = Track.DefaultHeight;
                //paint white and leave a black vertical bar
                for (int z = 0; z < id; z++) bmp.SetPixel(x, offset + z, white);
            }

            //display vocalisation state and thresholds used to determine endpoints
            double v1 = this.SegmentationThreshold_k1 / range;
            int k1 = Track.DefaultHeight - (int)(Track.DefaultHeight * v1);
            double v2 = this.SegmentationThreshold_k2 / range;
            int k2 = Track.DefaultHeight - (int)(Track.DefaultHeight * v2);
            //Console.WriteLine("SegmentationThreshold_k2=" + SegmentationThreshold_k2 + "   v2=" + v2 + "  k2=" + k2);
            if ((v1 < 0.0) || (v1 > 1.0)) return bmp;
            if ((v2 < 0.0) || (v2 > 1.0)) return bmp;
            int y1 = offset + k1;
            if (y1 >= height) y1 = height - 1;
            int y2 = offset + k2;
            if (y2 >= height) y2 = height - 1;
            for (int x = 0; x < width; x++)
            {
                bmp.SetPixel(x, y1, Color.Orange);
                bmp.SetPixel(x, y2, Color.Lime);

                //put state as top four pixels
                Color col = stateColors[this.intData[x]];
                bmp.SetPixel(x, offset, col);
                bmp.SetPixel(x, offset + 1, col);
                bmp.SetPixel(x, offset + 2, col);
                bmp.SetPixel(x, offset + 3, col);
            }
            return bmp;
        }

        public static TrackType GetTrackType(string typeName)
        {
            TrackType type = TrackType.none; //the default
            if ((typeName == null) || (typeName == "")) return TrackType.none;
            if (typeName.StartsWith("energy")) return TrackType.energy;
            if (typeName.StartsWith("zeroCrossings")) return TrackType.zeroCrossings;
            if (typeName.StartsWith("score")) return TrackType.scoreArray;
            return type;
        }
    }//end class Track
}