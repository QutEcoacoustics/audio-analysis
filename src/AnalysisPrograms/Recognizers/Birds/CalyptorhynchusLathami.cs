// <copyright file="CalyptorhynchusLathami.cs" company="QutEcoacoustics">
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
    /// A recognizer for the Glossy-black Cockatoo, https://en.wikipedia.org/wiki/Glossy_black_cockatoo.
    /// The glossy black cockatoo (Calyptorhynchus lathami), is the smallest member of the subfamily Calyptorhynchinae found in eastern Australia.
    /// This recognizer has been trained on good quality calls provided by NSW DPI by Brad Law and Kristen Thompson.
    /// </summary>
    public class CalyptorhynchusLathami : RecognizerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => Bibliography.Anthony.LastName;

        public override string SpeciesName => "Truskinger.CalyptorhynchusLathami";

        public override string CommonName => "Glossy-black Cockatoo";

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
            RuntimeHelpers.RunClassConstructor(typeof(CalyptorhynchusLathamiConfig).TypeHandle);
            var config = ConfigFile.Deserialize<CalyptorhynchusLathamiConfig>(file);

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
            var genericConfig = (CalyptorhynchusLathamiConfig)configuration;
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

        public class CalyptorhynchusLathamiConfig : GenericRecognizer.GenericRecognizerConfig
        {
        }
    }
}
