﻿using System;
using TowseyLib;

namespace AudioStuff
{
	public abstract class BaseClassifierParameters
	{
	}

	public class TemplateParameters : BaseClassifierParameters
	{
		public TemplateParameters(Configuration config)
		{
			SourcePath = config.GetString("WAV_FILE_PATH");
			var duration = config.GetDoubleNullable("WAV_DURATION");
			if (duration != null)
				SourceDuration = TimeSpan.FromSeconds(duration.Value);
			SampleRate = config.GetInt("WAV_SAMPLE_RATE");
		}

		#region Properties
		public string SourcePath { get; set; } // Path to original audio recording used to generate the template
		public TimeSpan SourceDuration { get; set; }

		public int SampleRate { get; set; } // Sample rate of the original source
		#endregion
	}
}