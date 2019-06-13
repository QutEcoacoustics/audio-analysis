// <copyright file="ZoomCommon.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.LongDurationSpectrograms.Zooming
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Indices;
    using TowseyLibrary;

    public static class ZoomCommon
    {
        public static (Dictionary<string, double[,]>, Dictionary<string, IndexProperties>) LoadSpectra(
            AnalysisIoInputDirectory io,
            string analysisTag,
            string fileStem,
            LdSpectrogramConfig config,
            Dictionary<string, IndexProperties> indexProperties)
        {
            var keys = config.GetKeys().Distinct();

            // add two necessary keys for zooming
            keys = keys.ToList().Append("SUM");
            keys = keys.ToList().Append("DIF");

            //add following matrix for possible subsequent BNG combination matrix.
            string comboIndexID = "RHZ";
            keys = keys.ToList().Append(comboIndexID);

            var relevantIndexProperties = keys.ToDictionary(x => x, x => indexProperties[x]);

            Dictionary<string, double[,]> spectra = IndexMatrices.ReadSpectralIndices(
                io.InputBase,
                fileStem,
                analysisTag,
                keys.ToArray());

            // Make a BNG COMBINATION Spectral matrix.
            //var comboMatrix = MatrixTools.MaxOfTwoMatrices(spectra["BNG"], spectra["RHZ"]);
            var comboMatrix = MatrixTools.AddMatricesWeightedSum(spectra["BGN"], 1.0, spectra[comboIndexID], 20.0);
            spectra["BGN"] = comboMatrix;

            return (spectra, relevantIndexProperties);
        }

        public static Image DrawIndexSpectrogramCommon(
            LdSpectrogramConfig config,
            IndexGenerationData indexGenerationData,
            Dictionary<string, IndexProperties> indexProperties,
            TimeSpan startTime,
            TimeSpan endTime,
            TimeSpan dataScale,
            TimeSpan imageScale,
            int imageWidth,
            Dictionary<string, double[,]> spectra,
            string basename)
        {
            double scalingFactor = Math.Round(imageScale.TotalMilliseconds / dataScale.TotalMilliseconds);
            Contract.Requires(scalingFactor >= 1.0, $"Compression scale `{scalingFactor}`is invalid");

            // calculate data duration from column count of abitrary matrix
            //TimeSpan dataDuration = TimeSpan.FromSeconds(matrix.GetLength(1) * dataScale.TotalSeconds);
            int columnCount = spectra.FirstValue().GetLength(1);

            var startIndex = (int)(startTime.Ticks / dataScale.Ticks);
            var endIndex = (int)(endTime.Ticks / dataScale.Ticks);

            Contract.Ensures(endIndex <= columnCount);

            // extract subset of target data
            var spectralSelection = new Dictionary<string, double[,]>();
            foreach (string key in spectra.Keys)
            {
                var matrix = spectra[key];
                int rowCount = matrix.GetLength(0);

                spectralSelection[key] = MatrixTools.Submatrix(matrix, 0, startIndex, rowCount - 1, endIndex - 1);
                Contract.Ensures(
                    spectralSelection[key].GetLength(1) == (endTime - startTime).Ticks / dataScale.Ticks,
                    "The expected number of frames should be extracted.");
            }

            // compress spectrograms to correct scale
            if (scalingFactor > 1)
            {
                // we add rounding to the compression so that fractional pixels get rendered
                spectralSelection = IndexMatrices.CompressIndexSpectrograms(
                    spectralSelection,
                    imageScale,
                    dataScale,
                    d => Math.Round(d, MidpointRounding.AwayFromZero));
            }
            else
            {
                // this else is unnecessary - completely defensive code
                Contract.Ensures(scalingFactor == 1);
            }

            // check that have not compressed matrices to zero length
            if (spectralSelection.FirstValue().GetLength(0) == 0 || spectralSelection.FirstValue().GetLength(1) == 0)
            {
                throw new InvalidOperationException("Spectral matrices compressed to zero size");
            }

            // DEFINE the DEFAULT colour maps for the false-colour spectrograms
            // Then obtain values from spectrogramDrawingConfig. NOTE: WE REQUIRE LENGTH = 11 chars.
            string colorMap1 = "ACI-ENT-EVN";
            if (config.ColorMap1 != null && config.ColorMap1.Length == 11)
            {
                colorMap1 = config.ColorMap1;
            }

            string colorMap2 = "BGN-PMN-EVN";
            if (config.ColorMap2 != null && config.ColorMap2.Length == 11)
            {
                colorMap2 = config.ColorMap2;
            }

            double backgroundFilterCoeff = indexGenerationData.BackgroundFilterCoeff;

            // double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation
            var cs1 = new LDSpectrogramRGB(config, indexGenerationData, colorMap1)
            {
                FileName = basename,
                BackgroundFilter = backgroundFilterCoeff,
            };
            cs1.SetSpectralIndexProperties(indexProperties); // set the relevant dictionary of index properties
            cs1.LoadSpectrogramDictionary(spectralSelection);

            // set up piecewise linear function to determine colour weights
            var logResolution = Math.Log(imageScale.TotalMilliseconds, 2);
            double upperResolution = Math.Log(32768, 2);
            double lowerResolution = Math.Log(256, 2);
            double range = upperResolution - lowerResolution;
            double blendWeight1;
            if (logResolution >= upperResolution)
            {
                blendWeight1 = 1.0;
            }
            else if (logResolution <= lowerResolution)
            {
                blendWeight1 = 0.0;
            }
            else
            {
                blendWeight1 = (logResolution - lowerResolution) / range;
            }

            double blendWeight2 = 1 - blendWeight1;

            //else if (imageScaleInMsPerPixel > 2000)
            //{
            //    blendWeight1 = 0.7;
            //    blendWeight2 = 0.3;
            //}
            //else if (imageScaleInMsPerPixel > 1000)
            //{
            //    blendWeight1 = 0.3;
            //    blendWeight2 = 0.7;
            //}
            //else if (imageScaleInMsPerPixel > 500)
            //{
            //    // > 0.5 seconds
            //    blendWeight1 = 0.2;
            //    blendWeight2 = 0.8;
            //}
            //else if (imageScaleInMsPerPixel > 300)
            //{
            //    // > 0.5 seconds
            //    blendWeight1 = 0.1;
            //    blendWeight2 = 0.9;
            //}

            var ldfcSpectrogram = cs1.DrawBlendedFalseColourSpectrogram(colorMap1, colorMap2, blendWeight1, blendWeight2);
            if (ldfcSpectrogram == null)
            {
                throw new InvalidOperationException("Null Image returned from DrawBlendedFalseColourSpectrogram");
            }

            return ldfcSpectrogram;
        }
    }
}