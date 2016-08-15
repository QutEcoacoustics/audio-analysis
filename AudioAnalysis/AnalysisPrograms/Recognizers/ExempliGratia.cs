using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Recognizers
{
    using System.Reflection;

    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Recognizers.Base;

    using AudioAnalysisTools.WavTools;

    using log4net;

    class ExempliGratia : RecognizerBase
    {
        public override string Author => "Truskinger";

        public override string Species => "ExempliGratia";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Summarize your results. This method is invoked exactly once.
        /// </summary>
        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // No operation - do nothing. Feel free to add your own logic.
        }

        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="audioRecording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(
            AudioRecording audioRecording,
            dynamic configuration,
            TimeSpan segmentStartOffset,
            Lazy<IEnumerable<SpectralIndexBase>> getSpectralIndexes)
        {
            
            var wavReader = new WavReader(audioRecording);
            var indices = getSpectralIndexes(wavReader);


            throw new NotImplementedException();
        }
    }
}
