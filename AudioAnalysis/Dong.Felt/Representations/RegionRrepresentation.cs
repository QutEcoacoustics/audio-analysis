
namespace Dong.Felt.Representations
{
    using AudioAnalysisTools;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class RegionRerepresentation : RidgeDescriptionNeighbourhoodRepresentation
    {
        #region public properties.
        
        /// <summary>
        /// Index (0-based) for this region's highest frequency in the source audio file, its unit is hz.
        /// </summary>
        public double MaxFrequencyIndex { get; set; }

        /// <summary>
        /// Index (0-based) for the start time where this region starts located in the source audio file, its unit is ms.
        /// </summary>
        public double TimeIndex { get; set; }

        /// <summary>
        /// For each nh in a region, NhRowIndex will indicate its row index. 
        /// </summary>
        public int NhRowIndex { get; set; }

        /// <summary>
        /// For each nh in a region, NhColIndex will indicate its col index. 
        /// </summary>
        public int NhColIndex { get; set; }

        /// <summary>
        /// A region matrix contains NhCountInRow rows. 
        /// </summary>
        public int NhCountInRow { get; set; }

        /// <summary>
        /// A region matrix contains NhCountInCol cols. 
        /// </summary>
        public int NhCountInCol { get; set; }

        /// <summary>
        /// Gets or sets the sourceAudioFile which contains the region.  
        /// </summary>
        public string SourceAudioFile { get; set; }

        //public List<RidgeDescriptionNeighbourhoodRepresentation> ridgeNeighbourhoods {get; set;}

        //public ICollection<RidgeDescriptionNeighbourhoodRepresentation> ridgeNeighbourhood
        //{
        //    get
        //    {
        //        // create a new list , so that only this class can change the list of neighbourhoods.
        //        return new List<RidgeDescriptionNeighbourhoodRepresentation>(this.ridgeNeighbourhoods);
        //    }
        //}
        #endregion

        #region  public constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RegionRerepresentation()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nh"></param>
        /// <param name="frequencyIndex"></param>
        /// <param name="frameIndex"></param>
        /// <param name="nhCountInRow"></param>
        /// <param name="nhCountInCol"></param>
        /// <param name="rowIndex"></param>
        /// <param name="colIndex"></param>
        public RegionRerepresentation(RidgeDescriptionNeighbourhoodRepresentation nh, double frequencyIndex, double frameIndex, int nhCountInRow, int nhCountInCol, int rowIndex, int colIndex, string file)
        {
            this.MaxFrequencyIndex = frequencyIndex;
            this.TimeIndex = frameIndex;
            this.NhCountInRow = nhCountInRow;
            this.NhCountInCol = nhCountInCol;
            this.NhRowIndex = rowIndex;
            this.NhColIndex = colIndex;
            this.magnitude = nh.magnitude;
            this.orientation = nh.orientation;
            this.POICount = nh.POICount;
            this.FrameIndex = nh.FrameIndex;
            this.FrequencyIndex = nh.FrequencyIndex;
            this.FrequencyRange = nhCountInRow * nh.FrequencyRange;
            this.Duration = TimeSpan.FromMilliseconds(nh.Duration.TotalMilliseconds * nhCountInCol);
            this.neighbourhoodSize = nh.neighbourhoodSize;
            this.HOrientationPOICount = nh.HOrientationPOICount;
            this.HOrientationPOIMagnitudeSum = nh.HOrientationPOIMagnitudeSum;
            this.VOrientationPOICount = nh.VOrientationPOICount;
            this.VOrientationPOIMagnitudeSum = nh.VOrientationPOIMagnitudeSum;
            this.PDOrientationPOICount = nh.PDOrientationPOICount;
            this.PDOrientationPOIMagnitudeSum = nh.PDOrientationPOIMagnitudeSum;
            this.NDOrientationPOICount = nh.NDOrientationPOICount;
            this.NDOrientationPOIMagnitudeSum = nh.NDOrientationPOIMagnitudeSum;
            this.SourceAudioFile = file;
        }

        //public RegionRerepresentation(List<RidgeDescriptionNeighbourhoodRepresentation> ridgeNeighbourhoods,
        //    int nhCountInRow, int nhCountInCol, FileInfo audioFile)
        //{
        //    this.ridgeNeighbourhoods = new List<RidgeDescriptionNeighbourhoodRepresentation>();
        //    foreach (var nh in ridgeNeighbourhoods)
        //    {
        //        this.ridgeNeighbourhoods.Add(nh);
        //    }
        //    // top left corner - this is the anchor point
        //    this.FrequencyIndex = ridgeNeighbourhoods[0].FrequencyIndex;
        //    this.TimeIndex = ridgeNeighbourhoods[0].FrameIndex;
        //    this.NhCountInRow = nhCountInRow;
        //    this.NhCountInCol = nhCountInCol;

        //    this.SourceAudioFile = audioFile;
        //}

        
        #endregion
    }
}
