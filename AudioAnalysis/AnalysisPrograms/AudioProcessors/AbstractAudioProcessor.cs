namespace AnalysisPrograms.AudioProcessors
{
    using System;
    using System.Data;
    using System.IO;

    using Acoustics.Shared;
    using Acoustics.Tools.Audio;


    public abstract class AbstractAudioProcessor
    {
        public AbstractAudioProcessor()
        {

        }

        /// <summary>
        /// Run the analysis by looping over all source file segments.
        /// </summary>
        /// <param name="config">Configuration for this run of the analysis.</param>
        /// <param name="sourceAudioFile">The raw, original</param>
        /// <returns></returns>
        public DataTable Run(DirectoryInfo analysisWorkingDirectory, AudioProcessorConfig config, FileInfo sourceAudioFile)
        {
            var audioUtility = new MasterAudioUtility();
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
    }
}
