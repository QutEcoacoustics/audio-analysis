using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisPrograms.SpectralPeakTracking
{
    using System.IO;
    using Acoustics.Shared.ConfigFile;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using Production.Arguments;
    using Production.Validation;
    using McMaster.Extensions.CommandLineUtils;

    public static class SpectralPeakTrackingEntry
    {
        public const string CommandName = "SpectralPeakTracking";

        private const string AdditionalNotes = @"";

        [Command(
            CommandName,
            Description = "TODO",
            ExtendedHelpText = AdditionalNotes)]
        public class Arguments
            : SourceConfigOutputDirArguments
        {
            public override Task<int> Execute(CommandLineApplication app)
            {
                SpectralPeakTrackingEntry.Execute(this);
                return this.Ok();
            }
        }

        public static void Execute(Arguments arguments)
        {
            // this is a generic command for testing
            // input should be only one-minute wav file
            // read in the config file
            // pass the config to the algorithm
            // output the results

            var configPath = @"SpectralPeakTrackingConfig.yml";
            var recordingPath = @"";

            var configFile = configPath.ToFileInfo();

            if (configFile == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!configFile.Exists)
            {
                throw new ArgumentException($"Config file {configFile.FullName} not found");
            }

            var configuration = ConfigFile.Deserialize<SpectralPeakTrackingConfig>(configFile);

            var recording = new AudioRecording(recordingPath);

            // get the nyquist value from the recording
            int nyquist = new AudioRecording(recordingPath).Nyquist;
            int frameSize = configuration.FrameWidth;
            double frameOverlap = configuration.FrameOverlap;
            int finalBinCount = 512;
            FreqScaleType scaleType = FreqScaleType.Linear;

            var sonoConfig = new SonogramConfig
            {
                WindowSize = frameSize,
                WindowOverlap = frameOverlap,
                DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // Noise Reduction to be added

            SpectralPeakTracking2018.SpectralPeakTracking(sonogram.Data, configuration.Settings);

        }

    }
}
