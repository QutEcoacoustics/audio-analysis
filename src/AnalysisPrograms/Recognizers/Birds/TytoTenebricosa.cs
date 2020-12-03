// <copyright file="TytoTenebricosa.cs" company="QutEcoacoustics">
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
    /// A recognizer for the Greater sooty owl, https://en.wikipedia.org/wiki/https://en.wikipedia.org/wiki/Greater_sooty_owl.
    /// The greater sooty owl (Tyto tenebricosa) is a medium to large owl found in south-eastern
    /// Australia, Montane rainforests of New Guinea and have been seen on
    /// Flinders Island in the Bass Strait. 
    /// This recognizer has been trained on good quality calls provided by NSW DPI by Brad Law and Kristen Thompson.
    /// </summary>
    public class TytoTenebricosa : RecognizerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => Bibliography.Anthony.LastName;

        public override string SpeciesName => "TytoTenebricosa";

        public override string CommonName => "Greater sooty owl";

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
            RuntimeHelpers.RunClassConstructor(typeof(TytoTenebricosaConfig).TypeHandle);
            var config = ConfigFile.Deserialize<TytoTenebricosaConfig>(file);

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
            var genericConfig = (TytoTenebricosaConfig)configuration;
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

        public class TytoTenebricosaConfig : GenericRecognizer.GenericRecognizerConfig
        {
        }
    }

}
