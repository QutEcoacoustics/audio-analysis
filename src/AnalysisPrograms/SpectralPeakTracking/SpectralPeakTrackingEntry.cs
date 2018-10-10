// <copyright file="SpectralPeakTrackingEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.SpectralPeakTracking
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading.Tasks;
    using Acoustics.Shared.ConfigFile;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using McMaster.Extensions.CommandLineUtils;
    using Production.Arguments;

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

            var configPath = @"C:\Users\kholghim\Mahnoosh\Night_parrot\SpectralPeakTrackingConfig.yml";
            var recordingPath = @"C:\Users\kholghim\Mahnoosh\Night_parrot\JY-(cleaned)-3-Night_Parrot-pair.Western_Qld.mp3";
            var imagePath = @"C:\Users\kholghim\Mahnoosh\Night_parrot\image.bmp";

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
            var hertzPerFreqBin = nyquist / finalBinCount;
            FreqScaleType scaleType = FreqScaleType.Linear;

            var sonoConfig = new SonogramConfig
            {
                WindowSize = frameSize,
                WindowOverlap = frameOverlap,
                DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
            };

            //var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            var amplitudeSpectrogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            var energySpectrogram = new EnergySpectrogram(amplitudeSpectrogram);
            var decibelSpectrogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            double frameStepSize = sonoConfig.GetFrameOffset();
            double stepDuration = frameStepSize / (nyquist * 2);



            // Noise Reduction to be added

            var output = SpectralPeakTracking2018.SpectralPeakTracking(energySpectrogram.Data, configuration.Settings, hertzPerFreqBin);

            // draw the local peaks
            double[,] hits = SpectralPeakTracking2018.MakeHitMatrix(energySpectrogram.Data, output.TargetPeakBinsIndex, output.BandIndex);
            var image = SpectralPeakTracking2018.DrawSonogram(decibelSpectrogram, hits);
            image.Save(imagePath, ImageFormat.Bmp);
        }
    }
}
