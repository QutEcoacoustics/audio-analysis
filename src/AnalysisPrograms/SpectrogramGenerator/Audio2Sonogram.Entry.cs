// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Audio2Sonogram.Entry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Produces various kinds of standard scale spectrograms.
//   ACTIVITY CODE: audio2sonogram
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.SpectrogramGenerator
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared.ConfigFile;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Validation;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using Path = System.IO.Path;

    /// <summary>
    /// Produces standard greyscale spectrograms of various types from a wav audio file.
    /// </summary>
    public class Audio2Sonogram
    {
        public const string CommandName = "Audio2Sonogram";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Command(
            Name = CommandName,
            Description = "[BETA] Generates multiple standard-scale spectrograms")]
        public class Arguments : SourceConfigOutputDirArguments
        {
            [Option(Description = "The start offset to start analyzing from (in seconds)")]
            [InRange(min: 0)]
            public double? StartOffset { get; set; }

            [Option(Description = "The end offset to stop analyzing (in seconds)")]
            [InRange(min: 0)]
            public double? EndOffset { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                Main(this);
                return this.Ok();
            }
        }

        public static void Main(Arguments arguments)
        {
            // 1. set up the necessary files
            var sourceRecording = arguments.Source;
            var configInfo = ConfigFile.Deserialize<SpectrogramGeneratorConfig>(arguments.Config.ToFileInfo());
            DirectoryInfo output = arguments.Output;
            if (!output.Exists)
            {
                output.Create();
            }

            //if (arguments.StartOffset.HasValue ^ arguments.EndOffset.HasValue)
            //{
            //    throw new InvalidStartOrEndException("If StartOffset or EndOffset is specified, then both must be specified");
            //}
            // set default offsets - only use defaults if not provided in arguments list
            // var offsetsProvided = arguments.StartOffset.HasValue && arguments.EndOffset.HasValue;
            //TimeSpan? startOffset;
            //TimeSpan? endOffset;
            //if (offsetsProvided)
            //{
            //    startOffset = TimeSpan.FromSeconds(arguments.StartOffset.Value);
            //    endOffset = TimeSpan.FromSeconds(arguments.EndOffset.Value);
            //}

            const string title = "# MAKE MULTIPLE SONOGRAMS FROM AUDIO RECORDING";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input  audio file: " + sourceRecording.Name);

            // 3: CREATE A TEMPORARY RECORDING
            int resampleRate = configInfo.GetIntOrNull("ResampleRate") ?? 22050;
            var tempAudioSegment = AudioRecording.CreateTemporaryAudioFile(sourceRecording, output, resampleRate);

            // 4: GENERATE SPECTROGRAM images
            //string sourceName = sourceRecording.FullName;
            string sourceName = Path.GetFileNameWithoutExtension(sourceRecording.FullName);
            var result = SpectrogramGenerator.GenerateSpectrogramImages(tempAudioSegment, configInfo, sourceName);

            // 5: Save the image
            var outputImageFile = new FileInfo(Path.Combine(output.FullName, sourceName + ".Spectrograms.png"));
            result.CompositeImage.Save(outputImageFile.FullName);
        }
    }

    /// <summary>
    /// In line class used to return results from the static method Audio2Sonogram.GenerateFourSpectrogramImages().
    /// </summary>
    public class AudioToSonogramResult
    {
        public SpectrogramStandard DecibelSpectrogram { get; set; }

        public Image<Rgb24> CompositeImage { get; set; }
    }
}