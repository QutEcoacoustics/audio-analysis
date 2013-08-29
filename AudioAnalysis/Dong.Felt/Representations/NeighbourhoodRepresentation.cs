namespace Dong.Felt
{
    using System;

    public abstract class NeighbourhoodRepresentation
    {
        // all neighbourhoods for one representation must be the same dimensions
        // row starts from start of file (left, 0ms)
        // column starts from bottom of spectrogram (0 hz)

        // gets or sets the rowIndex of a neighbourhood, which indicates the frequency band. 
        public int RowIndex { get; set; }

        // gets or sets the colIndex of a neighbourhood, which indicates the frame. 
        public int ColIndex { get; set; }

        // gets or sets the widthPx of a neighbourhood in pixels. 
        public int WidthPx { get; set; }

        // gets or sets the HeightPx of a neighbourhood in pixels.
        public int HeightPx { get; set; }

        // gets or sets the Duration of a neighbourhood in millisecond.
        public TimeSpan Duration { get; set; }

        // gets or sets the FrequencyRange of a neighbourhood in hZ.
        public double FrequencyRange { get; set; }

        public bool IsSquare { get { return this.WidthPx == this.HeightPx; } }

        //public TimeSpan TimeOffsetFromStart { get { return TimeSpan.FromMilliseconds(this.ColIndex * this.Duration.TotalMilliseconds); } }

        //public double FrequencyOffsetFromBottom { get { return this.RowIndex * this.FrequencyRange; } }
    }
}
