using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Text;
using AudioTools;
using QutSensors.AudioAnalysis.AED;
using QutSensors.Processor.WebServices;
using TowseyLib;
using AudioAnalysisTools;

namespace QutSensors.Processor
{
    public class EPRProcessing : Processor
    {
        public EPRProcessing(ProcessorSettings settings)
            : base(settings)
        {
        }

        public override IEnumerable<ProcessorJobItemResult> Process(TempFile inputFile, ProcessorJobItemDescription item, out TimeSpan? duration)
        {
            duration = DShowConverter.GetDuration(inputFile.FileName, item.MimeType);
            if (duration == null)
            {
                OnLog("Unable to calculate length");
                throw new Exception("Unable to calculate length");
            }

            OnLog("DATE AND TIME:" + DateTime.Now);
            OnLog("DETECTION OF ACOUSTIC EVENTS IN RECORDING\n");

            using (var converted = DShowConverter.ConvertTo(inputFile.FileName, item.MimeType, MimeTypes.WavMimeType, null, null) as BufferedDirectShowStream)
            {
                string appConfigPath = "";
                //string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";

                // this is a crap hack which needs fixing - if we are tying to convert from WAV -> WAV, converted == null
                string wavPath = converted == null ? inputFile.FileName : converted.BufferFile.FileName;

                AudioRecording recording = new AudioRecording(wavPath);

                OnLog("appConfigPath =" + appConfigPath);
                OnLog("wav File Path =" + wavPath);
                OnLog();

                SonogramConfig config = SonogramConfig.Load(appConfigPath);
                config.NoiseReductionType = ConfigKeys.NoiseReductionType.NONE;
                BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());
                double[,] matrix = sonogram.Data;

                OnLog("START: AED");
                IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(3.0, 100, matrix);
                OnLog("END: AED");

                //set up static variables for init Acoustic events
                //AcousticEvent.   doMelScale = config.DoMelScale;
                //AcousticEvent.FreqBinCount = config.FreqBinCount;
                double binWidth = config.FftConfig.NyquistFreq / (double)config.FreqBinCount;
                //  int minF        = (int)config.MinFreqBand;
                //  int maxF        = (int)config.MaxFreqBand;
                double frameOffset = config.GetFrameOffset();

                var events = new List<Util.Rectangle<double>>();
                foreach (Oblong o in oblongs)
                {
                    var e = new AcousticEvent(o, frameOffset, binWidth);
                    events.Add(Util.fcornersToRect(e.StartTime, e.EndTime, e.MaxFreq, e.MinFreq));
                    //Console.WriteLine(e.StartTime + "," + e.Duration + "," + e.MinFreq + "," + e.MaxFreq);
                }
                OnLog("START: EPR");
                IEnumerable<Util.Rectangle<double>> eprRects = EventPatternRecog.detectGroundParrots(events);
                OnLog("END: EPR");

                var eprEvents = new List<AcousticEvent>();
                foreach (Util.Rectangle<double> r in eprRects)
                {
                    var ae = new AcousticEvent(r.Left, r.Width, r.Bottom, r.Top);
                    //OnLog(ae.WriteProperties());
                    //OnLog(ae.StartTime + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
                    eprEvents.Add(ae);
                }

                StringReader reader = new StringReader(ResultSerializer.SerializeEPRResult(eprEvents).InnerXml);
                XElement element = XElement.Load(reader);

                var retVal = new List<ProcessorJobItemResult>();
                retVal.Add(new ProcessorJobItemResult()
                {
                    Start = 0,
                    Stop = (int)duration.Value.TotalMilliseconds,
                    Results = element,
                    RankingScoreValue = eprEvents.Count,
                    RankingScoreName = "Event Count",
                    RankingScoreLocation = 0.0
                }
                );

                OnLog("\nFINISHED!");

                return retVal;
            }
        }
    }
}
