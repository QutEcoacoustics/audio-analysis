// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectralIndexBase.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SpectrumBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase.ResultBases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class SpectralIndexBase : ResultBase
    {
        public abstract Dictionary<string, Func<SpectralIndexBase, double[]>> GetSelectors();

        protected static (Dictionary<string, Func<SpectralIndexBase, double[]>> CachedSelectors, Dictionary<string, Action<T, double[]>> CachedSetters, string[] Keys) MakeSelectors<T>()
            where T : SpectralIndexBase
        {
            var getters = ReflectionExtensions.GetGetters<T, double[]>();

            var cachedSelectors = new Dictionary<string, Func<SpectralIndexBase, double[]>>(getters.Count);
            foreach (var keyValuePair in getters)
            {
                // var key = keyValuePair.Key;
                var selector = keyValuePair.Value;

                cachedSelectors.Add(
                    keyValuePair.Key,
                    spectrumBase => selector((T)spectrumBase));
            }

            var keys = cachedSelectors.Keys.ToArray();

            var setters = ReflectionExtensions.GetSetters<T, double[]>();

            var cachedSetters = new Dictionary<string, Action<T, double[]>>(getters.Count);
            foreach (var keyValuePair in setters)
            {
                // var key = keyValuePair.Key;
                var setter = keyValuePair.Value;

                cachedSetters.Add(
                    keyValuePair.Key,
                    (spectrumBase, value) => setter(spectrumBase, value));
            }

            return (cachedSelectors, cachedSetters, keys);
        }
    }
}