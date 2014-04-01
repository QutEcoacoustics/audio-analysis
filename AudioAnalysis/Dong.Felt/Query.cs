namespace Dong.Felt
{
    using AudioAnalysisTools.Sonogram;
    using Dong.Felt.Configuration;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using TowseyLib;

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
        /// gets or sets the duration by endTime substracting startTime, its unit is millisecond. 
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

        /// <summary>
        /// Gets or sets the neighbourhood start row index, it lies in the bottom left corner of the region.
        /// </summary>
        public int nhStartRowIndex { get; set; }

        /// <summary>
        /// Gets or sets the neighbourhood start column index, it lies in the bottom left corner of the region.
        /// </summary>
        public int nhStartColIndex { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// default constructor.
        /// </summary>
        public Query()
        {
            maxFrequency = 0.0;
            minFrequency = 0.0;
            startTime = 0.0;
            endTime = 0.0;
            duration = endTime - startTime; 
        }

        /// <summary>
        /// set the properties through parsing the parameters. 
        /// </summary>
        /// <param name="maximumFrequency"></param>
        /// <param name="minimumFrequency"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        public Query(double maximumFrequency, double minimumFrequency, double starttime, double endtime, int neighbourhoodLength, int maxFrequencyIndex, SpectrogramConfiguration spectrogramConfig)
        {
            var secondToMillisecond = 1000;
            maxFrequency = maximumFrequency;
            minFrequency = minimumFrequency;
            startTime = starttime * secondToMillisecond; // millisecond
            endTime = endtime * secondToMillisecond; // millisecond
            duration = endTime - startTime; 
            frequencyRange = maximumFrequency - minimumFrequency;
            GetNhProperties(neighbourhoodLength, maxFrequencyIndex, spectrogramConfig);
        }

        // to get the nhCountInRow, nhCountInColumn, nhStartRowIndex, nhStartColIndex.
        public void GetNhProperties(int neighbourhoodLength, int maxFrequencyIndex, SpectrogramConfiguration spectrogramConfig)
        {
            var frequencyScale = spectrogramConfig.FrequencyScale;
            var timeScale = spectrogramConfig.TimeScale; // millisecond
            var nhFrequencyLength = neighbourhoodLength * frequencyScale;
            var nhFrameLength = neighbourhoodLength * timeScale;
            // get a greater value than the parameter for enlarging the later used NH boundary- enlarge.
            var enlargedOffset = 0;
            // ceiling is try to increase the value.
            var nhCountInRow = (int)Math.Ceiling(frequencyRange / nhFrequencyLength) + enlargedOffset;
            var nhCountInCol = (int)Math.Ceiling(this.duration / nhFrameLength) + enlargedOffset;
            /// Here is a trick. Trying to get the nearest and lowest NH frame and frequencyIndex.
            /// Try to enlarge the original region, the startColIndex and startRowIndex should plus or minus one. 
            this.nhStartColIndex = (int)Math.Floor(this.startTime / nhFrameLength) - enlargedOffset;
            this.nhStartRowIndex = maxFrequencyIndex - (int)Math.Floor(this.maxFrequency / nhFrequencyLength) - enlargedOffset;
            var nhendTime = (this.nhStartColIndex + nhCountInCol) * nhFrameLength;
            if (nhendTime < this.endTime)
            {
                nhCountInCol++;
            }
            var nhlowFrequency = (this.nhStartRowIndex + nhCountInRow) * nhFrequencyLength;
            if (nhlowFrequency > this.minFrequency)
            {
                nhCountInRow++;
            }
            this.nhCountInRow = nhCountInRow;
            this.nhCountInColumn = nhCountInCol;
            
        }
        
        public static Query QueryRepresentationFromQueryInfo(FileInfo queryCsvFile, int neighbourhoodLength, SpectralSonogram spectrogram, SpectrogramConfiguration spectrogramConfig)
        {
            var queryInfo = CSVResults.CsvToAcousticEvent(queryCsvFile);
            var nhFrequencyRange = neighbourhoodLength * spectrogram.FBinWidth;
            var nhCountInRow = (int)(spectrogram.NyquistFrequency / nhFrequencyRange);
            if (spectrogram.NyquistFrequency % nhFrequencyRange == 0)
            {
                nhCountInRow--;
            }
            var nhCountInColumn = (int)(spectrogram.FrameCount / neighbourhoodLength);
            if (spectrogram.FrameCount % neighbourhoodLength == 0)
            {
                nhCountInColumn--;
            }
            var result = new Query(queryInfo.MaxFreq, queryInfo.MinFreq, queryInfo.TimeStart, queryInfo.TimeEnd, neighbourhoodLength, nhCountInRow, spectrogramConfig);
            return result;
        }
        #endregion

    }
}
