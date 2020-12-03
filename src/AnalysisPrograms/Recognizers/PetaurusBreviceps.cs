// <copyright file="PetaurusBreviceps.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers
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
    /// A recognizer for the Sugar glider, https://en.wikipedia.org/wiki/Sugar_glider.
    /// The sugar glider (Petaurus breviceps) is a small, omnivorous, arboreal,
    /// and nocturnal gliding possum belonging to the marsupial infraclass.
    /// The common name refers to its predilection for sugary foods such as sap
    /// and nectar and its ability to glide through the air, much like a flying squirrel.
    /// This recognizer has been trained on good quality calls provided by NSW DPI by Brad Law and Kristen Thompson.
    /// </summary>
    public class PetaurusBreviceps : RecognizerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => Bibliography.Anthony.LastName;

        public override string SpeciesName => "PetaurusBreviceps";

        public override string CommonName => "Sugar glider";

        public override Status Status => Status.Alpha;

        public override string Description => $"Acoustic event recognizer for the {this.CommonName}.";

        public IReadOnlyCollection<Citation> Citations => new[] {
            Bibliography.NswDpiRecognisersProject with {
                Authors = Bibliography.BuiltByAnthony,
                Title = this.Description,
            },
        };

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            RuntimeHelpers.RunClassConstructor(typeof(PetaurusBrevicepsConfig).TypeHandle);
            var config = ConfigFile.Deserialize<PetaurusBrevicepsConfig>(file);

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
            var genericConfig = (PetaurusBrevicepsConfig)configuration;
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

        public class PetaurusBrevicepsConfig : GenericRecognizer.GenericRecognizerConfig
        {
        }
    }

}
