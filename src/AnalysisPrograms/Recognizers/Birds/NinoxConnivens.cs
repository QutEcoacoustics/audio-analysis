// <copyright file="NinoxConnivens.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Birds
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;
    using log4net;

    /// <summary>
    /// A recognizer for the Barking Owl, https://en.wikipedia.org/wiki/Barking_owl.
    /// The barking owl (Ninox connivens), also known as the winking owl, is a nocturnal bird species native to mainland Australia
    /// and partsof Papua New Guinea and the Moluccas. They are a medium-sized brown owl and have a characteristic voice with
    /// calls ranging from a barking dog noise to a shrill human-like howl of great intensity.
    /// This recognizer has been trained on good quality calls provided by NSW DPI by Brad Law and Kristen Thompson.
    /// </summary>
    public class NinoxConnivens : RecognizerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => Bibliography.Anthony.LastName;

        public override string SpeciesName => "NinoxConnivens";

        public override string CommonName => "Barking owl";

        public override Status Status => Status.InDevelopment;

        public override string Description => $"Acoustic event recognizer for the {this.CommonName}.";

        public IReadOnlyCollection<Citation> Citations => new[] {
            Bibliography.NswDpiRecognisersProject with {
                Authors = Bibliography.BuiltByAnthony,
                Title = this.Description,
            },
        };

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            RuntimeHelpers.RunClassConstructor(typeof(NinoxConnivensConfig).TypeHandle);
            var config = ConfigFile.Deserialize<NinoxConnivensConfig>(file);

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
            var genericConfig = (NinoxConnivensConfig)configuration;
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

        public class NinoxConnivensConfig : GenericRecognizer.GenericRecognizerConfig
        {
        }
    }
}
