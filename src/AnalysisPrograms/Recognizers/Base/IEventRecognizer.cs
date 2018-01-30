// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEventRecognizer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the IEventRecognizer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;

    /// <summary>
    /// This interface specializes IAnalyser2 to be a species recognizer.
    /// </summary>
    public interface IEventRecognizer : IAnalyser2
    {
        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="audioRecording">The audio recording to process - it should be a minute or two long</param>
        /// <param name="configuration">The configuration to use for this analysis</param>
        /// <param name="segmentStartOffset">In analyze long recording scenarios this is the time from the start of the original audio recording for this segment</param>
        /// <param name="getSpectralIndexes">Invoke this lazy function to get indices for the current segment</param>
        /// <param name="outputDirectory">The current output directory</param>
        /// <param name="imageWidth">The expected width of output images</param>
        /// <returns>A recognizer results object</returns>
        RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth);
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
                        .Where(t => t.IsClass && !t.IsAbstract)
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