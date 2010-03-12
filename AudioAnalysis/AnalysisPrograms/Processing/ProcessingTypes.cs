using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AudioAnalysisTools;
using QutSensors.Shared;

namespace AnalysisPrograms.Processing
{
    public static class ProcessingTypes
    {
        internal static IEnumerable<ProcessorResultTag> RunAED(FileInfo settingsFile, FileInfo audioFile)
        {
            // settings
            var INTENSITY_THRESHOLD = "intensityThreshold".ToUpperInvariant();
            double intensityThreshold = QutSensors.AudioAnalysis.AED.Default.intensityThreshold;

            var SMALL_AREA_THRESHOLD = "smallAreaThreshold".ToUpperInvariant();
            int smallAreaThreshold = QutSensors.AudioAnalysis.AED.Default.smallAreaThreshold;

            var config = new TowseyLib.Configuration(settingsFile.FullName);
            Dictionary<string, string> dict = config.GetTable();

            if (dict.ContainsKey(INTENSITY_THRESHOLD)) intensityThreshold = Convert.ToDouble(dict[INTENSITY_THRESHOLD]);
            if (dict.ContainsKey(SMALL_AREA_THRESHOLD)) smallAreaThreshold = Convert.ToInt32(dict[SMALL_AREA_THRESHOLD]);

            // execute
            var result = AED.Detect(audioFile.FullName, intensityThreshold, smallAreaThreshold);
            var events = result.Item2;

            // AcousticEvent results
            return events.Select(ae =>
                ProcessingUtils.GetProcessorResultTag(ae,
                    new ResultProperty(ae.Name, null,
                        new Dictionary<string, string>() { 
                            { "Description", "Normalised score is not applicable to AED." } 
                        }
                    )
                )
            );

        }

        internal static IEnumerable<ProcessorResultTag> RunOD(FileInfo settingsFile, FileInfo audioFile)
        {
            // settings
            var config = new TowseyLib.Configuration(settingsFile.FullName);
            Dictionary<string, string> dict = config.GetTable();

            int minHz = Int32.Parse(dict[OscillationRecogniser.key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[OscillationRecogniser.key_MAX_HZ]);
            double frameOverlap = Double.Parse(dict[OscillationRecogniser.key_FRAME_OVERLAP]);
            double dctDuration = Double.Parse(dict[OscillationRecogniser.key_DCT_DURATION]);
            int minOscilFreq = Int32.Parse(dict[OscillationRecogniser.key_MIN_OSCIL_FREQ]);
            int maxOscilFreq = Int32.Parse(dict[OscillationRecogniser.key_MAX_OSCIL_FREQ]);
            double minAmplitude = Double.Parse(dict[OscillationRecogniser.key_MIN_AMPLITUDE]);
            double eventThreshold = Double.Parse(dict[OscillationRecogniser.key_EVENT_THRESHOLD]);
            double minDuration = Double.Parse(dict[OscillationRecogniser.key_MIN_DURATION]);
            double maxDuration = Double.Parse(dict[OscillationRecogniser.key_MAX_DURATION]);

            // execute
            var results = OscillationRecogniser.Execute_ODDetect(
                audioFile.FullName, minHz, maxHz, frameOverlap, dctDuration, minOscilFreq,
                maxOscilFreq, minAmplitude, eventThreshold, minDuration, maxDuration);
            var events = results.Item4;

            // AcousticEvent results
            return events.Select(ae =>
                ProcessingUtils.GetProcessorResultTag(ae,
                    new ResultProperty(ae.Name, ae.NormalisedScore,
                        new Dictionary<string, string>() { 
                            { "Description", "Normalised score" } 
                        }
                    )
                )
            );

        }

        internal static IEnumerable<ProcessorResultTag> RunEPR(FileInfo audioFile)
        {
            // no settings, yet

            // execute
            var result = GroundParrotRecogniser.Detect(audioFile.FullName);
            var events = result.Item2;

            // AcousticEvent results
            return events.Select(ae =>
                ProcessingUtils.GetProcessorResultTag(ae,
                    new ResultProperty(ae.Name, ae.NormalisedScore,
                        new Dictionary<string, string>() { 
                            { "Description", "Normalised score" } 
                        }
                    )
                )
            );
        }
    }
}
