using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using Acoustics.Tools.Wav;


namespace AudioAnalysisTools.Sonogram
{
    public class SobelEdgeSonogram : BaseSonogram
    {
        public SobelEdgeSonogram(string configFile, WavReader wav)
            : base(SonogramConfig.Load(configFile), wav)
        { }

        public SobelEdgeSonogram(SonogramConfig config, WavReader wav)
            : base(config, wav)
        { }

        public override void Make(double[,] amplitudeM)
        {
            Data = SobelEdgegram(amplitudeM);
        }

        double[,] SobelEdgegram(double[,] matrix)
        {
            double[,] m = Speech.DecibelSpectra(matrix, this.Configuration.WindowPower, this.SampleRate, this.Configuration.epsilon); //from spectrogram
            //double[,] m = Speech.DecibelSpectra(matrix);

            //NOISE REDUCTION
            var output = SNR.NoiseReduce(m, Configuration.NoiseReductionType, this.Configuration.NoiseReductionParameter);
            this.SnrFullband.ModalNoiseProfile = output.Item2;
            return ImageTools.SobelEdgeDetection(output.Item1);
        }
    }// end SobelEdgeSonogram : BaseSonogram
}
