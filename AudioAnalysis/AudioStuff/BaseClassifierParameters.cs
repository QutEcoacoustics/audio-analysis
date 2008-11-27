using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioStuff
{
	public abstract class BaseClassifierParameters
	{
	}

	public class TemplateParameters : BaseClassifierParameters
	{
		#region Properties
		public string SourcePath { get; set; } // Path to original audio recording used to generate the template
		public TimeSpan SourceDuration { get; set; }

		public int SampleRate { get; set; } // Sample rate of the original source
		#endregion
	}
}