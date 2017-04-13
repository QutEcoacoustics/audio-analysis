namespace AudioAnalysisTools
{


    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using Indices;
    using TowseyLibrary;

    /// <summary>
    /// This class is experimental work on the Bird50 dataset provided by Herve Glotin.
    /// The work was started in Toulon (February 2016) and continued after my return in March 2016.
    /// The bird50 dataset is a randomly selected subset of the 2014 BirdClef 500 data set.
    /// The 2014 competition was won by Dan Stowell (QMUL).
    /// The 2015 competition containing 999 bird call recordings was won by Mario Lessek (Berlin)
    ///
    /// This class prepares species representations of bird calls by summing or averaging the instance representations.
    /// The representation consists of the concatenation of a set of spectra.
    /// There are currently five spectra derived from the spectra indices: SPT, RHZ, RVT, RPS and RNG.
    /// Each spectrum can be reduced from 256 values to say 160 by max pooling the top end of the spctrum.
    /// NOTE: MEL-scale DOES NOT work for birds because the dominant activity for birds is around 2-4 kHz.
    /// The MEL-scale effectively obliterates this band of the spectrum.
    /// </summary>
    public static class BirdClefExperiment1
    {

        const string FEATURE_KEYS = "SPT,RHZ,RVT,RPS,RNG";
        const string HEADERS = "index,Hz(top),SPT,RHZ,RVT,RPS,RNG";


        /// <summary>
        /// This DEV method runs the EXECUTE method in this class. It sets up the input/output arguments that go into the Aruments class.
        /// Access to this DEV class is from the EXECUTE class.
        /// Access to the EXECUTE class is currently from the Sandpit.cs class.
        /// "sandpit" as the FIRST AND ONLY command line argument
        /// Activity Codes for other tasks to do with spectrograms and audio files:
        ///
        /// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
        /// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
        /// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
        /// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
        /// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
        /// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
        ///
        /// audiofilecheck - Writes information about audio files to a csv file.
        /// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
        /// audiocutter - Cuts audio into segments of desired length and format
        /// createfoursonograms
        /// </summary>
        public static Arguments Dev()
        {
            // INPUT and OUTPUT DIRECTORIES



            // set up IP and OP directories
            string inputDir = @"C:\SensorNetworks\Output\BIRD50\TrainingCSV";
            //string imageInputDir = @"C:\SensorNetworks\Output\BIRD50\TrainingRidgeImages";
            string OutputDir = @"C:\SensorNetworks\Output\BIRD50\SpeciesTEMPLATES_6dbThresholdVersion4";
            //string imagOutputDireOutputDir = @"C:\SensorNetworks\Output\BIRD50\TestingRidgeImages";
            string speciesLabelsFile = @"C:\SensorNetworks\Output\BIRD50\AmazonBird50_training_output.csv";



            DirectoryInfo ipDir = new DirectoryInfo(inputDir);
            DirectoryInfo opDir = new DirectoryInfo(OutputDir);

            //FileInfo fiSpectrogramConfig = null;
            FileInfo fiSpectrogramConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramFalseColourConfig.yml");

            return new Arguments
            {
                InputDataDirectory = ipDir,
                OutputDirectory = opDir,
                // use the default set of index properties in the AnalysisConfig directory.
                IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
                SpectrogramConfigPath = fiSpectrogramConfig,
                SpeciesLabelsFile = speciesLabelsFile,
                SpeciesCount  = 50,
                InstanceCount = 924, //trainingCount
                //int instanceCount = 375; //testCount
                //instanceCount = 2;

                // background threshold value that is subtracted from all spectrograms.
                BgnThreshold = 3.0,
        };
            throw new Exception();
        } //Dev()


        /// <summary>
        /// AT: NOTE: arguments classes should not exist outside of the AnalysisPrograms project. I had to remove PowerArgs attributes.
        /// </summary>
        public class Arguments
        {
            public DirectoryInfo InputDataDirectory { get; set; }

            public DirectoryInfo OutputDirectory { get; set; }

            public FileInfo IndexPropertiesConfig { get; set; }

            public FileInfo SpectrogramConfigPath { get; set; }

            public int SpeciesCount { get; set; }
            public int InstanceCount { get; set; }
            public string SpeciesLabelsFile { get; set; }
            public double BgnThreshold { get; set; }
        }



        public class Output
        {
            // INIT array of instance IDs obtained from file names
            public string[] FileID = null;
            // INIT array of species ID for each instance
            public int[] SpeciesID = null;
            // INIT array of species counts
            public int[] InstanceNumbersPerSpecies = null;
            // INIT array of frame counts
            public int[] FrameNumbersPerInstance   = null;
            // INIT array of frame counts
            public int[] FrameNumbersPerSpecies = null;
            // length of spectrum array have reduction by max pooling
            public int ReducedSpectralLength = 0;
            // matrix: each row= one instance;  each column = one feature
            public double[,] InstanceFeatureMatrix = null;
            // matrix: each row= one Species;  each column = one feature
            public double[,] SpeciesFeatureMatrix = null;

            public double[,] SimilarityScores = null;

            public int[,] ConfusionMatrix = null;

            public int[,] RankOrderMatrix = null;

            public double[] Weights;
        }


        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
                bool verbose = true; // assume verbose if in dev mode
                if (verbose)
                {
                    string date = "# DATE AND TIME: " + DateTime.Now;
                    LoggedConsole.WriteLine("# ANALYSE THE BIRD-50 dataset from Herve Glotin");
                    LoggedConsole.WriteLine(date);
                    //LoggedConsole.WriteLine("# Spectrogram Config      file: " + arguments.SpectrogramConfigPath);
                    //LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);
                    LoggedConsole.WriteLine();
                } // if (verbose)
            } // if


            // This analysis consits of six steps.

            //[1] Obtain feature representation of every instance.
            //[2] Obtain feature representation of every species.
            //    This is done by summing or averaging the instance representations.
            //[3] Normalisation and Concatentation of spectra.
            //    This can be done in three ways ie (i) Unit length (ii) Unit Area (iii) Unit bounds i.e. 0,1.
            //[4] Calculation of similarity scores
            //    This will be done using Cosine similarity. Could also use Euclidian distance..
            //[5] Calculation of accuracy measures
            //    CONFUSION MATRIX and the RANK ORDER MATRIX.
            //[6] Construct datasets for WEKA machine learning

            //[1] Obtain feature representation of every instance.
            Output output = GetInstanceRepresentations(arguments);

            //[2] Obtain feature representation of every species.
            GetSpeciesRepresentations(arguments, output);
            DrawSpeciesImages(arguments, output);

            //[3] Normalisation and Concatentation of spectra.
            Normalise(arguments, output);

            //[4] Calculation of similarity scores
            CalculateSimilarityScores(arguments, output);

            //[5] Calculation of accuracy measures
            CalculateAccuracy(arguments, output);

            //[6] Construct datasets for WEKA machine learning
            ConstructWekaDatasets(arguments, output);
        } //Execute()



        public static Output GetInstanceRepresentations(Arguments arguments)
        {
            LoggedConsole.WriteLine("1. Read in all Instances and do feature extraction");

            //################################### FEATURE WEIGHTS
            //TRY DIFFERENT WEIGHTINGS assuming following "SPT,RHZ,RVT,RPS,RNG";
            bool doDeltaFeatures = false;
            double[] weights      = { 1.0, 1.0, 0.8, 0.7, 0.7 };
            double[] deltaWeights = { 1.0, 1.0, 0.8, 0.7, 0.7, 0.5, 0.4, 0.4, 0.2, 0.2 };
            if (doDeltaFeatures) weights = deltaWeights;

                //MAX-POOLING for SPECTRAL REDUCTION
                // frequency bins used to reduce dimensionality of the 256 spectral values.
                int startBin = 8;
            int maxOf2Bin = 117;
            int maxOf3Bin = 160;
            int endBin = 200;
            double[] testArray = new double[256];
            for (int i = 0; i < testArray.Length; i++) testArray[i] = i;
            double[] reducedArray = MaxPoolingLimited(testArray, startBin, maxOf2Bin, maxOf3Bin, endBin);
            int reducedSpectralLength = reducedArray.Length;

            LoggedConsole.WriteLine("     Reduced spectral length = " + reducedSpectralLength);
            int instanceCount = arguments.InstanceCount;
            int speciesCount = arguments.SpeciesCount;


            // READ IN THE SPECIES LABELS FILE AND SET UP THE DATA
            string[] fileID = new string[instanceCount];
            int[] speciesID = new int[speciesCount];
            ReadGlotinsSpeciesLabelFile(arguments.SpeciesLabelsFile, instanceCount, out fileID, out speciesID);


            // INIT array of species counts
            int[] instanceNumbersPerSpecies = new int[speciesCount];
            // INIT array of frame counts
            int[] frameNumbersPerInstance   = new int[instanceCount];

            // initialise species description matrix
            var keyArray = FEATURE_KEYS.Split(',');

            int totalFeatureCount = keyArray.Length * reducedArray.Length;
            Console.WriteLine("    Total Feature Count = " + totalFeatureCount);

            if (doDeltaFeatures)
            {
                    totalFeatureCount *= 2;
                LoggedConsole.WriteLine("    Total Delta Feature Count = " + totalFeatureCount);
            }

            // one matrix row per species
            double[,] instanceFeatureMatrix = new double[instanceCount, totalFeatureCount];


            // loop through all all instances
            for (int j = 0; j < instanceCount; j++)
            {
                LoggedConsole.Write(".");
                int frameCount = 0;
                // get the spectral index files
                int speciesLabel = speciesID[j];

                // dictionary to store feature spectra for instance.
                var aggreDictionary = new Dictionary<string, double[]>();
                // dictionary to store delta spectra for instance.
                var deltaDictionary = new Dictionary<string, double[]>();

                foreach (string key in keyArray)
                {
                    string name = string.Format("{0}_Species{1:d2}.{2}.csv", fileID[j], speciesLabel, key);
                    FileInfo file = new FileInfo(Path.Combine(arguments.InputDataDirectory.FullName, name));

                    if (file.Exists)
                    {
                        int binCount;
                        double[,] matrix = IndexMatrices.ReadSpectrogram(file, out binCount);

                        // create or get the array of spectral values.
                        double[] aggregateArray = new double[reducedSpectralLength];
                        double[] deltaArray     = new double[reducedSpectralLength];

                        double[] ipVector = MatrixTools.GetRow(matrix, 0);
                        ipVector = DataTools.SubtractValueAndTruncateToZero(ipVector, arguments.BgnThreshold);
                        reducedArray = MaxPoolingLimited(ipVector, startBin, maxOf2Bin, maxOf3Bin, endBin);
                        double[] previousArray = reducedArray;

                        // transfer spectral values to array.
                        int rowCount = matrix.GetLength(0);
                        //rowCount = (int)Math.Round(rowCount * 0.99); // ###################### USE ONLY 99% of instance
                        //if (rowCount > 1200) rowCount = 1200;
                        for (int r = 1; r < rowCount; r++)
                        {
                            ipVector = MatrixTools.GetRow(matrix, r);
                            ipVector = DataTools.SubtractValueAndTruncateToZero(ipVector, arguments.BgnThreshold);
                            reducedArray = MaxPoolingLimited(ipVector, startBin, maxOf2Bin, maxOf3Bin, endBin);

                            for (int c = 0; c < reducedSpectralLength; c++)
                            {
                                aggregateArray[c] += reducedArray[c];

                                // Calculate the DELTA values TWO OPTIONS ##################################################
                                double delta = Math.Abs(reducedArray[c] - previousArray[c]);
                                //double delta = reducedArray[c] - previousArray[c];
                                //if (delta < 0.0)  delta = 0.0;
                                //double delta = previousArray[c]; //previous array - i.e. do not calculate delta
                                deltaArray[c] += delta;
                            }
                            previousArray = reducedArray;
                        }
                        aggreDictionary[key] = aggregateArray;
                        deltaDictionary[key] = deltaArray;
                        frameCount = rowCount;

                    } //if (file.Exists)
                } //foreach (string key in keyArray)

                instanceNumbersPerSpecies[speciesLabel - 1]++;
                frameNumbersPerInstance[j] += frameCount;

                // create the matrix of instance descriptions which consists of concatenated vectors
                // j = index of instance ID = row number
                int featureID = 0;
                foreach (string key in keyArray)
                {
                    int featureOffset = featureID * reducedSpectralLength;
                    for (int c = 0; c < reducedSpectralLength; c++)
                    {
                        // TWO OPTIONS: SUM OR AVERAGE ######################################
                        //instanceFeatureMatrix[j, featureOffset + c] = dictionary[key][c];
                        instanceFeatureMatrix[j, featureOffset + c] = aggreDictionary[key][c] / frameCount;
                    }
                    featureID++;
                }

                if (doDeltaFeatures)
                {
                    foreach (string key in keyArray)
                    {
                        int featureOffset = featureID * reducedSpectralLength;
                        for (int c = 0; c < reducedSpectralLength; c++)
                        {
                            // TWO OPTIONS: SUM OR AVERAGE ######################################
                            //instanceFeatureMatrix[j, featureOffset + c] = dictionary[key][c];
                            instanceFeatureMatrix[j, featureOffset + c] = deltaDictionary[key][c] / frameCount;
                        }
                        featureID++;
                    }
                } // if doDeltaFeatures

            } // end for loop j over all instances

            LoggedConsole.WriteLine("Done!");


            LoggedConsole.WriteLine("\nSum of species number array = " + instanceNumbersPerSpecies.Sum());
            LoggedConsole.WriteLine("Sum of  frame  number array = " + frameNumbersPerInstance.Sum());
            bool addLineNumbers = true;
            string countsArrayOutputFilePath = Path.Combine(arguments.OutputDirectory.FullName, "BirdClef50_training_Counts.txt");
            FileTools.WriteArray2File(instanceNumbersPerSpecies, addLineNumbers, countsArrayOutputFilePath);

            // Initialise output data arrays
            Output output = new Output();
            output.FileID    = fileID;
            output.SpeciesID = speciesID;
            output.InstanceNumbersPerSpecies = instanceNumbersPerSpecies;
            output.ReducedSpectralLength = reducedSpectralLength;
            // INIT array of frame counts
            output.FrameNumbersPerInstance = frameNumbersPerInstance;
            // matrix: each row= one instance;  each column = one feature
            output.InstanceFeatureMatrix = instanceFeatureMatrix;

            output.Weights = weights;


            return output;
        } // GetInstanceRepresentations()


        public static void GetSpeciesRepresentations(Arguments arguments, Output output)
        {
            LoggedConsole.WriteLine("\n\n2a. Obtain feature representation of every species.");

            int instanceCount = arguments.InstanceCount;
            int speciesCount = arguments.SpeciesCount;
            var keyArray = FEATURE_KEYS.Split(',');

            int featureCount = output.InstanceFeatureMatrix.GetLength(1);

            // initialise species description matrix
            double[,] speciesFeatureMatrix = new double[speciesCount, featureCount];
            int[] frameNumbersPerSpecies = new int[speciesCount];


            // loop through all 50 species
            for (int i = 0; i < speciesCount; i++)
            {
                int speciesLabel = i + 1;
                LoggedConsole.Write(" " + speciesLabel);

                // loop through all instances multiple times - once for each species
                for (int j = 0; j < instanceCount; j++)
                {
                    if (output.SpeciesID[j] != speciesLabel) continue;

                    //aggregate the instance feature values
                    double[] ipVector = MatrixTools.GetRow(output.InstanceFeatureMatrix, j);
                    for (int c = 0; c < featureCount; c++)
                    {
                        speciesFeatureMatrix[i, c] += ipVector[c];
                    }

                    //output.InstanceNumbersPerSpecies[i]++;
                    frameNumbersPerSpecies[i] += output.FrameNumbersPerInstance[j];
                } // end for loop j over all instances

            } // loop through all 50 species
            LoggedConsole.WriteLine(" Done");

            output.SpeciesFeatureMatrix   = speciesFeatureMatrix;
            output.FrameNumbersPerSpecies = frameNumbersPerSpecies;

        } // GetSpeciesRepresentations()


        public static void DrawSpeciesImages(Arguments arguments, Output output)
        {
            LoggedConsole.WriteLine("2b. Draw feature representation of every species.");
            int scalingFactor = 20;
            int imageHeight = 100;

            int speciesCount = arguments.SpeciesCount;
            var keyArray = FEATURE_KEYS.Split(',');

            int featureCount = keyArray.Length * output.ReducedSpectralLength;

            // loop through all 50 species
            for (int r = 0; r < speciesCount; r++)
            {
                double[] ipVector = MatrixTools.GetRow(output.SpeciesFeatureMatrix, r);

                // now make images
                var images = new List<Image>();
                int featureID = 0;
                foreach (string key in keyArray)
                {
                    double[] vector = new double[output.ReducedSpectralLength];
                    int featureOffset = featureID * output.ReducedSpectralLength;
                    for (int c = 0; c < output.ReducedSpectralLength; c++)
                    {
                        vector[c] = ipVector[featureOffset + c];
                    }
                    featureID++;

                    vector = DataTools.Normalise2Probabilites(vector);
                    vector = DataTools.filterMovingAverage(vector, 3);
                    string label = string.Format("{0} {1} ({2})", (r + 1), key, output.InstanceNumbersPerSpecies[r]);
                    Image image = ImageTools.DrawGraph(label, vector, output.ReducedSpectralLength, imageHeight, scalingFactor);
                    images.Add(image);
                }
                Image combinedImage = ImageTools.CombineImagesVertically(images);
                string outputFileName = string.Format("Species{0}.SpectralFeatures.png", (r + 1));
                string path = Path.Combine(arguments.OutputDirectory.FullName, outputFileName);
                combinedImage.Save(path);

            } // loop through 50 species
        }

        /// <summary>
        /// Normalisation and Concatentation of spectra:
        /// can be done in three ways ie (i) Unit length (ii) Unit Area (iii) Unit bounds i.e. 0,1.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="output"></param>
        public static void Normalise(Arguments arguments, Output output)
        {

            var keyArray = FEATURE_KEYS.Split(',');
            int speciesCount = arguments.SpeciesCount;
            int instanceCount = arguments.InstanceCount;

            // loop through all species
            for (int r = 0; r < speciesCount; r++)
            {
                double[] ipVector = MatrixTools.GetRow(output.SpeciesFeatureMatrix, r);
                double[] normedVector = NormaliseVector(ipVector, output.Weights);

                for (int c = 0; c < normedVector.Length; c++)
                {
                    output.SpeciesFeatureMatrix[r, c] = normedVector[c];
                }
            }

            // loop through all instances
            for (int r = 0; r < instanceCount; r++)
            {
                double[] ipVector = MatrixTools.GetRow(output.InstanceFeatureMatrix, r);
                double[] normedVector = NormaliseVector(ipVector, output.Weights);

                for (int c = 0; c < normedVector.Length; c++)
                {
                    output.InstanceFeatureMatrix[r, c] = normedVector[c];
                }

            } // end for loop r over all instances
        }

        /// <summary>
        /// Normalises the parts of a concatenated vector separately.
        /// Finally does a unit length norm in prepration for a dot product to give cosine similairty.
        /// </summary>
        /// <param name="ipVector"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static double[] NormaliseVector(double[] ipVector, double[] weights)
        {
            int partCount = weights.Length;

            int partialLength = ipVector.Length / partCount;
            double[] normedVector = new double[ipVector.Length];
            for (int i = 0; i < partCount; i++)
            {
                int offset = i * partialLength;
                double[] subvector = DataTools.Subarray(ipVector, offset, partialLength);

                // NOTE: HERE WE HAVE SMOOTHING OPTIONS  ###############################################
                // window = 3 made things worse. Did not try any more.
                //subvector = DataTools.filterMovingAverage(subvector, 3);

                // NOTE: HERE WE HAVE THREE OPTIONS  ###############################################
                //subvector = DataTools.Normalise2Probabilites(subvector);
                subvector = DataTools.normalise2UnitLength(subvector);
                //subvector = DataTools.normalise(subvector);

                for (int j = 0; j < partialLength; j++)
                {
                    // ######################################WEIGHTINGS
                    //normedVector[offset + j] = subvector[j];
                    normedVector[offset + j] = subvector[j] * weights[i];
                }
            }

            // finally do a unit length normalisatsion
            normedVector = DataTools.normalise2UnitLength(normedVector);
            return normedVector;
        }



        /// <summary>
        /// This done using Cosine similarity. Could also use Euclidian distance.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public static void CalculateSimilarityScores(Arguments arguments, Output output)
        {
            int speciesCount = arguments.SpeciesCount;
            int instanceCount = arguments.InstanceCount;
            output.SimilarityScores = new double[instanceCount, speciesCount];

            // loop through all instances
            for (int r = 0; r < instanceCount; r++)
            {
                double[] instance = MatrixTools.GetRow(output.InstanceFeatureMatrix, r);

                for (int s = 0; s < speciesCount; s++)
                {
                    double[] species = MatrixTools.GetRow(output.SpeciesFeatureMatrix, s);
                    double similarity = DataTools.DotProduct(instance, species);
                    output.SimilarityScores[r,s] = similarity;
                }

            } // end for loop r over all instances
        }


        /// <summary>
        /// Produce a CONFUSION MATRIX and a RANK ORDER MATRIX.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public static void CalculateAccuracy(Arguments arguments, Output output)
        {
            int maxRank = 10;
            int speciesCount = arguments.SpeciesCount;
            int instanceCount = arguments.InstanceCount;
            output.ConfusionMatrix = new int[speciesCount, speciesCount];
            output.RankOrderMatrix = new int[instanceCount, maxRank];

            // loop through all instances
            for (int r = 0; r < instanceCount; r++)
            {
                int correctID = output.SpeciesID[r] - 1;
                double[] instanceScores = MatrixTools.GetRow(output.SimilarityScores, r);
                int maxID = DataTools.GetMaxIndex(instanceScores);
                output.ConfusionMatrix[correctID, maxID] ++;

                // calculate rank order matrix.
                if (maxID == correctID)
                {
                    output.RankOrderMatrix[r, 0] = 1;
                }
                instanceScores[maxID] = 0.0;
                for (int rank = 1; rank < maxRank; rank++)
                {
                    maxID = DataTools.GetMaxIndex(instanceScores);
                    if (maxID == correctID)
                    {
                        output.RankOrderMatrix[r, rank] = 1;
                        break;
                    }
                    instanceScores[maxID] = 0.0;
                }

            } // end for loop r over all instances


            int diagonalSum = 0;
            for (int r = 0; r < speciesCount; r++)
            {
                    diagonalSum += output.ConfusionMatrix[r, r];
            }
            LoggedConsole.WriteLine("Diagonal Sum = " + diagonalSum);
            LoggedConsole.WriteLine("% Accuracy = " + (100 * diagonalSum / instanceCount));


            LoggedConsole.WriteLine("% Rank");
            for (int rank = 0; rank < maxRank; rank++)
            {
                var colSum = MatrixTools.SumColumn(output.RankOrderMatrix, rank);
                double acc = 100 * colSum / (double)instanceCount;
                string str = string.Format("{0}   % Acc = {1:f2}", rank, acc);
                LoggedConsole.WriteLine(str);
            }


        }


        /// <summary>
        /// Construct datasets for WEKA machine learning
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="output"></param>
        public static void ConstructWekaDatasets(Arguments arguments, Output output)
        {
            // write the similarity score data set
            // write the header
            //output.SimilarityScores
            int speciesCount = arguments.SpeciesCount;
            int instanceCount = arguments.InstanceCount;

            // write header to csv file
            StringBuilder line = new StringBuilder();
            for (int s = 1; s <= speciesCount; s++)
            {
                line.Append(s + ",");
            }
            line.Append("class");

            var lines = new List<string>();
            lines.Add(line.ToString());

            for (int i = 0; i < instanceCount; i++)
            {
                line = new StringBuilder();
                for (int s = 0; s < speciesCount; s++)
                {
                    line.Append(output.SimilarityScores[i, s] + ",");
                }
                line.Append("'" + output.SpeciesID[i] + "'");
                lines.Add(line.ToString());
            }
            string outputFileName = string.Format("InstanceBySpecies.SimilarityScores1.csv");
            string path = Path.Combine(arguments.OutputDirectory.FullName, outputFileName);
            FileTools.WriteTextFile(path, lines.ToArray());


            // write the InstanceFeatureMatrix data set
            var keyArray = FEATURE_KEYS.Split(',');
            int spectralLength = output.ReducedSpectralLength;
            int featureCount = keyArray.Length * spectralLength;
            // write header to csv file
            line = new StringBuilder();
            for (int k = 0; k < keyArray.Length; k++)
            {
                for (int f = 1; f <= spectralLength; f++)
                {
                    line.Append(keyArray[k] + f + ",");
                }
            }
            line.Append("class");

            lines = new List<string>();
            lines.Add(line.ToString());

            for (int i = 0; i < instanceCount; i++)
            {
                line = new StringBuilder();
                for (int f = 0; f < featureCount; f++)
                {
                    line.Append(output.InstanceFeatureMatrix[i, f] + ",");
                }
                line.Append("'" + output.SpeciesID[i] + "'");
                lines.Add(line.ToString());
            }
            outputFileName = string.Format("InstanceByFeaturesDataSet.csv");
            path = Path.Combine(arguments.OutputDirectory.FullName, outputFileName);
            FileTools.WriteTextFile(path, lines.ToArray());



        } // ConstructWekaDatasets()


        public static void ReadGlotinsSpeciesLabelFile(string speciesLabelsFile, int count, out string[] fileID, out int[] speciesID)
        {
            // READ IN THE SPECIES LABELS FILE AND SET UP THE DATA
            var lines = new List<string>();
            if (speciesLabelsFile != null) lines = FileTools.ReadTextFile(speciesLabelsFile);

            speciesID = new int[lines.Count];
            fileID = new string[lines.Count];

            if (((speciesLabelsFile != null)) && (lines.Count != count))
            {
                LoggedConsole.WriteLine("lineCount != count    {0}  !=  {1}", lines.Count, count);
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                string[] words = lines[i].Split(',');
                fileID[i] = words[0];
                speciesID[i] = int.Parse(words[1]);
            }
        } // ReadGlotinsSpeciesLabelFile()


        public static void WriteDataTODOTODOTODOTODO()
        {

            // write inf to csv file
            //var lines = new List<string>();
            //    lines.Add(HEADERS);
            //    for (int c = 0; c < reducedSpectralLength; c++)
            //    {
            //        int herz = (int)Math.Round((c + 8) * 43.066);
            //        string line = (c + 1).ToString() + ",";
            //        line += herz.ToString() + ",";
            //        foreach (string key in keyArray)
            //        {
            //            line += dictionary[key][c] + ",";
            //        }
            //        lines.Add(line);
            //    }

            //string outputFileName = String.Format("Species{0}.SpectralFeatures.csv", speciesLabel);
            //string path = Path.Combine(arguments.OutputDirectory.FullName, outputFileName);
            //FileTools.WriteTextFile(path, lines.ToArray());


        }




        public static double[,] ReduceMatrixColumns(double[,] matrix, int minBin, int maxBin)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int reducedColCount = maxBin - minBin + 1;

            double[,] returnMatrix = new double[rows, reducedColCount];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < reducedColCount; c++)
                {
                    returnMatrix[r, c] = matrix[r, minBin + c];
                }
            }
            return returnMatrix;
        }

        public static double[,] MaxPoolMatrixColumns(double[,] matrix, int reducedColCount)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            double[,] returnMatrix = new double[rows, reducedColCount];
            for (int r = 0; r < rows; r++)
            {
                var rowVector = MatrixTools.GetRow(matrix, r);
                int[] bounds = { 8, 23, 53, 113, 233 };
                // ie reduce the 256 vector to 4 values
                for (int c = 0; c < reducedColCount; c++)
                {
                    int length = bounds[c + 1] - bounds[c];
                    double[] subvector = DataTools.Subarray(rowVector, bounds[c], length);
                    int max = DataTools.GetMaxIndex(subvector);
                    returnMatrix[r, c] = subvector[max];
                }
            }

            return returnMatrix;
        }
        public static double[,] ExoticMaxPoolingMatrixColumns(double[,] matrix, int reducedColCount)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            double[,] returnMatrix = new double[rows, reducedColCount];
            for (int r = 0; r < rows; r++)
            {
                var rowVector = MatrixTools.GetRow(matrix, r);
                // ie reduce the second half of vector by factor of two.
                for (int c = 0; c < 100; c++)
                {
                    returnMatrix[r, c] = rowVector[c];
                }

                int offset = 0;
                for (int c = 100; c < reducedColCount; c++)
                {
                    returnMatrix[r, c] = rowVector[c + offset];
                    offset += 1;
                }
            }

            return returnMatrix;
        }


        public static double[,] MaxPoolingLimited(double[,] M, int startBin, int maxOf2Bin, int maxOf3Bin, int endBin, int reducedBinCount)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);

            var reducedM = new double[rows, reducedBinCount];
            for (int r = 0; r < rows; r++)
            {
                var rowVector = MatrixTools.GetRow(M, r);
                double[] V = MaxPoolingLimited(rowVector, startBin, maxOf2Bin, maxOf3Bin, endBin);

                for (int c = 0; c < reducedBinCount; c++)
                {
                    reducedM[r, c] = V[c];
                }
            }
            return reducedM;
        }

        /// <summary>
        /// reduces the dimensionality of a vector by max pooling.
        /// Used specifically for representation of spectral frames in Herve Glotin work
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="startBin"></param>
        /// <param name="maxOf2Bin"></param>
        /// <param name="maxOf3Bin"></param>
        /// <param name="endBin"></param>
        /// <returns></returns>
        public static double[] MaxPoolingLimited(double[] vector, int startBin, int maxOf2Bin, int maxOf3Bin, int endBin)
        {
            double value = 0.0;
            List<double> opVector = new List<double>();
            for (int i = startBin; i < maxOf2Bin; i++)
            {
                opVector.Add(vector[i]);
            }
            for (int i = maxOf2Bin; i < maxOf3Bin; i++)
            {
                value = vector[i];
                if (value < vector[i + 1]) value = vector[i + 1];
                opVector.Add(value);
                i++;
            }
            for (int i = maxOf3Bin; i < endBin; i++)
            {
                value = vector[i];
                if (value < vector[i + 1]) value = vector[i + 1];
                if (value < vector[i + 2]) value = vector[i + 2];
                opVector.Add(value);
                i += 2;
            }

            return opVector.ToArray();
        }


        public static double[,] MaxPoolMatrixColumnsByFactor(double[,] matrix, int factor)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int reducedColCount = cols / factor;

            double[,] returnMatrix = new double[rows, reducedColCount];
            for (int r = 0; r < rows; r++)
            {
                var rowVector = MatrixTools.GetRow(matrix, r);
                int lowerBound = 0;
                // ie reduce the 256 vector to 4 values
                for (int c = 0; c < reducedColCount; c++)
                {
                    double[] subvector = DataTools.Subarray(rowVector, lowerBound, factor);
                    int max = DataTools.GetMaxIndex(subvector);
                    returnMatrix[r, c] = subvector[max];
                    lowerBound += factor;
                }
            }

            return returnMatrix;
        }



    }
}
