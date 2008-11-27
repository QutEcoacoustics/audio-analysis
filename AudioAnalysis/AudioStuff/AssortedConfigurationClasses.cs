using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioStuff
{
	public class FftConfiguration
	{
		public FftConfiguration(Configuration config)
		{
			WindowFunction = config.GetString("WINDOW_FUNCTION");
			NPointSmoothFFT = config.GetIntNullable("N_POINT_SMOOTH_FFT") ?? 0;
		}

		#region Properties
		public string WindowFunction { get; set; }
		public int NPointSmoothFFT { get; set; } // Number of points to smooth FFT spectra
		#endregion
	}

	public class MfccConfiguration
	{
		#region Properties
		public int FilterbankCount { get; set; }
		public int MelBinCount { get; set; }    // Number of mel spectral values 
		public double MinMelPower { get; set; } // Min power in mel sonogram
		public double MaxMelPower { get; set; } // Max power in mel sonogram
		public double MaxMel { get; set; }      // Nyquist frequency on Mel scale
		#endregion
	}
}