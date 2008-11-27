using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.Text.RegularExpressions;

namespace AudioStuff
{
	/// <summary>
	/// Represents a single audio recording including its metadata and audio data.
	/// </summary>
	public class AudioRecording
	{
		#region Statics
		public static AudioRecording Load(string fileName)
		{
			var retVal = new AudioRecording() { FileName = fileName };
			retVal.ParseFileName();
			return retVal;
		}
		#endregion

		#region Properties
		public string FileName { get; private set; }
		public string DeploymentName { get; private set; }
		public DateTime? Time { get; private set; }

		public int TimeSlot
		{
			get
			{
				if (Time == null)
					return 0;
				//############ WARNING!!! THE FOLLOWING LINE MUST BE CONSISTENT WITH TIMESLOT CONSTANT
				return ((Time.Value.Hour * 60) + Time.Value.Minute) / 30; //convert to half hour time slots
			}
		}
		#endregion

		public WavReader GetWavData()
		{
			return new WavReader(FileName);
		}

		void ParseFileName()
		{
			var m = Regex.Match(FileName, @"(.*?)_(.*)");
			if (m.Success)
			{
				DeploymentName = m.Groups[1].Value;
				DateTime time;
				if (DateTime.TryParseExact(m.Groups[2].Value, "yyyyMMdd-HHmmss", System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out time))
					Time = time;
			}
		}
	}
}