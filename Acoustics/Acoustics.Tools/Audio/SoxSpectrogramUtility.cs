namespace Acoustics.Tools.Audio
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using Acoustics.Shared;

    using log4net;

    public class SoxSpectrogramUtility : AbstractSpectrogramUtility, ISpectrogramUtility
    {
        private readonly FileInfo soxExe;

        public SoxSpectrogramUtility(FileInfo soxExe)
        {
            this.CheckExe(soxExe, "sox.exe");
            this.soxExe = soxExe;
        }

        /// <summary>
        /// Create a spectrogram from a segment of the <paramref name="source"/> audio file.
        /// <paramref name="output"/> image file will be created.
        /// </summary>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="sourceMimeType">
        /// The source Mime Type.
        /// </param>
        /// <param name="output">
        /// The output image file. Ensure the file does not exist.
        /// </param>
        /// <param name="outputMimeType">
        /// The output Mime Type.
        /// </param>
        public void Create(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType)
        {
            var process = new ProcessRunner(this.soxExe.FullName);


            /*
            sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.wav" -n stat stats trim 0 60 spectrogram -m -r -l -w Bartlett -X 45 -y 257 -o "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.png" stats stat

sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8.wav" -n trim 0 10 noiseprof | sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8.wav" "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8-reduced.wav" noisered

sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8-reduced.wav" -n spectrogram -m -r -l -w Bartlett -X 45 -y 257 -o "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8-reduced.png" stats stat

I:\Projects\QUT\QutSensors\sensors-trunk\Extra Assemblies\sox>sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.wav" -n trim 0 60  spectrogram -m -r -l -w Bartlett -X 45 -y 257 -o "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoal
a MaleKoala.png" -z 180 -q 100 stats stat noiseprof
             */

            string args = string.Empty;//ConstructResamplingArgs(source, output);

            this.RunExe(process, args, output.DirectoryName);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
                Log.Debug("Output " + this.BuildFileDebuggingOutput(output));
            }
        }
    }
}