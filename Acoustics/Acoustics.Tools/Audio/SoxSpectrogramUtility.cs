namespace Acoustics.Tools.Audio
{
    using System;
    using System.IO;

    using Acoustics.Shared;

    using log4net;

    public class SoxSpectrogramUtility : ISpectrogramUtility
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(SoxSpectrogramUtility));

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

            process.Run(args, output.DirectoryName);

            log.Debug(process.BuildLogOutput());
        }

        /// <exception cref="ArgumentNullException"><paramref name="file" /> is <c>null</c>.</exception>
        /// <exception cref="FileNotFoundException">Could not find exe.</exception>
        /// <exception cref="ArgumentException">file</exception>
        protected void CheckExe(FileInfo file, string expectedFileName)
        {
            if (string.IsNullOrEmpty(expectedFileName))
            {
                throw new ArgumentNullException("expectedFileName");
            }

            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (!File.Exists(file.FullName))
            {
                throw new FileNotFoundException("Could not find exe: " + file.FullName, file.FullName);
            }

            if (file.Name != expectedFileName)
            {
                throw new ArgumentException("Expected file name to be " + expectedFileName + ", but was: " + file.Name, "file");
            }
        }
    }
}