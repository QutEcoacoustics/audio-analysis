using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics;

namespace QutBioacosutics.Xie
{
    using MathNet.Numerics.LinearAlgebra.Single;
    using TowseyLib;

    class ExtractTracks
    {
        public double[,] GetTracks(double[,] matrix,int binToreance)
        {
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);

            var row = matrix.GetLength(0);
            var column = matrix.GetLength(1);


            // save local peaks to an array of list

            var pointList = new List<Peak>[column];
            
            for (int j = 0; j < column; j++)
            {
                var Points = new List<Peak>();            
                for (int i = 0; i < row; i++)
                {
                    var point = new Peak();
                    if (matrix[i, j] > 0)
                    {
                        point.X = i;
                        point.Y = j;
                        point.Amplitude = matrix[i, j];
                        Points.Add(point);
                    }
                                       
                }

                pointList[j] = Points;

            }

            // use neareast distance to form tracks of the first two columns
                        
            var shortTrackList = new List<Track>();
            var longTrackList = new List<Track>();

            var longTrackXList = new List<List<double>>();
            var longTrackYList = new List<List<double>>();
            //var longTrackAmplitudeList = new List<double>();


            //var longTrackAmplitude = new List<double>();


            var indexI = new List<int>();
            var indexJ = new List<int>();

            // save nearest peaks into longTracks
            for (int i = 0; i < pointList[0].Count; i++)
            {               
                for(int j = 0; j < pointList[1].Count; j++)
                {
                    var longTrackX = new List<double>();
                    var longTrackY = new List<double>();
                    if (Math.Abs(pointList[0][i].X - pointList[1][j].X) < 3)
                    {
                        indexI.Add(i);
                        indexJ.Add(j);
                        longTrackX.AddMany(i, j);
                        longTrackY.AddMany(pointList[0][i].X, pointList[1][j].X);

                        longTrackXList.Add(longTrackX);
                        longTrackYList.Add(longTrackY);
                    }

                }
                
            }

            for (int i = 0; i < indexI.Count; i++) 
            {
                var longTrack = new Track();
                longTrack.StartFrame = 0;
                longTrack.EndFrame = 1;
                longTrack.LowBin = Math.Min(pointList[0][indexI[i]].X, pointList[1][indexJ[i]].X);
                longTrack.HighBin = Math.Max(pointList[0][indexI[i]].X, pointList[1][indexJ[i]].X);
                longTrackList.Add(longTrack);

                //longTrackX[i] = pointList[0][indexI[i]].X;
                //longTrackY[i] = pointList[0][indexJ[i]].Y;

                //longTrackAmplitude.Add(matrix[indexI[i],indexJ[i]]);

                //longTrackXList.Add(longTrackX);
                //longTrackYList.Add(longTrackY);
                //longTrackAmplitudeList.Add(longTrackAmplitude);
            }
            

            // remove peaks which have already been used to produce long tracks
            
            pointList[0].RemoveRange(indexI[0], indexI.Count);
            pointList[1].RemoveRange(indexJ[1],indexJ.Count);
         
            // save individual peaks into shortTracks 
            for (int i = 0; i < pointList[0].Count; i++)
            {
                var shortTrack = new Track();
                shortTrack.StartFrame = 0;
                shortTrack.EndFrame = 0;
                shortTrack.LowBin = pointList[0][i].X;
                shortTrack.HighBin = pointList[0][i].X;
                shortTrackList.Add(shortTrack);
            
            }

            for (int i = 0; i < pointList[1].Count; i++)
            {
                var shortTrack = new Track();
                shortTrack.StartFrame = 0;
                shortTrack.EndFrame = 0;
                shortTrack.LowBin = pointList[1][i].X;
                shortTrack.HighBin = pointList[1][i].X;
                shortTrackList.Add(shortTrack);

            }
            
            // use linear regression to extend long tracks and use neareast distance to extend short tracks
            int c = 2;
            while (c < column)
            {
                // use linear regression to predict the next position of long tracks
                for (int i = 0; i < longTrackXList.Count; i++)
                {
                    var xdata = new double[longTrackXList.Count];
                    xdata = longTrackXList[i].ToArray();

                    var ydata = new double[longTrackYList.Count];
                    ydata = longTrackYList[i].ToArray();

                    var p = Fit.Line(xdata, ydata);
                    var offset = p.Item1;
                    var slope = p.Item2;

                    var position = c * offset + slope;

                    for (int j = 0; j < pointList[c].Count; j++)
                    {

                        if ((position - pointList[c][j].Y) < binToreance)
                        {
                            // add individual peaks to long tracks
                            longTrackList[i].EndFrame = c;
                            longTrackList[i].LowBin = Math.Min();
                            longTrackList[i].HighBin = Math.Max(); 

                        }
                    
                    }



                }
            }




                return null;
        }

    }
}
