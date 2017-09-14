﻿namespace Dong.Felt
{
    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using Configuration;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    using Representations;

    using TowseyLibrary;

    // The query is defined by a region bounded with a fixed frequency range and duration range.
    public class Query
    {
        #region Public Properties

        /// <summary>
        /// gets or sets the maxFrequency, the above boundary of the region.
        /// </summary>
        public double maxFrequency { get; set; }

        /// <summary>
        /// gets or sets the minFrequency, the bottom boundary of the region.
        /// </summary>
        public double minFrequency { get; set; }

        /// <summary>
        /// gets or sets the startTime, the left boundary of the region.
        /// The unit is second.
        /// </summary>
        public double startTime { get; set; }

        /// <summary>
        /// gets or sets the endTime, the right boundary of the region.
        /// The unit is second.
        /// </summary>
        public double endTime { get; set; }

        /// <summary>
        /// gets or sets the duration: endTime substracting startTime, its unit is millisecond.
        /// </summary>
        public double duration { get; set; }

        /// <summary>
        /// gets or sets the frequencyRange by maxFrequency substracting minFrequency.
        /// </summary>
        public double frequencyRange { get; set; }

        /// <summary>
        /// gets or sets the nhCountInRow in a region, which indicates the rowscount of neighbourhoods in the region.
        /// </summary>
        public int nhCountInRow { get; set; }

        /// <summary>
        /// gets or sets the nhCountInColumn in a region, which indicates the columnscount of neighbourhoods in the region.
        /// </summary>
        public int nhCountInColumn { get; set; }

        public int maxNhRowIndex { get; set; }

        public int maxNhColIndex { get; set; }

        /// <summary>
        /// Gets or sets the neighbourhood start row index, it lies in the bottom left corner of the region.
        /// </summary>
        public int nhStartRowIndex { get; set; }

        /// <summary>
        /// Gets or sets the neighbourhood start column index, it lies in the bottom left corner of the region.
        /// </summary>
        public int nhStartColIndex { get; set; }

        public int BottomInPixel { get; set; }

        public int TopInPixel { get; set; }

        public int LeftInPixel { get; set; }

        public int RightInPixel { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// default constructor.
        /// </summary>
        public Query()
        {
            this.maxFrequency = 0.0;
            this.minFrequency = 0.0;
            this.startTime = 0.0;
            this.endTime = 0.0;
            this.duration = this.endTime - this.startTime;
        }

        /// <summary>
        /// set the properties through parsing the parameters.
        /// </summary>
        /// <param name="maximumFrequency"></param>
        /// <param name="minimumFrequency"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        public Query(double maximumFrequency, double minimumFrequency,
            double startTime, double endTime, int neighbourhoodLength,
            int maxFrequencyIndex, int maxFrameIndex,
            SpectrogramConfiguration spectrogramConfig)
        {
            // the unit is confusing
            var secondToMillisecond = 1000;
            this.maxFrequency = maximumFrequency;
            this.minFrequency = minimumFrequency;
            this.startTime = startTime * secondToMillisecond; // millisecond
            this.endTime = endTime * secondToMillisecond; // millisecond
            this.duration = this.endTime - this.startTime;
            this.frequencyRange = this.maxFrequency - this.minFrequency;
            this.maxNhColIndex = maxFrameIndex;
            this.maxNhRowIndex = maxFrequencyIndex;
            this.GetNhProperties(neighbourhoodLength, spectrogramConfig);
        }

        /// <summary>
        /// set the properties through parsing the parameters.
        /// </summary>
        /// <param name="maximumFrequency"></param>
        /// <param name="minimumFrequency"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        public Query(double maximumFrequency, double minimumFrequency, double startTime, double endTime, int neighbourhoodLength,
            int maxFrequencyIndex, int maxFrameIndex,
            SpectrogramConfiguration spectrogramConfig,
            CompressSpectrogramConfig compressConfig)
        {
            var secondToMillisecond = 1000;
            this.maxFrequency = maximumFrequency * compressConfig.FreqCompressRate;
            this.minFrequency = minimumFrequency * compressConfig.FreqCompressRate;
            this.startTime = startTime * secondToMillisecond * compressConfig.TimeCompressRate; // millisecond
            this.endTime = endTime * secondToMillisecond * compressConfig.TimeCompressRate; // millisecond
            this.duration = this.endTime - this.startTime;
            this.frequencyRange = this.maxFrequency - this.minFrequency;
            this.maxNhColIndex = maxFrameIndex;
            this.maxNhRowIndex = maxFrequencyIndex;
            this.GetNhProperties(neighbourhoodLength, spectrogramConfig);
        }

        public Query(double maximumFrequency, double minimumFrequency,
            double startTime, double endTime,
            CompressSpectrogramConfig compressConfig)
        {
            // the unit is confusing
            var secondToMillisecond = 1000;
            this.maxFrequency = maximumFrequency;
            this.minFrequency = minimumFrequency;
            this.startTime = startTime * secondToMillisecond * compressConfig.TimeCompressRate; // millisecond
            this.endTime = endTime * secondToMillisecond * compressConfig.TimeCompressRate; // millisecond
            this.duration = this.endTime - this.startTime;
            this.frequencyRange = this.maxFrequency - this.minFrequency;
        }

        public Query(double maximumFrequency, double minimumFrequency, double startTime, double endTime)
        {
            // the unit is confusing
            var secondToMillisecond = 1000;
            this.maxFrequency = maximumFrequency;
            this.minFrequency = minimumFrequency;
            this.startTime = startTime * secondToMillisecond; // millisecond
            this.endTime = endTime * secondToMillisecond; // millisecond
            this.duration = this.endTime - this.startTime;
            this.frequencyRange = this.maxFrequency - this.minFrequency;

        }

        // to get the nhCountInRow, nhCountInColumn, nhStartRowIndex, nhStartColIndex.
        public void GetNhProperties(int neighbourhoodLength, SpectrogramConfiguration spectrogramConfig
            )
        {
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            var nhFrequencyLength = neighbourhoodLength * frequencyScale;
            var nhFrameLength = neighbourhoodLength * timeScale;
            // get a greater value than the parameter for enlarging the later used NH boundary- enlarge.
            var enlargedOffset = 1;
            // ceiling is try to increase the value.
            var nhCountInRows = (int)Math.Ceiling(this.frequencyRange / nhFrequencyLength) + enlargedOffset;
            var nhCountInCols = (int)Math.Ceiling(this.duration / nhFrameLength) + enlargedOffset;
            /// Here is a trick. Trying to get the nearest and lowest NH frame and frequencyIndex.
            this.nhStartColIndex = (int)Math.Floor(this.startTime / nhFrameLength);
            this.nhStartRowIndex = this.maxNhRowIndex - (int)Math.Ceiling(this.maxFrequency / nhFrequencyLength);
            var nhendTime = (this.nhStartColIndex + nhCountInCols) * nhFrameLength;
            //var nhLowerFreq = (this.nhStartRowIndex + nhCountInRow) * nhFrequencyLength;
            if (this.nhStartRowIndex + nhCountInRows > this.maxNhRowIndex)
            {
                this.nhStartRowIndex--;
            }
            if (this.nhStartColIndex + nhCountInCols > this.maxNhColIndex)
            {
                this.nhStartColIndex--;
            }
            if (nhCountInRows > this.maxNhRowIndex)
            {
                nhCountInRows = this.maxNhRowIndex;
            }
            if (nhCountInCols > this.maxNhColIndex)
            {
                nhCountInCols = this.maxNhColIndex;
            }
            this.nhCountInRow = nhCountInRows;
            this.nhCountInColumn = nhCountInCols;

        }

        /// <summary>
        /// Keep it consistent with neighbourhoodRepresentation list.
        /// </summary>
        /// <param name="queryCsvFile"></param>
        /// <param name="neighbourhoodLength"></param>
        /// <param name="spectrogram"></param>
        /// <param name="spectrogramConfig"></param>
        /// <param name="compressConfig"></param>
        /// <returns></returns>
        public static Query QueryRepresentationFromQueryInfo(FileInfo queryCsvFile, int neighbourhoodLength,
            SpectrogramStandard spectrogram, SpectrogramConfiguration spectrogramConfig, CompressSpectrogramConfig compressConfig)
        {
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            var nhCountInRow = spectrogram.Data.GetLength(1) / neighbourhoodLength;
            var nhCountInColumn = spectrogram.Data.GetLength(0) / neighbourhoodLength;
            var result = new Query(queryInfo.HighFrequencyHertz, queryInfo.LowFrequencyHertz, queryInfo.TimeStart,
                queryInfo.TimeEnd, neighbourhoodLength,
                nhCountInRow, nhCountInColumn, spectrogramConfig, compressConfig);
            return result;
        }

        public static Query QueryRepresentationFromQueryInfo(FileInfo queryCsvFile)
        {
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            var result = new Query(queryInfo.HighFrequencyHertz, queryInfo.LowFrequencyHertz, queryInfo.TimeStart, queryInfo.TimeEnd);
            return result;
        }

        public static Query QueryRepresentationFromQueryInfo(FileInfo queryCsvFile, int neighbourhoodLength,
            SpectrogramStandard spectrogram, SpectrogramConfiguration spectrogramConfig)
        {
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            var nhCountInRow = (spectrogram.Data.GetLength(1) - 1) / neighbourhoodLength;
            if ((spectrogram.Data.GetLength(1) - 1) % neighbourhoodLength == 0)
            {
                nhCountInRow--;
            }
            var nhCountInColumn = spectrogram.Data.GetLength(0) / neighbourhoodLength;
            if (spectrogram.Data.GetLength(0) % neighbourhoodLength == 0)
            {
                nhCountInColumn--;
            }
            var result = new Query(queryInfo.HighFrequencyHertz, queryInfo.LowFrequencyHertz, queryInfo.TimeStart,
                queryInfo.TimeEnd, neighbourhoodLength,
                nhCountInRow, nhCountInColumn, spectrogramConfig);
            return result;
        }

        public static Query QueryRepresentationFromQueryInfo(FileInfo queryCsvFile,
            CompressSpectrogramConfig compressConfig)
        {
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            var result = new Query(queryInfo.HighFrequencyHertz, queryInfo.LowFrequencyHertz, queryInfo.TimeStart,
                queryInfo.TimeEnd, compressConfig);
            return result;
        }

        public static Query QueryRepresentationFromQueryInfo(FileInfo queryCsvFile,
            SpectrogramStandard spectrogram)
        {
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            var result = new Query(queryInfo.HighFrequencyHertz, queryInfo.LowFrequencyHertz, queryInfo.TimeStart,
                queryInfo.TimeEnd);
            var timeScale = spectrogram.FrameDuration - spectrogram.Configuration.GetFrameOffset();
            var freqScale = spectrogram.FBinWidth;

            result.BottomInPixel = (int)(result.minFrequency / freqScale);
            result.LeftInPixel = (int)(result.startTime / 1000 / timeScale);
            result.TopInPixel = (int)(result.maxFrequency / freqScale);
            result.RightInPixel = (int)(result.endTime / 1000 / timeScale);

            return result;
        }
        #endregion

    }
}
