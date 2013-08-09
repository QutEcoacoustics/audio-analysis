namespace Acoustics.Tools.Audio
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;

    using Acoustics.Shared;

    /// <summary>
    /// Spectrogram utility that uses sox.exe to generate spectrograms.
    /// </summary>
    public class SoxSpectrogramUtility : AbstractSpectrogramUtility, ISpectrogramUtility
    {
        /// <summary>
        /// Default to verbose output, smaple rate of 22050, monochrome spectrogram using hann window function without axises.
        /// Uses pixels per second measurement from Towsey's spectrograms.
        /// </summary>
        /// <remarks>
        /// See http://sox.sourceforge.net/sox.html for details on the arguments for spectrogram.
        /// </remarks>
        private const string ArgsFormatString = " -V \"{0}\" -n rate 22050 spectrogram -m -r -l -a -q 249 -w hann -y 257 -X 43.06640625 -z 100 -o \"{1}\"";

        private readonly IAudioUtility audioUtility;

        private readonly FileInfo soxExe;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoxSpectrogramUtility"/> class.
        /// </summary>
        /// <param name="audioUtility">
        /// The audio utility.
        /// </param>
        /// <param name="soxExe">
        /// The sox exe.
        /// </param>
        public SoxSpectrogramUtility(IAudioUtility audioUtility, FileInfo soxExe)
        {
            this.CheckExe(soxExe, "sox");
            this.audioUtility = audioUtility;
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
        /// <param name="request">
        /// The spectrogram request.
        /// </param>
        public void Create(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType, SpectrogramRequest request)
        {
            this.ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            this.CanProcess(output, new[] { MediaTypes.MediaTypePng, MediaTypes.MediaTypeJpeg }, null);

            // to get a proper image from sox, need to remove DC value, plus 1px from top and left. 
            var wavFile = TempFileHelper.NewTempFileWithExt(MediaTypes.ExtWav);
            var originalSoxFile = TempFileHelper.NewTempFileWithExt(MediaTypes.ExtPng);

            var audioUtilRequest = new AudioUtilityRequest { OffsetStart = request.Start, OffsetEnd = request.End, MixDownToMono = true, TargetSampleRate = 22050 };

            this.audioUtility.Modify(source, sourceMimeType, wavFile, MediaTypes.MediaTypeWav, audioUtilRequest);

            // generate spectrogram using sox.
            if (this.Log.IsDebugEnabled)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Spectrogram(wavFile, originalSoxFile);

                stopwatch.Stop();

                this.Log.DebugFormat(
                    "Generated and saved spectrogram for {0}. Took {1} ({2}ms).",
                    source.Name,
                    stopwatch.Elapsed.Humanise(),
                    stopwatch.Elapsed.TotalMilliseconds);

                this.Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
                this.Log.Debug("Output " + this.BuildFileDebuggingOutput(output));
            }
            else
            {
                this.Spectrogram(wavFile, originalSoxFile);
            }

            wavFile.Delete();

            // modify the original image to match the request
            using (var sourceImage = Image.FromFile(originalSoxFile.FullName))
            {
                // remove 1px from top, bottom (DC value) and left
                var sourceRectangle = new Rectangle(1, 1, sourceImage.Width - 1, sourceImage.Height - 2);

                using (var requestedImage = new Bitmap(
                    request.IsCalculatedWidthAvailable ? request.CalculatedWidth : sourceRectangle.Width,
                    request.Height.HasValue ? request.Height.Value : sourceRectangle.Height))
                using (var graphics = Graphics.FromImage(requestedImage))
                {
                    var destRectangle = new Rectangle(0, 0, requestedImage.Width, requestedImage.Height);
                    graphics.DrawImage(sourceImage, destRectangle, sourceRectangle, GraphicsUnit.Pixel);

                    var format = MediaTypes.GetImageFormat(MediaTypes.GetExtension(outputMimeType));

                    if (this.Log.IsDebugEnabled)
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        requestedImage.Save(output.FullName, format);

                        stopwatch.Stop();

                        this.Log.DebugFormat(
                            "Saved spectrogram for {0} to {1}. Took {2} ({3}ms).",
                            source.Name,
                            output.Name,
                            stopwatch.Elapsed.Humanise(),
                            stopwatch.Elapsed.TotalMilliseconds);

                        this.Log.Debug("Output " + this.BuildFileDebuggingOutput(output));
                    }
                    else
                    {
                        requestedImage.Save(output.FullName, format);
                    }
                }
            }

            originalSoxFile.Delete();
        }

        private void Spectrogram(FileInfo sourceAudioFile, FileInfo outputImageFile)
        {
            var process = new ProcessRunner(this.soxExe.FullName);

            string args = this.ConstructArgs(sourceAudioFile, outputImageFile);

            this.RunExe(process, args, outputImageFile.DirectoryName);

            if (this.Log.IsDebugEnabled)
            {
                this.Log.Debug("Source " + this.BuildFileDebuggingOutput(sourceAudioFile));
                this.Log.Debug("Output " + this.BuildFileDebuggingOutput(outputImageFile));
            }
        }

        private string ConstructArgs(FileInfo sourceAudioFile, FileInfo outputImageFile)
        {
            var args = string.Format(ArgsFormatString, sourceAudioFile.FullName, outputImageFile.FullName);

            return args;

            /*
            sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.wav" -n stat stats trim 0 60 spectrogram -m -r -l -w Bartlett -X 45 -y 257 -o "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.png" stats stat

sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8.wav" -n trim 0 10 noiseprof | sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8.wav" "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8-reduced.wav" noisered

sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8-reduced.wav" -n spectrogram -m -r -l -w Bartlett -X 45 -y 257 -o "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\GParrots_JB2_20090607-173000.wav_minute_8-reduced.png" stats stat

I:\Projects\QUT\QutSensors\sensors-trunk\Extra Assemblies\sox>sox "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoala MaleKoala.wav" -n trim 0 60  spectrogram -m -r -l -w Bartlett -X 45 -y 257 -o "I:\Projects\QUT\QutSensors\sensors-trunk\QutSensors.Test\TestData\FemaleKoal
a MaleKoala.png" -z 180 -q 100 stats stat noiseprof
             */
            /*
            cd "C:\Work\Software Dev\test-audio"
"C:\Work\Software Dev\svn-trunk\Extra Assemblies\sox\sox.exe" -V "C:\Work\Software Dev\test-audio\cane toad_20120523_082701.wav" -n rate 22050 spectrogram -m -l -a -q 249 -Z 0 -z 150 -y 257 -X 43.0664 -o "cane toad_20120523_082701.png" -w hann
             * " -V \"{0}\"  -n rate 22050 spectrogram -m -l -a -q 249 -Z 0 -z 150 -y 257 -X 43.0664 -o \"{1}\" -w hann",
    */


        }
    }
}