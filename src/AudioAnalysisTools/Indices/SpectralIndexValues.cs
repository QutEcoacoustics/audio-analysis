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
        private static readonly Dictionary<string, Func<SpectralIndexBase, double[]>> CachedSelectorsInternal;
        private static readonly Dictionary<string, Action<SpectralIndexValues, double[]>> CachedSettersInternal;

        static SpectralIndexValues()
        {
            var getters = ReflectionExtensions.GetGetters<SpectralIndexValues, double[]>();

            CachedSelectorsInternal = new Dictionary<string, Func<SpectralIndexBase, double[]>>(getters.Count);
            foreach (var keyValuePair in getters)
            {
                // var key = keyValuePair.Key;
                var selector = keyValuePair.Value;

                CachedSelectorsInternal.Add(
                    keyValuePair.Key,
                    spectrumBase => selector((SpectralIndexValues)spectrumBase));
            }

            var setters = ReflectionExtensions.GetSetters<SpectralIndexValues, double[]>();

            CachedSettersInternal = new Dictionary<string, Action<SpectralIndexValues, double[]>>(getters.Count);
            foreach (var keyValuePair in setters)
            {
                // var key = keyValuePair.Key;
                var setter = keyValuePair.Value;

                CachedSettersInternal.Add(
                    keyValuePair.Key,
                    (spectrumBase, value) => setter(spectrumBase, value));
            }
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
                this.SetPropertyValue(cachedSetter.Key, initArray);
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
            return dictionaryOfSpectra.FromTwoDimensionalArray<SpectralIndexValues, double>(CachedSetters, TwoDimensionalArray.Rotate90AntiClockWise);
        }

        /// <summary>
        /// Used to check that the keys in the indexProperties dictionary correspond to Properties in the SpectralIndexValues class.
        /// Call this method before entering a loop because do not want the error message at every iteration through loop.
        /// </summary>
        public static void CheckExistenceOfSpectralIndexValues(Dictionary<string, IndexProperties> indexProperties)
        {
            var siv = new SpectralIndexValues();
            double[] dummyArray = null;

            foreach (var kvp in indexProperties)
            {
                if (!kvp.Value.IsSpectralIndex)
                {
                    continue;
                }

                var success = siv.TrySetPropertyValue(kvp.Key, dummyArray);
                if (!success)
                {
                    LoggedConsole.WriteWarnLine(
                        "### WARNING: The PROPERTY <" + kvp.Key + "> does not exist in the SpectralIndexValues class!");
                }
            }
        }

        public static Dictionary<string, Func<SpectralIndexBase, double[]>> CachedSelectors
        {
            get
            {
                return CachedSelectorsInternal;
            }
        }

        public static Dictionary<string, Action<SpectralIndexValues, double[]>> CachedSetters
        {
            get
            {
                return CachedSettersInternal;
            }
        }

        public static string[] GetKeys()
        {
            return CachedSelectorsInternal.Keys.ToArray();
        }

        public static Image CreateImageOfSpectralIndices(SpectralIndexValues spectralIndices)
        {
            //string[] keys = { "ACI", "BGN", "CVR", "ENT", "EVN", "OSC", "PMN", "R3D", "RHZ", "RNG", "RPS", "RVT", "SPT" };
            var keys = InitialiseIndexProperties.ListOfKeys;
            var images = new List<Image>();
            foreach (var key in keys)
            {
                double[] normalisedIndex = null;

                switch (key)
                {
                    case "ACI":
                        normalisedIndex = DataTools.normalise(spectralIndices.ACI);
                        break;
                    case "BGN":
                        normalisedIndex = DataTools.normalise(spectralIndices.BGN);
                        break;
                    case "CVR":
                        normalisedIndex = DataTools.normalise(spectralIndices.CVR);
                        break;
                    case "ENT":
                        normalisedIndex = DataTools.normalise(spectralIndices.ENT);
                        break;
                    case "EVN":
                        normalisedIndex = DataTools.normalise(spectralIndices.EVN);
                        break;
                    case "OSC":
                        normalisedIndex = DataTools.normalise(spectralIndices.OSC);
                        break;
                    case "PMN":
                        normalisedIndex = DataTools.normalise(spectralIndices.PMN);
                        break;
                    case "RHZ":
                        normalisedIndex = DataTools.normalise(spectralIndices.RHZ);
                        break;
                    case "RNG":
                        normalisedIndex = DataTools.normalise(spectralIndices.RNG);
                        break;
                    case "RPS":
                        normalisedIndex = DataTools.normalise(spectralIndices.RPS);
                        break;
                    case "RVT":
                        normalisedIndex = DataTools.normalise(spectralIndices.RVT);
                        break;
                    case "R3D":
                        normalisedIndex = DataTools.normalise(spectralIndices.R3D);
                        break;
                    case "SPT":
                        normalisedIndex = DataTools.normalise(spectralIndices.SPT);
                        break;
                    default:
                        break;
                }

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
        /// different parameters - we needed a way to diambiguate them.
        /// </remarks>
        public IndexCalculateConfig Configuration { get; }

        // 1:
        public double[] ACI { get; set; }

        // 2:
        public double[] BGN { get; set; }

        // 3:
        public double[] CVR { get; set; }

        // 4:
        public double[] ENT { get; set; }

        // 5:
        public double[] EVN { get; set; }

        /// <summary>
        /// 6: Gets or sets the OSC spectral index should eventually be depracated.
        /// It is derived from an incorrect way of averaging decibel values
        /// </summary>
        public double[] OSC { get; set; }

        /// <summary>
        /// 7: Gets or sets pMN = Power Minus Noise.
        /// PMN is measured in decibels but should replace POW as the average decibel spectrogram.
        /// PMN calculates the average decibel spectrogram correctly.
        /// </summary>
        public double[] PMN { get; set; }

        // 8: Sum of Spectral Ridges in Horizontal, postive and neg slope directions (RHZ+RPS+RNG)
        public double[] R3D { get; set; }

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

        // The following two indices are not standard acoustic indices but are only used in the intermediate calculations
        public double[] DIF { get; set; }

        public double[] SUM { get; set; }

        public override Dictionary<string, Func<SpectralIndexBase, double[]>> GetSelectors()
        {
            return CachedSelectors;
        }
    }
}