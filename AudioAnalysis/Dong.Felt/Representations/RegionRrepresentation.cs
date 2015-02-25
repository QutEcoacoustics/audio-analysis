
namespace Dong.Felt.Representations
{
    using AudioAnalysisTools;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Dong.Felt.Features;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    using Acoustics.Shared;

    using AudioAnalysisTools.StandardSpectrograms;

    public class RegionRepresentation : RidgeDescriptionNeighbourhoodRepresentation
    {
        #region public properties.

        /// <summary>
        /// gets or sets the fft features for a region. This is based on Bardeli's algorithm.
        /// </summary>
        public PointOfInterest[,] fftFeatures { get; set; }
   
        /// <summary>
        /// Index (0-based) for this region's highest frequency in the source audio file, its unit is hz.
        /// </summary>
        public double MaxFrequencyIndex { get; set; }       

        /// <summary>
        /// Index (0-based) for the start time where this region starts located in the source audio file, its unit is ms.
        /// </summary>
        public double TimeIndex { get; set; }

        /// <summary>
        /// start row index for a region
        /// </summary>
        public int StartRowIndex { get; set; }

        public int EndRowIndex { get; set; }

        public int StartColIndex { get; set; }

        public int EndColIndex { get; set; }
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
        /// To get or set the the ColumnEnergyEntropy of pointsOfinterest in a neighbourhood.  
        /// </summary>
        public double ColumnEnergyEntropy { get; set; }

        /// <summary>
        /// To get or set the the RowEnergyEntropy of pointsOfinterest in a neighbourhood.  
        /// </summary>
        public double RowEnergyEntropy { get; set; }

        /// <summary>
        /// A region matrix contains NhCountInCol cols. 
        /// </summary>
        public int NhCountInCol { get; set; }

        public Feature Features { get; set; }

        /// <summary>
        /// Gets or sets the sourceAudioFile which contains the region.  
        /// </summary>
        public string SourceAudioFile { get; set; }

        public List<double> HistogramOfOrientatedGradient { get; set; }
        /// <summary>
        /// Gets or sets the count of points of interest (pois) with horizontal orentation in the neighbourhood.
        /// </summary>
        public double HOrientationPOIHistogram { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with positive diagonal orientation in the neighbourhood.
        /// </summary>
        public double PDOrientationPOIHistogram { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with vertical orientation in the neighbourhood.
        /// </summary>
        public double VOrientationPOIHistogram { get; set; }

        /// <summary>
        /// Gets or sets the count of points of interest (pois) with negative diagonal orientation in the neighbourhood.
        /// </summary>
        public double NDOrientationPOIHistogram { get; set; }


        public List<RidgeDescriptionNeighbourhoodRepresentation> ridgeNeighbourhoods { get; set; }

        /// <summary>
        /// In a query region, this is a distance between the region left  and the bottom left vetex in the event list.
        /// </summary>
        public int leftToBottomLeftVertex { get; set; }

        /// <summary>
        /// In a query region, this is a distance between the region right and the bottom left vetex in the event list.
        /// </summary>
        public int rightToBottomLeftVertex { get; set; }

        /// <summary>
        /// In a query region, this is a distance between the region top and the bottom left vetex in the event list.
        /// </summary>
        public int topToBottomLeftVertex { get; set; }

        /// <summary>
        /// In a query region, this is a distance between the region bottom and the bottom left vetex in the event list.
        /// </summary>
        public int bottomToBottomLeftVertex { get; set; }

        public int TopInPixel { get; set; }

        public int BottomInPixel { get; set; }

        public int LeftInPixel { get; set; }

        public int RightInPixel { get; set; }

        public EventBasedRepresentation MajorEvent { get; set; }

        public List<EventBasedRepresentation> vEventList { get; set; }

        public List<EventBasedRepresentation> hEventList { get; set; }

        public List<EventBasedRepresentation> pEventList { get; set; }

        public List<EventBasedRepresentation> nEventList { get; set; }

        public int NotNullEventListCount { get; set; }

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
        public RegionRepresentation()
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
        public RegionRepresentation(RidgeDescriptionNeighbourhoodRepresentation nh, double frequencyIndex, double frameIndex, 
            int nhCountInRow, int nhCountInCol, int rowIndex, int colIndex, string file)
        {
            this.MaxFrequencyIndex = frequencyIndex;
            this.TimeIndex = frameIndex;
            this.NhCountInRow = nhCountInRow;
            this.NhCountInCol = nhCountInCol;
            this.NhRowIndex = rowIndex;
            this.NhColIndex = colIndex;
            if (nh != null)
            {
                this.magnitude = nh.magnitude;
                this.orientation = nh.orientation;
                this.POICount = nh.POICount;
                this.FrameIndex = nh.FrameIndex;
                this.FrequencyIndex = nh.FrequencyIndex;
                this.FrequencyRange = nhCountInRow * nh.FrequencyRange;
                this.Duration = TimeSpan.FromMilliseconds(nh.Duration.TotalMilliseconds * nhCountInCol);
                this.neighbourhoodSize = nh.neighbourhoodSize;
                this.HistogramOfOrientatedGradient = nh.histogramOfGradient;
                this.HOrientationPOICount = nh.HOrientationPOICount;
                this.HOrientationPOIMagnitudeSum = nh.HOrientationPOIMagnitudeSum;
                this.VOrientationPOICount = nh.VOrientationPOICount;
                this.VOrientationPOIMagnitudeSum = nh.VOrientationPOIMagnitudeSum;
                this.PDOrientationPOICount = nh.PDOrientationPOICount;
                this.PDOrientationPOIMagnitudeSum = nh.PDOrientationPOIMagnitudeSum;
                this.NDOrientationPOICount = nh.NDOrientationPOICount;
                this.NDOrientationPOIMagnitudeSum = nh.NDOrientationPOIMagnitudeSum;
                this.LinearHOrientation = nh.LinearHOrientation;
                this.LinearVOrientation = nh.LinearVOrientation;
                this.HOrientationPOIMagnitude = nh.HOrientationPOIMagnitude;
                this.VOrientationPOIMagnitude = nh.VOrientationPOIMagnitude;
                this.HLineOfBestfitMeasure = nh.HLineOfBestfitMeasure;
                this.VLineOfBestfitMeasure = nh.VLineOfBestfitMeasure;
                this.ColumnEnergyEntropy = nh.ColumnEnergyEntropy;
                this.RowEnergyEntropy = nh.RowEnergyEntropy;
                this.HOrientationPOIHistogram = nh.HOrientationPOIHistogram;
                this.VOrientationPOIHistogram = nh.VOrientationPOIHistogram;
                this.PDOrientationPOIHistogram = nh.PDOrientationPOIHistogram;
                this.NDOrientationPOIHistogram = nh.PDOrientationPOIHistogram;
            }
            this.SourceAudioFile = file;
            
        }

        public RegionRepresentation(List<RidgeDescriptionNeighbourhoodRepresentation> ridgeNeighbourhoods,
            int nhCountInRow, int nhCountInCol, double frequencyIndex, double frameIndex,
            int rowIndex, int colIndex, string audioFile)
        {
            this.ridgeNeighbourhoods = new List<RidgeDescriptionNeighbourhoodRepresentation>();
            foreach (var nh in ridgeNeighbourhoods)
            {               
                this.ridgeNeighbourhoods.Add(nh);
            }
            // top left corner - this is the anchor point          
            this.NhCountInRow = nhCountInRow;
            this.NhCountInCol = nhCountInCol;
            this.MaxFrequencyIndex = frequencyIndex;
            this.TimeIndex = frameIndex;
            this.NhCountInRow = nhCountInRow;
            this.NhCountInCol = nhCountInCol;
            this.NhRowIndex = rowIndex;
            this.NhColIndex = colIndex;
            this.magnitude = ridgeNeighbourhoods[0].magnitude;
            this.orientation = ridgeNeighbourhoods[0].orientation;
            this.POICount = ridgeNeighbourhoods[0].POICount;
            this.FrameIndex = ridgeNeighbourhoods[0].FrameIndex;
            this.FrequencyIndex = ridgeNeighbourhoods[0].FrequencyIndex;
            this.FrequencyRange = nhCountInRow * ridgeNeighbourhoods[0].FrequencyRange;
            this.Duration = TimeSpan.FromMilliseconds(ridgeNeighbourhoods[0].Duration.TotalMilliseconds * nhCountInCol);
            this.neighbourhoodSize = ridgeNeighbourhoods[0].neighbourhoodSize;
            this.HOrientationPOICount = ridgeNeighbourhoods[0].HOrientationPOICount;
            this.HOrientationPOIMagnitudeSum = ridgeNeighbourhoods[0].HOrientationPOIMagnitudeSum;
            this.VOrientationPOICount = ridgeNeighbourhoods[0].VOrientationPOICount;
            this.VOrientationPOIMagnitudeSum = ridgeNeighbourhoods[0].VOrientationPOIMagnitudeSum;
            this.PDOrientationPOICount = ridgeNeighbourhoods[0].PDOrientationPOICount;
            this.PDOrientationPOIMagnitudeSum = ridgeNeighbourhoods[0].PDOrientationPOIMagnitudeSum;
            this.NDOrientationPOICount = ridgeNeighbourhoods[0].NDOrientationPOICount;
            this.NDOrientationPOIMagnitudeSum = ridgeNeighbourhoods[0].NDOrientationPOIMagnitudeSum;
            this.LinearHOrientation = ridgeNeighbourhoods[0].LinearHOrientation;
            this.LinearVOrientation = ridgeNeighbourhoods[0].LinearVOrientation;
            this.HOrientationPOIMagnitude = ridgeNeighbourhoods[0].HOrientationPOIMagnitude;
            this.VOrientationPOIMagnitude = ridgeNeighbourhoods[0].VOrientationPOIMagnitude;
            this.HLineOfBestfitMeasure = ridgeNeighbourhoods[0].HLineOfBestfitMeasure;
            this.VLineOfBestfitMeasure = ridgeNeighbourhoods[0].VLineOfBestfitMeasure;
            this.SourceAudioFile = audioFile;
        }

        public RegionRepresentation(List<EventBasedRepresentation> eventList, string file)
        {
            this.vEventList = eventList;            
            var allEventsInRegion = GroupEventBasedRepresentations(
               this.vEventList, this.hEventList, this.pEventList, this.nEventList);
            this.MajorEvent = FindLargestEvent(allEventsInRegion);
            this.SourceAudioFile = file;
        }

        public RegionRepresentation(List<List<EventBasedRepresentation>> eventList, string file, Query query)
        {           
            // step 1 select events from specific boundary
            this.vEventList = EventBasedRepresentation.ReadQueryAsAcousticEventList(eventList[0], query);
            this.hEventList = EventBasedRepresentation.ReadQueryAsAcousticEventList(eventList[1], query);
            this.pEventList = EventBasedRepresentation.ReadQueryAsAcousticEventList(eventList[2], query);
            this.nEventList = EventBasedRepresentation.ReadQueryAsAcousticEventList(eventList[3], query);

            var allEventsInRegion = GroupEventBasedRepresentations(
                this.vEventList, this.hEventList, this.pEventList, this.nEventList);
            // step 2 find the largest area of event in a specific region
            this.MajorEvent = FindLargestEvent(allEventsInRegion);

            // step 3 specify the boundary of the region 
            if (this.MajorEvent != null)
            {               
                this.topToBottomLeftVertex = query.TopInPixel - this.MajorEvent.Bottom;
                this.bottomToBottomLeftVertex = this.MajorEvent.Bottom - query.BottomInPixel;
                this.leftToBottomLeftVertex = this.MajorEvent.Left - query.LeftInPixel;
                this.rightToBottomLeftVertex = query.RightInPixel - this.MajorEvent.Left;
                this.TopInPixel = query.TopInPixel;
                this.BottomInPixel = query.BottomInPixel;
                this.LeftInPixel = query.LeftInPixel;
                this.RightInPixel = query.RightInPixel;
                this.NotNullEventListCount = NotNullListCount(
                    this.vEventList,
                    this.hEventList,
                    this.pEventList,
                    this.nEventList);
            }
            this.SourceAudioFile = file;
        }

        public RegionRepresentation(List<List<EventBasedRepresentation>> eventList, string file)
        {           
            this.vEventList = eventList[0];
            this.hEventList = eventList[1];
            this.pEventList = eventList[2];
            this.nEventList = eventList[3];

            var allEventsInRegion = GroupEventBasedRepresentations(
               this.vEventList, this.hEventList, this.pEventList, this.nEventList);            
            this.MajorEvent = FindLargestEvent(allEventsInRegion);
            this.SourceAudioFile = file;
        }
        
        public static List<EventBasedRepresentation> GroupEventBasedRepresentations(List<EventBasedRepresentation> vEvents, List<EventBasedRepresentation> hEvents,
            List<EventBasedRepresentation> pEvents, List<EventBasedRepresentation> nEvents)
        {
            var overallRegionRepresentation = new List<EventBasedRepresentation>();
            foreach (var v in vEvents)
            {
                overallRegionRepresentation.Add(v);
            }
            foreach (var h in hEvents)
            {
                overallRegionRepresentation.Add(h);
            }
            foreach (var p in pEvents)
            {
                overallRegionRepresentation.Add(p);
            }
            foreach (var n in nEvents)
            {
                overallRegionRepresentation.Add(n);
            }
            return overallRegionRepresentation;
        }

        public static EventBasedRepresentation FindLargestEvent(List<EventBasedRepresentation> events)
        {
            if (events.Count > 0)
            {
                events.Sort((ae1, ae2) => ae1.Area.CompareTo(ae2.Area));
                var majorEvent = events[events.Count - 1];
                return majorEvent;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This method aims to extract candidate region representation according to the provided
        /// marquee of the queryRepresentation.
        /// </summary>
        /// <param name="queryRepresentations"></param>
        /// <param name="candidateEventList"></param>
        /// <param name="centroidFreqOffset"> 
        /// </param>
        /// <returns></returns>
        public static List<RegionRepresentation> ExtractAcousticEventList(SpectrogramStandard spectrogram,
            RegionRepresentation queryRepresentations,
            List<List<EventBasedRepresentation>> candidateEventList, string file, int centroidFreqOffset)
        {
            var result = new List<RegionRepresentation>();

            var anchorCentroid = queryRepresentations.MajorEvent.Centroid;
            var orientationType = queryRepresentations.MajorEvent.InsideRidgeOrientation;
            var maxFreq = spectrogram.Configuration.FreqBinCount;
            var maxFrame = spectrogram.FrameCount;

            var potentialCandidatesStart = new List<EventBasedRepresentation>();
            foreach (var c in candidateEventList[orientationType])
            {
                if (Math.Abs(c.Centroid.Y - anchorCentroid.Y) < centroidFreqOffset)
                {
                    potentialCandidatesStart.Add(c);
                }
            }           
            foreach (var pc in potentialCandidatesStart)
            {
                var maxFreqPixelIndex = queryRepresentations.topToBottomLeftVertex + pc.Bottom;
                var minFreqPixelIndex = pc.Bottom - queryRepresentations.bottomToBottomLeftVertex;
                var startTimePixelIndex = pc.Left - queryRepresentations.leftToBottomLeftVertex;
                var endTimePixelIndex = queryRepresentations.rightToBottomLeftVertex + pc.Left;

                if (StatisticalAnalysis.checkBoundary(minFreqPixelIndex, startTimePixelIndex, maxFreq, maxFrame)
                    && StatisticalAnalysis.checkBoundary(maxFreqPixelIndex, endTimePixelIndex, maxFreq, maxFrame))
                {
                    var allEvents = EventBasedRepresentation.AddSelectedEventLists(candidateEventList, minFreqPixelIndex, maxFreqPixelIndex, startTimePixelIndex, 
                    endTimePixelIndex, maxFreq, maxFrame);
                    if (allEvents[orientationType].Count > 0)
                    {
                        var candidateRegionRepre = new RegionRepresentation(allEvents, file);
                        candidateRegionRepre.MajorEvent = pc;
                        candidateRegionRepre.topToBottomLeftVertex = maxFreqPixelIndex - pc.Bottom;
                        candidateRegionRepre.bottomToBottomLeftVertex = pc.Bottom - minFreqPixelIndex;
                        candidateRegionRepre.leftToBottomLeftVertex = pc.Left - startTimePixelIndex;
                        candidateRegionRepre.rightToBottomLeftVertex = endTimePixelIndex - pc.Left;
                        candidateRegionRepre.TopInPixel = maxFreqPixelIndex;
                        candidateRegionRepre.BottomInPixel = minFreqPixelIndex;
                        candidateRegionRepre.LeftInPixel = startTimePixelIndex;
                        candidateRegionRepre.RightInPixel = endTimePixelIndex;
                        candidateRegionRepre.NotNullEventListCount = NotNullListCount(
                        candidateRegionRepre.vEventList,
                        candidateRegionRepre.hEventList,
                        candidateRegionRepre.pEventList,
                        candidateRegionRepre.nEventList);
                        result.Add(candidateRegionRepre);
                    }                  
                }                
            }
            return result;
        }
    
        /// <summary>
        /// This representation is derived on eventRepresentations. 
        /// </summary>
        /// <param name="eventRepresentations"></param>
        /// <param name="file"></param>
        /// <param name="query"></param>
        public RegionRepresentation(List<EventBasedRepresentation> eventRepresentations, string file, Query query)
        {            
            var queryEventList = EventBasedRepresentation.ReadQueryAsAcousticEventList(
                    eventRepresentations,
                    query);

            this.vEventList = new List<EventBasedRepresentation>();
            foreach (var e in queryEventList)
            {
                this.vEventList.Add(e);
            }
            if (queryEventList.Count > 0)
            {
                //queryEventList.Sort((ae1, ae2) => ae1.TimeStart.CompareTo(ae2.TimeStart));               
                //this.bottomLeftEvent = queryEventList[0]; 
                queryEventList.Sort((ae1, ae2) => ae1.Area.CompareTo(ae2.Area));
                this.MajorEvent = queryEventList[queryEventList.Count - 1];
                // get the distance difference between four sides and vertex of the bottomLeftEvent: left, bottom, right, top
                this.topToBottomLeftVertex = query.TopInPixel - this.MajorEvent.Bottom;
                this.bottomToBottomLeftVertex = this.MajorEvent.Bottom - query.BottomInPixel;
                this.leftToBottomLeftVertex = this.MajorEvent.Left - query.LeftInPixel;
                this.rightToBottomLeftVertex = query.RightInPixel - this.MajorEvent.Left;               
                this.TopInPixel = query.TopInPixel;
                this.BottomInPixel = query.BottomInPixel;
                this.LeftInPixel = query.LeftInPixel;
                this.RightInPixel = query.RightInPixel; 
            }           
            this.SourceAudioFile = file;
        }    
  
        public static int NotNullListCount(List<EventBasedRepresentation> vEvents, List<EventBasedRepresentation> hEvents,
            List<EventBasedRepresentation> pEvents, List<EventBasedRepresentation> nEvents)
        {
            var count = 0;
            if (vEvents.Count > 0)
            {
                count++;
            }
            if (hEvents.Count > 0)
            {
                count++;
            }
            if (pEvents.Count > 0)
            {
                count++;
            }
            if (nEvents.Count > 0)
            {
                count++;
            }
            return count;
        }

        #endregion
    }
}
