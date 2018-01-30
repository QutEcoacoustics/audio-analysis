namespace Dong.Felt.SpectrogramDrawing
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using Configuration;
    using Preprocessing;
    using Representations;
    using ResultsOutput;

    public class DrawSpectrogram
    {
        public static Image DrawSonogram(BaseSonogram sonogram, List<double> scores, List<AcousticEvent> acousticEvent,
            double eventThreshold, List<PointOfInterest> poiList)
        {
            bool doHighlightSubband = true; bool add1kHzLines = false;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            //image.AddTrack(ImageTrack.GetSimilarityScoreTrack(scores.ToArray(), 0.0, 0.0, 0.0, 0));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));
            if ((acousticEvent != null) && (acousticEvent.Count > 0))
            {
                image.AddEvents(acousticEvent, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }
            return image.GetImage();
        } //DrawSonogram()

        public static Image DrawRankingSonogram(BaseSonogram sonogram, List<double> scores, string s, string outputFilePath)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(ImageTrack.GetSimilarityScoreTrack(scores.ToArray(), 0.0, scores.Max(), 0.0, 13));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));

            return image.GetImage();
        } //DrawSonogram()

        public static Image DrawImageLeftIndicator(Image image, string s)
        {
            var bmp = new Bitmap(image);
            RectangleF rectf = new RectangleF(50, 8, 100, 100);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawString(s, new Font("Tahoma", 20, FontStyle.Bold), Brushes.Black, rectf);
            g.Flush();
            return bmp;
        }

        public static Image DrawQueryBoundary(Image image)
        {
            var bmp = new Bitmap(image);
            var rect = new Rectangle(0, 0, 5, 5);
            Graphics g = Graphics.FromImage(bmp);
            var pen = new Pen(Color.Cyan);
            g.DrawRectangle(pen, rect);
            g.Flush();
            return bmp;
        }

        public static Image DrawVerticalLine(Image image)
        {
            var bmp = new Bitmap(image);
            Graphics g = Graphics.FromImage(bmp);
            var brush = new SolidBrush(Color.Black);
            var rect = new Rectangle(0, 0, 3, image.Height);
            g.FillRectangle(brush, rect);
            g.Flush();
            return bmp;
        }

        public static void DrawDFTImage(string outputImagePath, double[,] imageData, Bitmap bitmap)
        {
            //imageData = MatrixTools.NormaliseMatrixValues(imageData);

            for (var i = 0; i < imageData.GetLength(0); i++)
            {
                for (var j = 0; j < imageData.GetLength(1); j++)
                {
                    var color = Color.White;
                    if (imageData[i, j] > 0.0)
                    {
                        double v = imageData[i, j];
                        // int R = (int)(255 * v);
                        int R = (int)v;
                        if (R > 255) R = 255;
                        color = Color.FromArgb(R, R, R);
                    }
                    bitmap.SetPixel(j, i, color);
                }
            }
            var image = (Image)bitmap;
            image.Save(outputImagePath);
        }

        public static Image DrawFileName(Image image, Candidates candidate)
        {
            double similarityScore = candidate.Score;
            var audioFilePath = new FileInfo(candidate.SourceFilePath);
            var audioFileName = audioFilePath.Name;
            var bmp = new Bitmap(image);
            var height = image.Height;
            RectangleF rectf1 = new RectangleF(10, height - 39, 260, 70);
            RectangleF rectf2 = new RectangleF(10, height - 15, 70, 30);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawString(similarityScore.ToString(), new Font("Tahoma", 7, FontStyle.Bold), Brushes.Black, rectf2);
            //g.DrawString(audioFileName, new Font("Tahoma", 7, FontStyle.Bold), Brushes.Black, rectf1);
            g.Flush();
            return bmp;
        }

        public static Bitmap DrawFrequencyIndicator(Bitmap bitmap, List<double> frequencyBands, double herzScale, double nyquistFrequency, int frameOffset)
        {
            var i = 0;
            foreach (var f in frequencyBands)
            {
                var y = (int)((nyquistFrequency - f) / herzScale);
                int x = i * frameOffset;
                bitmap.SetPixel(x, y, Color.Red);
                i++;
            }
            return bitmap;
        }

        public static Image DrawNullSonogram(BaseSonogram sonogram)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));
            var intensityData = sonogram.Data;
            var rowsCount = intensityData.GetLength(0);
            var colsCount = intensityData.GetLength(1);
            for (int rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < colsCount; colIndex++)
                {
                    sonogram.Data[rowIndex, colIndex] = 0.0;
                }
            }
            return image.GetImage();
        } //DrawSonogram()

        /// <summary>
        /// stacks the passed images one on top of the other. Assum that all images have the same width.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static Image CombineImagesHorizontally(Image[] array)
        {
            var compositeWidth = 0;
            var height = 0;
            if (array != null)
            {
                int width = array[0].Width;   // assume all images have the same width
                height = array[0].Height; // assume all images have the same height

                for (int i = 0; i < array.Length; i++)
                {
                    compositeWidth += array[i].Width;
                }
            }
            Bitmap compositeBmp = new Bitmap(compositeWidth, height, PixelFormat.Format24bppRgb);
            int xOffset = 0;
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);

            for (int i = 0; i < array.Length; i++)
            {
                gr.DrawImage(array[i], xOffset, 0); //draw in the top spectrogram
                xOffset += array[i].Width;
            }
            return (Image)compositeBmp;
        }

        /// <summary>
        /// Change poi spectrogram into black and white image and just show the poi on the spectrogram.
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="poiList"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public static double[,] ShowPOIOnSpectrogram(SpectrogramStandard spectrogram, List<PointOfInterest> poiList, int rows, int cols)
        {
            foreach (var poi in poiList)
            {
                /// This one is for ridge detection
                var xCoordinate = poi.Point.Y;
                var yCoordinate = poi.Point.X;
                ///This one is for structure tensor
                //var xCoordinate = poi.Point.X;
                //var yCoordinate = poi.Point.Y;
                //if (xCoordinate >= rows)
                //{
                //    xCoordinate = rows -1;
                //}
                //if (yCoordinate >= cols)
                //{
                //    yCoordinate = cols - 1;
                //}
                spectrogram.Data[yCoordinate, cols - xCoordinate-1] = 20.0;
            }
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (spectrogram.Data[r, c] == 20.0)
                    {
                        spectrogram.Data[r, c] = 1.0;
                    }
                    else
                    {
                        spectrogram.Data[r, c] = 0.0;
                    }
                }
            }
            return spectrogram.Data;
        }

        public static void DrawingCandiOutputStSpectrogram(string candidateCsvFilePath, string queryCsvFilePath, string queryAudioFilePath,
            string outputPath, int rank, StructureTensorConfiguration stConfig, SonogramConfig config, string featurePropSet, DirectoryInfo tempDirectory)
        {
            var candidateFilePathInfo = new FileInfo(candidateCsvFilePath);
            var candidateDirectory = candidateFilePathInfo.DirectoryName;

            var file = new FileInfo(candidateCsvFilePath);
            var candidates = CSVResults.CsvToCandidatesList(file);
            var queryCsvFile = new FileInfo(queryCsvFilePath);
            var query = new Candidates();
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            query.StartTime = queryInfo.TimeStart * 1000;
            query.EndTime = query.StartTime + queryInfo.EventDurationSeconds * 1000;
            query.MaxFrequency = queryInfo.HighFrequencyHertz;
            query.MinFrequency = queryInfo.LowFrequencyHertz;
            query.SourceFilePath = queryAudioFilePath;
            candidates.Insert(0, query);
            var querycsvFilePath = new FileInfo(queryCsvFilePath);
            var queryFileDirectory = querycsvFilePath.DirectoryName;
            var pathString = Path.Combine(tempDirectory.FullName, Path.GetFileName(queryAudioFilePath), featurePropSet);
            var outPutFileDirectory = Directory.CreateDirectory(pathString);
            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count(); i++)
                {
                    var outPutFileName = i + ".wav";
                    var outPutFilePath = Path.Combine(outPutFileDirectory.FullName, outPutFileName);
                    OutputResults.AudioSegmentBasedCandidates(candidates[i], outPutFilePath.ToFileInfo());
                }
                var listString = new List<string>();
                listString.Add("Q");
                for (int i = 0; i < rank; i++)
                {
                    int tempValue = i + 1;
                    listString.Add(tempValue.ToString());
                }
                var imageArray = DrawingStSpectFromAudios(outPutFileDirectory, config, listString, rank, candidates, stConfig).ToArray();
                var imageResult = CombineImagesHorizontally(imageArray);
                var temp = new FileInfo(candidates[0].SourceFilePath);
                var imageOutputName = featurePropSet + temp.Name + "Combined image.png";
                var imagePath = outputPath + imageOutputName;
                imageResult.Save(imagePath, ImageFormat.Png);
            }
        }

        public static List<Image> DrawingStSpectFromAudios(DirectoryInfo audioFileDirectory, SonogramConfig config, List<string> s, int rank,
       List<Candidates> candidates, StructureTensorConfiguration stConfig)
        {
            var result = new List<Image>();
            if (!Directory.Exists(audioFileDirectory.FullName))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", audioFileDirectory));
            }

            // because the result is obtained like this order, 0, 1, 2, 10, 3, 4, 5, 6, ...9
            var audioFiles = Directory.GetFiles(audioFileDirectory.FullName, @"*.wav", SearchOption.TopDirectoryOnly);
            var audioFilesCount = audioFiles.Count();
            var improvedAudioFiles = new string[audioFilesCount];
            for (int j = 0; j < audioFilesCount; j++)
            {
                var audioFileNames = Convert.ToInt32(Path.GetFileNameWithoutExtension(audioFiles[j]));
                if (audioFileNames != j)
                {
                    improvedAudioFiles[audioFileNames] = audioFiles[j];
                }
                else
                {
                    improvedAudioFiles[j] = audioFiles[j];
                }
            }

            for (int i = 0; i < rank + 1; i++)
            {
                /// because the query always come from first place.
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, improvedAudioFiles[i]);
                var structuretensors = StructureTensorAnalysis.ExtractPOIFromStructureTensor(spectrogram, stConfig.AvgStNhLength, stConfig.Threshold);
                /// To show the ridges on the spectrogram.
                var scores = new List<double>();
                scores.Add(0.0);
                double eventThreshold = 0.5; // dummy variable - not used
                var startTime = 1.0;
                var secondToMilliSecond = 1000.0;
                var duration = (candidates[i].EndTime - candidates[i].StartTime) / secondToMilliSecond;
                var endTime = candidates[i].EndTime / secondToMilliSecond;
                if (candidates[i].StartTime / secondToMilliSecond < 1)
                {
                    startTime = candidates[i].StartTime / secondToMilliSecond;
                }
                if (endTime > 59)
                {
                    //startTime = startTime + 60 - endTime;
                    startTime = (candidates[i].StartTime - candidates[i].EndTime) / secondToMilliSecond + 2;
                }
                endTime = startTime + duration;
                if (i == 0)
                {
                    var acousticEventlistForQuery = new List<AcousticEvent>();

                    var queryAcousticEvent = new AcousticEvent(TimeSpan.Zero, startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);
                    queryAcousticEvent.BorderColour = Color.Crimson;
                    acousticEventlistForQuery.Add(queryAcousticEvent);
                    Image image = DrawSonogram(spectrogram, scores, acousticEventlistForQuery, eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in structuretensors)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    image = (Image)bmp;
                    var seperatedImage = DrawVerticalLine(image);
                    var improvedImage = DrawImageLeftIndicator(seperatedImage, s[i]);
                    var finalImage = DrawFileName(improvedImage, candidates[i]);
                    result.Add(finalImage);
                }
                else
                {
                    var acousticEventlistForCandidate = new List<AcousticEvent>();
                    var candAcousticEvent = new AcousticEvent(TimeSpan.Zero, startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);
                    candAcousticEvent.BorderColour = Color.Green;
                    acousticEventlistForCandidate.Add(candAcousticEvent);
                    Image image = DrawSonogram(spectrogram, scores, acousticEventlistForCandidate, eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in structuretensors)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    image = (Image)bmp;
                    var seperatedImage = DrawVerticalLine(image);
                    var improvedImage = DrawImageLeftIndicator(seperatedImage, s[i]);
                    var finalImage = DrawFileName(improvedImage, candidates[i]);
                    result.Add(finalImage);
                }
            }
            return result;
        }

        /// <summary>
        /// Drawing Candidate spectrogram.
        /// </summary>
        /// <param name="candidateCsvFilePath"></param>
        /// <param name="queryCsvFilePath"></param>
        /// <param name="queryAudioFilePath"></param>
        /// <param name="outputPath"></param>
        /// <param name="rank"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="config"></param>
        /// <param name="featurePropSet"></param>
        /// <param name="tempDirectory"></param>
        public static void DrawingCandiOutputSpectrogram(string candidateCsvFilePath, string queryCsvFilePath, string queryAudioFilePath,
            string outputPath, int rank, RidgeDetectionConfiguration ridgeConfig, SonogramConfig config,
            CompressSpectrogramConfig compressConfig,
            string featurePropSet, DirectoryInfo tempDirectory)
        {
            var candidateFilePathInfo = new FileInfo(candidateCsvFilePath);
            var candidateDirectory = candidateFilePathInfo.DirectoryName;

            var file = new FileInfo(candidateCsvFilePath);
            var candidates = CSVResults.CsvToCandidatesList(file);
            var queryCsvFile = new FileInfo(queryCsvFilePath);
            var query = new Candidates();
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            query.StartTime = queryInfo.TimeStart * 1000;
            query.EndTime = query.StartTime + queryInfo.EventDurationSeconds * 1000;
            query.MaxFrequency = queryInfo.HighFrequencyHertz;
            query.MinFrequency = queryInfo.LowFrequencyHertz;
            query.SourceFilePath = queryAudioFilePath;
            candidates.Insert(0, query);
            var querycsvFilePath = new FileInfo(queryCsvFilePath);
            var queryFileDirectory = querycsvFilePath.DirectoryName;
            var pathString = Path.Combine(tempDirectory.FullName, Path.GetFileName(queryAudioFilePath), featurePropSet);
            var outPutFileDirectory = Directory.CreateDirectory(pathString);
            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count(); i++)
                {
                    var outPutFileName = i + ".wav";
                    var outPutFilePath = Path.Combine(outPutFileDirectory.FullName, outPutFileName);
                    OutputResults.AudioSegmentBasedCandidates(candidates[i], outPutFilePath.ToFileInfo());
                }
                var listString = new List<string>();
                listString.Add("Q");
                for (int i = 0; i < rank; i++)
                {
                    int tempValue = i + 1;
                    listString.Add(tempValue.ToString());
                }
                var imageArray = DrawingSpectrogramsFromAudios(outPutFileDirectory, config, listString, rank,
                    candidates, ridgeConfig, compressConfig).ToArray();
                var imageResult = CombineImagesHorizontally(imageArray);
                var temp = new FileInfo(candidates[0].SourceFilePath);
                var imageOutputName = featurePropSet + temp.Name + "Combined image.png";
                var imagePath = outputPath + imageOutputName;
                imageResult.Save(imagePath, ImageFormat.Png);
            }
        }

        /// <summary>
        /// Drawing Candidate spectrogram.
        /// </summary>
        /// <param name="candidateCsvFilePath"></param>
        /// <param name="queryCsvFilePath"></param>
        /// <param name="queryAudioFilePath"></param>
        /// <param name="outputPath"></param>
        /// <param name="rank"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="config"></param>
        /// <param name="featurePropSet"></param>
        /// <param name="tempDirectory"></param>
        public static void DrawingCandiOutputSpectrogram(string candidateCsvFilePath, string queryCsvFilePath, string queryAudioFilePath,
            string outputPath, int rank, RidgeDetectionConfiguration ridgeConfig, SonogramConfig config,
            string featurePropSet, DirectoryInfo tempDirectory)
        {
            var candidateFilePathInfo = new FileInfo(candidateCsvFilePath);
            var candidateDirectory = candidateFilePathInfo.DirectoryName;

            var file = new FileInfo(candidateCsvFilePath);
            var candidates = CSVResults.CsvToCandidatesList(file);
            var queryCsvFile = new FileInfo(queryCsvFilePath);
            var query = new Candidates();
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            query.StartTime = queryInfo.TimeStart * 1000;
            query.EndTime = query.StartTime + queryInfo.EventDurationSeconds * 1000;
            query.MaxFrequency = queryInfo.HighFrequencyHertz;
            query.MinFrequency = queryInfo.LowFrequencyHertz;
            query.SourceFilePath = queryAudioFilePath;
            candidates.Insert(0, query);
            var querycsvFilePath = new FileInfo(queryCsvFilePath);
            var queryFileDirectory = querycsvFilePath.DirectoryName;
            var pathString = Path.Combine(tempDirectory.FullName, Path.GetFileName(queryAudioFilePath), featurePropSet);
            var outPutFileDirectory = Directory.CreateDirectory(pathString);
            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count(); i++)
                {
                    var outPutFileName = i + ".wav";
                    var outPutFilePath = Path.Combine(outPutFileDirectory.FullName, outPutFileName);
                    OutputResults.AudioSegmentBasedCandidates(candidates[i], outPutFilePath.ToFileInfo());
                }
                var listString = new List<string>();
                listString.Add("Q");
                for (int i = 0; i < rank; i++)
                {
                    int tempValue = i + 1;
                    listString.Add(tempValue.ToString());
                }
                var imageArray = DrawingSpectrogramsFromAudios(outPutFileDirectory, config, listString, rank,
                    candidates, ridgeConfig).ToArray();
                var imageResult = CombineImagesHorizontally(imageArray);
                var temp = new FileInfo(candidates[0].SourceFilePath);
                var imageOutputName = featurePropSet + temp.Name + "Combined image.png";
                var imagePath = outputPath + imageOutputName;
                imageResult.Save(imagePath, ImageFormat.Png);
            }
        }

        /// <summary>
        ///  This version is made for drawing spectrogram for candidates generated by MFCC
        /// </summary>
        /// <param name="candidateCsvFilePath"></param>
        /// <param name="outputPath"></param>
        /// <param name="rank"></param>
        /// <param name="config"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="tempDirectory"></param>
        public static void DrawingCandiOutputSpectrogram(string candidateCsvFilePath, string audioFilePath,
            string inputPath, int rank, SonogramConfig config, RidgeDetectionConfiguration ridgeConfig,
            DirectoryInfo tempDirectory)
        {
            var file = new FileInfo(candidateCsvFilePath);
            var pathString = Path.Combine(tempDirectory.FullName, file.Name);
            var outPutFileDirectory = Directory.CreateDirectory(pathString);

            var candidates = CSVResults.CsvToCandidatesList(file);
            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count(); i++)
                {
                    var outPutFileName = i + ".wav";
                    var outPutFilePath = Path.Combine(outPutFileDirectory.FullName, outPutFileName);
                    var audioSourceFilePath = Path.Combine(audioFilePath, candidates[i].SourceFilePath);
                    candidates[i].SourceFilePath = audioSourceFilePath;
                    OutputResults.AudioSegmentBasedCandidates(candidates[i], outPutFilePath.ToFileInfo());
                }
                var listString = new List<string>();
                for (int i = 0; i < candidates.Count(); i++)
                {
                    int tempValue = i + 1;
                    listString.Add(tempValue.ToString());
                }
                var imageArray = DrawingSpectrogramsFromAudios(outPutFileDirectory, config, listString, rank,
                    candidates, ridgeConfig).ToArray();
                var imageResult = CombineImagesHorizontally(imageArray);
                var imageOutputName = file.Name + "Combined image.png";
                var imagePath = Path.Combine(inputPath, imageOutputName);
                imageResult.Save(imagePath, ImageFormat.Png);
            }
        }

        public static void DrawingOutputSpectrogram(string candidateCsvFilePath, string queryCsvFilePath, string queryAudioFilePath,
            string outputPath, int rank, RidgeDetectionConfiguration ridgeConfig, SonogramConfig config, CompressSpectrogramConfig compressConfig,
            string featurePropSet, DirectoryInfo tempDirectory)
        {
            var candidateFilePathInfo = new FileInfo(candidateCsvFilePath);
            var candidateDirectory = candidateFilePathInfo.DirectoryName;

            var file = new FileInfo(candidateCsvFilePath);
            var candidates = CSVResults.CsvToCandidatesList(file);
            var queryCsvFile = new FileInfo(queryCsvFilePath);
            var query = new Candidates();
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            query.StartTime = queryInfo.TimeStart * 1000;
            query.EndTime = query.StartTime + queryInfo.EventDurationSeconds * 1000;
            query.MaxFrequency = queryInfo.HighFrequencyHertz;
            query.MinFrequency = queryInfo.LowFrequencyHertz;
            query.SourceFilePath = queryAudioFilePath;
            candidates.Insert(0, query);
            var pathString = Path.Combine(tempDirectory.FullName, Path.GetFileName(queryAudioFilePath), featurePropSet);
            var outPutFileDirectory = Directory.CreateDirectory(pathString);
            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count(); i++)
                {
                    var outPutFileName = i + ".wav";
                    var outPutFilePath = Path.Combine(outPutFileDirectory.FullName, outPutFileName);
                    OutputResults.AudioSegmentBasedCandidates(candidates[i], outPutFilePath.ToFileInfo());
                }
                var listString = new List<string>();
                listString.Add("Q");
                for (int i = 0; i < rank; i++)
                {
                    int tempValue = i + 1;
                    listString.Add(tempValue.ToString());
                }
                var imageArray = DrawingSpectrogramsFromAudios(outPutFileDirectory, config, listString, rank,
                    candidates, ridgeConfig, compressConfig, featurePropSet).ToArray();
                var imageResult = CombineImagesHorizontally(imageArray);
                var temp = new FileInfo(candidates[0].SourceFilePath);
                var imageOutputName = featurePropSet + temp.Name + "Combined image.png";
                var imagePath = outputPath + imageOutputName;
                imageResult.Save(imagePath, ImageFormat.Png);
            }
        }

        /// <summary>
        /// Drawing combined spectrogram from a buntch of audio. Especially designed for xueyan's similarity search algorithm.
        /// </summary>
        /// <param name="audioFileDirectory"></param>
        /// <param name="config"></param>
        /// <param name="s"></param>
        /// <param name="rank"></param>
        /// <param name="candidates"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="compressConfig"></param>
        /// <returns></returns>
        public static List<Image> DrawingSpectrogramsFromAudios(DirectoryInfo audioFileDirectory, SonogramConfig config, List<string> s, int rank,
            List<Candidates> candidates, RidgeDetectionConfiguration ridgeConfig)
        {
            var result = new List<Image>();
            if (!Directory.Exists(audioFileDirectory.FullName))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", audioFileDirectory));
            }

            // because the result is obtained like this order, 0, 1, 2, 10, 3, 4, 5, 6, ...9
            var audioFiles = Directory.GetFiles(audioFileDirectory.FullName, @"*.wav", SearchOption.TopDirectoryOnly);
            var audioFilesCount = audioFiles.Count();
            var improvedAudioFiles = new string[audioFilesCount];
            for (int j = 0; j < audioFilesCount; j++)
            {
                var audioFileNames = Convert.ToInt32(Path.GetFileNameWithoutExtension(audioFiles[j]));
                if (audioFileNames != j)
                {
                    improvedAudioFiles[audioFileNames] = audioFiles[j];
                }
                else
                {
                    improvedAudioFiles[j] = audioFiles[j];
                }
            }

            // to do : modify the rank to rank + 1 after MFCC calculation
            for (int i = 0; i <= rank; i++)
            {
                /// because the query always come from first place.
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, improvedAudioFiles[i]);
                /// To show the ridges on the spectrogram.
                var ridges = POISelection.PostRidgeDetection4Dir(spectrogram, ridgeConfig);
                var scores = new List<double>();
                scores.Add(0.0);
                double eventThreshold = 0.5; // dummy variable - not used
                var startTime = 1.0;
                var secondToMilliSecond = 1000.0;
                var duration = (candidates[i].EndTime - candidates[i].StartTime) / secondToMilliSecond;
                var endTime = candidates[i].EndTime / secondToMilliSecond;
                if (candidates[i].StartTime / secondToMilliSecond < 1)
                {
                    startTime = candidates[i].StartTime / secondToMilliSecond;
                }
                if (endTime > 59)
                {
                    //startTime = startTime + 60 - endTime;
                    startTime = (candidates[i].StartTime - candidates[i].EndTime) / secondToMilliSecond + 2;
                }
                endTime = startTime + duration;
                if (i == 0)
                {
                    var acousticEventlistForQuery = new List<AcousticEvent>();
                    var queryAcousticEvent = new AcousticEvent(TimeSpan.Zero, startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);

                    queryAcousticEvent.BorderColour = Color.Crimson;
                    acousticEventlistForQuery.Add(queryAcousticEvent);
                    scores.Add(candidates[i].Score);
                    Image image = DrawSonogram(spectrogram, scores, acousticEventlistForQuery,
                        eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in ridges)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    image = (Image)bmp;
                    var seperatedImage = DrawVerticalLine(image);
                    var improvedImage = DrawImageLeftIndicator(seperatedImage, s[i]);
                    var finalImage = DrawFileName(improvedImage, candidates[i]);
                    result.Add(finalImage);
                }
                else
                {
                    var acousticEventlistForCandidate = new List<AcousticEvent>();
                    var candAcousticEvent = new AcousticEvent(TimeSpan.Zero, startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);
                    candAcousticEvent.BorderColour = Color.Green;
                    acousticEventlistForCandidate.Add(candAcousticEvent);
                    scores.Add(candidates[i].Score);
                    Image image = DrawSonogram(spectrogram, scores, acousticEventlistForCandidate,
                        eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in ridges)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    image = (Image)bmp;
                    var seperatedImage = DrawVerticalLine(image);
                    var improvedImage = DrawImageLeftIndicator(seperatedImage, s[i]);
                    var finalImage = DrawFileName(improvedImage, candidates[i]);
                    result.Add(finalImage);
                }
            }
            return result;
        }

        static List<Image> DrawingSpectrogramsFromAudios(DirectoryInfo audioFileDirectory, SonogramConfig config, List<string> s, int rank,
            List<Candidates> candidates, RidgeDetectionConfiguration ridgeConfig, CompressSpectrogramConfig compressConfig,
            string featurePropSet)
        {
            var result = new List<Image>();
            if (!Directory.Exists(audioFileDirectory.FullName))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", audioFileDirectory));
            }

            // because the result is obtained like this order, 0, 1, 2, 10, 3, 4, 5, 6, ...9
            var audioFiles = Directory.GetFiles(audioFileDirectory.FullName, @"*.wav", SearchOption.TopDirectoryOnly);
            var audioFilesCount = audioFiles.Count();
            var improvedAudioFiles = new string[audioFilesCount];
            for (int j = 0; j < audioFilesCount; j++)
            {
                var audioFileNames = Convert.ToInt32(Path.GetFileNameWithoutExtension(audioFiles[j]));
                if (audioFileNames != j)
                {
                    improvedAudioFiles[audioFileNames] = audioFiles[j];
                }
                else
                {
                    improvedAudioFiles[j] = audioFiles[j];
                }
            }

            for (int i = 0; i < rank + 1; i++)
            {
                /// because the query always come from first place.
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, improvedAudioFiles[i]);
                var ridgeMatrix = POISelection.ModifiedRidgeDetection(spectrogram, config, ridgeConfig, compressConfig, improvedAudioFiles[i],
                   featurePropSet);
                var ridges = StatisticalAnalysis.TransposeMatrixToPOIlist(ridgeMatrix);
                /// To show the ridges on the spectrogram.
                var scores = new List<double>();
                scores.Add(0.0);
                double eventThreshold = 0.5; // dummy variable - not used
                var startTime = 1.0;
                var secondToMilliSecond = 1000.0;
                var duration = (candidates[i].EndTime - candidates[i].StartTime) / secondToMilliSecond;
                var endTime = candidates[i].EndTime / secondToMilliSecond;
                if (candidates[i].StartTime / secondToMilliSecond < 1)
                {
                    startTime = candidates[i].StartTime / secondToMilliSecond;
                }
                if (endTime > 59)
                {
                    startTime = (candidates[i].StartTime - candidates[i].EndTime) / secondToMilliSecond + 2;
                }
                endTime = startTime + duration;
                var eventList = new List<AcousticEvent>();
                if (i == 0)
                {
                    var queryAcousticEvent = new AcousticEvent(TimeSpan.Zero, startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);

                    queryAcousticEvent.BorderColour = Color.Crimson;
                    eventList.Add(queryAcousticEvent);
                }
                else
                {
                    var candAcousticEvent = new AcousticEvent(TimeSpan.Zero, startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);
                    candAcousticEvent.BorderColour = Color.Green;
                    eventList.Add(candAcousticEvent);
                }
                Image image = DrawSonogram(spectrogram, scores, eventList,
                        eventThreshold, null);
                Bitmap bmp = (Bitmap)image;
                foreach (PointOfInterest poi in ridges)
                {
                    poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                }
                image = (Image)bmp;
                var seperatedImage = DrawVerticalLine(image);
                var improvedImage = DrawImageLeftIndicator(seperatedImage, s[i]);
                var finalImage = DrawFileName(improvedImage, candidates[i]);
                result.Add(finalImage);


            }
            return result;
        }
        /// <summary>
        /// Drawing combined spectrogram from a buntch of audio. Especially designed for xueyan's similarity search algorithm.
        /// </summary>
        /// <param name="audioFileDirectory"></param>
        /// <param name="config"></param>
        /// <param name="s"></param>
        /// <param name="rank"></param>
        /// <param name="candidates"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="compressConfig"></param>
        /// <returns></returns>
        public static List<Image> DrawingSpectrogramsFromAudios(DirectoryInfo audioFileDirectory, SonogramConfig config, List<string> s, int rank,
            List<Candidates> candidates, RidgeDetectionConfiguration ridgeConfig, CompressSpectrogramConfig compressConfig)
        {
            var result = new List<Image>();
            if (!Directory.Exists(audioFileDirectory.FullName))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", audioFileDirectory));
            }

            // because the result is obtained like this order, 0, 1, 2, 10, 3, 4, 5, 6, ...9
            var audioFiles = Directory.GetFiles(audioFileDirectory.FullName, @"*.wav", SearchOption.TopDirectoryOnly);
            var audioFilesCount = audioFiles.Count();
            var improvedAudioFiles = new string[audioFilesCount];
            for (int j = 0; j < audioFilesCount; j++)
            {
                var audioFileNames = Convert.ToInt32(Path.GetFileNameWithoutExtension(audioFiles[j]));
                if (audioFileNames != j)
                {
                    improvedAudioFiles[audioFileNames] = audioFiles[j];
                }
                else
                {
                    improvedAudioFiles[j] = audioFiles[j];
                }
            }

            for (int i = 0; i < rank + 1; i++)
            {
                /// because the query always come from first place.
                var spectrogram = AudioPreprosessing.AudioToSpectrogram(config, improvedAudioFiles[i]);
                if (compressConfig.TimeCompressRate != 1.0)
                {
                    spectrogram.Data = AudioPreprosessing.CompressSpectrogramInTime(spectrogram.Data, compressConfig.TimeCompressRate);
                }
                else
                {
                    if (compressConfig.FreqCompressRate != 1.0)
                    {
                        spectrogram.Data = AudioPreprosessing.CompressSpectrogramInFreq(spectrogram.Data, compressConfig.FreqCompressRate);
                    }
                }
                var ridges = POISelection.PostRidgeDetection4Dir(spectrogram, ridgeConfig);
                //var ridges = POISelection.PostRidgeDetection8Dir(spectrogram, ridgeConfig);
                /// To show the ridges on the spectrogram.
                var scores = new List<double>();
                scores.Add(0.0);
                double eventThreshold = 0.5; // dummy variable - not used
                var startTime = 1.0 * compressConfig.TimeCompressRate;
                var secondToMilliSecond = 1000.0;
                var duration = (candidates[i].EndTime - candidates[i].StartTime) / secondToMilliSecond;
                var endTime = candidates[i].EndTime / secondToMilliSecond;
                if (candidates[i].StartTime / secondToMilliSecond < 1)
                {
                    startTime = candidates[i].StartTime / secondToMilliSecond;
                }
                if (endTime > 59)
                {
                    //startTime = startTime + 60 - endTime;
                    startTime = (candidates[i].StartTime - candidates[i].EndTime) / secondToMilliSecond + 2;
                }
                endTime = startTime + duration;
                if (i == 0)
                {
                    var acousticEventlistForQuery = new List<AcousticEvent>();

                    var queryAcousticEvent = new AcousticEvent(TimeSpan.Zero, startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);
                    queryAcousticEvent.HighFrequencyHertz = queryAcousticEvent.HighFrequencyHertz * compressConfig.FreqCompressRate;
                    queryAcousticEvent.LowFrequencyHertz = queryAcousticEvent.LowFrequencyHertz * compressConfig.FreqCompressRate;

                    queryAcousticEvent.BorderColour = Color.Crimson;
                    acousticEventlistForQuery.Add(queryAcousticEvent);
                    Image image = DrawSonogram(spectrogram, scores, acousticEventlistForQuery,
                        eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in ridges)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    image = (Image)bmp;
                    var seperatedImage = DrawVerticalLine(image);
                    var improvedImage = DrawImageLeftIndicator(seperatedImage, s[i]);
                    var finalImage = DrawFileName(improvedImage, candidates[i]);
                    result.Add(finalImage);
                }
                else
                {
                    var acousticEventlistForCandidate = new List<AcousticEvent>();
                    var candAcousticEvent = new AcousticEvent(TimeSpan.Zero, startTime, duration,
                        candidates[i].MinFrequency, candidates[i].MaxFrequency);
                    candAcousticEvent.HighFrequencyHertz = candAcousticEvent.HighFrequencyHertz * compressConfig.FreqCompressRate;
                    candAcousticEvent.LowFrequencyHertz = candAcousticEvent.LowFrequencyHertz * compressConfig.FreqCompressRate;

                    candAcousticEvent.BorderColour = Color.Green;
                    acousticEventlistForCandidate.Add(candAcousticEvent);
                    Image image = DrawSonogram(spectrogram, scores, acousticEventlistForCandidate, eventThreshold, null);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in ridges)
                    {
                        poi.DrawOrientationPoint(bmp, (int)spectrogram.Configuration.FreqBinCount);
                    }
                    image = (Image)bmp;
                    var seperatedImage = DrawVerticalLine(image);
                    var improvedImage = DrawImageLeftIndicator(seperatedImage, s[i]);
                    var finalImage = DrawFileName(improvedImage, candidates[i]);
                    result.Add(finalImage);
                }
            }
            return result;
        }


        /// <summary>
        /// Gaussian blur on amplitude spectrogram.
        /// </summary>
        /// <param name="audioFileDirectory"></param>
        /// <param name="config"></param>
        /// <param name="ridgeConfig"></param>
        /// <param name="sigma"></param>
        /// <param name="size"></param>
        public static void GaussianBlurAmplitudeSpectro(string audioFileDirectory, SonogramConfig config,
           RidgeDetectionConfiguration ridgeConfig, double sigma, int size)
        {
            if (Directory.Exists(audioFileDirectory))
            {
                var audioFiles = Directory.GetFiles(audioFileDirectory, @"*.wav", SearchOption.TopDirectoryOnly);
                var audioFilesCount = audioFiles.Count();
                for (int i = 0; i < audioFilesCount; i++)
                {
                    var sonogram = AudioPreprosessing.AudioToAmplitudeSpectrogram(config, audioFiles[i]);
                    Image image = sonogram.GetImageFullyAnnotated("AMPLITUDE SPECTROGRAM + Bin LCN (Local Contrast Normalisation)");
                    var ridges = POISelection.PostRidgeDetectionAmpSpec(sonogram, ridgeConfig);
                    var rows = sonogram.Data.GetLength(1) - 1;
                    var cols = sonogram.Data.GetLength(0);
                    var ridgeMatrix = StatisticalAnalysis.TransposePOIsToMatrix(ridges, rows, cols);
                    var gaussianBlurRidges = ClusterAnalysis.GaussianBlurOnPOI(ridgeMatrix, rows, cols, size, sigma);
                    var dividedPOIList = POISelection.POIListDivision(gaussianBlurRidges);
                    var verSegmentList = new List<List<PointOfInterest>>();
                    var horSegmentList = new List<List<PointOfInterest>>();
                    var posDiSegmentList = new List<List<PointOfInterest>>();
                    var negDiSegmentList = new List<List<PointOfInterest>>();

                    //ClusterAnalysis.ClusterRidgesToEvents(dividedPOIList[0], dividedPOIList[1], dividedPOIList[2], dividedPOIList[3],
                    //    rows, cols, ref verSegmentList, ref horSegmentList, ref posDiSegmentList, ref negDiSegmentList);
                    //var groupedRidges = ClusterAnalysis.GroupeSepRidges(verSegmentList, horSegmentList, posDiSegmentList, negDiSegmentList);
                    Bitmap bmp = (Bitmap)image;
                    foreach (PointOfInterest poi in ridges)
                    {
                        poi.DrawOrientationPoint(bmp, (int)sonogram.Configuration.FreqBinCount);
                    }
                    var FileName = new FileInfo(audioFiles[i]);
                    string annotatedImageFileName = Path.ChangeExtension(FileName.Name, "-Filtered Gaussian blur-improved.png");
                    string annotatedImagePath = Path.Combine(audioFileDirectory, annotatedImageFileName);
                    image = (Image)bmp;
                    image.Save(annotatedImagePath);
                }
            }
        }

    }
}
