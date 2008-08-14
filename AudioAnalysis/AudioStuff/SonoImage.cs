using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using TowseyLib;


/// <summary>
/// Summary description for BitMaps
/// </summary>
/// 

namespace AudioStuff
{


    public enum TrackType { none, energy, zeroCrossings, score }



    public sealed class SonoImage
    {

        public const double zScoreMax = 5.0; //max SDs shown in score track of image
        public const int    scaleHt   = 10;   //pixel height of the top and bottom time scales
        public const int    trackHt   = 50;   //pixel height of the score tracks

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
        private TrackType trackType;

        private double[] decibels;
        private double minDecibelReference;
        private double maxDecibelReference;
        private double SegmentationThreshold_k1;
        private double SegmentationThreshold_k2;
        private int[] zeroCrossings;
        private int[] sigState;



        public SonoImage(Sonogram sonogram, TrackType trackType)
        {
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
            this.trackType = trackType;

            this.decibels = sonogram.Decibels;
            this.minDecibelReference = state.MinDecibelReference;
            this.maxDecibelReference = state.MaxDecibelReference;
            this.SegmentationThreshold_k1 = state.SegmentationThreshold_k1;
            this.SegmentationThreshold_k2 = state.SegmentationThreshold_k2;
            this.zeroCrossings = sonogram.ZeroCross;
            this.sigState = sonogram.SigState;
            //Console.WriteLine("SonoImage.sonogramType=" + this.sonogramType);
        }

        public SonoImage(SonoConfig state, SonogramType sonogramType, TrackType trackType)
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
            this.trackType = trackType;
        }


        /// <summary>
        /// converts matrix coordinates to image coordinates through 90 degree rotation
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="c1"></param>
        /// <param name="r2"></param>
        /// <param name="c2"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
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
            return CreateBitmap(matrix, null);
        }

        public Bitmap CreateBitMapOfTemplate(double[] featureVector)
        {
            this.addGrid = false;
            int fVLength = featureVector.Length;
            int avLength = fVLength / 3; //assume that feature vector is composed of three parts.
            int rowWidth = 15;
            this.sonogramType = SonogramType.acousticVectors;

            //create a matrix of the required image
            double[,] M = new double[rowWidth*3,avLength];
            for (int r = 0; r < rowWidth; r++)
            {
                for (int c = 0; c < avLength; c++) M[r, c] = featureVector[c];
            }
            for (int r = rowWidth; r < 2*rowWidth; r++)
            {
                for (int c = 0; c < avLength; c++) M[r, c] = featureVector[avLength+c];
            }
            for (int r = (2*rowWidth); r < (3*rowWidth); r++)
            {
                for (int c = 0; c < avLength; c++) M[r, c] = featureVector[(2 * avLength)+c];
            }

            return CreateBitmap(M, null);
        }

        public Bitmap CreateBitmap(double[,] matrix, double[] dataArray)
        {
            int width   = matrix.GetLength(0); //number of spectra in sonogram
            int sHeight = matrix.GetLength(1); //number of freq bins in sonogram

            //Console.WriteLine("sonogramType=" + this.sonogramType);
            int binHt = 1; // 1 pixel per freq bin
            if (this.sonogramType == SonogramType.cepstral) binHt = 256 / sHeight; //several pixels per cepstral coefficient
            else
            if (this.sonogramType == SonogramType.acousticVectors) binHt = 256 / sHeight; //several pixels per cepstral coefficient

            int imageHt   = sHeight * binHt;     //image ht = sonogram ht. Later include grid and score scales
            double hzBin  = NyquistF / (double)sHeight;

            if (this.doMelScale) //do mel scale conversions
            {
                double melBin = Speech.Mel(this.NyquistF) / (double)sHeight;
                double topMel = Speech.Mel(this.topScanBin * hzBin);
                double botMel = Speech.Mel(this.bottomScanBin*hzBin);
                this.topScanBin    = (int)(topMel / melBin);
                this.bottomScanBin = (int)(botMel / melBin);
            }

            //calculate total height of the bmp
            int totalHt = imageHt;
            if (this.addGrid)
            {
                totalHt = scaleHt + imageHt + scaleHt;
                if (this.trackType != TrackType.none) totalHt += trackHt;
            }

            Bitmap bmp = new Bitmap(width, totalHt, PixelFormat.Format24bppRgb);
            //bmp = AddSonogram(bmp, sonogram, binHt, unsafePixels);
            bmp = AddSonogram(bmp, matrix, binHt);

            if (!addGrid) return bmp;
            bmp = Add1kHzLines(bmp);
            bmp = AddXaxis(bmp);
            if (this.trackType == TrackType.score) AddScoreTrack(bmp, dataArray);  //add a score track
            else if (this.trackType == TrackType.energy) AddDecibelTrack(bmp, this.decibels);
            else if (this.trackType == TrackType.zeroCrossings)
            {
                double[] zxs = DataTools.normalise(this.zeroCrossings);
                zxs = DataTools.Normalise(zxs, this.minDecibelReference, this.maxDecibelReference);
                AddDecibelTrack(bmp, zxs);
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


        public Bitmap AddSonogram(Bitmap bmp, double[,] sonogram, int binHt)
        {
            int width = sonogram.GetLength(0);
            int sHeight = sonogram.GetLength(1);
            int imageHt = sHeight * binHt;     //image ht = sonogram ht. Later include grid and score scales
            int totalHt = imageHt;
            if (this.addGrid) totalHt = scaleHt + imageHt + scaleHt;
            int yOffset = imageHt;
            if (this.addGrid) yOffset = scaleHt + imageHt;

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
                        if ((y > this.topScanBin) && (y < this.bottomScanBin))col = Color.FromArgb(c,g,c);
                        //int row = sOffset - y;
                        bmp.SetPixel(x, yOffset-1, col);
                    }//for all pixels in line

                    yOffset--;
                } //end repeats over one track
            }//end over all freq bins
            return bmp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="herzInterval"></param>
        /// <param name="imageHt"></param>
        /// <returns></returns>
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
        /// <param name="herzInterval"></param>
        /// <param name="imageHt"></param>
        /// <param name="dummy"></param>
        /// <returns></returns>
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


        public Bitmap AddXaxis(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            byte[] hScale = CreateXaxis(width, this.recordingLength);
            Color black = Color.Black;
            Color gray  = Color.Gray;
            Color white = Color.White;
            Color c = white;
            int offset = height -1 - trackHt; //offset for the lower x-axis tics

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
            if (this.trackType != TrackType.none) sHeight -= (SonoImage.trackHt);

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



        /// <summary>
        /// This method assumes that the passed array is of zScores, min=0.0, max = approx 8-16.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="scoreArray"></param>
        /// <returns></returns>
        public Bitmap AddScoreTrack(Bitmap bmp, double[] scoreArray)
        {
            int width  = bmp.Width;
            int height = bmp.Height; //height = 10 + 513 + 10 + 50 = 583 
            //Console.WriteLine("arrayLength=" + scoreArray.Length + "  imageLength=" + width);
            //Console.WriteLine("height=" + height);
            int offset = height - SonoImage.trackHt;
            Color gray = Color.Gray;
            Color white = Color.White;


            for (int x = 0; x < width; x++)
            {
                int id = SonoImage.trackHt - 1 - (int)(SonoImage.trackHt * scoreArray[x] / SonoImage.zScoreMax);
                if (id < 0) id = 0;
                else if (id > SonoImage.trackHt) id = SonoImage.trackHt;
                //paint white and leave a black vertical histogram bar
                for (int z = 0; z < id; z++) bmp.SetPixel(x, offset + z, white);
            }

            //add in horizontal threshold significance line
            int lineID = (int)(SonoImage.trackHt * (1 - (this.scoreThreshold / SonoImage.zScoreMax)));
            for (int x = 0; x < width; x++) bmp.SetPixel(x, offset + lineID, gray);

            return bmp;
        }


        /// <summary>
        /// This method assumes that the passed decibel array has zero minimum bounds determined by constants
        /// in the Sonogram class i.e. Sonogram.minLogEnergy and Sonogram.maxLogEnergy.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="scoreArray"></param>
        /// <returns></returns>
        public Bitmap AddDecibelTrack(Bitmap bmp, double[] array)
        {
            
            int width  = bmp.Width;
            int height = bmp.Height; //height = 10 + 513 + 10 + 50 = 583 

            int offset = height - SonoImage.trackHt; //row id offset for placing track pixels
            Color white = Color.White;
            double range = this.maxDecibelReference - this.minDecibelReference;
            //Console.WriteLine("range=" + range + "  minReference=" + minReference + "  maxReference=" + maxReference);
            Color[] stateColors = { Color.White, Color.Green, Color.Red };

            for (int x = 0; x < width; x++)
            {
                double norm = (array[x] - this.minDecibelReference) / range;
                int id = SonoImage.trackHt - 1 - (int)(SonoImage.trackHt * norm);
                if (id < 0) id = 0;
                else if (id > SonoImage.trackHt) id = SonoImage.trackHt;
                //paint white and leave a black vertical bar
                for (int z = 0; z < id; z++) bmp.SetPixel(x, offset + z, white);

            }

            //display vocalisation state and thresholds used to determine endpoints
            double v1 = this.SegmentationThreshold_k1 / range;
            int k1 = SonoImage.trackHt - (int)(SonoImage.trackHt * v1);
            double v2 = this.SegmentationThreshold_k2 / range;
            int k2 = SonoImage.trackHt - (int)(SonoImage.trackHt * v2);
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
                Color col = stateColors[this.sigState[x]];
                bmp.SetPixel(x, offset, col);
                bmp.SetPixel(x, offset + 1, col);
                bmp.SetPixel(x, offset + 2, col);
                bmp.SetPixel(x, offset + 3, col);
            }
            return bmp;
        }




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

        public static TrackType GetTrackType(string typeName)
        {
            TrackType type = TrackType.none; //the default
            if ((typeName == null) || (typeName == "")) return TrackType.none; 
            if (typeName.StartsWith("energy")) return TrackType.energy;
            if (typeName.StartsWith("zeroCrossings")) return TrackType.zeroCrossings;
            if (typeName.StartsWith("score")) return TrackType.score;
            return type;
        }


    }// end class SonoImage

}
