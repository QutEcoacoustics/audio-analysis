using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalysisTools.StandardSpectrograms
{
    using Acoustics.Tools.Wav;
    using TowseyLibrary;

    public class EnergySpectrogram : BaseSonogram
    {
        public EnergySpectrogram(SonogramConfig config, double[,] amplitudeSpectrogram)
            : base(config, amplitudeSpectrogram)
        {
            this.Configuration = config;
            this.FrameCount = amplitudeSpectrogram.GetLength(0);
            this.Data = amplitudeSpectrogram;
            this.Make(this.Data);
        }

        public EnergySpectrogram(AmplitudeSonogram sg)
            : base(sg.Configuration)
        {
            this.Data = MatrixTools.SquareValues(sg.Data);
        }

        public override void Make(double[,] amplitudeM)
        {
            this.Data = MatrixTools.SquareValues(amplitudeM);
        }
    }
}
