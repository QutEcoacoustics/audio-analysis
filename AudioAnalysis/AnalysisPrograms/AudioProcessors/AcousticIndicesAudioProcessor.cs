namespace AnalysisPrograms
{
    using System.Data;
    using System.IO;

    using AnalysisPrograms.AudioProcessors;


    class AcousticIndicesAudioProcessor : AbstractAudioProcessor
    {
        protected override DataTable Analysis(AudioProcessorConfig config, FileInfo segmentAudioFile)
        {
            var data = new DataTable();
            return data;
        }
    }
}
