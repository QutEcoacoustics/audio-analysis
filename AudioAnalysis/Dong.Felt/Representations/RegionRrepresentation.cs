
namespace Dong.Felt.Representations
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class RegionRerepresentation
    {
        /// <summary>
        /// Index (0-based) for this region's lowest frequency in the source audio file.
        /// </summary>
        //public int AudioFrequencyIndex { get; private set; }
        public double AudioFrequencyIndex { get; set; }

        /// <summary>
        /// Index (0-based) for the time where this region starts located in the source audio file.
        /// </summary>
        //public int AudioTimeIndex { get; private set; }
        public double AudioTimeIndex { get; set; }

        public List<double> score { get; set; }

        public int regionCountInRow { get; set; }

        public int regionCountInCol { get; set; }


        // frequency range, total duration
        public double maxFrequency { get; private set; }

        public double minFrequency { get; private set; }

        public double startTime { get; private set; }

        public double endTime { get; private set; }

        public double frequencyRange { get; private set; }

        public double duration { get; private set; }

        public FileInfo SourceAudioFile { get; private set; }

        public FileInfo SourceTextFile { get; private set; }

        private List<NeighbourhoodRepresentation> neighbourhoods;
       
        public ICollection<NeighbourhoodRepresentation> Neighbourhoods
        {
            get
            {
                // create a new list , so that only this class can change the list of neighbourhoods.
                return new List<NeighbourhoodRepresentation>(this.neighbourhoods);
            }
        }

        public ICollection<RidgeDescriptionNeighbourhoodRepresentation> RidgeNeighbourhoods { get; set; }

        public RegionRerepresentation(int audioFrequencyIndex, int audioTimeIndex)
        {
            this.AudioFrequencyIndex = audioFrequencyIndex;
            this.AudioTimeIndex = audioTimeIndex;
        }

        public RegionRerepresentation(int audioFrequencyIndex, int audioTimeIndex,
            FileInfo sourceAudioFile, FileInfo sourceTextFile)
        {
            this.AudioFrequencyIndex = audioFrequencyIndex;
            this.AudioTimeIndex = audioTimeIndex;
            this.SourceAudioFile = sourceAudioFile;
            this.SourceTextFile = sourceTextFile;
            this.neighbourhoods = new List<NeighbourhoodRepresentation>();

        }

        public void AddNeighbourhood(NeighbourhoodRepresentation neighbourhood)
        {
            // do other things. e.g. check the bounds, count the PoIs, save an image of it...

            this.neighbourhoods.Add(neighbourhood);
        }

        

    }
}
