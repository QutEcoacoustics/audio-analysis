// <copyright file="PcaWhitening.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using Accord.Math;
    using Accord.Statistics.Analysis;
    using System;
    using System.Collections.Generic;
    using TowseyLibrary;

    public static class PcaWhitening
    {
        //Outputting the Projection Matrix, whitened matrix, eigen vectors, and the number of pca components
        //that is used to to transform the data into the new feature subspace.
        //in Accord.net, this matrix is called "ComponentVectors", which its columns contain the
        //principle components, known as Eigenvectors.
        public static Tuple<double[,], double[,], double[,], int> Whitening(double[,] spectrogram)
        {

            if (spectrogram == null)
            {
                throw new ArgumentNullException("spectrogram");
            }

            // Step 1: convert matrix to a jagged array
            double[][] jaggedArr = spectrogram.ToJagged();

            // Step 2: do PCA whitening
            var pca = new PrincipalComponentAnalysis()
            {
                //the "Center" method only subtracts the mean.
                Method = PrincipalComponentMethod.Center,
                Whiten = true,
            };

            pca.Learn(jaggedArr);

            double[][] output1 = pca.Transform(jaggedArr);

            //according to Dieleman's paper, exp var = 0.99 (Multiscale approaches to music audio feature learning)
            pca.ExplainedVariance = 0.95;

            //double[,] projectionMatrix = pca.Transform(jaggedArr, );
            double[][] output2 = pca.Transform(jaggedArr);
            double[,] projectedData = output2.ToMatrix();
            double[,] eigenVectors = pca.ComponentVectors.ToMatrix();
            int components = pca.Components.Count;

            //double[] eigneValues = pca.Eigenvalues; //sorted
            //int rows = projectedData.GetLength(0);
            //int columns = projectedData.GetLength(1); //this is actually the number of output vectors before reversion

            // Step 3: revert a set of projected data into its original space
            //the output of the "Revert(Double[][])" method in Accord did not make sense.
            //however, we use its API to do so.
            double[,] reversion = Revert(projectedData, eigenVectors, components);

            //Build Projection Matrix
            //To do so, we need eigenVectors, and the number of columns of the projected data
            double[,] projectionMatrix = GetProjectionMatrix(eigenVectors, projectedData.GetLength(1));
            //double[][] m = projectionMatrix.ToJagged();

            //write the projection matrix to disk


            /*
            //FIRST STEP: sort the eigenvectors based on the eigenvalue
            var eigPairs = new List<Tuple<double, double[]>>();

            for (int i = 0; i < eigneValues.GetLength(0); i++)
            {
                eigPairs.Add(Tuple.Create(Math.Abs(eigneValues[i]), GetColumn(eigenVectors, i)));
            }

            //sort in descending order based on the eigenvalues
            eigPairs.Sort((x, y) => y.Item1.CompareTo(x.Item1));
            */

            //return reversion;
            return new Tuple<double[,], double[,], double[,], int>(projectionMatrix, reversion, eigenVectors, components);
        }

        // RMS Normalization
        public static double[,] RmsNormalization(double[,] spectrogram)
        {
            double s = 0;
            double[,] normSpec = new double[spectrogram.GetLength(0), spectrogram.GetLength(1)];
            for (int i = 0; i < spectrogram.GetLength(0); i++)
            {
                for (int j = 0; j < spectrogram.GetLength(1); j++)
                {
                    s += spectrogram[i, j] * spectrogram[i, j];
                }
            }

            double rms = Math.Sqrt(s / (spectrogram.GetLength(0) * spectrogram.GetLength(1)));

            for (int i = 0; i < spectrogram.GetLength(0); i++)
            {
                for (int j = 0; j < spectrogram.GetLength(1); j++)
                {
                    normSpec[i, j] = spectrogram[i, j] / rms;
                }
            }

            return normSpec;
        }

        //retrieving a full column of a matrix
        //colNo is the column index we want to access
        public static double[] GetColumn(double[,] matrix, int colNo)
        {
            double[] col = new double[matrix.GetLength(0)];
            for (int r = 0; r < matrix.GetLength(0); r++)
            {
                col[r] = matrix[r, colNo];
            }

            return col;
        }

        //retrieving a full row of a matrix
        //rowNo is the column index we want to access
        public static double[] GetRow(double[,] matrix, int rowNo)
        {
            double[] row = new double[matrix.GetLength(1)];
            for (int c = 0; c < matrix.GetLength(1); c++)
            {
                row[c] = matrix[rowNo, c];
            }

            return row;
        }

        //Build the Projection Matrix
        //To do so, we need eigenVectors
        //and the number of columns of the projected data which is the number of outputs (principle components) used to transform the data
        public static double[,] GetProjectionMatrix(double[,] eigenVector, int numberOfOuputs)
        {
            double[,] projMatrix = new double[eigenVector.GetLength(0), eigenVector.GetLength(1)];

            for (int j = 0; j < eigenVector.GetLength(1); j++)
            {
                for (int i = 0; i < numberOfOuputs; i++)
                {
                    projMatrix[i, j] = eigenVector[i, j];
                }

                for (int k = numberOfOuputs; k < eigenVector.GetLength(0); k++)
                {
                    projMatrix[k, j] = 0;
                }
            }

            return projMatrix;
        }

        //revert a set of projected data into its original space
        //the output of the "Revert(Double[][])" method in Accord did not make sense.
        //however, we use its API to do so.
        public static double[,] Revert(double[,] projectedData, double[,] eigenVectors, int numberOfComponents)
        {
            int rows = projectedData.GetLength(0);
            int columns = projectedData.GetLength(1); //this is actually the number of output vectors before reversion
            //int components = pca.Components.Count;
            double[,] reversion = new double[rows, numberOfComponents];

            for (int i = 0; i < numberOfComponents; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < columns; k++)
                    {
                        reversion[j, i] += projectedData[j, k] * eigenVectors[k, i];
                    }
                }
            }

            return reversion;
        }

        //reconstruct the spectrogram using sequential patches and the projection matrix
        public static double[,] ReconstructSpectrogram(double[,] projectionMatrix, double[,] sequentialPatchMatrix, double[,] eigenVectors, int numberOfComponents)
        {
            double[][] patches = new double[sequentialPatchMatrix.GetLength(0)][];
            //new double[sequentialPatchMatrix.GetLength(0)] [sequentialPatchMatrix.GetLength(1)];
            //List<double[]> patches = new List<double[]>();
            for (int i = 0; i < sequentialPatchMatrix.GetLength(0); i++)
            {
                double[] patch = GetRow(sequentialPatchMatrix, i);
                //double[] cleanedPatch = projectionMatrix.Multiply(patch);
                double[] cleanedPatch = projectionMatrix.Dot(patch);
                //patches.Add(cleanedPatch);
                patches[i] = cleanedPatch;
            }

            double[,] cleanedPatches = patches.ToMatrix();
            double[,] reconsSpec = Revert(cleanedPatches, eigenVectors, numberOfComponents);
            return reconsSpec;
        }

        public static double[,] NoiseReduction(double[,] matrix)
        {
            double[,] nrm = matrix;

            //calculate modal noise profile
            //NoiseProfile profile = NoiseProfile.CalculateModalNoiseProfile(matrix, sdCount: 0.0);
            NoiseProfile profile = NoiseProfile.CalculateMedianNoiseProfile(matrix);

            //smooth the noise profile
            double[] smoothedProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, width: 7);

            nrm = SNR.TruncateBgNoiseFromSpectrogram(nrm, smoothedProfile);

            //nrm = SNR.NoiseReduce_Standard(nrm, smoothedProfile, nhBackgroundThreshold: 2.0);

            return nrm;
        }
    }
}
