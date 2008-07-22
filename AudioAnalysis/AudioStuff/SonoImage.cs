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


    public enum ImageType { linearScale, melScale, ceptral }



    public sealed class SonoImage
    {

        public const double zScoreMax = 8.0; //max SDs shown in score track of image
        public const int    scaleHt   = 10;   //pixel height of the top and bottom time scales
        public const int    trackHt   = 50;   //pixel height of the score tracks

        private int sf; //sample rate or sampling frequency
        private int NyquistF = 0;  //max frequency = Nyquist freq = sf/2
        private double recordingLength; //length in seconds 
        private double scoreThreshold;
        private bool addGrid;

        private int topScanBin;  //top Scan Bin i.e. the index of the bin
        private int bottomScanBin; //bottom Scan Bin




        public SonoImage(SonoConfig state)
        {
            this.sf = state.SampleRate;
            this.NyquistF = this.sf / 2; //max frequency
            this.recordingLength = state.AudioDuration;
            this.addGrid = state.AddGrid;
            this.topScanBin = state.TopScanBin;       //only used when scanning with a template
            this.bottomScanBin = state.BottomScanBin;
            this.scoreThreshold = state.ZScoreThreshold;
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
            ImageType type = ImageType.linearScale; //image is linear scale not mel scale
            return CreateBitmap(matrix, null, type);
        }


        public Bitmap CreateBitmap(double[,] sonogram, double[] scoreArray, ImageType type)
        {
            int width     = sonogram.GetLength(0);
            int sHeight   = sonogram.GetLength(1);
            int imageHt   = sHeight; //image ht= sonogram ht. Later include grid and score scales
            double hzBin  = NyquistF / (double)sHeight;
            double melBin = Speech.Mel(this.NyquistF) / (double)sHeight;

            byte[] hScale = CreateHorizintalScale(width);
            int[] vScale = CreateLinearVerticleScale(1000, sHeight); //calculate location of 1000Hz grid lines

            if (type == ImageType.melScale) //do mel scale conversions
            {
                vScale = CreateMelVerticleScale(1000, sHeight); //calculate location of 1000Hz grid lines in Mel scale
                double topMel = Speech.Mel(this.topScanBin*hzBin);
                double botMel = Speech.Mel(this.bottomScanBin*hzBin);
                this.topScanBin    = (int)(topMel / melBin);
                this.bottomScanBin = (int)(botMel / melBin);
            }

            if (addGrid) imageHt = scaleHt + sHeight + scaleHt;
            if (scoreArray != null) imageHt += trackHt;
            bool add1kHzLines = addGrid;
            if(type == ImageType.ceptral) add1kHzLines = false;

            double min;
            double max;
            DataTools.MinMax(sonogram, out min, out max);
            double range  = max - min;

            Bitmap bmp = new Bitmap(width, imageHt, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, imageHt), ImageLockMode.WriteOnly, bmp.PixelFormat);
            if (bmpData.Stride < 0) throw new NotSupportedException("Bottum-up bitmaps");


            unsafe
            {
                byte* p0 = (byte*)bmpData.Scan0;

                if (addGrid) //add top time line with one second tick intervals
                {
                    byte* p1 = p0;

                    for (int h = 0; h < scaleHt; h++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            *p1++ = hScale[x]; //b          
                            *p1++ = hScale[x]; //g          
                            *p1++ = hScale[x]; //r           
                        }
                        //following sometimes need as filler
                        //*p1++ = hScale[width-1]; //b          
                        //*p1++ = hScale[width-1]; //g          
                        //*p1++ = hScale[width-1]; //r 

                        p0 += bmpData.Stride;  //add in the scale
                    }
                } //end of adding time grid

                int gridCount = vScale.Length-1; //used to control addition of horizontal grid lines
                for (int y = sHeight; y > 0; y--) //over all freq bins
                {
                    byte* p1 = p0;

                    //add in 1 kHz grid line
                    if (add1kHzLines && (gridCount >= 0) && (y == vScale[gridCount]))
                    {
                        gridCount--;
                        for (int x = 0; x < width; x++)
                        {
                            *p1++ = (byte)200;   //b         
                            *p1++ = (byte)200;   //g  //light gray - for red: b=50,g=100,r=200
                            *p1++ = (byte)200;   //r       
                        }
                        p0 += bmpData.Stride;
                        continue;
                    }

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
                        {   *p1++ = (byte)c; //b           c 
                            *p1++ = (byte)g; //g          (c/2)
                            *p1++ = (byte)c; //r           0
                        }else
                        {   
                            *p1++ = (byte)c; //b           c 
                            *p1++ = (byte)c; //g          (c/2)
                            *p1++ = (byte)c; //r           0
                        }


                    }
                    p0 += bmpData.Stride;
                }//end over all freq bins

                if (addGrid)//add bottom time line with one second tick intervals
                {
                    byte* p1 = p0;
                    for (int h = 0; h < scaleHt; h++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            *p1++ = hScale[x]; //b          
                            *p1++ = hScale[x]; //g          
                            *p1++ = hScale[x]; //r           
                        }
                        p0 += bmpData.Stride;  //add in the scale
                    }
                } //end of adding seconds ticks

                if (scoreArray != null) //add a score track
                {
                    byte[,] scoreTrack = CreateTrack(scoreArray);
                    //Console.WriteLine("arrayLength=" + scoreArray.Length + "  imageLength=" + width);

                    byte* p1 = p0;
                    for (int h = 0; h < trackHt; h++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            *p1++ = scoreTrack[x, h]; //b          
                            *p1++ = scoreTrack[x, h]; //g          
                            *p1++ = scoreTrack[x, h]; //r
                        }
                        p0 += bmpData.Stride;  //add in the track
                    }
                } // end of adding track if (scoreArray != null)

            } //end unsafe

            bmp.UnlockBits(bmpData);
            return bmp;
        }


        public int[] CreateLinearVerticleScale(int herzInterval, int imageHt)
        {
            double herzBin = this.NyquistF / (double)imageHt;
            int gridCount = this.NyquistF / herzInterval;

            int binsPerGridBand = (int)(herzInterval / herzBin);
            int[] vScale = new int[gridCount];
            for (int i = 0; i < gridCount; i++)
            {
                vScale[i] = (1 + i) * binsPerGridBand;
                //Console.WriteLine("grid " + i + " =" + vScale[i]);
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
        public int[] CreateMelVerticleScale(int herzInterval, int imageHt)
        {
            double herzBin = this.NyquistF / (double)imageHt;
            int gridCount = this.NyquistF / herzInterval;
            double melBin = Speech.Mel(this.NyquistF) / (double)imageHt;

            int binsPerGridBand = (int)(herzInterval / herzBin);
            int[] vScale = new int[gridCount];
            for (int i = 0; i < gridCount; i++)
            {
                double freq = (1 + i) * herzInterval; //draw grid line at this freq
                double mel = Speech.Mel(freq);
                vScale[i] = (int)(mel / melBin);
                //Console.WriteLine("freq="+freq+"  mel="+mel+"   grid " + i + " =" + vScale[i]);
            }
            return vScale;
        }




        public byte[] CreateHorizintalScale(int width)
        {
            byte[] ba = new byte[width]; //byte array
            double period = this.recordingLength/width;

            for (int x = 0; x < width; x++) ba[x] = (byte)230; //set background pale gray colour
            for (int x = 0; x < width-1; x++)
            {
                double elapsedTime = x * this.recordingLength / (double)width;
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
            //for (int x = 0; x < width; x++) Console.WriteLine(x+"   "+ba[x]);
            return ba; //byte array
        }




        /// <summary>
        /// This method assumes that the passed score array has been range normalised.
        /// </summary>
        /// <param name="scoreArray"></param>
        /// <param name="trackHt"></param>
        /// <param name="SDs"></param>
        /// <returns></returns>
        public byte[,] CreateTrack(double[] scoreArray)
        {          
            int width = scoreArray.GetLength(0);
            byte[,] track = new byte[width, trackHt];//matrix to hold track's grey-scale intensity data

            for (int x = 0; x < width; x++) //over length of image
            {
                int id = SonoImage.trackHt - 1 - (int)(SonoImage.trackHt * scoreArray[x] / SonoImage.zScoreMax);
                if (id < 0) id = 0;
                else if (id > SonoImage.trackHt) id = SonoImage.trackHt;

                for (int z = 0; z < id; z++) track[x, z] = (byte)200; //black vertical histogram bar
            }

            //add in horizontal threshold significance line
            int significanceLine = (int)(SonoImage.trackHt * (1 - (this.scoreThreshold / SonoImage.zScoreMax)));
            for (int x = 0; x < width; x++) track[x, significanceLine] = (byte)100;  //grey colour
            return track;
        } //end CreateTrack()



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

}
