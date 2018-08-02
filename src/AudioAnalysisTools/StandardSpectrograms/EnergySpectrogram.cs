// <copyright file="EnergySpectrogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using Acoustics.Tools.Wav;
    using TowseyLibrary;

    /// <summary>
    /// There are two CONSTRUCTORS
    /// </summary>
    public class EnergySpectrogram
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnergySpectrogram"/> class.
        /// Use this constructor when you have config and audio objects
        /// </summary>
        public EnergySpectrogram(SpectrogramSettings config, WavReader wav)
            : this(new AmplitudeSpectrogram(config, wav))
        {
        }

        public EnergySpectrogram(AmplitudeSpectrogram amplitudeSpectrogram)
        {
            this.Configuration = amplitudeSpectrogram.Configuration;
            this.Attributes = amplitudeSpectrogram.Attributes;

            // CONVERT AMPLITUDES TO ENERGY
            this.Data = MatrixTools.SquareValues(amplitudeSpectrogram.Data);
        }

        public SpectrogramSettings Configuration { get; set; }

        public SpectrogramAttributes Attributes { get; set; }

        /// <summary>
        /// Gets or sets the spectrogram data matrix of doubles
        /// Note matrix orientation: ROWS = spectra;  COLUMNS = frequency bins
        /// </summary>
        public double[,] Data { get; set; }

        public void GetPsd(string path)
        {
            var psd = MatrixTools.GetColumnAverages(this.Data);

            FileTools.WriteArray2File(psd, path + ".csv");
            GraphsAndCharts.DrawGraph(psd, "Title", new FileInfo(path));

            //GraphsAndCharts.DrawGraph("Title", psd, width, height, 4 new FileInfo(path));
            //image.Save(path, ImageFormat.Png);
        }

        public void DrawLogPsd(string path)
        {
            var psd = MatrixTools.GetColumnAverages(this.Data);
            var logPsd = DataTools.LogValues(psd);
            FileTools.WriteArray2File(logPsd, path + ".csv");
            GraphsAndCharts.DrawGraph(logPsd, "log PSD", new FileInfo(path));

            //GraphsAndCharts.DrawGraph("Title", psd, width, height, 4 new FileInfo(path));
            //image.Save(path, ImageFormat.Png);
        }

        public double[] GetLogPsd()
        {
            var psd = MatrixTools.GetColumnAverages(this.Data);
            var logPsd = DataTools.LogValues(psd);
            return logPsd;

        }
    }
}
