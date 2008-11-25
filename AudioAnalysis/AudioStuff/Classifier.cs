using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using TowseyLib;
using AudioTools;
using System.Drawing;

namespace AudioStuff
{
    /// <summary>
    /// The classes in this file are used to scan and score a sonogram.
    /// </summary>

    /// <summary>
    /// this class scans a sonogram using a template.
    /// </summary>
    public class Recogniser
    {
        private readonly int noiseSampleCount = 5000;

        private List<Template> templates = new List<Template>(); //an array of class Template
        private SonoConfig templateConfig;
        private Sonogram currentSonogram;

		public FeatureVector[] FVs { get; private set; }

        // TEMPLATE RESULTS 
        private List<Results> resultsList = new List<Results>(); //an array of class Results
        public List<Results> ResultsList { get { return resultsList; } set { resultsList = value; } }
		public Results Result { get { return ResultsList.Count > 0 ? ResultsList[ResultsList.Count - 1] : null; } } //a set of results from current scan of this recogniser

        public Recogniser()
        {
        } 

        public Recogniser(Template t)
        {
            AddTemplate(t);
        }

        public void AddTemplate(Template t)
        {
            if (t == null) throw new ArgumentNullException("t", "Template == null in Classifier()");
			if (t.TemplateState == null) throw new ArgumentNullException("t.TemplateState", "TemplateState == null in Classifier()");
            templates.Add(t);
        }

        public void GenerateSymbolSequence()
        {
			Log.WriteIfVerbose("\n########################### Recogniser.GenerateSymbolSequence()");

            Results results = new Results(); //set up results class for this template 
			Template currentTemplate = templates[0];
			templateConfig = currentTemplate.TemplateState;
			currentSonogram = currentTemplate.Sonogram;
            if (currentSonogram == null)
            {
				Log.WriteLine("wavPath=" + templateConfig.WavFilePath);
				currentSonogram = PrepareSonogram(templateConfig.WavFilePath, templateConfig);
            }

            if (currentSonogram == null)
                throw new Exception("##### FATAL ERROR!!!! Cannot find wav file used to create the template!");

			FVs = SetFeatureVectors(currentTemplate.FeatureVectors);
			results.AcousticMatrix = GenerateAcousticMatrix(currentSonogram, GetNoiseFVPath(currentTemplate));
            AcousticMatrix2SymbolSequence(results);
            resultsList.Add(results);
        }

        public void GenerateSymbolSequence(string wavPath)
        {
            Log.WriteIfVerbose("\n########################### Recogniser.GenerateSymbolSequence()");
            Log.WriteIfVerbose("\tSonogram prepared from WAV file: " + wavPath);

            //scan sonogram with default template
            Results results = new Results(); //set up results class for this template 
			var template = templates[0];
            templateConfig = template.TemplateState;
			FVs = SetFeatureVectors(template.FeatureVectors);
            currentSonogram = PrepareSonogram(wavPath, templateConfig);

			results.AcousticMatrix = GenerateAcousticMatrix(currentSonogram, GetNoiseFVPath(template));
            AcousticMatrix2SymbolSequence(results);
            resultsList.Add(results);
        }

		private string GetNoiseFVPath(Template t)
		{
			if (t.FileName == null)
				return null;
			var noiseFVPath = Path.Combine(Path.GetDirectoryName(t.FileName), "template" + t.CallID + "_NoiseFV.txt");
			return noiseFVPath;
		}

        public List<Results> ScanAudioFileWithTemplates(string wavPath)
        {
            Log.WriteIfVerbose("  Sonogram prepared from WAV file: " + wavPath);

			var retVal = new List<Results>();
            // scan sonogram with all templates
            for (int i = 0; i < templates.Count; i++) // for each template
            {
                Log.WriteIfVerbose("\n########################### SCANNING SONOGRAM WITH TEMPLATE " + (i + 1));
				var currentTemplate = templates[i];
				templateConfig = currentTemplate.TemplateState;
                // check the MM is valid
                MarkovModel mm = this.templateConfig.WordModel;
                if (mm == null || mm.GraphType == HMMType.UNDEFINED)
                {
                    Log.WriteIfVerbose("\t##### WARNING: RECOGNISER WARNING: MARKOV MODEL " + (i + 1) + " IS NULL OR UNDEFINED!");
                    continue;
                }

				FVs = SetFeatureVectors(currentTemplate.FeatureVectors);
                this.currentSonogram = PrepareSonogram(wavPath, templateConfig); // each template requires different feature extraction

                var currentResult = new Results(this.templateConfig); // set up results class for this scan of the template 
				currentResult.AcousticMatrix = GenerateAcousticMatrix(currentSonogram, GetNoiseFVPath(currentTemplate));
				AcousticMatrix2SymbolSequence(currentResult);
				ScanSymbolSequenceWithMM(currentResult);

				resultsList.Add(currentResult);
				retVal.Add(currentResult);

                Log.WriteIfVerbose("\n########################### END OF SCANNING SONOGRAM WITH TEMPLATE " + (i + 1));
                Log.WriteIfVerbose("################## RESULTS LIST CONTAINS " + resultsList.Count + " ENTRIES.");
            } // end of all templates
			return retVal;
        }

		public List<Results> ScanAudioFileWithTemplates(StreamedWavReader wav)
		{
			List<Results> retVal = new List<Results>();
			// scan sonogram with all templates
			for (int i = 0; i < templates.Count; i++) // for each template
			{
				Log.WriteIfVerbose("\n########################### SCANNING SONOGRAM WITH TEMPLATE " + (i + 1));
				var currentTemplate = templates[i];
				templateConfig = currentTemplate.TemplateState;

				// check the MM is valid
				MarkovModel mm = this.templateConfig.WordModel;
				if (mm == null || mm.GraphType == HMMType.UNDEFINED)
				{
					Log.WriteIfVerbose("\t##### WARNING: RECOGNISER WARNING: MARKOV MODEL " + (i + 1) + " IS NULL OR UNDEFINED!");
					continue;
				}

				FVs = SetFeatureVectors(currentTemplate.FeatureVectors);
				currentSonogram = PrepareSonogram(wav, templateConfig); // each template requires different feature extraction

				var currentResult = new Results(templateConfig); // set up results class for this scan of the template 
				currentResult.AcousticMatrix = GenerateAcousticMatrix(currentSonogram, GetNoiseFVPath(currentTemplate));
				AcousticMatrix2SymbolSequence(currentResult);
				ScanSymbolSequenceWithMM(currentResult);

				resultsList.Add(currentResult);
				retVal.Add(currentResult);

				Log.WriteIfVerbose("\n########################### END OF SCANNING SONOGRAM WITH TEMPLATE " + (i + 1));
				Log.WriteIfVerbose("################## RESULTS LIST CONTAINS " + resultsList.Count + " ENTRIES.");
			} // end of all templates
			return retVal;
		}

        /// <summary>
        /// transfers feature vectors from the template to the classifier.
        /// Need to insert an additional NOISE feature vector in the zero index
        /// The noise fv will later be used to assess the statistical significance of the template scores
        /// </summary>
        public FeatureVector[] SetFeatureVectors(FeatureVector[] featureVectors)
        {
            int fvCount = featureVectors.Length + 1;
            FeatureVector[] v = new FeatureVector[fvCount];
            for (int n = 1; n < fvCount; n++) //skip zero position where noise FV will be placed
            {
                v[n] = featureVectors[n - 1];
                v[n].FvID = n; //reset the fv's ID also
            }

            //reset the path strings to the FV files
            //SonoConfig cfg = this.template.TemplateState;
            if (this.templateConfig.FeatureVectorPaths != null) //there are no file paths if template just created
            {
                string[] paths = new string[fvCount];
                paths[0] = this.templateConfig.DefaultNoiseFVFile;
                for (int n = 1; n < fvCount; n++) paths[n] = this.templateConfig.FeatureVectorPaths[n - 1];
                this.templateConfig.FeatureVectorPaths = paths;
            }

            //reset the selected frames to the FV files
            //if (cfg.FeatureVector_SelectedFrames != null)
            //{
            //    Console.WriteLine(" L=" + cfg.FeatureVector_SelectedFrames.Length + "  fvCount=" + fvCount);
            //    for (int n = 1; n < fvCount; n++)
            //    {
            //        Console.WriteLine(" n=" + n + "  " + cfg.FeatureVector_SelectedFrames[n - 1]);
            //        int[] frameIDs = new int[1];
            //        frameIDs[0] = cfg.FeatureVector_SelectedFrames[n - 1];
            //    }
            //}

            //reset the source files to the FVs
            if (this.templateConfig.FVSourceFiles != null)
            {
                string[] sourceFs = new string[fvCount];
                for (int n = 1; n < fvCount; n++) sourceFs[n] = this.templateConfig.FVSourceFiles[n - 1];
                this.templateConfig.FVSourceFiles = sourceFs;
            }

            return v;
        }

        Sonogram PrepareSonogram(string wavPath, SonoConfig cfg)
        {
			if (string.IsNullOrEmpty(wavPath))
				throw new ArgumentNullException("wavPath");
            //set up the config file for this run
			cfg.WavFilePath = wavPath;
            cfg.SetDateAndTime(Path.GetFileNameWithoutExtension(wavPath));
            cfg.SonogramType = SonogramType.acousticVectors; //to MAKE MATRIX OF dim = 117 = 13x3x3 ACOUSTIC VECTORS

            //read the .WAV file
            WavReader wav = new WavReader(wavPath);

            //check the sampling rate
            int sr = wav.SampleRate;
            if (sr != this.templateConfig.SampleRate)
                throw new Exception("RECOGNISER.PrepareSonogram():- Sampling rate of wav file not equal to that of template:  wavFile(" + sr + ") != template(" + this.templateConfig.SampleRate + ")");
            Log.WriteIfVerbose("RECOGNISER.PrepareSonogram():- Sampling rates of wav file and template are equal: " + sr + " = " + this.templateConfig.SampleRate);

            Sonogram sonogram = new Sonogram(cfg, wav);

            return sonogram;
        } //end PrepareSonogram()

		Sonogram PrepareSonogram(StreamedWavReader wav, SonoConfig cfg)
		{
			cfg.SonogramType = SonogramType.acousticVectors; //to MAKE MATRIX OF dim = 117 = 13x3x3 ACOUSTIC VECTORS

			// check the sampling rate
			if (wav.SampleRate != templateConfig.SampleRate)
				throw new Exception("RECOGNISER.PrepareSonogram():- Sampling rate of wav file not equal to that of template:  wavFile(" + wav.SampleRate + ") != template(" + templateConfig.SampleRate + ")");
			Log.WriteIfVerbose("RECOGNISER.PrepareSonogram():- Sampling rates of wav file and template are equal: " + wav.SampleRate + " = " + templateConfig.SampleRate);

			Sonogram sonogram = new Sonogram(cfg, wav);

			return sonogram;
		} //end PrepareSonogram()

        /// <summary>
        /// SCANS A SONOGRAM WITH PREDEFINED FEATURE VECTORS
        /// using a previously loaded template.
        /// </summary>
		public double[,] GenerateAcousticMatrix(Sonogram s, string noiseFVPath)
        {
            Log.WriteIfVerbose("\nSCAN SONOGRAM WITH TEMPLATE");
            //##################### DERIVE NOISE FEATURE VECTOR OR READ PRE-COMPUTED NOISE FILE
            Log.WriteIfVerbose("\tStep 1: Derive NOISE Feature Vector, FV[0], from the passed SONOGRAM");
			int fvCount = this.FVs.Length;

            int count;
            double dbThreshold = s.State.MinDecibelReference + s.State.SegmentationThreshold_k2;  // FreqBandNoise_dB;
			FVs[0] = GetNoiseFeatureVector(s.AcousticM, s.Decibels, dbThreshold, out count);
            //if sonogram does not have sufficient noise frames read default noise FV from file
			if (FVs[0] == null)
            {
                Log.WriteIfVerbose("\tDerive NOISE Feature Vector from file: " + templateConfig.FeatureVectorPaths[0]);
				FVs[0] = new FeatureVector(templateConfig.FeatureVectorPaths[0], templateConfig.FeatureVectorLength, 0);
            }
            else if (noiseFVPath != null)   //Write noise vector to file. It can then be used as a sample noise vector.
            {
                /*string name = "template" + templateConfig.CallID + "_NoiseFV.txt";
                string fPath = Path.Combine(templateConfig.TemplateDir, name);*/
				Log.WriteIfVerbose("\tWriting noise template to file:- " + noiseFVPath);
                //this.fvs[0].Write2File(fPath);

                //string fPath = dirPath + templateStemName + "_" + this.CallID + "_" + this.templateState.FileDescriptor + "_FV" + (i + 1) + fvectorFExt;
				FVs[0].SaveDataAndImageToFile(noiseFVPath, templateConfig);
            }

            //##################### PREPARE MATRIX OF NOISE VECTORS AND THEN SET NOISE RESPONSE FOR EACH feature vector
            Log.WriteIfVerbose("\n\tStep 2: Obtain noise response for each feature vector");
            double[,] noiseM = GetRandomNoiseMatrix(s.AcousticM, this.noiseSampleCount);
            //following alternative to above method only gets noise estimate from low energy frames
            //double[,] noiseM = GetRandomNoiseMatrix(s.AcousticM, this.noiseSampleCount, this.decibels, this.decibelThreshold);
			for (int n = 0; n < fvCount; n++) this.FVs[n].SetNoiseResponse(noiseM);

            //##################### DERIVE ACOUSTIC MATRIX OF SYLLABLE Z-SCORES
            Log.WriteIfVerbose("\n\tStep 3: Obtain ACOUSTIC MATRIX of syllable z-scores");
            int frameCount = s.AcousticM.GetLength(0);
            int window = this.templateConfig.ZscoreSmoothingWindow;
            double[,] acousticMatrix = new double[frameCount, fvCount];
            for (int n = 0; n < fvCount; n++)  //for all feature vectors
            {
                Log.WriteIfVerbose("\t... with FV " + n);

                //now calculate z-score for each score value
				double[] zscores = this.FVs[n].Scan_CrossCorrelation(s.AcousticM);
                zscores = DataTools.filterMovingAverage(zscores, window);  //smooth the Z-scores
                //if(n==0) this.results.Zscores = zscores;

                for (int i = 0; i < frameCount; i++) acousticMatrix[i, n] = zscores[i];// transfer z-scores to matrix of acoustic z-scores
            }//end for loop over all feature vectors

            return acousticMatrix;
        }//end GenerateAcousticMatrix()

        /// <summary>
        /// GENERATES A SYMBOL SEQUENCE FROM THE ACOUSTIC MATRIX
        /// </summary>
        /// <param name="resultsCard"></param>
        /// <returns></returns>
        public void AcousticMatrix2SymbolSequence(Results resultsCard)
        {
            int[] integerSequence = null;
            double threshold      = this.templateConfig.ZScoreThreshold;

            Log.WriteIfVerbose("\n\tStep 4: Convert ACOUSTIC MATRIX >> SYMBOL SEQUENCE");
            Log.WriteIfVerbose("\t\tThreshold=" + threshold.ToString("F2"));

            int frameCount = resultsCard.AcousticMatrix.GetLength(0);
            int fvCount    = resultsCard.AcousticMatrix.GetLength(1);  //number of feature vectors or syllables types

            StringBuilder sb = new StringBuilder();
            integerSequence = new int[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                double[] fvScores = new double[fvCount];//init the FV scores
                for (int n = 0; n < fvCount; n++) fvScores[n] = resultsCard.AcousticMatrix[i, n]; //transfer the frame scores to array

                //get the maximum score
                int maxIndex;
                DataTools.getMaxIndex(fvScores, out maxIndex);
                if (maxIndex == 0) //this frame is noise
                {
                    sb.Append('n');
                    continue; 
                }
                char c = 'x';
                int val = Int32.MaxValue; //used as integer to represent 'x' or garbage.
                if (fvScores[maxIndex] >= threshold) //only set symbol or int if fv score exceeds threshold  
                {
                    c = DataTools.Integer2Char(maxIndex);
                    val = maxIndex;
                }
                sb.Append(c);
                integerSequence[i] = val;
            }//end of frames

            //need to convert the garbage integer in the integer sequence.
            //garbage symbol = 'x' = Int32.MaxValue. Convert Int32 to numberOfStates-1
            //states will be represented by integers: noise=0, fv=1..N, garbage=N+1
            int garbageID = fvCount;
            for (int i = 0; i < frameCount; i++) if (integerSequence[i] == Int32.MaxValue) integerSequence[i] = garbageID;

            resultsCard.SyllSymbols = sb.ToString();
            resultsCard.SyllableIDs = integerSequence;
            //Console.WriteLine("\n################## THE SYMBOL SEQUENCE\n" + sb.ToString());
        }

        public void ScanSymbolSequenceWithMM(Results resultsCard)
        {
            double[,] acousticMatrix = resultsCard.AcousticMatrix;
            string symbolSequence = resultsCard.SyllSymbols;
            int[] integerSequence = resultsCard.SyllableIDs;
            int frameCount = integerSequence.Length;

            //##################### PARSE SYMBOL STREAM USING MARKOV MODELS
            double windowLength = this.templateConfig.SongWindow;
            int clusterWindow = (int)Math.Floor(windowLength * this.templateConfig.FramesPerSecond);
            //int countThreshold = clusterWindow / 8;   // A true song must have a syllable in 1/8 of frames
            double zThreshold = this.templateConfig.ZScoreThreshold;
            MarkovModel mm = this.templateConfig.WordModel;
            if (Log.Verbosity > 0)
            {
                Log.WriteLine("\nLANGUAGE MODEL");
                mm.WriteInfo(false);
            }

            if (mm.GraphType == HMMType.OLD_PERIODIC)
            {
                double[] scores = WordSearch(symbolSequence, acousticMatrix, this.templateConfig.Words);
                resultsCard.VocalScores = scores;
                resultsCard.VocalCount = DataTools.CountPositives(scores);
                if (resultsCard.VocalCount <= 1) return; //cannot do anything more in this case

                //find peaks and process them
                bool[] peaks = DataTools.GetPeaks(scores);
                peaks = RemoveSubThresholdPeaks(scores, peaks, this.templateConfig.ZScoreThreshold);
                scores = ReconstituteScores(scores, peaks);
                //transfer scores for all frames to score matrix
                resultsCard.VocalScores = scores;
                resultsCard.VocalCount = DataTools.CountPositives(scores);

                int period_ms = mm.Periodicity_ms; //set in template
                Log.WriteIfVerbose("\n\tPeriodicity = " + period_ms + " ms");
                if ((resultsCard.VocalCount < 2) || (period_ms <= 0))
                    Log.WriteLine("### Classifier.ScanSymbolSequenceWithMM(): WARNING!!!!   PERIODICITY CANNOT BE ANALYSED.");
                else
                {
                    int maxIndex = DataTools.GetMaxIndex(scores);
                    resultsCard.VocalBest = scores[maxIndex];
                    resultsCard.VocalBestLocation = (double)maxIndex * this.templateConfig.FrameOffset;

                    resultsCard.CallPeriodicity_ms = period_ms;
                    int period_frames = mm.Periodicity_frames;
                    resultsCard.CallPeriodicity_frames = period_frames;
                    int period_NH = mm.Periodicity_NH_frames;
                    bool[] periodPeaks = Periodicity(peaks, period_frames, period_NH);
                    resultsCard.NumberOfPeriodicHits = DataTools.CountTrues(periodPeaks);
                    //Console.WriteLine("period_frame=" + period_frames + "+/-" + period_NH + " periodic hits=" + results.NumberOfPeriodicHits);
                    for (int i = 0; i < frameCount; i++) if (!periodPeaks[i]) scores[i] = 0.0;
                }
            } //end of periodic analysis
            else //normal MARKOV MODEL
            {
                double[] scores = null;
                int hitCount;
                double bestHit;
                int bestLocation;
                mm.ScoreSequence_v2(integerSequence, out scores, out hitCount, out bestHit, out bestLocation);
                //mm.ScanSequence_Chi2(integerSequence, out scores, out hitCount);
                //resultsCard.VocalScores = new double[integerSequence.Length];
                resultsCard.VocalScores = scores;
                resultsCard.VocalCount = hitCount;
                resultsCard.VocalBest = bestHit;
                resultsCard.VocalBestLocation = (double)bestLocation * this.templateConfig.FrameOffset;

                Log.WriteLine("####  VocalCount=" + hitCount + "  VocalBest=" + bestHit.ToString("F3") + "  bestFrame=" + bestLocation + " @ " + resultsCard.VocalBestLocation.ToString("F1") + "s");
            }
        }  // end of Scan(Sonogram s)

        /// <summary>
        /// Extracts all those frames passed sonogram matrix whose signal energy is below the threshold and 
        ///                     returns an average of the feature vectors derived from those frames.
        /// If there are not enough low energy frames, then the method returns null and caller must get
        /// noise FV from another source.
        /// 
        /// </summary>
        /// <param name="acousticM"></param>
        /// <param name="decibels"></param>
        /// <param name="decibelThreshold"></param>
        /// <returns></returns>
        public FeatureVector GetNoiseFeatureVector(double[,] acousticM, double[] decibels, double decibelThreshold, out int count)
        {
            int rows = acousticM.GetLength(0);
            int cols = acousticM.GetLength(1);

            int id = 0; //place default noise FV in zero position
            double[] noiseFV = new double[cols];

            int targetCount = rows / 5; //want a minimum of 20% of frames for a noise estimate
            count = 0;
            for (int i = 0; i < rows; i++)
            {
                if (decibels[i] <= decibelThreshold) count++;
                //Console.WriteLine("  " + i + " decibels[i]=" + decibels[i] + " count=" + count);
            }
            //Console.WriteLine("  " + count + " >= targetCount=" + targetCount + "   @ decibelThreshold=" + decibelThreshold);

            if (count < targetCount)
            {
                Console.WriteLine("  TOO FEW LOW ENERGY FRAMES.");
                Console.WriteLine("  Low energy frame count=" + count + " < targetCount=" + targetCount 
                    + "   @ decibelThreshold=" + decibelThreshold.ToString("F3")+" = reference+k2threshold.");
                Console.WriteLine("  TOO FEW LOW ENERGY FRAMES. READ DEFAULT NOISE FEATURE VECTOR.");
                //READ DEFAULT NOISE FEATURE VECTOR
                return null; // new FeatureVector(noiseFV, id);
            }
			else
                Log.WriteIfVerbose("        NOISE Vector is average of " + count + " frames having energy < " + decibelThreshold.ToString("F2") + " dB.");

            //now transfer low energy frames to noise vector
            for (int i = 0; i < rows; i++)
            {
                if (decibels[i] <= decibelThreshold)
                {
                    for (int j = 0; j < cols; j++) noiseFV[j] += acousticM[i, j];
                }
            }

            //take average
            for (int j = 0; j < cols; j++) noiseFV[j] /= (double)count;
            //string fPath = @"C:\SensorNetworks\Sonograms\noise.bmp";
            //ImageTools.DrawMatrix(noise, fPath);

            FeatureVector fv = new FeatureVector(noiseFV, id);
            return fv;
        }

        /// <summary>
        /// returns a matrix of noise vectors. Each noise vector is a random sample from the original sonogram.
        /// </summary>
        /// <param name="dataMatrix"></param>
        /// <param name="noiseCount"></param>
        /// <returns></returns>
        public double[,] GetRandomNoiseMatrix(double[,] dataMatrix, int noiseCount)
        {
            int frameCount   = dataMatrix.GetLength(0);
            int featureCount = dataMatrix.GetLength(1);
            //Console.WriteLine(" frameCount=" + frameCount + " featureCount=" + featureCount + " noiseCount=" + noiseCount);

            double[,] noise = new double[noiseCount, featureCount];
            RandomNumber rn = new RandomNumber();

            for (int i = 0; i < noiseCount; i++)
            {
                for (int j = 0; j < featureCount; j++)
                {
                    int id = rn.getInt(frameCount);
                    //Console.WriteLine(id);
                    noise[i, j] = dataMatrix[id, j];
                }
                //double nsd; double nav;
                //NormalDist.AverageAndSD(noiseV, out nav, out nsd);
                //double fsd; double fav;
                //NormalDist.AverageAndSD(this.Features, out fav, out fsd);
                //Console.WriteLine("fvAv=" + fav.ToString("F3") + " noiseAv=" + nav.ToString("F3") + " noiseScore[" + n + "]=" + noiseScores[n].ToString("F3"));
            }

            //string fPath = @"C:\SensorNetworks\Sonograms\noiseMatrix.bmp";
            //ImageTools.DrawMatrix(noise, fPath);

            return noise;
        } //end GetRandomNoiseMatrix()

        /// <summary>
        /// returns a matrix of noise vectors. Each noise vector is a random sample from a matrix of low energy frames
        /// that has been derive from the passed dataMatrix[] which is actually the original sonogram.
        /// </summary>
        /// <param name="dataMatrix"></param>
        /// <param name="noiseCount"></param>
        /// <param name="decibels"></param>
        /// <param name="decibelThreshold"></param>
        /// <returns></returns>
        public double[,] GetRandomNoiseMatrix(double[,] dataMatrix, int noiseCount, double[] decibels, double decibelThreshold)
        {
            double[,] lowEnergyFrames = GetMatrixOfLowEnergyFrames(dataMatrix, decibels, decibelThreshold);
            double[,] noise = GetRandomNoiseMatrix(lowEnergyFrames, noiseCount);
            return noise;
        } //end GetRandomNoiseMatrix()

        /// <summary>
        /// returns a matrix of low energy frames derived from the passed dataMatrix[] which is actually the original sonogram
        /// </summary>
        /// <param name="dataMatrix"></param>
        /// <param name="decibels"></param>
        /// <param name="decibelThreshold"></param>
        /// <returns></returns>
        public double[,] GetMatrixOfLowEnergyFrames(double[,] dataMatrix, double[] decibels, double decibelThreshold)
        {
            //Console.WriteLine("GetNoiseMatrix(double[,] matrix, double[] decibels, double decibelThreshold="+decibelThreshold+")");
            int frameCount = dataMatrix.GetLength(0);
            int featureCount = dataMatrix.GetLength(1);

            int targetCount = frameCount / 5; //want a minimum of 20% of frames for a noise estimate
            double threshold = decibelThreshold + 1.0; //set min Decibel threshold for noise inclusion
            int count = 0;
            while (count < targetCount)
            {
                count = 0;
                for (int i = 0; i < frameCount; i++) if (decibels[i] <= threshold) count++;
                //Console.WriteLine("decibelThreshold=" + threshold.ToString("F1") + " count=" + count);
                threshold += 1.0;
            }
            //Console.ReadLine();

            //now transfer low energy frames to noise matrix
            double[,] lowEnergyFrames = new double[count, featureCount];
            threshold -= 1.0; //take threshold back to the proper value
            count = 0;
            for (int i = 0; i < frameCount; i++)
            {
                if (decibels[i] <= threshold)
                {
                    for (int j = 0; j < featureCount; j++) lowEnergyFrames[count, j] = dataMatrix[i, j];
                    count++;
                }
            }
            //string fPath = @"C:\SensorNetworks\Sonograms\noise.bmp";
            //ImageTools.DrawMatrix(noise, fPath);

            return lowEnergyFrames;
        } //end GetRandomNoiseMatrix()

        /// <summary>
        /// Returns an array of doubles that simulates noise or average row from the passed sonogram matrix
        /// </summary>
		[Obsolete]
        public double[] GetRandomNoiseVector(double[,] matrix)
        {
            int frameCount = matrix.GetLength(0);
            int featureCount = matrix.GetLength(1);
            //Console.WriteLine(" ... dimM[1]=" + M.GetLength(1) + " ... dim fvs[fvID]=" + fvs[fvID].FvLength);

            double[] noise = new double[featureCount];
            RandomNumber rn = new RandomNumber();
            for (int j = 0; j < featureCount; j++)
            {
                int id = rn.getInt(frameCount);
                noise[j] = matrix[id, j];
            }
            //Console.ReadLine();
            return noise;
        } //end GetRandomNoiseVector()
        
        /// <summary>
        /// scans a symbol string for the passed words and returns for each position in the string the match score of
        /// that word which obtained the maximum score. The matchscore is derived from a zscore matrix.
        /// NOTE: adding z-scores is similar to adding the logs of probabilities derived from a Gaussian distribution.
        ///     log(p) = -log(sd) - log(sqrt(2pi)) - (Z^2)/2  = Constant - (Z^2)/2
        ///         I am adding Z-scores instead of the squares of Z-scores.
        /// </summary>
        /// <param name="symbolSequence"></param>
        /// <param name="zscoreMatrix"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public static double[] WordSearch(string symbolSequence, double[,] zscoreMatrix, string[] words)
        {
            int sequenceLength = symbolSequence.Length;
            int symbolCount = zscoreMatrix.GetLength(1);
            int wordCount = words.Length;
            double[] wordScores = new double[sequenceLength];
            int maxWordLength = words[0].Length; //user must place longest word first in the list !!!
            //Console.WriteLine("maxWordLength=" + maxWordLength + "  wordCount=" + wordCount);
            //Console.WriteLine("zscoreMatrix dim =" + zscoreMatrix.GetLength(0) + ", " + zscoreMatrix.GetLength(1));

            for (int i = 0; i < sequenceLength - maxWordLength; i++) //WARNING: user must place longest word first in the list !!!
            {
                if ((symbolSequence[i] == 'x') || (symbolSequence[i] == 'n'))
                {
                    wordScores[i] = 0.0;
                    continue;
                }
                //have a possible word so check what it is
                double[] scores = new double[wordCount];
                for (int w = 0; w < wordCount; w++)
                {
                    int wordLength = words[w].Length;
                    int[] intArray = MarkovModel.String2IntegerArray(words[w]);
                    //Console.Write(i + "  wordCount=" + wordCount + "  arrayLength=" + intArray.Length + "  " + words[w]);
                    double sum = 0.0;
                    for (int s = 0; s < wordLength; s++)
                    {
                        //Console.WriteLine("s=" + s + "    " + intArray[s]);
                        if (intArray[s] >= symbolCount) 
                        {
                            throw new Exception("WORD <" + words[w] + "> IN GRAMMAR CONTAINS ILLEGAL SYMBOL."); 
                        }
                        sum += zscoreMatrix[i + s, intArray[s]];
                    }
                    scores[w] = sum;

                }//end of getting word scores for this position

                //get the maxmum score
                int maxIndex;
                DataTools.getMaxIndex(scores, out maxIndex);
                wordScores[i] = scores[maxIndex];
                int winningWordLength = words[maxIndex].Length;

                //now check that sum is more than the noise score over same frames - sum the noise scores
                //double noise = 0.0;
                //for (int s = 0; s < winningWordLength; s++) noise += zscoreMatrix[i + s, 0];
                //Console.WriteLine("maxIndex=" + maxIndex + "  wordscore[" + i + "]=" + scores[maxIndex] + "  noise=" + noise);
                //if (wordScores[i] < noise) Console.WriteLine("WINNING SCORE < NOISE");

                //i++; //skip next position if have a hit. Missed positions will have zero score

            }//end of symbol string
            return wordScores;
        }

        /// <summary>
        /// scans a symbol string for the passed words and returns for each position in the string the match score of
        /// that word which obtained the maximum score. The matchscore is derived from the Levenshtein edit distance.
        /// This method did not work so well because the edit scores are discrete and too chunky for these purposes
        /// </summary>
        /// <param name="symbolSequence"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public double[] WordSearch(string symbolSequence, string[] words)
        {
            int symbolCount = symbolSequence.Length;
            int wordCount = words.Length;
            int wordLength = words[0].Length; //assume all words are the same length for now!!!
            for (int n = 0; n < wordCount; n++) Console.WriteLine("WORD"+(n+1)+"="+words[n]);

            int[] editD = new int[wordCount];
            double[] editScore = new double[symbolCount];

            for (int i = 0; i < symbolCount - wordLength; i++)
            {
                string substring = symbolSequence.Substring(i, wordLength);
                for (int w = 0; w < wordCount; w++)
                {
                    editD[w] = TextUtilities.LD(words[w], substring); //Levenshtein edit distance between two strings.
                }
                //if (editD[0]!= 1) Console.WriteLine(i + "   " + editD[0] + "   " + editD[1] + "   " + editD[2]);
                int max; int min;
                DataTools.MinMax(editD, out min, out max);
                //if (min == 0) Console.WriteLine(i + "  min=" + min);
                editScore[i] = (double)(5 - (min*min));
                if (editScore[i] < 0) editScore[i] = 0.0;
                //if (editScore[i] != 5) Console.WriteLine(i +"  "+editScore[i]);
            }
            return editScore;
        }

        //public int CountClusters(double[] zScores, double thresholdZ)
        //{
        //    int frameCount = zScores.Length;
        //    int clusterCount = 0;
        //    for (int i = 1; i < frameCount; i++)
        //    {
        //        bool lo2hi = ((zScores[i - 1] <  thresholdZ) && (zScores[i] >= thresholdZ));
        //        bool hi2lo = ((zScores[i - 1] >= thresholdZ) && (zScores[i] <  thresholdZ));
        //        if (lo2hi || hi2lo) clusterCount++;  //count number of times score goes above or below threshold
        //    }
        //    return (clusterCount / 2);
        //}

        public void SaveSymbolSequences(string pathName, bool includeUserDefinedVocabulary)
        {
            Console.WriteLine("\tRECOGNISER.DisplaySymbolSequences(): Preparing symbol sequences");
            if ((resultsList == null) || (resultsList.Count == 0))
            {
                Console.WriteLine("\t\tNo sequences to display!");
                return;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < resultsList.Count; i++) //add result tracks one at a time
            {
                sb.Append("\n==================================RESULTS TRACK "+i+" ==============================================================\n\n");
                sb.Append(FormatSymbolSequence(i));
                //sb.Append("\n================================================================================================\n\n");
                if (includeUserDefinedVocabulary)
                {
                    //sb.Append(DisplayUserDefinedVocabulary(i));
                }
            }
            Console.WriteLine("\tWriting symbol sequence data to file: " + pathName);
            FileTools.WriteTextFile(pathName, sb.ToString());
        }

        public string FormatSymbolSequence(int templateID)
        {
            StringBuilder sb = new StringBuilder();

            // display the symbol sequence, one second per line
            sb.Append("\n################## THE SYMBOL SEQUENCE DERIVED FROM TEMPLATE " + templateID);
            sb.Append("\n################## Number of user defined symbols/feature vectors =" + this.templateConfig.FeatureVectorCount);
            sb.Append("\n################## n=noise.   x=garbage i.e. frame has unrecognised acoustic energy.\n");
            string ss = this.resultsList[templateID].SyllSymbols; //symbol Sequence
            if(ss == null)
            {
                string message = "#### WARNING! FormatSymbolSequence(int templateID): SEQUENCE = NULL";
                Console.WriteLine("\t"+message);
                return message;
            }
            sb.Append(FormatSymbolSequence(ss));

            //display N-grams
            int N = 2;
            var _2grams = ExtractNgramSequences(ss, N);
            var ht2 = DataTools.WordsHisto(_2grams);
            sb.Append("\n################# Number of 2grams=" + _2grams.Count + ".  Distinct=" + ht2.Count + ".\n\t# 2gram (count,RF)\n");
            int count = 0;
            foreach (string str in ht2.Keys)
            {
                double rf = ht2[str] / (double)_2grams.Count;
                sb.Append("\t" + ((++count).ToString("D2")) + " " + str + " (" + ((int)ht2[str]).ToString("D2") + "," + rf.ToString("F3") + ")\n");
            }

            N = 3;
            var _3grams = ExtractNgramSequences(ss, N);
            var ht3 = DataTools.WordsHisto(_3grams);
            sb.Append("\n################# Number of 3grams=" + _3grams.Count + ".  Distinct=" + ht3.Count + ".\n\t# 3gram (count,RF)\n");

            count = 0;
			foreach (string str in ht3.Keys)
                sb.Append("\t" + ((++count).ToString("D2")) + " " + str + " (" + ht3[str] + ")\n");

            //display the sequences of valid syllables
            var list = ExtractWordSequences(ss);
            //DataTools.WriteArrayList(list);
            var ht = DataTools.WordsHisto(list);
            sb.Append("\n################# Number of Words = " + list.Count + "  Number of Distinct Words = " + ht.Count+"\n");

            count = 0;
			foreach (string str in ht.Keys)
                sb.Append((++count).ToString("D2") + "  " + str + " \t(" + ht[str] + ")\n");

            int maxGap = 80;
            double durationMS = this.templateConfig.FrameOffset * 1000;
            sb.Append("\n################# Distribution of Gaps between Detected : (Max gap=" + maxGap + " frames)\n");
            sb.Append("                   Duration of each frame = " + durationMS.ToString("F1") + " ms\n"); 
            int[] gaps = CalculateGaps(ss, maxGap); //lengths of 'n' and 'x' - noise and garbage
            for (int i = 0; i < maxGap; i++) if (gaps[i] > 0) 
                sb.Append("Frame Gap=" + i + " count=" + gaps[i] + " (" + (i * durationMS).ToString("F1") + "ms)\n");
            sb.Append("\n");

            return sb.ToString();
        }// end DisplaySymbolSequence(int templateID)

        public string DisplayUserDefinedVocabulary(int templateID)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\n################# User Defined Vocabulary (Number of symbols =" + this.templateConfig.FeatureVectorCount + ")\n");
            if ((this.templateConfig.Words == null) || (this.templateConfig.Words.Length == 0))
                Console.WriteLine("\tWord list not defined.");
            else
            {
                Console.WriteLine("\tWord list defined in template.");
                for (int i = 0; i < this.templateConfig.Words.Length; i++)
                    sb.Append(i + "  " + this.templateConfig.Words[i] + "\n");
            }
            if (this.templateConfig.WordModel == null)
                Console.WriteLine("\tMarkov Word Model not defined.");
            else
            {
                Console.WriteLine("\tMarkov Word Model defined in template.");
                sb.Append(this.templateConfig.WordModel.Name + "\n");
            }

            return sb.ToString();
        } //end DisplayUserDefinedVocabulary(int templateID)

        public List<string> ExtractWordSequences(string sequence)
        {
			var list = new List<string>();
            bool inWord = false;
            int L = sequence.Length;
            int wordStart = 0;
            int buffer = 3;

            for (int i = 0; i < L-buffer; i++)
            {
                bool endWord = true;
                char c = sequence[i];
                if (IsSyllable(c))
                {
                    if (!inWord) wordStart = i;
                    inWord = true;
                    endWord = false;
                }
                else
                {
                    if (ContainsSyllable(sequence.Substring(i, buffer))) endWord = false;
                }

                if ((inWord) && (endWord))
                {
                    //Console.WriteLine(i + "  L=" + (i - wordStart) + "  " + sequence.Substring(wordStart, i - wordStart));
                    list.Add(sequence.Substring(wordStart, i - wordStart));
                    inWord = false;
                }
            }//end loop over sequence 

            return list;
        }

        public int[] CalculateGaps(string sequence, int maxGap)
        {
            int[] gaps = new int[maxGap];
            bool inGap = false;
            int L = sequence.Length;
            int gapStart = 0;

            for (int i = 0; i < L; i++)
            {
                bool endGap = true;
                char c = sequence[i];
                if (!IsSyllable(c)) //ie is noise or garbage frame
                {
                    if (!inGap) gapStart = i;
                    inGap = true;
                    endGap = false;
                }

                if ((inGap) && (endGap))
                {
                    int gap = i - gapStart;
                    if (gap >= maxGap) gaps[maxGap-1]++; else gaps[gap]++;
                    inGap = false;
                }
            }
            return gaps;
        } //end of CalculateGaps()

        private bool IsSyllable(char c)
        {
			return (c != 'n') && (c != 'x');
        }

        private bool ContainsSyllable(string str)
        {
			// NOTE: from Richard - this doesn't seem correct, but it's what was written.
			return !string.IsNullOrEmpty(str) && IsSyllable(str[0]);
        }

		public List<string> ExtractNgramSequences(string sequence, int N)
        {
            var list = new List<string>();
            int L = sequence.Length;

            for (int i = 0; i < L - N; i++)
            {
                if (IsSyllable(sequence[i]) && IsSyllable(sequence[i+N-1]))
					list.Add(sequence.Substring(i, N));
            }//end loop over sequence 

            return list;
        }
        public string FormatSymbolSequence(string sequence)
        {
            StringBuilder sb = new StringBuilder("sec\tSEQUENCE\n");
            int L = sequence.Length;
            int symbolRate = (int)Math.Round(this.templateConfig.FramesPerSecond);
            int secCount = L / symbolRate;
            int tail     = L % symbolRate;
            for (int i = 0; i < secCount; i++)
            {
                int start = i * symbolRate;
                sb.Append(i.ToString("D3") + "\t" + sequence.Substring(start, symbolRate) + "\n");
            }
            sb.Append(secCount.ToString("D3") + "\t" + sequence.Substring(secCount * symbolRate) + "\n");
            return sb.ToString();
        }

        //***************************************************************************************************************************
        //***************************************************************************************************************************
        //***************************************************************************************************************************
        //****************************** STATE MACHINE TO DETERMINE LARGE SCALE STRUCTURE OF CALL ***********************************

		public bool[] Periodicity(bool[] peaks, int period_frame, int period_NH)
        {
            int L = peaks.Length;
            bool[] hits = new bool[L];
            int index = 0;

            //find the first peak
            for (int n = 0; n < L; n++)
            {
                index = n;
                if (peaks[n]) break;
            }
            if (index == L - 1) return hits; //i.e. no peaks in the array

            // have located index of the first peak. Now look for peaks correct distance apart
            int minDist = period_frame - period_NH;
            int maxDist = period_frame + period_NH;
            for (int n = index + 1; n < L; n++)
            {
                if (peaks[n])
                {
                    int period = n - index;
                    if ((period >= minDist) && (period <= maxDist))
                    {
                        hits[index] = true;
                        hits[n] = true;
                    }
                    index = n; //set new position
                }
            }
            //DataTools.writeArray(periods);

            return hits;
        }

        /// <summary>
        /// returns a reconstituted array of zscores.
        /// Only gives values to score elements in vicinity of a peak.
        /// </summary>
        /// <param name="scores"></param>
        /// <param name="peaks"></param>
        /// <param name="tHalfWidth"></param>
        /// <returns></returns>
        public static double[] ReconstituteScores(double[] scores, bool[] peaks)
        {
            int length = scores.Length;
            double[] newScores = new double[length];
            for (int n = 0; n < length; n++)
            {
                if (peaks[n]) newScores[n] = scores[n];
            } return newScores;
        } // end of ReconstituteScores()

        public static bool[] RemoveSubThresholdPeaks(double[] scores, bool[] peaks, double threshold)
        {
            int length = peaks.Length;
            bool[] newPeaks = new bool[length];
            for (int n = 0; n < length; n++)
            {
                newPeaks[n] = peaks[n];
                if (scores[n] < threshold) newPeaks[n] = false;
            }
            return newPeaks;
        }

        public static bool[] RemoveIsolatedPeaks(bool[] peaks, int period, int minPeakCount)
        {
            int nh = period * minPeakCount / 2;
            int length = peaks.Length;
            bool[] newPeaks = new bool[length];
            //copy array
            for (int n = 0; n < length; n++) newPeaks[n] = peaks[n];

            for (int n = nh; n < length - nh; n++)
            {
                bool isolated = true;
                for (int i = n - nh; i < n - 3; i++) if (peaks[i]) isolated = false;
                for (int i = n + 3; i < n + nh; i++) if (peaks[i]) isolated = false;
                if (isolated) newPeaks[n] = false;
            }//end for loop
            //DataTools.writeArray(newPeaks);
            return newPeaks;
        }

        /// <summary>
        /// Calculates a histogram of the intervals between hits (trues) in the peaks array
        /// </summary>
        /// <param name="peaks"></param>
        /// <param name="maxPeriod"></param>
        /// <returns></returns>
        public int[] GetHitPeriods(bool[] peaks, int maxPeriod)
        {
            int[] periods = new int[maxPeriod];
            int length = peaks.Length;
            int index = 0;

            for (int n = 0; n < length; n++)
            {
                index = n;
                if (peaks[n]) break;
            }
            if (index == length - 1) return periods; //i.e. no peaks in the array

            // have located index of the first peak
            for (int n = index + 1; n < length; n++)
            {
                if (peaks[n])
                {
                    int period = n - index;
                    if (period >= maxPeriod) period = maxPeriod - 1;
                    periods[period]++;
                    index = n;
                }
            }
            //DataTools.writeArray(periods);
            return periods;
        }

        //******************************************************************************************************************
        //******************************************************************************************************************
        //******************************************************************************************************************
        //******************************************************************************************************************
        //******************************************************************************************************************
        //******************************************************************************************************************
        //******************************************************************************************************************

        public static string ResultsHeader()
        {
            return AudioStuff.Results.ResultsHeader();
        }

        public void WriteRecognitionResults2Console()
        {
            if ((resultsList == null)||(resultsList.Count ==0))
            {
                Console.WriteLine("\t##### WARNING! RECOGNISER.WriteResults2Console(): NO RESULTS ARE AVAILABLE TO PRINT!");
                return;
            }
			
            Console.WriteLine("\n===========================================================================================");
			// Which CallID? There could be multiple templates
            Console.Write(/*"Call ID " + CallID + */": RESULTS FOR " + resultsList.Count + " RECOGNISER");
            if (resultsList.Count > 1)
				Console.WriteLine("S");
			else
				Console.WriteLine();
            for (int i = 0; i < resultsList.Count; i++)
            {
                Console.WriteLine(GetRecognitionResultsAsString(i));
                Console.WriteLine("===========================================================================================\n");
            }
        }

        public string GetRecognitionResultsAsString(int templateID)
        {
            StringBuilder sb = new StringBuilder();

			var template = templates[templateID];
			sb.Append("\nCall ID " + template.CallID + ": RESULTS FOR RECOGNISER " + (templateID + 1) + "\n");
			sb.Append(" Template Name = " + template.CallName + "\n");
			sb.Append(" " + template.CallComment + "\n");
            sb.Append(" Z-score threshold = " + templateConfig.ZScoreThreshold + "\n");

			var result = resultsList[templateID];
            //DataTools.WriteMinMaxOfArray(" Min/max of word scores", this.results.HitScores);
			sb.Append(" Number of vocalisation events found = " + result.VocalCount + "\n");
            if (result.VocalCount < 1)
				return sb.ToString();

			sb.Append(" Maximum Score = " + result.VocalBest.ToString("F1") + " at " + result.VocalBestLocation.ToString("F1") + " sec\n");

            if (templateConfig.WordModel.Periodicity_ms == 0)
				return sb.ToString();

            //report periodicity results - if required
			int period = result.CallPeriodicity_frames;
            if (period > 1)
            {
                int NH_frames = templateConfig.WordModel.Periodicity_NH_frames;
                int NH_ms = templateConfig.WordModel.Periodicity_NH_ms;
				sb.Append(" Required periodicity = " + period + "±" + NH_frames + " frames or " + result.CallPeriodicity_ms + "±" + NH_ms + " ms\n");
				sb.Append(" Number of hits with required periodicity = " + result.NumberOfPeriodicHits + "\n");
            }
            return sb.ToString();
        }

        public void AppendResults2File(string fPath, int templateID)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DATE=" + DateTime.Now.ToString("u"));
            sb.Append(",Number of template hits=" + this.resultsList[templateID].VocalCount);

            FileTools.Append2TextFile(fPath, sb.ToString());
        }

        public string OneLineResult(int scanID)
        {
            StringBuilder sb = new StringBuilder();
            //#	Name                	Date    	Dploy	Durat	Hour	Min 	TSlot	Hits 	MaxScr	MaxLoc	
            sb.Append(scanID + Results.spacer); //CALLID
            //sb.Append(DateTime.Now.ToString("u") + spacer); //DATE
            sb.Append(Path.GetFileNameWithoutExtension(templateConfig.WavFilePath) + Results.spacer); //sonogram FNAME
			sb.Append(templateConfig.Time); //sonogram date
			sb.Append(Results.spacer);
            sb.Append(templateConfig.DeploymentName + Results.spacer); //Deployment name
            sb.Append(templateConfig.TimeDuration.ToString("F2") + Results.spacer); //length of recording
			if (templateConfig.Time != null)
			{
				sb.Append(templateConfig.Time.Value.TimeOfDay.Hours + Results.spacer); //hour when recording made
				sb.Append(templateConfig.Time.Value.TimeOfDay.Minutes + Results.spacer); //hour when recording made
			}
			else
				sb.Append(Results.spacer + Results.spacer);
            sb.Append(templateConfig.TimeSlot + Results.spacer); //half hour when recording made
            //sb.Append(templateConfig.FrameNoise_dB.ToString("F4") + Results.spacer);
            //sb.Append(templateConfig.Frame_SNR.ToString("F4") + Results.spacer);
            //sb.Append(templateConfig.PowerMax.ToString("F3") + Results.spacer);
            //sb.Append(templateConfig.PowerAvg.ToString("F3") + Results.spacer);
            //sb.Append(Result.VocalCount + Results.spacer);
            sb.Append(templates[0].CallID + Results.spacer); // Richard - Not sure if this is the correct callid since there could be multiple templates
			if (Result != null)
			{
				sb.Append(Result.NumberOfPeriodicHits + Results.spacer);
				sb.Append(Result.VocalBest.ToString("F1") + Results.spacer);
				sb.Append(Result.VocalBestLocation.ToString("F1") + Results.spacer);
			}
			else
				sb.Append(Results.spacer + Results.spacer + Results.spacer);
            return sb.ToString();
        }

		public Image GetImage()
		{
			if (resultsList == null || resultsList.Count == 0)
				throw new InvalidOperationException("No results to accompany image");
			var tracks = Results.Results2VocalisationTracks(resultsList);
			return currentSonogram.GetImage(SonogramType.spectral, tracks);
		}

        public void SaveImage(string path)
        {
			var image = GetImage();
			image.Save(path);
        }

        public void SaveImage(string path, TrackType type)
        {
            if ((this.resultsList == null) || (this.resultsList.Count == 0))
            {
				Log.WriteLine("\t##### WARNING! RECOGNISER.SaveImage(): NO RESULTS TO ACCOMPANY IMAGE");
                return;
            }
            if (TrackType.syllables == type)
				currentSonogram.SaveImage(path, SonogramType.spectral, type, resultsList[0].SyllableIDs);
        }
    }// end of class RECOGNISER

	//**********************************************************************************************************************************
    //**********************************************************************************************************************************
    //**********************************************************************************************************************************
    //**********************************************************************************************************************************
    //**********************************************************************************************************************************
    //**********************************************************************************************************************************
    //*******************************************  RESULTS CLASS ***********************************************************************

    /// <summary>
    /// this class contains the results obtained from the Classifer.
    /// </summary>
    public class Results
	{
		#region Statics
		public const string spacer = ",";  //used when writing data arrays to file
        public const char spacerC   = ',';  //used as match for splitting string
        public const int analysisBandCount = 11;   //number of bands in which to divide freq columns of sonogram for analysis

		public static List<Track> Results2VocalisationTracks(List<Results> results)
		{
			if ((results == null) || (results.Count == 0))
			{
				//throw new Exception("WARNING!!!!  matrix==null CANNOT SAVE THE SONOGRAM AS IMAGE!");
				Console.WriteLine("WARNING!!!!  Results.Results2VocalTracks(): Results==null CANNOT EXTRACT DATA TRACKS FOR IMAGE!");
				return null;
			}

			var tracks = new List<Track>();
			foreach (var result in results)
				tracks.AddRange(result.GetVocalTracks());
			return tracks;
		}

		public static string ResultsHeader()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Scan ID" + spacer);
			sb.Append("Wav File Name" + spacer);
			sb.Append("Date" + spacer);
			sb.Append("Deployment" + spacer);
			sb.Append("Duration" + spacer);
			sb.Append("Hour" + spacer);
			sb.Append("Min " + spacer);
			sb.Append("24hr ID" + spacer);

			sb.Append("Template ID" + spacer);
			sb.Append("Hits" + spacer);
			sb.Append("Max Score" + spacer);
			sb.Append("Location (sec)" + spacer);
			return sb.ToString();
		}

		public static string AnalysisHeader()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("#" + spacer);
			sb.Append("Name                " + spacer);
			sb.Append("Date    " + spacer);
			sb.Append("Dploy" + spacer);
			sb.Append("Durat" + spacer);
			sb.Append("Hour" + spacer);
			sb.Append("Min " + spacer);
			sb.Append("TSlot" + spacer);

			sb.Append("SgMaxi" + spacer);
			sb.Append("SgAvMx" + spacer);
			sb.Append("SgRati" + spacer);
			sb.Append("PwrMax" + spacer);
			sb.Append("PowrAvg" + spacer);

			for (int f = 0; f < analysisBandCount; f++) sb.Append("Syl" + f + spacer);
			sb.Append("Sylls" + spacer);
			for (int f = 0; f < analysisBandCount; f++) sb.Append("Cat" + f + spacer);
			sb.Append("Catgs" + spacer);
			for (int f = 0; f < analysisBandCount; f++) sb.Append("Monot" + f + spacer);
			sb.Append("Monotny" + spacer);
			sb.Append("Name" + spacer);

			return sb.ToString();
		}

		public static string Analysis24HourHeader()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Time" + spacer);

			sb.Append("SgMaxi" + spacer);
			sb.Append("SgAvMx" + spacer);
			sb.Append("SgRati" + spacer);
			sb.Append("PwrMax" + spacer);
			sb.Append("PowrAvg" + spacer);

			for (int f = 0; f < analysisBandCount; f++) sb.Append("Syl" + f + spacer);
			sb.Append("Sylls" + spacer);
			for (int f = 0; f < analysisBandCount; f++) sb.Append("Cat" + f + spacer);
			sb.Append("Catgs" + spacer);
			for (int f = 0; f < analysisBandCount; f++) sb.Append("Mon" + f + spacer);
			sb.Append("Mntny" + spacer);


			// element content
			//7 Time    48 timeslots in 24 hours
			//8 SigMax
			//9 SgAvMx
			//10 SgRati
			//11 PwrMax
			//12 PwrAvg

			//13 FrBnd0 syllables in freq band
			//14 FrBnd1
			//15 FrBnd2
			//16 FrBnd3
			//17 FrBnd4
			//18 FrBnd5
			//19 FrBnd6
			//20 FrBnd7
			//21 FrBnd8
			//22 FrBnd9
			//23 FrBnd10
			//24 Sylls  syllable total over all freq bands of sonogram

			//25 FrBnd0 clusters in freq band 
			//26 FrBnd1
			//27 FrBnd2
			//28 FrBnd3
			//29 FrBnd4
			//30 FrBnd5
			//31 FrBnd6
			//32 FrBnd7
			//33 FrBnd8
			//34 FrBnd9
			//35 FrBnd10
			//36 Catgs  cluster total over all freq bands of sonogram

			//37 FrBnd0 monotony in freq band
			//38 FrBnd1
			//39 FrBnd2
			//40 FrBnd3
			//41 FrBnd4
			//42 FrBnd5
			//43 FrBnd6
			//44 FrBnd7
			//45 FrBnd8
			//46 FrBnd9
			//47 FrBnd10
			//48 Av monotony over all freq bands of sonogram

			return sb.ToString();
		}
		#endregion

		private double zScoreThreshold;
        private int garbageID;

        // RESULTS FOR SPECIFIC ANIMAL CALL ANALYSIS
        public double[,] AcousticMatrix { get; set; }   // matrix of fv x time frames
        public string    SyllSymbols { get; set; }      // array of symbols  representing winning user defined feature templates
        public int[]     SyllableIDs { get; set; }      // array of integers representing winning user defined feature templates
        public double[]  VocalScores { get; set; }      // array of scores for user defined call templates
        public int       VocalCount  { get; set; }      // number of hits whose score exceeds some threshold
        public double    VocalBest   { get; set; }      // the best score in recording, and .....
        public double    VocalBestLocation { get; set; }// its location in seconds from beginning of recording

        public int CallPeriodicity_frames { get; set; }
        public int CallPeriodicity_ms { get; set; }
        public int NumberOfPeriodicHits { get; set; }

        // RESULTS FOR GENERAL ACOUSTIC ANALYSIS
        public double[] PowerHisto { get; set; }
        public double[] EventHisto { get; set; }
        public double EventAverage
		{ 
            get
			{
				double sum = 0.0;
				for (int i = 0; i < Results.analysisBandCount; i++)
					sum += EventHisto[i];
				return sum / (double)Results.analysisBandCount;
            }
		}
        public double EventEntropy { get; set; }
        public double[] ActivityHisto { get; set; }

		#region Constructors
		/// <summary>
		/// CONSTRUCTOR 1
		/// </summary>
		public Results() { }

		/// <summary>
		/// CONSTRUCTOR 2
		/// </summary>
		public Results(SonoConfig config)
		{
			this.zScoreThreshold = config.ZScoreThreshold;
			this.garbageID = config.FeatureVectorCount + 2 - 1;
		}
		#endregion

		public List<Track> GetVocalTracks()
        {
			var tracks = new List<Track>();
            //add syllable ID track
            Track track1 = new Track(TrackType.syllables, this.SyllableIDs);
            track1.GarbageID = this.garbageID;
            tracks.Add(track1);

            //add score track
            Track track2 = new Track(TrackType.scoreArray, this.VocalScores);
            track2.ScoreThreshold = this.zScoreThreshold;
            tracks.Add(track2);
            return tracks;
        }

		#region Main Method
		/// <summary>
        /// main method in class RESULTS
        /// </summary>
        static void Main()
        {
            //WARNING!! timeSlotCount = 48 MUST BE CONSISTENT WITH TIMESLOT CALCULATION IN class SonoConfig.SetDateAndTime(string fName)
            // that is, every half hour
            const int timeSlotCount = 48;

            const string testDirName = @"C:\SensorNetworks\TestOutput_Exp7\";
            const string resultsFile = "outputAnalysisExp7.txt";
            string ipPath = testDirName + resultsFile;
            const string opFile = "Exp7_24hrCycle.txt";
            string opPath = testDirName + opFile;

            Log.WriteLine("START ANALYSIS. \n  output to: " + opPath);

            //set up arrays to contain TimeSlot info
            int[] counts = new int[timeSlotCount]; //require counts in each time slot for averages
            double[] signalMax = new double[timeSlotCount]; //column 8
            double[] sigAvgMax = new double[timeSlotCount]; //column 9
            double[] sigMRatio = new double[timeSlotCount]; //column 10
            double[] powerMaxs = new double[timeSlotCount];  //column 11
            double[] powerAvgs = new double[timeSlotCount];  //column 12

            double[,] syllBand = new double[timeSlotCount, Results.analysisBandCount]; //columns 13 - 23 syllables in freq band
            double[] syllables = new double[timeSlotCount];  //column 24

            double[,] clstBand = new double[timeSlotCount, Results.analysisBandCount]; //columns 25 - 35 syllables in freq band
            double[] clusters  = new double[timeSlotCount];  //column 36

            double[,] monotony = new double[timeSlotCount, Results.analysisBandCount]; //columns 37 - 47 syllables in freq band
            double[] avMonotny = new double[timeSlotCount];  //column 48

            
            using (TextReader reader = new StreamReader(ipPath))
            {
                string line = reader.ReadLine(); //skip the first header line
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "\t") continue;
                    if (line == "") continue;
                    string[] words = line.Split(spacerC);
                    Console.WriteLine(words[0] + "   " + words[7] + "   " + words[13]);
                   int id = Int32.Parse(words[7]);  //the time slot

                   counts[id]++; //require counts in each time slot for averages
                   signalMax[id] += Double.Parse(words[8]);  //SUM ALL VALUES FOR CALCULATION OF AVERAGES
                   sigAvgMax[id] += Double.Parse(words[9]);
                   sigMRatio[id] += Double.Parse(words[10]);
                   powerMaxs[id] += Double.Parse(words[11]);
                   powerAvgs[id] += Double.Parse(words[12]);

                   for (int f = 0; f < Results.analysisBandCount; f++) syllBand[id, f] += Int32.Parse(words[13 + f]);
                   syllables[id] += Int32.Parse(words[24]);

                   for (int f = 0; f < Results.analysisBandCount; f++) clstBand[id, f] += Int32.Parse(words[25 + f]);
                   clusters[id] += Int32.Parse(words[36]);

                   for (int f = 0; f < Results.analysisBandCount; f++) monotony[id, f] += Double.Parse(words[37 + f]);
                   avMonotny[id] += Double.Parse(words[48]);

                }//end while
            }//end using

            var opLines = new List<string>();
            opLines.Add(Results.Analysis24HourHeader());
            for (int i = 0; i < timeSlotCount; i++)
            {
                string line = ((i / (double)2).ToString("F1") //calculate time as 24 hour clock
                +"\t" + (signalMax[i] / (double)counts[i]).ToString("F4")
                +"\t" + (sigAvgMax[i] / (double)counts[i]).ToString("F4")
                +"\t" + (sigMRatio[i] / (double)counts[i]).ToString("F4")
                +"\t" + (powerMaxs[i] / (double)counts[i]).ToString("F2")
                +"\t" + (powerAvgs[i] / (double)counts[i]).ToString("F2"));

                for (int f = 0; f < Results.analysisBandCount; f++) line += ("\t" + (syllBand[i, f] / (double)counts[i]).ToString("F2"));
                line += "\t" + (syllables[i] / (double)counts[i]).ToString("F2");

                for (int f = 0; f < Results.analysisBandCount; f++) line += ("\t" + (clstBand[i, f] / (double)counts[i]).ToString("F2"));
                line += "\t" + (clusters[i] / (double)counts[i]).ToString("F2");

                for (int f = 0; f < Results.analysisBandCount; f++) line += ("\t" + (monotony[i, f] / (double)counts[i]).ToString("F2"));
                line += "\t" + (avMonotny[i] / (double)counts[i]).ToString("F2");

                //Console.WriteLine(line);
                opLines.Add(line);
            }
            FileTools.WriteTextFile(opPath, opLines);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        } //end of Main
		#endregion
	}//end class Results
}