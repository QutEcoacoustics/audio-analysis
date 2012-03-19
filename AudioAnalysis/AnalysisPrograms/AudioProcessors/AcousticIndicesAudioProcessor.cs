namespace AnalysisPrograms
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
