// <copyright file="ManorinaMelanophrys.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Birds
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;
    using log4net;

    /// <summary>
    /// A recognizer for the Bell miner, https://en.wikipedia.org/wiki/Bell_miner.
    /// The bell miner (Manorina melanophrys), commonly known as the bellbird, is a colonial honeyeater, endemic to southeastern Australia.
    /// The common name refers to their bell-like call. 'Miner' is an old alternative spelling of 'myna', and is shared with other members of the genus Manorina.
    /// This recognizer has been trained on good quality calls provided by NSW DPI by Brad Law and Kristen Thompson.
    /// </summary>
    public class ManorinaMelanophrys : RecognizerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => Bibliography.Anthony.LastName;

        public override string SpeciesName => "ManorinaMelanophrys";

        public override string CommonName => "Bell miner";

        public override Status Status => Status.Alpha;

        public override string Description => $"Acoustic event recognizer for the {this.CommonName}.";

        public IReadOnlyCollection<Citation> Citations => new[] {
            Bibliography.QutEcoacousticsProject with {
                Authors = Bibliography.BuiltByAnthony,
                Title = this.Description,
            },
        };

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            RuntimeHelpers.RunClassConstructor(typeof(ManorinaMelanophrysConfig).TypeHandle);
            var config = ConfigFile.Deserialize<ManorinaMelanophrysConfig>(file);

            // validation of configs can be done here
            GenericRecognizer.ValidateProfileTagsMatchAlgorithms(config.Profiles, file);
            return config;
        }

        public override RecognizerResults Recognize(
            AudioRecording audioRecording,
            Config configuration,
            TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes,
            DirectoryInfo outputDirectory,
            int? imageWidth)
        {
            var genericConfig = (ManorinaMelanophrysConfig)configuration;
            var recognizer = new GenericRecognizer();

            // Use the generic recognizers to find all generic events.
            RecognizerResults combinedResults = recognizer.Recognize(
                audioRecording,
                genericConfig,
                segmentStartOffset,
                getSpectralIndexes,
                outputDirectory,
                imageWidth);

            return combinedResults;
        }

        public class ManorinaMelanophrysConfig : GenericRecognizer.GenericRecognizerConfig
        {
        }
    }
}
