using System;
using System.Drawing;
using System.Drawing.Imaging;
using TowseyLib;


/// <summary>
/// Summary description for BitMaps
/// </summary>
/// 

namespace AudioStuff
{

    public sealed class BitMaps
    {

        public const double zScoreMax = 8.0; //max SDs shown in score track of image

        int scaleHt = 10;//pixel height of the top and bottom time scales
        int trackHt = 50;//pixel height of the score tracks
        int maxF    = 0; //max frequency

        int sf; //smaple rate or sampling frequency
        double recordingLength; //length in seconds 
        double threshold;

        public BitMaps(int sampleRate, double recordingLength, double threshold)
        {
            this.sf = sampleRate;
            this.recordingLength = recordingLength;
            this.threshold = threshold;
        }


        public Bitmap CreateBitmap(double[,] sonogram, double min, double max, Boolean addGrid,
            double[] scoreArray, int topFreqBin, int bottomFreqBin)
        {
            if (max < min) throw new ArgumentException("max must be greater than or equal to min");

            int width = sonogram.GetLength(0);
            int height = sonogram.GetLength(1);
            double range = max - min;
            int imageHt = height;

            double samplesPerSec = width / recordingLength;
            //Console.WriteLine("width=" + width + " length=" + recordingLength + " sps=" + samplesPerSec);
            byte[,] hScale = createHorizintalScale(width, samplesPerSec);

            if (addGrid) imageHt = scaleHt + height + scaleHt;
            if (scoreArray != null) imageHt += trackHt;

            Bitmap bmp = new Bitmap(width, imageHt, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, imageHt), ImageLockMode.WriteOnly, bmp.PixelFormat);
            if (bmpData.Stride < 0) throw new NotSupportedException("Bottum-up bitmaps");

            maxF = sf / 2; //max frequency
            double hzBin = maxF / (double)height;



            unsafe
            {
                byte* p0 = (byte*)bmpData.Scan0;

                if (addGrid) //add top time line with one second tick intervals
                {
                    byte* p1 = p0;
                    for (int h = 0; h < scaleHt - 1; h++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            *p1++ = hScale[x, 0]; //b          
                            *p1++ = hScale[x, 1]; //g          
                            *p1++ = hScale[x, 2]; //r           
                        }
                        p0 += bmpData.Stride;  //add in the scale
                    }
                } //end of adding grid

                for (int y = height; y > 0; y--) //over all freq bins
                {
                    int hz = (int)(hzBin * y);
                    double mod = hz % 1000;
                    byte* p1 = p0;

                    //add in 1000Hz red line
                    if (addGrid && (mod <= hzBin))
                    {
                        for (int x = 0; x < width; x++)
                        {
                            *p1++ = (byte)50;   //b         
                            *p1++ = (byte)100;   //g   
                            *p1++ = (byte)200;   //r       
                        }
                        p0 += bmpData.Stride;
                        continue;
                    }

                    for (int x = 0; x < width; x++) //for pixels in the line
                    {
                        //normalise and bound the value
                        //use min bound, max and 255 image intensity range
                        int c = (int)Math.Floor(255.0 * (sonogram[x, y - 1] - min) / range);
                        if (c < 0) c = 0;      //truncate if < 0
                        if (c >= 256) c = 255; //truncate if >255
                        c = 255 - c; //reverse the gray scale

                        int g = c + 40;
                        if (g >= 256) g = 255;
                        //  *p1++ = (byte)c; //b           c 
                        //  *p1++ = (byte)c; //g          (c/2)
                        //  *p1++ = (byte)c; //r           0

                        if ((y > topFreqBin) && (y < bottomFreqBin))
                        {   *p1++ = (byte)c; //b           c 
                            *p1++ = (byte)g; //g          (c/2)
                            *p1++ = (byte)c; //r           0
                        }else
                        {   *p1++ = (byte)c; //b           c 
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
                            *p1++ = hScale[x, 0]; //b          
                            *p1++ = hScale[x, 1]; //g          
                            *p1++ = hScale[x, 2]; //r           
                        }
                        p0 += bmpData.Stride;  //add in the scale
                    }
                } //end of adding seconds ticks

                if (scoreArray != null) //add a score track
                {
                    byte[,] scoreTrack = CreateTrack(scoreArray, trackHt, zScoreMax, threshold);

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

        public byte[,] createHorizintalScale(int width, double samplesPerSec)
        {
            byte[,] bm = new byte[width, 3]; //byteMatrix
            double period = 1 / samplesPerSec;
            for (int x = 0; x < width; x++)
            {
                double time = x / samplesPerSec;
                double mod1sec = time % 1.0;
                double mod10sec = time % 10.0;
                if (mod10sec < period)
                {
                    //Console.WriteLine("time=" + time + " mod=" + mod + " samplesPerSec=" + samplesPerSec);
                    bm[x, 0] = (byte)0;   //b          
                    bm[x, 1] = (byte)0;   //g          
                    bm[x, 2] = (byte)255; //r           
                }else
                if (mod1sec < period)
                    {
                        //Console.WriteLine("time=" + time + " mod=" + mod + " samplesPerSec=" + samplesPerSec);
                        bm[x, 0] = (byte)250; //b          
                        bm[x, 1] = (byte)100; //g          
                        bm[x, 2] = (byte)100;  //r           
                    }
                    else
                    {
                        bm[x, 0] = (byte)230; //b           
                        bm[x, 1] = (byte)230; //g          
                        bm[x, 2] = (byte)230; //r           
                    }
            }
            return bm; //byteMatrix
        }

        public void addHorizintalScale(BitmapData bmpData, byte[,]bm, int width, int scaleHt)
        {
            unsafe
            {
                byte* p0 = (byte*)bmpData.Scan0;
                byte* p1 = p0;
                    for (int h = 0; h < scaleHt; h++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            *p1++ = bm[x, 0]; //b          
                            *p1++ = bm[x, 1]; //g          
                            *p1++ = bm[x, 2]; //r           
                        }
                        p0 += bmpData.Stride;  //add in the scale
                    }
            } //end unsafe

        }

        /// <summary>
        /// This method assumes that the passed score array has been range normalised.
        /// </summary>
        /// <param name="scoreArray"></param>
        /// <param name="trackHt"></param>
        /// <param name="SDs"></param>
        /// <returns></returns>
        public byte[,] CreateTrack(double[] scoreArray, int trackHt, double SDs, double threshold)
        {          
            int width = scoreArray.GetLength(0);
            byte[,] track = new byte[width, trackHt];

            for (int x = 0; x < width; x++)
            {
                int id = trackHt - 1 - (int)(trackHt * scoreArray[x]);
                if (id < 0) id=0;
                else if (id>trackHt) id=trackHt;

                for (int z = 0; z < id; z++) track[x, z] = (byte)200;
            }
            //int significanceLine = 27;//equiv to p=0.01 ie z=2.33
            int significanceLine = (int)(trackHt*(1-(threshold/SDs)));
            for (int x = 0; x < width; x++)
            {
                track[x, significanceLine] = (byte)100;
            }
            return track;
        } //end CreateTrack()


    }

}