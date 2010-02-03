using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutSensors.Processor.WebServices;
using AudioTools;
using System.Threading;
using QutSensors.Data;
using System.Xml.Linq;
using System.IO;
using TowseyLib;
using QutSensors.AudioAnalysis.AED;
using QutSensors.Processor;
using AudioAnalysisTools;

namespace QutSensors.Processor
{
    public class TemplateProcessing : Processor
    {
        public TemplateProcessing(ProcessorSettings settings)
            : base(settings)
        {
        }

        public override IEnumerable<ProcessorJobItemResult> Process(TempFile inputFile, ProcessorJobItemDescription item, out TimeSpan? duration)
        {
            BaseTemplate.LoadDefaultConfig();

            // Something odd is happening here. foo1 is null yet baz is a Template_CC
            // Revisit how the template was serialized in the first place
            var foo = item.Job.Parameters.BinaryDeserialize();
            var foo1 = foo as Template_CCAuto;
            byte[] bar = (byte[])foo;
            var baz = bar.BinaryDeserialize();
            Template_CCAuto template = baz as Template_CCAuto;

            Recogniser recogniser = new Recogniser(template);

            OnLog("Job Retrieved - JobItemID {0} for {1}({2})", item.JobItemID, item.Job.Name, template.CallName);

            OnLog("Analysing {0}", item.AudioReadingUrl);


            return AnalyseFile(inputFile, item.MimeType, recogniser, out duration);
        }

        IEnumerable<ProcessorJobItemResult> AnalyseFile(TempFile file, string mimeType, Recogniser recogniser, out TimeSpan? duration)
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
                    BaseResult results = recogniser.Analyse(new AudioRecording(converted.BufferFile.FileName)) as BaseResult;

                    //OnLog("RESULT: {0}, {1}, {2}", result.NumberOfPeriodicHits, result.VocalBestFrame, result.VocalBestLocation);

                    StringReader reader = new StringReader(ResultSerializer.SerializeTemplateResult(results).InnerXml);

                    XElement element = XElement.Load(reader);

                    retVal.Add(new ProcessorJobItemResult()
                    {
                        Start = i,
                        Stop = i + 60000,
                        Results = element,
                        RankingScoreValue = results.RankingScoreValue ?? 0.0,
                        RankingScoreName = results.RankingScoreName,
                        RankingScoreLocation = results.TimeOfMaxScore ?? 0.0
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
