// <copyright file="SURFFeatures.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class SURFFeatures
    {
        /*
        /// <summary>
        /// this class used for SURF intermediate data.
        /// </summary>
        public class SURFData
        {
            public Image<Gray, Byte> Image   { get; set; }
            public VectorOfKeyPoint POIs     { get; set; }
            public Matrix<float> Descriptors { get; set; }
        }


        public static void SURF_TEST()
        {
            //string inputPath1 = @"C:\SensorNetworks\Output\SURFImages\TESTMATRIXquery.png";
            //string inputPath1 = @"C:\SensorNetworks\Output\SURFImages\TESTLewinsRailQuery.png";
            //string inputPath1 = @"C:\SensorNetworks\Output\XueyanDataset\Test\sunhat.jpg";
            //string inputPath1 = @"C:\SensorNetworks\Output\XueyanDataset\Test\BASIC256_greysacle_Grey_Mona_lisa.jpg";
            string inputPath1 = @"C:\SensorNetworks\Output\SURFImages\womanface2.jpg";
            //string inputPath1 = @"Y:\XueyanDataset\Query\4. Eastern whipbird\Query1\NEJB_NE465_20101014-052000-0521000-estern whipbird.wav";

            //string inputPath2 = @"C:\SensorNetworks\Output\Test\TESTMATRIX2.png";
            string inputPath2 = @"C:\SensorNetworks\Output\SURFImages\MonaLisaColour.jpg";
            string opDir = @"C:\SensorNetworks\Output\XueyanDataset";
            // string opDir = @"Y:\XueyanDataset\SURFOutput";
            SURFFeatures.SURF(new FileInfo(inputPath1), new FileInfo(inputPath2), new DirectoryInfo(opDir));
        }


        public static void SURF(FileInfo Q_inputFile, FileInfo T_inputFile, DirectoryInfo opDir)
        {
            Image<Gray, Byte> image1 = SURFFeatures.GetGreyScaleImage(Q_inputFile.FullName);
            SURFData model = SURFFeatures.GetKeyPoints(image1);
            if (model.POIs.Size == 0)
            {
                LoggedConsole.WriteLine("WARNING: There are no Points Of Interest in image <{0}>", Q_inputFile.Name);
                return;
            }
            else
            {
                LoggedConsole.WriteLine("Image POI count = {0}", model.POIs.Size);
                int min = Math.Min(model.POIs.Size, 10);
                for(int i = 0; i < model.POIs.Size; i++)
                {
                    MKeyPoint poi = model.POIs[i];
                    LoggedConsole.WriteLine("POI<X,Y> = <{0:f0},{1:f0}> \tAngle={2:f0}, \tClassId={3}, \tOctave={4}, \tResponse={5:f0} \tSize={6}",
                                             poi.Point.X, poi.Point.Y, poi.Angle, poi.ClassId, poi.Octave, poi.Response, poi.Size);
                }
            }

            Image<Gray, Byte> image2 = SURFFeatures.GetGreyScaleImage(T_inputFile.FullName);
            SURFData obsvd = SURFFeatures.GetKeyPoints(image2);
            if (obsvd.POIs.Size == 0)
            {
                LoggedConsole.WriteLine("WARNING: There are no Points Of Interest in image <{0}>", T_inputFile.Name);
                return;
            }
            else
            {
                LoggedConsole.WriteLine("Image POI count = {0}", obsvd.POIs.Size);
            }

            Image<Bgr, Byte> image = MatchKeyPoints(model, obsvd);
            if (image != null)
            {
                string fileName = Q_inputFile.Name + "_" + T_inputFile.Name + ".png";
                image.Save(Path.Combine(opDir.FullName, fileName));
            }
        }


        public static Image<Gray, Byte> GetGreyScaleImage(string path)
        {
            Bitmap bmp = new Bitmap(path);
            Image<Gray, Byte> image = new Image<Gray, Byte>(bmp);
            //Image<Gray, Byte> modelImage = new Image<Gray, Byte>(path);

            //code for colour images
            //Image<Bgr, Byte> colorImage = new Image<Bgr, Byte>(bmp);
            // convert colour image to grayscale
            //Image<Gray, Byte> modelImage = new Image<Gray, byte>(colorImage.Bitmap);
            // OR
            //Capture cap = new Capture(path);
            //Image<Bgr, Byte> colorImage = cap.QueryFrame();
            //Image<Gray, Byte> modelImage = colorImage.Convert<Gray, Byte>();
            return image;
        }

        public static SURFData GetKeyPoints(Image<Gray, Byte> image)
        {
            //extract features from the object image
            SURFDetector surfCPU = new SURFDetector(500, false);
            VectorOfKeyPoint keyPoints = surfCPU.DetectKeyPointsRaw(image, null);

            if (keyPoints.Size == 0)
            {
                return null;
            }
            Matrix<float> descriptors = surfCPU.ComputeDescriptorsRaw(image, null, keyPoints);

            var data = new SURFData();
            data.Image = image;
            data.Descriptors = descriptors;
            data.POIs = keyPoints;
            return data;
        }


        public static Image<Bgr, Byte> MatchKeyPoints(SURFData model, SURFData obsvd)
        {
            HomographyMatrix homography = null;
            Matrix<int> indices;
            Matrix<byte> mask;
            int k = 2;
            double uniquenessThreshold = 0.8;

            BruteForceMatcher<float> matcher = new BruteForceMatcher<float>(DistanceType.L2);
                matcher.Add(model.Descriptors);

                indices = new Matrix<int>(obsvd.Descriptors.Rows, k);
                using (Matrix<float> dist = new Matrix<float>(obsvd.Descriptors.Rows, k))
                {
                    matcher.KnnMatch(obsvd.Descriptors, indices, dist, k, null);
                    mask = new Matrix<byte>(dist.Rows, 1);
                    mask.SetValue(255);
                    Features2DToolbox.VoteForUniqueness(dist, uniquenessThreshold, mask);
                }

                int nonZeroCount = CvInvoke.cvCountNonZero(mask);
                if (nonZeroCount >= 4)
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(model.POIs, obsvd.POIs, indices, mask, 1.5, 20);
                    if (nonZeroCount >= 4)
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(model.POIs, obsvd.POIs, indices, mask, 2);
                }


            //Draw the matched keypoints
            var drawtype = Features2DToolbox.KeypointDrawType.DEFAULT;
            //var drawtype = Features2DToolbox.KeypointDrawType.NOT_DRAW_SINGLE_POINTS;
            //var drawtype = Features2DToolbox.KeypointDrawType.DRAW_RICH_KEYPOINTS;
            Image<Bgr, Byte> result = Features2DToolbox.DrawMatches(model.Image, model.POIs, obsvd.Image, obsvd.POIs,
               indices, new Bgr(Color.Red), new Bgr(Color.Magenta), mask, drawtype);

            #region draw the projected region on the image
            if (homography != null)
            {  //draw a rectangle along the projected model
                Rectangle rect = model.Image.ROI;
                PointF[] pts = new PointF[] {
               new PointF(rect.Left, rect.Bottom),
               new PointF(rect.Right, rect.Bottom),
               new PointF(rect.Right, rect.Top),
               new PointF(rect.Left, rect.Top)};
                homography.ProjectPoints(pts);

                result.DrawPolyline(Array.ConvertAll<PointF, Point>(pts, Point.Round), true, new Bgr(Color.Blue), 1);
            }
            #endregion

            return result;
        }




        /// <summary>
        /// THIS iS THE ORIGINAL SOURCE CODE USED TO UNDERSTAND THE ALGORITHM
        /// Draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="modelImage">The model image</param>
        /// <param name="observedImage">The observed image</param>
        /// <param name="matchTime">The output total time for computing the homography matrix.</param>
        /// <returns>The model image and observed image, the matched features and homography projection.</returns>
        public static Image<Bgr, Byte> Draw(Image<Gray, Byte> modelImage, Image<Gray, byte> observedImage, out long matchTime)
        {
            Stopwatch watch;
            HomographyMatrix homography = null;

            SURFDetector surfCPU = new SURFDetector(500, false);
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            Matrix<int> indices;

            Matrix<byte> mask;
            int k = 2;
            double uniquenessThreshold = 0.8;
            //if (GpuInvoke.HasCuda)
            if (false)
            {
                GpuSURFDetector surfGPU = new GpuSURFDetector(surfCPU.SURFParams, 0.01f);
                using (GpuImage<Gray, Byte> gpuModelImage = new GpuImage<Gray, byte>(modelImage))
                //extract features from the object image
                using (GpuMat<float> gpuModelKeyPoints = surfGPU.DetectKeyPointsRaw(gpuModelImage, null))
                using (GpuMat<float> gpuModelDescriptors = surfGPU.ComputeDescriptorsRaw(gpuModelImage, null, gpuModelKeyPoints))
                using (GpuBruteForceMatcher<float> matcher = new GpuBruteForceMatcher<float>(DistanceType.L2))
                {
                    modelKeyPoints = new VectorOfKeyPoint();
                    surfGPU.DownloadKeypoints(gpuModelKeyPoints, modelKeyPoints);
                    watch = Stopwatch.StartNew();

                    // extract features from the observed image
                    using (GpuImage<Gray, Byte> gpuObservedImage = new GpuImage<Gray, byte>(observedImage))
                    using (GpuMat<float> gpuObservedKeyPoints = surfGPU.DetectKeyPointsRaw(gpuObservedImage, null))
                    using (GpuMat<float> gpuObservedDescriptors = surfGPU.ComputeDescriptorsRaw(gpuObservedImage, null, gpuObservedKeyPoints))
                    using (GpuMat<int> gpuMatchIndices = new GpuMat<int>(gpuObservedDescriptors.Size.Height, k, 1, true))
                    using (GpuMat<float> gpuMatchDist = new GpuMat<float>(gpuObservedDescriptors.Size.Height, k, 1, true))
                    using (GpuMat<Byte> gpuMask = new GpuMat<byte>(gpuMatchIndices.Size.Height, 1, 1))
                    using (Emgu.CV.GPU.Stream stream = new Emgu.CV.GPU.Stream())
                    {
                        matcher.KnnMatchSingle(gpuObservedDescriptors, gpuModelDescriptors, gpuMatchIndices, gpuMatchDist, k, null, stream);
                        indices = new Matrix<int>(gpuMatchIndices.Size);
                        mask = new Matrix<byte>(gpuMask.Size);

                        //gpu implementation of voteForUniquess
                        using (GpuMat<float> col0 = gpuMatchDist.Col(0))
                        using (GpuMat<float> col1 = gpuMatchDist.Col(1))
                        {
                            GpuInvoke.Multiply(col1, new MCvScalar(uniquenessThreshold), col1, stream);
                            GpuInvoke.Compare(col0, col1, gpuMask, CMP_TYPE.CV_CMP_LE, stream);
                        }

                        observedKeyPoints = new VectorOfKeyPoint();
                        surfGPU.DownloadKeypoints(gpuObservedKeyPoints, observedKeyPoints);

                        //wait for the stream to complete its tasks
                        //We can perform some other CPU intesive stuffs here while we are waiting for the stream to complete.
                        stream.WaitForCompletion();

                        gpuMask.Download(mask);
                        gpuMatchIndices.Download(indices);

                        if (GpuInvoke.CountNonZero(gpuMask) >= 4)
                        {
                            int nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, indices, mask, 1.5, 20);
                            if (nonZeroCount >= 4)
                                homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, indices, mask, 2);
                        }

                        watch.Stop();
                    }
                }
            }
            else
            {
                watch = Stopwatch.StartNew();

                //extract features from the object image
                modelKeyPoints = surfCPU.DetectKeyPointsRaw(modelImage, null);
                if (modelKeyPoints.Size == 0)
                {
                    LoggedConsole.WriteLine("WARNING: There are no Key Points Of Interest in the Model/Query image");
                    watch.Stop();
                    matchTime = watch.ElapsedMilliseconds;
                    return null;
                }
                Matrix<float> modelDescriptors = surfCPU.ComputeDescriptorsRaw(modelImage, null, modelKeyPoints);

                watch = Stopwatch.StartNew();

                // extract features from the observed image
                observedKeyPoints = surfCPU.DetectKeyPointsRaw(observedImage, null);
                Matrix<float> observedDescriptors = surfCPU.ComputeDescriptorsRaw(observedImage, null, observedKeyPoints);
                BruteForceMatcher<float> matcher = new BruteForceMatcher<float>(DistanceType.L2);
                matcher.Add(modelDescriptors);

                indices = new Matrix<int>(observedDescriptors.Rows, k);
                using (Matrix<float> dist = new Matrix<float>(observedDescriptors.Rows, k))
                {
                    matcher.KnnMatch(observedDescriptors, indices, dist, k, null);
                    mask = new Matrix<byte>(dist.Rows, 1);
                    mask.SetValue(255);
                    Features2DToolbox.VoteForUniqueness(dist, uniquenessThreshold, mask);
                }

                int nonZeroCount = CvInvoke.cvCountNonZero(mask);
                if (nonZeroCount >= 4)
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, indices, mask, 1.5, 20);
                    if (nonZeroCount >= 4)
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, indices, mask, 2);
                }

                watch.Stop();
            }

            //Draw the matched keypoints
            var drawtype = Features2DToolbox.KeypointDrawType.DEFAULT;
            //var drawtype = Features2DToolbox.KeypointDrawType.NOT_DRAW_SINGLE_POINTS;
            Image<Bgr, Byte> result = Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                                                                    indices, new Bgr(Color.Red), new Bgr(Color.Magenta), mask, drawtype);

            #region draw the projected region on the image
            if (homography != null)
            {  //draw a rectangle along the projected model
                Rectangle rect = modelImage.ROI;
                PointF[] pts = new PointF[] {
               new PointF(rect.Left, rect.Bottom),
               new PointF(rect.Right, rect.Bottom),
               new PointF(rect.Right, rect.Top),
               new PointF(rect.Left, rect.Top)};
                homography.ProjectPoints(pts);

                result.DrawPolyline(Array.ConvertAll<PointF, Point>(pts, Point.Round), true, new Bgr(Color.Blue), 1);
            }
            #endregion

            matchTime = watch.ElapsedMilliseconds;

            return result;
        }
*/
    }
}
