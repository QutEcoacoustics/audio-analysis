using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLibrary;
using Acoustics.Tools.Wav;
using AudioAnalysisTools.DSP;



namespace AudioAnalysisTools.StandardSpectrograms
{
    public class SpectrogramSobelEdge : BaseSonogram
    {
        public SpectrogramSobelEdge(string configFile, WavReader wav)
            : base(SonogramConfig.Load(configFile), wav)
        { }

        public SpectrogramSobelEdge(SonogramConfig config, WavReader wav)
            : base(config, wav)
        { }

        public override void Make(double[,] amplitudeM)
        {
            Data = SobelEdgegram(amplitudeM);
        }

        double[,] SobelEdgegram(double[,] matrix)
        {
            double[,] m = MFCCStuff.DecibelSpectra(matrix, this.Configuration.WindowPower, this.SampleRate, this.Configuration.epsilon); //from spectrogram
            //double[,] m = Speech.DecibelSpectra(matrix);

            //NOISE REDUCTION
            var output = SNR.NoiseReduce(m, Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.SnrFullband.ModalNoiseProfile = output.Item2;
            return ImageTools.SobelEdgeDetection(output.Item1);
        }
    }// end SobelEdgeSonogram : BaseSonogram
}
