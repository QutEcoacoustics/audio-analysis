using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioStuff
{
	public class MMTemplate : TemplateParameters
	{
		#region Properties
		public FeatureSonogramConfig SonogramConfiguration { get; set; }

		
		public double FrameOffset { get; set; }    // Duration of non-overlapped part of window/frame in seconds
		public int FrameCount { get; set; }        // Number of frames
		public double FramesPerSecond { get; set; }

		// MFCC parameters
		public bool DoMelScale { get; set; }
		public bool DoNoiseReduction { get; set; }
		public int ccCount { get; set; }     //number of cepstral coefficients
		public double MinCepPower { get; set; } //min value in cepstral sonogram
		public double MaxCepPower { get; set; } //max value in cepstral sonogram
		public int DeltaT { get; set; }
		public bool IncludeDelta { get; set; }
		public bool IncludeDoubleDelta { get; set; }

		//FEATURE VECTOR PARAMETERS 
		public FV_Source FeatureVectorSource { get; set; }
		public string[] FeatureVector_SelectedFrames { get; set; } //store frame IDs as string array
		public int MarqueeStart { get; set; }
		public int MarqueeEnd { get; set; }
		public FV_Extraction FeatureVectorExtraction { get; set; }
		public int FeatureVectorExtractionInterval { get; set; }
		public bool FeatureVector_DoAveraging { get; set; }
		public string FeatureVector_DefaultNoiseFile { get; set; }

		public int FeatureVectorCount { get; set; }
		public int FeatureVectorLength { get; set; }
		public string[] FeatureVectorPaths { get; set; }
		public string[] FVSourceFiles { get; set; }
		public string DefaultNoiseFVFile { get; set; }
		public int ZscoreSmoothingWindow = 3; //NB!!!! THIS IS NO LONGER A USER DETERMINED PARAMETER

		//THE LANGUAGE MODEL
		public int WordCount { get; set; }
		public string[] Words { get; set; }
		public MarkovModel WordModel { get; set; }
		public HMMType HmmType { get; set; }
		public string HmmName { get; set; }
		public double SongWindow { get; set; } //window duration in seconds - used to calculate statistics
		#endregion
	}
}