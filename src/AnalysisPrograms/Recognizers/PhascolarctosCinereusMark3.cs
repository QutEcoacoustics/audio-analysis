// <copyright file="PhascolarctosCinereusMark3.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.EventStatistics;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using TowseyLibrary;
    using static AnalysisPrograms.Recognizers.GenericRecognizer;

    /// <summary>
    /// A recognizer for bellows of the male Koala (Phascolarctos cinereus).
    /// 
    /// THE KOALA BELLOW:
    /// The canonical male bellow is characterised by three phases: 
    ///    1: an introductory series of "coughs" that gradually get deeper in frequency;
    ///    2: a series of alternating "snoring" inhalations and shorter "belching" exhalations.
    ///    3: a "pulsitile" phase that gradually gets quieter and has no formants.
    /// For the purposes of koala call detection the second phase of alternating inhalations and exhalations is the best target for bellow recognition.
    /// Furthermore, the inhalations contain more acoustic structure than the shorter exhalations and are therefore more useful for recognition purposes.
    /// The exhalations consist of a series of oscillations, each pulse of which consists of a set of formants.
    /// Formants higher than the sixth formant are often poorly defined so these are probably not reliable for recognition purposes.
    /// Likewise the fundamental (which can be as low as 20 Hz, see note below) will not be obvious in the spectrograms which we produce and will be very difficult to distinguish from background environmental noise.
    /// 
    /// A NOTE ON FORMANTS:
    /// According to source-filter theory, vocalisations result from a source signal produced by the vocal chords which is then filtered in the cavities of the vocal tract to produce a set of resonances.
    /// The vibating vocal chords produce a broadband signal as they strike one another. The length of the vocal chords determines the fundamental tone.
    /// The filtering takes place as the sound waves pass through through the vocal tract and resonate within the sinuses and other cavities to produce the sound which is ultimately heard externally.
    /// A persistant amplified resonance is call a "formant".
    /// 
    /// KOALA FORMANTS:
    /// The fundamental frequency of a bellow inhalation is around 20-30 Hz (approximately three octaves below middle C, comparable to that of an elephant roar).
    /// THis is about 20 times lower than one would expect for a koala size animal. The first, second and third inhalation formants are typically around 200, 400 and 600 Hz respectively.
    /// Formants 4, 5 and 6 fluctuate around 1000, 1500 and 2000 Hz respectively. Note that these values vary from animal to animal but are distinctive for an individual and can be used to identify individuals.
    /// Koala exhalations also produce formants but they are less well defined and tend to 'slide' in frequency like the formants in human speech.
    ///
    /// KOALA OSCILLATIONS:
    /// The snoring-like oscillations of the koala inhalation typically start with a period of ~0.08s, gradually becoming faster to end with a period of ~0.02s.
    ///
    /// CALL VARIABILITY:
    /// Not all koala calls have the canonical structure described above. Sometimes the call starts with introductory coughs but fades before embarking on the snores and belching.
    /// Adding to this varibility is the distance of the animal from the microphone. With increasing distance, inhalations tend to fade first.The most persistent component of a bellow is the snoring.
    /// Consequently the oscillatory snoring is the most reliable part of the call for recognition purposes. However, experience with Koala Recognizer 1 indicates that
    ///  reliance on oscillation detection alone produces a range of false-positive detections.
    ///
    /// OBJECTIVES FOR THIS RECOGNIZER MARK 3:
    /// The expectation is that detection of additional bellow components (additional to oscillations) will help to reduce false-positives.However this is unlikely to reduce false-negatives.
    ///
    /// REFERENCE: See following paper for more detail and links to other publications:
    /// "Estimating the Active Space of Male Koala Bellows: Propagation of Cues to Size and Identity in a Eucalyptus Forest"
    /// Benjamin D.Charlton, David Reby, William Ellis, Jacqui Brumm, Tecumseh Fitch
    /// Published: September 20, 2012. https://doi.org/10.1371/journal.pone.0045420
    ///
    /// The spectrograms used by Ben Charlton had following parameters:
    ///    Window length 0.05s.
    ///    Time step = 0.004s
    ///    Frequency step = 10Hz.
    /// The spectrogram images in the above paper suggest Charlton's spectrogram Nyquist = 4kHz.
    ///
    /// Guided by the above parameters used by Charlton, this Koala recognizer down-samples recordings to 10240 Hz which yields a Nyquist of 5120 Hz.
    /// We use a window size of 512 samples giving a frame duration = 0.05s and a frequency bin width = 20 Hz.
    /// We initially use a frame step of 256 samples.Charlton's frame overlap of >90% is computationally intensive and probably not necessary for our purposes.
    /// </summary>
    internal class PhascolarctosCinereusMark3 : RecognizerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => "Towsey";

        public override string SpeciesName => "PhascolarctosCinereusMark3";

        public override string Description => "Detects male Koala bellows. This is Koala Recognizer, Mark 3";

        public override string CommonName => "Koala";

        public override Status Status => Status.InDevelopment;

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            RuntimeHelpers.RunClassConstructor(typeof(KoalaConfig3).TypeHandle);
            var config = ConfigFile.Deserialize<KoalaConfig3>(file);

            // validation of configs can be done here
            GenericRecognizer.ValidateProfileTagsMatchAlgorithms(config.Profiles, file);

            // This call sets a restriction so that only one generic algorithm is used.
            // CHANGE this to accept multiple generic algorithms as required.
            //if (result.Profiles.SingleOrDefault() is ForwardTrackParameters)
            if (config.Profiles?.Count == 1 && config.Profiles.First().Value is OscillationParameters)
            {
                return config;
            }

            throw new ConfigFileException("Koala Recognizer Mark3 expects one and only one Oscillation algorithm.", file);
        }

        /// <summary>
        /// This method is called once per segment. Segments are typically one-minute duration.
        /// </summary>
        /// <param name="audioRecording">one minute of audio recording.</param>
        /// <param name="configuration">config file that contains parameters used by profiles.</param>
        /// <param name="segmentStartOffset">when the segment starts relative to recording start.</param>
        /// <param name="getSpectralIndexes">not applicable.</param>
        /// <param name="outputDirectory">where the recognizer results can be found.</param>
        /// <param name="imageWidth"> assuming ????.</param>
        /// <returns>recognizer results.</returns>
        public override RecognizerResults Recognize(
            AudioRecording audioRecording,
            Config configuration,
            TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes,
            DirectoryInfo outputDirectory,
            int? imageWidth)
        {
            //class KoalaConfig3 is defined at bottom of this file.
            var genericConfig = (KoalaConfig3)configuration;

            // Instead of calling the GenericRecgonizer, call recognizer for Koala..
            // THis is so that the output from the Profiles can be filtered BEFORE passing to Post-processing step.
            //var recognizer = new GenericRecognizer();

            RecognizerResults combinedResults = RecognizeKoalaBellows(
                audioRecording,
                genericConfig,
                segmentStartOffset,
                getSpectralIndexes,
                outputDirectory,
                imageWidth);

            var count = combinedResults.NewEvents.Count;
            if (count == 0)
            {
                return combinedResults;
            }

            // ################### DO POST-POST-PROCESSING of EVENTS HERE ###################

            //throw new NotImplementedException();
            return combinedResults;
        }

        public static RecognizerResults RecognizeKoalaBellows(
            AudioRecording audioRecording,
            Config genericConfig,
            TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes,
            DirectoryInfo outputDirectory,
            int? imageWidth)
        {
            var configuration = (GenericRecognizerConfig)genericConfig;

            if (configuration.Profiles?.Count < 1)
            {
                throw new ConfigFileException("The Koala recognizer needs at least one profile set. Zero were found.");
            }

            int count = configuration.Profiles.Count;
            var message = $"Found {count} analysis profile(s): " + configuration.Profiles.Keys.Join(", ");
            Log.Info(message);

            var results = RunProfiles(audioRecording, configuration, segmentStartOffset);

            // ############################### ADDITIONAL FILTERING OF OUTPUT EVENTS FROM PROFILES ###############################
            results = FilterProfileEvents(results, segmentStartOffset);

            // ############################### POST-PROCESSING OF GENERIC EVENTS ###############################

            var postprocessingConfig = configuration.PostProcessing;
            if (postprocessingConfig is not null)
            {
                results = PostProcessAcousticEvents(configuration, results, segmentStartOffset);
            }

            //############################# COMMENT OUT THE FOLLOWING LINES IF WANT TO VIEW THE ORIGINAL SPECTROGRAM
            // Here we replace the original koala spectrogram with a shorter one that is easier to interpret because it is shorter, because less frame overlap.
            int windowSize = 512;
            int windowStep = 256;
            results = RescaleResultsSpectrogram(results, windowSize, windowStep, audioRecording);

            return results;
        }

        private static RecognizerResults FilterProfileEvents(RecognizerResults results, TimeSpan segmentOffset)
        {
            var events = results.NewEvents;
            var spectrogram = results.Sonogram;
            int count = 0;

            // create new list of events
            var newList = new List<EventCommon>();

            foreach (var ev in events)
            {
                count++;
                var spectralEvent = ev as SpectralEvent;
                var start = TimeSpan.FromSeconds(spectralEvent.EventStartSeconds);
                var end = TimeSpan.FromSeconds(spectralEvent.EventEndSeconds);
                var lowFreq = spectralEvent.LowFrequencyHertz;
                var topFreq = spectralEvent.HighFrequencyHertz;

                var stats = EventStatisticsCalculate.CalculateEventStatstics(
                        spectrogram,
                        (start, end).AsInterval(),
                        (lowFreq, topFreq).AsInterval(),
                        segmentOffset);

                // now filter event on its stats.
                // s1 and s2 are measures of energy concentration. i.e. 1-entropy.
                var s1 = stats.SpectralPeakCount;
                var s2 = stats.SpectralEnergyDistribution;
                var s3 = stats.SpectralCentroid;
                var s4 = stats.DominantFrequency;

                // var message = $" EVENT{count} starting {start.TotalSeconds:F1}sec: PeakCount {s1}; SpectEnergyDistr {s2:F4}; SpectralCentroid {s3}; DominantFreq {s4}";

                if (s3 < 700)
                {
                    newList.Add(ev);
                    var message = $" EVENT{count} starting {start.TotalSeconds:F1}sec ACCEPTED: PeakCount {s1}; SpectralCentroid {s3} < 700; DominantFreq {s4} < 700";
                    Log.Info(message);
                }
                else
                {
                    var message = $" EVENT{count} starting {start.TotalSeconds:F1}sec REJECTED: PeakCount {s1};  SpectralCentroid {s3} >= 700; DominantFreq {s4} > 700";
                    Log.Info(message);
                }
            }

            // return filtered list of events in results.
            results.NewEvents = newList;
            return results;
        }

        private static RecognizerResults RescaleResultsSpectrogram(RecognizerResults results, int windowSize, int windowStep, AudioRecording audioRecording)
        {
            var newConfig = new SonogramConfig()
            {
                WindowSize = windowSize,
                WindowStep = windowStep,
                WindowOverlap = (windowSize - windowStep) / (double)windowSize,
                WindowFunction = "HANNING",
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
            };

            var newSpectrogram = new SpectrogramStandard(newConfig, audioRecording.WavReader);
            results.Sonogram = newSpectrogram;

            // also need to resize the plots
            foreach (var plot in results.Plots)
            {
                plot.ScaleDataArray(newSpectrogram.FrameCount);
            }

            return results;
        }

        public class KoalaConfig3 : GenericRecognizerConfig, INamedProfiles<object>
        {
        }
    }
}
