using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysis
{
	[Serializable]
	public class BaseResult
	{
        public double[] Scores { get; set; }		// array of scores derived from arbitrary source
	}
}