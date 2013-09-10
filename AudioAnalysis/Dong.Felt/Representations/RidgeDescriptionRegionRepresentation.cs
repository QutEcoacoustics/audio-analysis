
namespace Dong.Felt.Representations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    //public class RidgeDescriptionRegionRepresentation : RegionRerepresentation
    //{       
    //    public int nhRowCountInRegion {get; private set;}
        
    //    public int nhColumnCountInRegion {get; private set;}

    //    public void SetRegionProperties(int neighbourhoodLength)
    //    {
    //        GetNhProperties(neighbourhoodLength);
    //    }

        //public void GetNhProperties(int neighbourhoodLength)
        //{
        //    var frequencyRange = this.maxFrequency - this.minFrequency;
        //    var frequencyScale = 43.0;
        //    var timeScale = 11.6; // millisecond
        //    var nhRowsCount = (int)(frequencyRange / (neighbourhoodLength * frequencyScale)) + 1;
        //    if (this.maxFrequency > nhRowsCount * neighbourhoodLength * frequencyScale)
        //    {
        //        nhRowsCount++;
        //    }
        //    var nhColsCount = (int)(this.duration / (neighbourhoodLength * timeScale)) + 1;
        //    this.AudioTimeIndex = (int)(this.startTime / (neighbourhoodLength * timeScale));

        //    var nhendTime = (this.AudioTimeIndex + nhColsCount) * neighbourhoodLength * timeScale;
        //    if (nhendTime < this.endTime)
        //    {
        //        nhColsCount++;
        //    }
        //    this.nhRowCountInRegion = nhRowsCount;
        //    this.nhColumnCountInRegion = nhColsCount;
        //    this.AudioFrequencyIndex = (int)(this.minFrequency / (neighbourhoodLength * frequencyScale));
    //    //}

    //}
}
