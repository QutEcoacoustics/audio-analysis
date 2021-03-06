// <copyright file="SpectrogramSobelEdge.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using Acoustics.Tools.Wav;
    using AudioAnalysisTools.DSP;
    using TowseyLibrary;

    public class SpectrogramSobelEdge : BaseSonogram
    {
        public SpectrogramSobelEdge(string configFile, WavReader wav)
            : base(SonogramConfig.Load(configFile), wav)
        {
        }

        public SpectrogramSobelEdge(SonogramConfig config, WavReader wav)
            : base(config, wav)
        {
        }

        public override void Make(double[,] amplitudeM)
        {
            this.Data = this.SobelEdgegram(amplitudeM);
        }

        private double[,] SobelEdgegram(double[,] matrix)
        {
            double[,] m = MFCCStuff.DecibelSpectra(matrix, this.Configuration.WindowPower, this.SampleRate, this.Configuration.epsilon); //from spectrogram

            //double[,] m = Speech.DecibelSpectra(matrix);

            //NOISE REDUCTION
            var output = SNR.NoiseReduce(m, this.Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.SnrData.ModalNoiseProfile = output.Item2;
            return ImageTools.SobelEdgeDetection(output.Item1);
        }
    }
}