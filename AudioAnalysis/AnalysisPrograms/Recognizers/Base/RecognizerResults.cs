// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecognizerResults.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the RecognizerResults type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Base
{
    using System.Collections.Generic;
    using System.Drawing;

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

        public Image ScoreTrack { get; set; }

        #endregion
    }
}