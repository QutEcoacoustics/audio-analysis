// -----------------------------------------------------------------------
// <copyright file="AnalysisHelpers.cs" company="QUT">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AnalysisPrograms.Process
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    using AudioAnalysisTools;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class AnalysisHelpers
    {
        public static DataTable BuildDefaultDataTable(List<AcousticEvent> eventResults)
        {
            var table = new DataTable("OscillationRecogniserAnalysisResults");

            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ScoreNormalised", typeof(double));
            table.Columns.Add("EventStartSeconds", typeof(double));
            table.Columns.Add("EventEndSeconds", typeof(double));
            table.Columns.Add("EventFrequencyMaxSeconds", typeof(double));
            table.Columns.Add("EventFrequencyMinSeconds", typeof(double));
            table.Columns.Add("Information", typeof(string));

            foreach (var eventResult in eventResults)
            {
                var newRow = table.NewRow();
                newRow["Name"] = eventResult.Name;
                newRow["ScoreNormalised"] = eventResult.ScoreNormalised;
                newRow["EventStartSeconds"] = eventResult.TimeStart;
                newRow["EventEndSeconds"] = eventResult.TimeEnd;
                newRow["EventFrequencyMaxSeconds"] = eventResult.MinFreq;
                newRow["EventFrequencyMinSeconds"] = eventResult.MaxFreq;

                if (eventResult.ResultPropertyList != null && eventResult.ResultPropertyList.Any())
                {
                    newRow["Information"] =
                        eventResult.ResultPropertyList.Where(i => i != null).Select(i => i.ToString());
                }
                else
                {
                    newRow["Information"] = "No Information";
                }

                table.Rows.Add(newRow);
            }

            return table;
        }
    }
}
