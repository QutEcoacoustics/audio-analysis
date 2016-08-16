// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEventRecognizer.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the IEventRecognizer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;

    public interface IEventRecognizer : IAnalyser2
    {

        RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, int imageWidth);
    }


    public static class EventRecognizers
    {
        private static IEnumerable<IEventRecognizer> eventRecognizersCached;

        /// <summary>
        /// Get recognizers using a method that is compatible with MONO environment..
        /// </summary>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; AnalysisPrograms.IEventRecognizer].
        /// </returns>
        public static IEnumerable<IEventRecognizer> GetRecognizers(Assembly assembly)
        {
            if (eventRecognizersCached == null)
            {
                // to find the assembly, get the type of a class in that assembly
                // eg. typeof(MainEntry).Assembly
                var analyzerType = typeof(IEventRecognizer);

                var recognizers =
                    assembly.GetTypes()
                        .Where(analyzerType.IsAssignableFrom)
                        .Select(t => Activator.CreateInstance(t) as IEventRecognizer);

                eventRecognizersCached = recognizers;
            }

            return eventRecognizersCached;
        }

        public static IEventRecognizer FindAndCheckRecognizers(string analysisIdentifier)
        {
            var eventRecognizers = GetRecognizers(typeof(MainEntry).Assembly).ToList();
            IEventRecognizer foundRecognizer = eventRecognizers.FirstOrDefault(a => a.Identifier == analysisIdentifier);
            if (foundRecognizer == null)
            {
                LoggedConsole.WriteLine("###################################################\n");
                LoggedConsole.WriteLine("Analysis failed. UNKNOWN EventRecognizer/Analyzer: <{0}>", analysisIdentifier);
                LoggedConsole.WriteLine("Available analyzers are:");
                foreach (IEventRecognizer recognizer in eventRecognizers)
                {
                    LoggedConsole.WriteLine("\t  " + recognizer.Identifier);    
                }
                LoggedConsole.WriteLine("###################################################\n");

                throw new Exception("Cannot find a valid IEventRecognizer");
            }

            return foundRecognizer;
        }

    }
}