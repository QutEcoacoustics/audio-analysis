using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using TowseyLib;


namespace AudioAnalysis
{


    public enum TrackType { none, deciBels, segmentation, syllables, scoreArray, scoreMatrix, zeroCrossings, hits, timeTics }


    public sealed class Image_Track
    {
        public const int DefaultHeight = 30;   //pixel height of a track
        public const int timeScaleHt = 10;   //pixel height of the top and bottom time scales
        public const int syllablesTrackHeight = 10;   //pixel height of a track
        public const int scoreTrackHeight = 20;   //pixel height of a track
        public double scoreMax = 8.0; //max score displayed in score track of image
        public double ScoreMax { get { return scoreMax; } set { scoreMax = value; } }

        public TrackType TrackType { get; set; }

        public int Offset { get; set; }
        private int height = DefaultHeight;
        public int Height { get { return height; } set { height = value; } }
        private int[] intData = null;
        private double[] doubleData = null;
        private double[,] doubleMatrix = null;

        TimeSpan timeSpan { set; get; }

        public double MinDecibelReference { set; get; }
        public double MaxDecibelReference { set; get; }
        public double SegmentationThreshold_k1 { set; get; }
        public double SegmentationThreshold_k2 { set; get; }

        public double ScoreThreshold { set; get; }
        private int garbageID = 0;
        public int GarbageID { get { return garbageID; } set { garbageID = value; } }


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




        #region Constructors
        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public Image_Track(TrackType type, int[] data)
        {
            this.TrackType = type;
            this.intData = data;
            this.height = SetTrackHeight();
            //if(SonoImage.Verbose)Console.WriteLine("\tTrack CONSTRUCTOR: trackType = " + type + "  Data = " + data.ToString());
        }
        public Image_Track(TrackType type, double[] data)
        {
            this.TrackType = type;
            this.doubleData = data;
            this.height = SetTrackHeight();
            //if (SonoImage.Verbose) Console.WriteLine("\tTrack CONSTRUCTOR: trackType = " + type + "  Data = " + data.ToString());
        }
        public Image_Track(TrackType type, double[,] data)
        {
            this.TrackType = type;
            this.doubleMatrix = data;
            this.height = SetTrackHeight();
            //if (SonoImage.Verbose) Console.WriteLine("\tTrack CONSTRUCTOR: trackType = " + type + "  Data = " + data.ToString());
        }
        public Image_Track(TrackType type, TimeSpan t)
        {
            this.TrackType = type;
            this.timeSpan = t;
            this.height = SetTrackHeight();
        }
        public Image_Track(TrackType type)
        {
            this.TrackType = type;
        }

        #endregion


        public void SetIntArray(int[] data)
        {
            this.intData = data;
        }

        private int SetTrackHeight()
        {
            switch (TrackType)
            {
                case TrackType.timeTics:
                    return timeScaleHt;
                case TrackType.syllables:
                    return syllablesTrackHeight;
                case TrackType.scoreArray:
                    return scoreTrackHeight;
                case TrackType.deciBels:
                    return DefaultHeight;
                case TrackType.segmentation:
                    return DefaultHeight;
                case TrackType.scoreMatrix:
                    return DefaultHeight;
                default:
                    return DefaultHeight;
            }
        }

        public void DrawTrack(Bitmap bmp)
        {
            Log.WriteIfVerbose("\tDrawing track type =" + TrackType);
            switch (TrackType)
            {
                case TrackType.timeTics:
                    AddTimeTrack(bmp);    //time scale track
                    break;
                case TrackType.deciBels:
                    AddDecibelTrack(bmp); //frame energy track
                    break;
                case TrackType.segmentation:
                    AddSegmentationTrack(bmp); //segmentation track
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
                default:
                    Log.WriteLine("WARNING******** !!!! Image_Track.DrawTrack():- TRACKTYPE NOT DEFINED");
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
                Color col = TrackColors[intData[x]];
                if (intData[x] == 0) col = white;
                if (intData[x] == this.garbageID) col = gray;
                for (int z = 0; z < this.height; z++) bmp.SetPixel(x, Offset + z, col);  //add in hits
                bmp.SetPixel(x, bottom, Color.Black);
            }
            return bmp;
        }

        /// <summary>
        /// This method assumes that the passed data array is of values, min=0.0, max=approx 8-16.
        /// </summary>
        public Bitmap AddScoreArrayTrack(Bitmap bmp)
        {
            if (doubleData == null) return bmp;
            Color gray = Color.LightGray;
            Color white = Color.White;
            int bmpWidth = bmp.Width;
            int bmpHt = bmp.Height;
            for (int x = 0; x < Math.Min(bmp.Width, doubleData.Length); x++)
            {
                //if (doubleData[x] != 0.0) Console.WriteLine(x + "  " + doubleData[x]);
                int id = this.Height - 1 - (int)(this.Height * doubleData[x] / this.ScoreMax);
                if (id < 0) id = 0;
                else if (id > this.Height) id = this.Height;
                //paint white and leave a black vertical histogram bar
                for (int z = 0; z < id; z++) bmp.SetPixel(x, Offset + z, white);
            }

            //add in horizontal threshold significance line
            double max = 2 * this.ScoreThreshold;
            if (max < this.ScoreMax) max = this.ScoreMax;
            int lineID = (int)(this.Height * (1 - (this.ScoreThreshold / max)));
            for (int x = 0; x < bmpWidth; x++) bmp.SetPixel(x, Offset + lineID, gray);

            return bmp;
        }


        public Bitmap AddTimeTrack(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;

            byte[] hScale = CreateXaxis(width, this.timeSpan);
            Color black = Color.Black;
            Color gray = Color.Gray;
            Color white = Color.White;
            Color c = white;
            Offset += (timeScaleHt - 1);//shift offset to bottom of scale

            for (int x = 0; x < width; x++)
            {
                c = white;
                if (hScale[x] == 50) c = gray;
                else
                    if (hScale[x] == 0) c = black;

                //top axis
                //for (int h = 0; h < timeScaleHt; h++) bmp.SetPixel(x, h, c);
                //bmp.SetPixel(x, timeScaleHt - 1, black);

                //bottom axis tick marks
                for (int h = 0; h < timeScaleHt; h++) bmp.SetPixel(x, Offset - h, c);
                bmp.SetPixel(x, Offset, black);                    // top line of scale
                bmp.SetPixel(x, Offset - timeScaleHt + 1, black);  // bottom line of scale
            } //end of adding time grid
            return bmp;
        }

        public byte[] CreateXaxis(int width, TimeSpan timeSpan)
        {
            byte[] ba = new byte[width]; //byte array
            double duration = timeSpan.TotalSeconds;
            double period = duration / (double)width;

            for (int x = 0; x < width; x++) ba[x] = (byte)230; //set background pale gray colour
            for (int x = 0; x < width - 1; x++)
            {
                double elapsedTime = x * period; // / (double)width;
                double mod1sec = elapsedTime % 1.0000;
                double mod10sec = elapsedTime % 10.0000;
                if (mod10sec < period)//double black line
                {
                    ba[x] = (byte)0;
                    ba[x + 1] = (byte)0;
                }
                else
                    if (mod1sec <= period)
                    {
                        ba[x] = (byte)50;
                    }
            }
            return ba; //byte array
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
            int offset = bmpHt - Image_Track.DefaultHeight;
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

                int id = this.Height - 1 - (int)(this.Height * scores[maxIndex] / this.ScoreMax);
                if (id < 0) id = 0;
                else if (id > this.Height) id = this.Height;
                //paint white and leave a black vertical histogram bar
                for (int z = 0; z < id; z++) bmp.SetPixel(x, offset + z, white);
                for (int z = id; z < this.Height; z++) bmp.SetPixel(x, offset + z, TrackColors[maxIndex + 15]);
            }

            //add in horizontal threshold significance line
            double max = 2 * this.ScoreThreshold;
            if (max < this.ScoreMax) max = this.ScoreMax;
            int lineID = (int)(this.Height * (1 - (this.ScoreThreshold / max)));
            for (int x = 0; x < bmpWidth; x++) bmp.SetPixel(x, offset + lineID, gray);

            return bmp;
        }

        /// <summary>
        /// This method assumes that the passed decibel array has bounds determined by constants
        /// in the Sonogram class i.e. Sonogram.minLogEnergy and Sonogram.maxLogEnergy.
        /// </summary>
        public Bitmap AddDecibelTrack(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height; 

            int offset = height - Image_Track.DefaultHeight; //row id offset for placing track pixels
            Color white = Color.White;
            double range = this.MaxDecibelReference - this.MinDecibelReference;

            Color[] stateColors = { Color.White, Color.Green, Color.Red };

            for (int x = 0; x < width; x++)
            {
                double norm = (doubleData[x] - this.MinDecibelReference) / range;
                int id = Image_Track.DefaultHeight - 1 - (int)(Image_Track.DefaultHeight * norm);
                if (id < 0) id = 0;
                else if (id > Image_Track.DefaultHeight) id = Image_Track.DefaultHeight;
                //paint white and leave a black vertical bar
                for (int z = 0; z < id; z++) bmp.SetPixel(x, offset + z, white);
            }

            return bmp;
        }

        /// <summary>
        /// This method assumes that the passed decibel array has bounds determined by constants
        /// in the Sonogram class i.e. Sonogram.minLogEnergy and Sonogram.maxLogEnergy.
        /// Also requires values to be set for SegmentationThreshold_k1 and SegmentationThreshold_k2

        /// </summary>
        public Bitmap AddSegmentationTrack(Bitmap bmp)
        {
            int width = bmp.Width;
            int bmpHeight = bmp.Height;
            int trackHeight = Image_Track.DefaultHeight;

            int offset = bmpHeight - trackHeight; //row id offset for placing track pixels
            Color white = Color.White;
            double range = this.MaxDecibelReference - this.MinDecibelReference;
            for (int x = 0; x < width; x++)
            {
                double norm = (doubleData[x] - this.MinDecibelReference) / range;
                int id = trackHeight - 1 - (int)(trackHeight * norm);
                if (id < 0) id = 0;
                else if (id > trackHeight) id = trackHeight;
                //paint white and leave a black vertical bar
                for (int z = 0; z < id; z++) bmp.SetPixel(x, offset + z, white);
            }

            //display vocalisation state and thresholds used to determine endpoints
            if (this.intData == null) return bmp;     //cannot show becuase no state info
            Color[] stateColors = { Color.White, Color.Green, Color.Red };
            double v1 = this.SegmentationThreshold_k1 / range;
            int k1 = trackHeight - (int)(trackHeight * v1);
            double v2 = this.SegmentationThreshold_k2 / range;
            int k2 = trackHeight - (int)(trackHeight * v2);
            if ((v1 < 0.0) || (v1 > 1.0)) return bmp;
            if ((v2 < 0.0) || (v2 > 1.0)) return bmp;
            int y1 = offset + k1;
            if (y1 >= bmpHeight) y1 = bmpHeight - 1;
            int y2 = offset + k2;
            if (y2 >= bmpHeight) y2 = bmpHeight - 1;
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

        public static Image_Track GetDecibelTrack(BaseSonogram sg)
        {
            var track = new Image_Track(TrackType.deciBels, sg.Decibels);
            track.MinDecibelReference = sg.MinDecibelReference;
            track.MaxDecibelReference = sg.MaxDecibelReference;
            return track;
        }

        public static Image_Track GetSegmentationTrack(BaseSonogram sg)
        {
            var track = new Image_Track(TrackType.segmentation, sg.Decibels);
            track.intData = sg.SigState;
            track.MinDecibelReference = sg.MinDecibelReference;
            track.MaxDecibelReference = sg.MaxDecibelReference;
            track.SegmentationThreshold_k1 = sg.SegmentationThresholdK1;
            track.SegmentationThreshold_k2 = sg.SegmentationThresholdK2;
            return track;
        }

        public static Image_Track GetTimeTrack(TimeSpan t)
        {
            var track = new Image_Track(TrackType.timeTics, t);
            string name = track.TrackType.ToString();
            int height = track.Height;
            return track;
        }
        public static Image_Track GetSyllablesTrack(int[] SyllableIDs, int garbageID)
        {
            var track = new Image_Track(TrackType.syllables, SyllableIDs);
            track.GarbageID = garbageID;
            return track;
        }
        public static Image_Track GetScoreTrack(double[] scores, double? scoreMax, double? scoreThreshold)
        {
            var track = new Image_Track(TrackType.scoreArray, scores);
            track.ScoreThreshold = scoreThreshold ?? 0;
            track.ScoreMax       = scoreMax ?? 0;
            return track;
        }


    }// end  class Image_Track
}
