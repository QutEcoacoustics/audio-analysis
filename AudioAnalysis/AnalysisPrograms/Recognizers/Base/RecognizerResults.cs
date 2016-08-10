namespace AnalysisPrograms.Recognizers.Base
{
    using System.Collections.Generic;

    using AnalysisBase.ResultBases;

    using AudioAnalysisTools;

    using TowseyLibrary;

    public class RecognizerResults
    {
        #region Public Properties

        public List<AcousticEvent> Events { get; set; }

        public double[,] Hits { get; set; }

        public Plot Plot { get; set; }

        public BaseSonogram Sonogram { get; set; }

        #endregion
    }
}