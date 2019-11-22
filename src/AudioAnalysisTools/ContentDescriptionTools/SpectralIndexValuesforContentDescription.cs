// <copyright file="SpectralIndexValuesForContentDescription.cs" company="QutEcoacoustics">
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
    //using Fasterflect;
    using TowseyLibrary;

    /// <summary>
    /// TODO THIS CLASS IS WORK IN PROGRESS.
    /// IT IS PART OF CONTENT DESCRIPTION project.
    /// Purpose of this class is to avoid using the class IndexCalculateResult for returning results from IndexCalculateSixOnly.Analysis();
    /// This class is stripped down to just the required six spectral indices.
    /// </summary>
    public class SpectralIndexValuesForContentDescription : SpectralIndexBase
    {
        // Static constructors are called implicitly when the type is first used.
        // Do NOT delete even if it has 0 references.
        static SpectralIndexValuesForContentDescription()
        {
            var result = MakeSelectors<SpectralIndexValuesForContentDescription>();
            CachedSelectors = result.CachedSelectors;
            CachedSetters = result.CachedSetters;
            Keys = result.Keys;
        }

        public SpectralIndexValuesForContentDescription()
        {
            // empty constructor important!
        }

        public static Dictionary<string, Func<SpectralIndexBase, double[]>> CachedSelectors { get; }

        public static Dictionary<string, Action<SpectralIndexValuesForContentDescription, double[]>> CachedSetters { get; }

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
        

        // 1:
        public double[] ACI { get; set; }

        // 2:
        public double[] BGN { get; set; }

        // Entropy
        // 4:
        public double[] ENT { get; set; }

        // 5:
        public double[] EVN { get; set; }

        /// <summary>
        /// Gets or sets the oscillation spectral index index. Created October 2018.
        /// </summary>
        public double[] OSC { get; set; }

        /// <summary>
        /// Gets or sets PMN = Power Minus Noise.
        /// PMN is measured in decibels but should replace POW as the average decibel spectrogram.
        /// </summary>
        public double[] PMN { get; set; }

        public override Dictionary<string, Func<SpectralIndexBase, double[]>> GetSelectors() => CachedSelectors;
    }
}