using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysis;
using AudioTools;
using System.Threading;
using QutSensors.Data;
using System.Xml.Linq;
using System.IO;
using TowseyLib;
using QutSensors.AudioAnalysis.AED;
using QutSensors.Processor;

using QutSensors.Processor.WebServices;

namespace QutSensors.Processor
{
    public class AEDProcessing : Processor
    {
        public AEDProcessing(ProcessorSettings settings)
            : base(settings)
        {
        }

        public override IEnumerable<ProcessorJobItemResult> Process(TempFile inputFile, ProcessorJobItemDescription item, out TimeSpan? duration)
        {
            return FindAcousticEvents(inputFile, item.MimeType, out duration);
        }

        IEnumerable<ProcessorJobItemResult> FindAcousticEvents(TempFile file, string mimeType, out TimeSpan? duration)
        {
            var retVal = new List<ProcessorJobItemResult>();

            duration = DShowConverter.GetDuration(file.FileName, mimeType);
            if (duration == null)
            {
                OnLog("Unable to calculate length");
                throw new Exception("Unable to calculate length");
            }
            OnLog("Total length: {0}", duration);
            for (int i = 0; i < duration.Value.TotalMilliseconds; i += 60000)
            {
                OnLog("\t{0}-{1}", TimeSpan.FromMilliseconds(i), TimeSpan.FromMilliseconds(i + 60000));
                using (var converted = DShowConverter.ConvertTo(file.FileName, mimeType, MimeTypes.WavMimeType, i, i + 60000) as BufferedDirectShowStream)
                {

                    SonogramConfig config = new SonogramConfig();
                    config.NoiseReductionType = ConfigKeys.NoiseReductionType.NONE;
                    BaseSonogram sonogram = new SpectralSonogram(config, new AudioRecording(converted.BufferFile.FileName).GetWavReader());
                    double[,] matrix = sonogram.Data;

                    Console.WriteLine("START: DETECTION");
                    IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(Default.intensityThreshold, Default.smallAreaThreshold, matrix);
                    Console.WriteLine("END: DETECTION");

                    //set up static variables for init Acoustic events
                    //AcousticEvent.   doMelScale = config.DoMelScale;
                    AcousticEvent.FreqBinCount = config.FreqBinCount;
                    AcousticEvent.FreqBinWidth = config.FftConfig.NyquistFreq / (double)config.FreqBinCount;
                    //  int minF        = (int)config.MinFreqBand;
                    //  int maxF        = (int)config.MaxFreqBand;
                    AcousticEvent.FrameDuration = config.GetFrameOffset();


                    var events = new List<AcousticEvent>();
                    foreach (Oblong o in oblongs)
                    {
                        var e = new AcousticEvent(o);
                        events.Add(e);
                    }

                    //OnLog("RESULT: {0}, {1}, {2}", result.NumberOfPeriodicHits, result.VocalBestFrame, result.VocalBestLocation);

                    StringReader reader = new StringReader(ResultSerializer.SerializeAEDResult(events).InnerXml);

                    XElement element = XElement.Load(reader);

                    retVal.Add(new ProcessorJobItemResult()
                    {
                        Start = i,
                        Stop = i + 60000,
                        Results = element,
                        RankingScoreValue = events.Count,
                        RankingScoreName = "Event Count",
                        RankingScoreLocation = 0.0
                    }
                    );

                }
                //if (State == ProcessorState.Stopping)
                //{
                //    return null;
                //}
            }
            return retVal;
        }
    }
}
