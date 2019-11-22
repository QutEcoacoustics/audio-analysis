// <copyright file="SpectralIndexValues.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Acoustics.Shared;
    using AnalysisBase.ResultBases;
    using Fasterflect;
    using TowseyLibrary;

    public class SpectralIndexValues : SpectralIndexBase
    {
        // Static constructors are called implicitly when the type is first used.
        // Do NOT delete even if it has 0 references.
        static SpectralIndexValues()
        {
            var result = MakeSelectors<SpectralIndexValues>();
            CachedSelectors = result.CachedSelectors;
            CachedSetters = result.CachedSetters;
            Keys = result.Keys;
        }

        public SpectralIndexValues()
        {
            // empty constructor important!
        }

        public SpectralIndexValues(int spectrumLength, Dictionary<string, IndexProperties> indexProperties, IndexCalculateConfig configuration)
        {
            foreach (var cachedSetter in CachedSetters)
            {
                var defaultValue = 0.0;

                if (indexProperties.ContainsKey(cachedSetter.Key))
                {
                    var indexProperty = indexProperties[cachedSetter.Key];
                    if (indexProperty.IsSpectralIndex)
                    {
                        defaultValue = indexProperty.DefaultValue;
                    }
                }

                double[] initArray = new double[spectrumLength].FastFill(defaultValue);

                // WARNING: Potential throw site
                // No need to give following warning because should call CheckExistenceOfSpectralIndexValues() method before entering loop.
                // This prevents multiple warnings through loop.
                //this.SetPropertyValue(cachedSetter.Key, initArray);

                cachedSetter.Value(this, initArray);
            }

            this.Configuration = configuration;
        }

        /// <summary>
        /// Imports a dictionary of spectra.
        /// Assumes `CheckExistenceOfSpectralIndexValues` has already been called.
        /// Assumes frequency component is in fist index (i.e. frequency is rows) and time in second index (time is columns).
        /// </summary>
        /// <param name="dictionaryOfSpectra">
        /// The dictionary to convert to spectral index base
        /// </param>
        public static SpectralIndexValues[] ImportFromDictionary(Dictionary<string, double[,]> dictionaryOfSpectra)
        {
            return dictionaryOfSpectra.FromTwoDimensionalArray(CachedSetters, TwoDimensionalArray.Rotate90AntiClockWise);
        }

        /// <summary>
        /// Used to check that the keys in the indexProperties dictionary correspond to Properties in the SpectralIndexValues class.
        /// Call this method before entering a loop because do not want the error message at every iteration through loop.
        /// </summary>
        public static void CheckExistenceOfSpectralIndexValues(Dictionary<string, IndexProperties> indexProperties)
        {
            foreach (var kvp in indexProperties)
            {
                if (!kvp.Value.IsSpectralIndex)
                {
                    continue;
                }

                var success = CachedSelectors.ContainsKey(kvp.Key);
                if (!success)
                {
                    LoggedConsole.WriteWarnLine(
                        "### WARNING: The PROPERTY <" + kvp.Key + "> does not exist in the SpectralIndexValues class!");
                }
            }
        }

        public static Dictionary<string, Func<SpectralIndexBase, double[]>> CachedSelectors { get; }

        public static Dictionary<string, Action<SpectralIndexValues, double[]>> CachedSetters { get; }

        public static string[] Keys { get; }

        public static Image CreateImageOfSpectralIndices(SpectralIndexValues spectralIndices)
        {
            var images = new List<Image>();
            foreach (var key in Keys)
            {
                var spectrum = CachedSelectors[key](spectralIndices);
                double[] normalisedIndex = DataTools.normalise(spectrum);

                var image = GraphsAndCharts.DrawGraph(key, normalisedIndex, 100);
                images.Add(image);
            }

            var combinedImage = ImageTools.CombineImagesVertically(images.ToArray());
            return combinedImage;
        }

        /// <summary>
        /// Gets the configuration used to generate these results.
        /// </summary>
        /// <remarks>
        /// This property was added when we started generating lots of results that used
        /// different parameters - we needed a way to disambiguate them.
        /// </remarks>
        public IndexCalculateConfig Configuration { get; }

        // 1:
        public double[] ACI { get; set; }

        // 2:
        public double[] BGN { get; set; }

        // 3:
        public double[] CVR { get; set; }

        // Entropy
        // 4:
        public double[] ENT { get; set; }

        // 5:
        public double[] EVN { get; set; }

        /// <summary>
        /// Gets or sets the oscillation spectral index index. Created October 2018.
        /// </summary>
        public double[] OSC { get; set; }

        // 8: Sum of Spectral Ridges in Horizontal, postive and neg slope directions (RHZ+RPS+RNG)
        public double[] R3D { get; set; }

        /// <summary>
        /// Gets or sets PMN = Power Minus Noise.
        /// PMN is measured in decibels but should replace POW as the average decibel spectrogram.
        /// </summary>
        public double[] PMN { get; set; }

        // 9: Spectral Ridges Horizontal
        public double[] RHZ { get; set; }

        // 10: Spectral Ridges Vertical
        public double[] RVT { get; set; }

        // 11: Spectral Ridges Positive slope
        public double[] RPS { get; set; }

        // 12: Spectral Ridges Negative Slope
        public double[] RNG { get; set; }

        // 13: Spectral Peak Tracks
        public double[] SPT { get; set; }

        // This property only calculated for ACI when zooming
        // The following two indices are not standard acoustic indices but are only used in the intermediate calculations
        public double[] DIF { get; set; }

        public double[] SUM { get; set; }

        public override Dictionary<string, Func<SpectralIndexBase, double[]>> GetSelectors() => CachedSelectors;
    }
}