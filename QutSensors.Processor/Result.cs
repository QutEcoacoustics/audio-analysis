using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutSensors.Data.Linq;

namespace QutSensors.Processor
{
	public class Result
	{
		internal Result(Processor_Results result)
		{
			JobItemID = result.JobItemID;
			PeriodicHits = result.PeriodicHits;
			BestHitScore = result.BestHitScore;
			BestHitLocation = TimeSpan.FromMilliseconds(result.BestHitLocation);
		}

		public int JobItemID { get; private set; }
		public int PeriodicHits { get; private set; }
		public double BestHitScore { get; private set; }
		public TimeSpan BestHitLocation { get; private set; }
	}
}