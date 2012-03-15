namespace AnalysisPrograms.AudioProcessors
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AudioTools.AudioUtlity;

    using QutSensors.Shared;
    using QutSensors.Shared.Tools;

    public abstract class AbstractAudioProcessor
    {
        public AbstractAudioProcessor()
        {

        }

        /// <summary>
        /// Run the analysis.
        /// </summary>
        /// <param name="config">Configuration for this run of the analysis.</param>
        /// <param name="sourceAudioFile">The raw, original</param>
        /// <returns></returns>
        public DataTable Run(DirectoryInfo analysisWorkingDirectory, AudioProcessorConfig config, FileInfo sourceAudioFile)
        {
            var audioUtility = Create(config);
            var ext = Path.GetExtension(sourceAudioFile.FullName);
            var mimeType = MediaTypes.GetMediaType(ext);

            var duration = audioUtility.Duration(sourceAudioFile, mimeType);

            var segmenter = new Segmenter();

            var desiredSegmentSize = TimeSpan.FromMinutes(1);
            var minSegmentSize = TimeSpan.FromSeconds(30);
            var segments = segmenter.CreateSegments(duration, desiredSegmentSize, minSegmentSize);

            var dataTable = new DataTable();

            foreach (var segment in segments)
            {
                var currentSegmentFileName = Path.GetFileNameWithoutExtension(sourceAudioFile.FullName) + "_" + segment.Minimum.TotalMinutes + "_" + segment.Minimum.TotalMilliseconds + "." + MediaTypes.ExtWav;
                var currentSegmentFile = new FileInfo(Path.Combine(analysisWorkingDirectory.FullName, currentSegmentFileName));

                audioUtility.Segment(sourceAudioFile, mimeType, currentSegmentFile, MediaTypes.MediaTypeWav, segment.Minimum, segment.Maximum);

                var results = Analysis(config, currentSegmentFile);

                foreach (var row in results.Rows)
                {
                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }

        protected abstract DataTable Analysis(AudioProcessorConfig config, FileInfo segmentAudioFile);

        private IAudioUtility Create(AudioProcessorConfig config)
        {
            SpecificWavAudioUtility audioUtility = SpecificWavAudioUtility.Create();
            audioUtility.SoxAudioUtility.ResampleQuality = SoxAudioUtility.SoxResampleQuality.VeryHigh; //Options: Low, Medium, High, VeryHigh 
            //audioUtility.SoxAudioUtility.TargetSampleRateHz = config.GetInt("ResampleRate");
            audioUtility.SoxAudioUtility.ReduceToMono = true;
            audioUtility.SoxAudioUtility.UseSteepFilter = true;
            return audioUtility;
        }
    }
}
