// <copyright file="DescriptionResult.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class holds the results of content description for a unit of recording, assumed to be one-minute.
    /// The results are held in a dictionary.
    /// </summary>
    public class DescriptionResult
    {
        private readonly Dictionary<string, double> descriptionDictionary = new Dictionary<string, double>();

        public DescriptionResult(int startTimeInMinutes) => this.StartTimeInCurrentRecordingFile = TimeSpan.FromMinutes(startTimeInMinutes);

        public TimeSpan StartTimeInCurrentRecordingFile { get; set; }

        public void AddDescription(KeyValuePair<string, double> kvp) => this.descriptionDictionary.Add(kvp.Key, kvp.Value);

        public Dictionary<string, double> GetDescriptionDictionary() => this.descriptionDictionary;
    }
}