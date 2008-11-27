using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;

namespace AudioStuff
{
	public class AudioRecording
	{
		#region Properties
		public string FileName { get; set; }
		#endregion

		public WavReader GetWavData()
		{
			return new WavReader(FileName);
		}
	}
}