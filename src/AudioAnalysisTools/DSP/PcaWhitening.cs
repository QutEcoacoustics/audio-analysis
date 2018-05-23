// <copyright file="PcaWhitening.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using Accord.Math;
    using Accord.Statistics.Analysis;
    using TowseyLibrary;

    public static class PcaWhitening
    {
        /// <summary>
        /// Outputting the Projection Matrix, whitened matrix, eigen vectors, and the number of PCA components
        /// that is used to to transform the data into the new feature subspace.
        /// in Accord.net, this matrix is called "ComponentVectors", which its columns contain the
        /// principle components, known as Eigenvectors.
        /// </summary>
        public class Output
        {
            public double[,] ProjectionMatrix { get; set; }

            public double[,] Reversion { get; set; }

            public double[,] EigenVectors { get; set; }

            public int Components { get; set; }
        }

        public static Output Whitening(double[,] matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

            // Step 1: convert matrix to a jagged array
            double[][] jaggedArray = matrix.ToJagged();

            // Step 2: do PCA whitening
            var pca = new PrincipalComponentAnalysis()
            {
                // the "Center" method only subtracts the mean.
                Method = PrincipalComponentMethod.Center,
                Whiten = true,
            };

            pca.Learn(jaggedArray);

            pca.Transform(jaggedArray);

            pca.ExplainedVariance = 0.95;

            double[][] transformedData = pca.Transform(jaggedArray);
            double[,] projectedData = transformedData.ToMatrix();
            double[,] eigenVectors = pca.ComponentVectors.ToMatrix();
            int components = pca.Components.Count;

            // double[] eigneValues = pca.Eigenvalues; //sorted
            // int rows = projectedData.GetLength(0);
            int columns = projectedData.GetLength(1); //this is actually the number of output vectors before reversion

            // Step 3: revert a set of projected data into its original space
            // the output of the "Revert(Double[][])" method in Accord did not make sense.
            // however, we use its API to do so.
            double[,] reversion = Revert(projectedData, eigenVectors, components);

            // Build Projection Matrix
            // To do so, we need eigenVectors, and the number of columns of the projected data
            double[,] projectionMatrix = GetProjectionMatrix(eigenVectors, columns);

            // write the projection matrix to disk
            /*
            // FIRST STEP: sort the eigenvectors based on the eigenvalue
            var eigPairs = new List<Tuple<double, double[]>>();

            for (int i = 0; i < eigneValues.GetLength(0); i++)
            {
                eigPairs.Add(Tuple.Create(Math.Abs(eigneValues[i]), GetColumn(eigenVectors, i)));
            }

            // sort in descending order based on the eigenvalues
            eigPairs.Sort((x, y) => y.Item1.CompareTo(x.Item1));
            */
            var output = new Output()
            {
                ProjectionMatrix = projectionMatrix,
                Reversion = reversion,
                EigenVectors = eigenVectors,
                Components = components,
            };

            return output;
        }

        /// <summary>
        /// Build the Projection Matrix
        /// To do so, we need eigenVectors and the number of columns of the projected data
        /// which is the number of outputs (principle components) used to transform the data
        /// </summary>
        public static double[,] GetProjectionMatrix(double[,] eigenVector, int numberOfOutputs)
        {
            double[,] projectionMatrix = eigenVector.EmptyCopy();

            for (int j = 0; j < eigenVector.GetLength(1); j++)
            {
                for (int i = 0; i < numberOfOutputs; i++)
                {
                    projectionMatrix[i, j] = eigenVector[i, j];
                }

                for (int k = numberOfOutputs; k < eigenVector.GetLength(0); k++)
                {
                    projectionMatrix[k, j] = 0;
                }
            }

            return projectionMatrix;
        }

        /// <summary>
        /// revert a set of projected data into its original space
        /// the output of the "Revert(Double[][])" method in Accord did not make sense.
        /// however, we use its API to do so.
        /// </summary>
        public static double[,] Revert(double[,] projectedData, double[,] eigenVectors, int numberOfComponents)
        {
            int rows = projectedData.GetLength(0);

            // this is actually the number of output vectors before reversion
            int columns = projectedData.GetLength(1);
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

        /// <summary>
        /// reconstruct the spectrogram using sequential patches and the projection matrix
        /// </summary>
        public static double[,] ReconstructSpectrogram(double[,] projectionMatrix, double[,] sequentialPatchMatrix, double[,] eigenVectors, int numberOfComponents)
        {
            double[][] patches = new double[sequentialPatchMatrix.GetLength(0)][];
            for (int i = 0; i < sequentialPatchMatrix.GetLength(0); i++)
            {
                double[] patch = sequentialPatchMatrix.GetRow(i);
                double[] cleanedPatch = projectionMatrix.Dot(patch);
                patches[i] = cleanedPatch;
            }

            double[,] cleanedPatches = patches.ToMatrix();
            double[,] reconstructedSpectrogram = Revert(cleanedPatches, eigenVectors, numberOfComponents);
            return reconstructedSpectrogram;
        }

        /// <summary>
        /// Median Noise Reduction
        /// </summary>
        public static double[,] NoiseReduction(double[,] matrix)
        {
            double[,] nrm = matrix;

            // calculate modal noise profile
            // NoiseProfile profile = NoiseProfile.CalculateModalNoiseProfile(matrix, sdCount: 0.0);
            NoiseProfile profile = NoiseProfile.CalculateMedianNoiseProfile(matrix);

            // smooth the noise profile
            double[] smoothedProfile = DataTools.filterMovingAverage(profile.NoiseThresholds, width: 7);

            nrm = SNR.TruncateBgNoiseFromSpectrogram(nrm, smoothedProfile);

            // nrm = SNR.NoiseReduce_Standard(nrm, smoothedProfile, nhBackgroundThreshold: 2.0);

            return nrm;
        }
    }
}
