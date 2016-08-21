using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Recognizers
{
    using System.Reflection;

    using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Recognizers.Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;
    using System.Drawing;

    /// <summary>
    /// This is a template recognizer
    /// </summary>
    class BlueCatfish : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "BlueCatfish";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Summarize your results. This method is invoked exactly once per original file.
        /// </summary>
        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // No operation - do nothing. Feel free to add your own logic.
            base.SummariseResults(settings, inputFileSegment, events, indices, spectralIndices, results);
        }

        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="audioRecording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording audioRecording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, int? imageWidth)
        {

            //string path = @"C:\SensorNetworks\WavFiles\Freshwater\savedfortest.wav";
            //audioRecording.Save(path); // this does not work

            int scalingFactor = 50;
            //double[] subsample = DataTools.Subarray(audioRecording.WavReader.Samples, 584000, 3500);
            int imageHeight = 400;
            //Image image1 = ImageTools.DrawWaveform("wave, scale*5", subsample, subsample.Length, imageHeight, scalingFactor);
            //string path1 = @"C:\SensorNetworks\Output\Freshwater\subsample1.png";
            //image1.Save(path1);

            // high pass filter
            //double[] subsample = DataTools.Subarray(audioRecording.WavReader.Samples, 584000, 3500);
            //double[] signalHighPass = DSP_Filters.PreEmphasis(subsample, 1.0);
            //Image image2 = ImageTools.DrawWaveform("highpass filtered, scale*5", signalHighPass, signalHighPass.Length, imageHeight, scalingFactor);
            //string path2 = @"C:\SensorNetworks\Output\Freshwater\subsample2.png";
            //image2.Save(path2);

            ////low pass filter
            //string filterName = "Chebyshev_Lowpass_1000, scale*5";
            //DSP_IIRFilter filter = new DSP_IIRFilter(filterName);
            //int order = filter.order;
            //System.LoggedConsole.WriteLine("\nTest " + filterName + ", order=" + order);
            //double[] filteredSignal;
            //filter.ApplyIIRFilter(signalHighPass, out filteredSignal);
            //Image image3 = ImageTools.DrawWaveform("lowpass and highpass filtered", filteredSignal, filteredSignal.Length, imageHeight, scalingFactor*50);
            //string path3 = @"C:\SensorNetworks\Output\Freshwater\subsample3.png";
            //image3.Save(path3);


            // high pass filter
            double[] signalHighPass = DSP_Filters.PreEmphasis(audioRecording.WavReader.Samples, 1.0);
            //low pass filter
            string filterName = "Chebyshev_Lowpass_1000, scale*5";
            DSP_IIRFilter filter = new DSP_IIRFilter(filterName);
            int order = filter.order;
            System.LoggedConsole.WriteLine("\nTest " + filterName + ", order=" + order);
            double[] filteredSignal;
            filter.ApplyIIRFilter(signalHighPass, out filteredSignal);


            // count number of 1000 sample segments
            int signalLength = filteredSignal.Length;
            int blockLength = 1000;
            int blockCount = signalLength / blockLength;
            int[] indexOfMax = new int[blockCount];
            double[] maxInBlock = new double[blockCount];
            
            for (int i = 0; i < blockCount; i++)
            {
                double max = -2.0;
                int blockStart = blockLength * i;
                for (int s = 0; s < blockLength; s++)
                {
                    double absValue = Math.Abs(filteredSignal[blockStart + s]);
                    if (absValue > max)
                    {
                        max = absValue;
                        maxInBlock[i] = max;
                        indexOfMax[i] = blockStart + s;
                    }
                }
            }

            // find the blocks that contain a max value that is > neighbouring blocks
            var indexList = new List<int>();
            for (int i = 1; i < blockCount-1; i++)
            {
                if ((maxInBlock[i] > maxInBlock[i-1]) && (maxInBlock[i] > maxInBlock[i+1]))
                {
                    indexList.Add(indexOfMax[i]);
                }
            }

            // now process neighbourhood of each max
            FFT.WindowFunc wf = FFT.Hamming;
            var fft = new FFT(2048, wf);

            int id = 0;
            foreach (int location in indexList)
            {
                System.LoggedConsole.WriteLine("Location " + location + ", id=" + id);

                double[] subsample = DataTools.Subarray(filteredSignal, location-1024, 2048);
                Image image4a = ImageTools.DrawWaveform("lowpass and highpass filtered", subsample, subsample.Length, imageHeight, scalingFactor * 30);
            
                var spectrum = fft.Invoke(subsample);
                int requiredBinCount = spectrum.Length / 11; // this assumes that nyquiust = 11,025
                double hzPerBin = 11025 / (double)1024;
                var subBand = DataTools.Subarray(spectrum, 1, requiredBinCount);
                string title = String.Format("FFT 1-1000 Hz.,    hz/bin={0:f1}", hzPerBin);
                Image image4b = ImageTools.DrawGraph(title, subBand, subsample.Length, imageHeight, 30);

                var imageList = new List<Image>();
                imageList.Add(image4a);
                imageList.Add(image4b);
                var image4 = ImageTools.CombineImagesVertically(imageList);
                string path4 = String.Format(@"C:\SensorNetworks\Output\Freshwater\subsamples\subsample_{0}_{1}.png", id, location);
                image4.Save(path4);
                id++;
            }




            // Get a value from the config file - with a backup default
            int minHz = (int?)configuration[AnalysisKeys.MinHz] ?? 600;

            // Get a value from the config file - with no default, throw an exception if value is not present
            //int maxHz = ((int?)configuration[AnalysisKeys.MaxHz]).Value;

            // Get a value from the config file - without a string accessor, as a double
            double someExampleSettingA = (double?)configuration.someExampleSettingA ?? 0.0;

            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";



            // get samples
            var samples = audioRecording.WavReader.Samples;


            // make a spectrogram
            var config = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.STANDARD,
                NoiseReductionParameter = (double?)configuration[AnalysisKeys.NoiseBgThreshold] ?? 0.0
            };
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);

            // get high resolution indices

            // when the value is accessed, the indices are calculated
            var indices = getSpectralIndexes.Value;

            // check if the indices have been calculated - you shouldn't actually need this
            if (getSpectralIndexes.IsValueCreated)
            {
                // then indices have been calculated before
            }

            var foundEvents = new List<AcousticEvent>();

            // some kind of loop where you scan through the audio

            // 'find' an event - if you find an event, store the data in the AcousticEvent class
            var anEvent = new AcousticEvent(
                new Oblong(50, 50, 100, 100),
                sonogram.NyquistFrequency,
                sonogram.Configuration.FreqBinCount,
                sonogram.FrameDuration,
                sonogram.FrameStep,
                sonogram.FrameCount);
            anEvent.Name = "FAKE!";

            foundEvents.Add(anEvent);

            // end loop

            return new RecognizerResults()
            {
                Events = foundEvents,
                Hits = null,
                ScoreTrack = null,
                //Plots = null,
                Sonogram = sonogram
            };
        }
    }
}
