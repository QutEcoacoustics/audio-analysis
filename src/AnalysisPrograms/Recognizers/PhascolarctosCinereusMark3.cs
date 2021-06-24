// <copyright file="PhascolarctosCinereusMark3.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers
{
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
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
        private static readonly ILog BoobookLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => "Towsey";

        public override string SpeciesName => "PhascolarctosCinereus";

        public override string Description => "Detects male Koala bellows. This is Koala Mark 3";

        public override string CommonName => "Koala";

        public override Status Status => Status.InDevelopment;

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
            var recognizer = new GenericRecognizer();

            RecognizerResults combinedResults = recognizer.Recognize(
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

        public class KoalaConfig3 : GenericRecognizerConfig, INamedProfiles<object>
        {
        }
    }
}
