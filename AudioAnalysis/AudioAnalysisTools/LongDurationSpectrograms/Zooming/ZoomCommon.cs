// <copyright file="ZoomCommon.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.LongDurationSpectrograms.Zooming
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using Acoustics.Shared.Contracts;
    using Indices;
    using TowseyLibrary;

    public static class ZoomCommon
    {
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
            Contract.Requires(scalingFactor >= 1.0);

            // calculate data duration from column count of abitrary matrix
            var matrix = spectra["ACI"]; // assume this key will always be present!!
            TimeSpan dataDuration = TimeSpan.FromSeconds(matrix.GetLength(1) * dataScale.TotalSeconds);
            int columnCount = matrix.GetLength(1);

            var startIndex = (int)(startTime.Ticks / dataScale.Ticks);
            var endIndex = (int)(endTime.Ticks / dataScale.Ticks);
//            if (endIndex >= columnCount)
//            {
//                endIndex = columnCount - 1;
//            }

            var spectralSelection = new Dictionary<string, double[,]>();
            foreach (string key in spectra.Keys)
            {
                matrix = spectra[key];
                int rowCount = matrix.GetLength(0);

                spectralSelection[key] = MatrixTools.Submatrix(matrix, 0, startIndex, rowCount - 1, endIndex - 1);
                Debug.Assert(spectralSelection[key].GetLength(1) == (endTime - startTime).Ticks / dataScale.Ticks, "The expected number of frames should be extracted.");
            }

            // compress spectrograms to correct scale
            if (scalingFactor > 1)
            {
                spectralSelection = IndexMatrices.CompressIndexSpectrograms(
                    spectralSelection,
                    imageScale,
                    dataScale);
            }

            // check that have not compressed matrices to zero length
            // Assume that will always have an ACI matrix
            if ((spectralSelection["ACI"].GetLength(0) == 0) || (spectralSelection["ACI"].GetLength(1) == 0))
            {
                return null;
            }

            // DEFINE the DEFAULT colour maps for the false-colour spectrograms
            // Then obtain values from spectrogramDrawingConfig. NOTE: WE REQUIRE LENGTH = 11 chars.
            string colorMap1 = "ACI-ENT-EVN";
            if ((config.ColorMap1 != null) && (config.ColorMap1.Length == 11))
            {
                colorMap1 = config.ColorMap1;
            }

            string colorMap2 = "BGN-POW-EVN";
            if ((config.ColorMap2 != null) && (config.ColorMap2.Length == 11))
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

            var imageScaleInMsPerPixel = (int)imageScale.TotalMilliseconds;
            double blendWt1 = 0.1;
            double blendWt2 = 0.9;

            if (imageScaleInMsPerPixel > 15000)
            {
                blendWt1 = 1.0;
                blendWt2 = 0.0;
            }
            else if (imageScaleInMsPerPixel > 10000)
            {
                blendWt1 = 0.9;
                blendWt2 = 0.1;
            }
            else if (imageScaleInMsPerPixel > 5000)
            {
                blendWt1 = 0.8;
                blendWt2 = 0.2;
            }
            else if (imageScaleInMsPerPixel > 1000)
            {
                blendWt1 = 0.6;
                blendWt2 = 0.4;
            }
            else if (imageScaleInMsPerPixel > 500)
            {
                blendWt1 = 0.3;
                blendWt2 = 0.7;
            }

            Image LdSpectrogram = cs1.DrawBlendedFalseColourSpectrogram(
                "NEGATIVE",
                colorMap1,
                colorMap2,
                blendWt1,
                blendWt2);

            if (LdSpectrogram == null)
            {
                throw new NullReferenceException("Null Image of DrawBlendedFalseColourSpectrogram");
            }

            return LdSpectrogram;
        }
    }
}