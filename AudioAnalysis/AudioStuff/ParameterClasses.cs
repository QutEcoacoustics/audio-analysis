namespace AudioStuff
{
	public class CallDescriptor
	{
		public string callName = "NO NAME";
		public string callComment = "DEFAULT COMMENT";
		public string destinationFileDescriptor = "Descriptor"; //should be short ie < 10 chars
		public string sourcePath = "NO_PATH";
		public string sourceFile = "NO_NAME";
	}

	public class MfccParameters
	{
		public int frameSize = 512;
		public double frameOverlap = 0.5;
		public int filterBankCount = 64;
		public bool doMelConversion = true;
		public bool doNoiseReduction = false;
		public int ceptralCoeffCount = 12;
		public bool includeDeltaFeatures = true;
		public bool includeDoubleDeltaFeatures = true;
		public int deltaT = 2; // i.e. + and - two frames gap when constructing feature vector
	}

	class FeatureVectorParameters
	{
		public FV_Source fv_Source = FV_Source.SELECTED_FRAMES;  //FV_Source.MARQUEE;
		public string selectedFrames = "0";
		public int min_Freq = 0; //Hz
		public int max_Freq = 9999; //Hz
		public int marqueeStart = 999;
		public int marqueeEnd = 999;
	}

	public class FVExtractionParameters
	{
		public FV_Extraction fv_Extraction = FV_Extraction.AT_ENERGY_PEAKS;  //AT_FIXED_INTERVALS
		public int fvExtractionInterval = 999; //milliseconds
		public bool doFvAveraging = false;
		public string fvDefaultNoiseFile = @"C:\SensorNetworks\Templates\template_2_DefaultNoise.txt";
		public double zScoreThreshold = 1.98; //options are 1.98, 2.33, 2.56, 3.1
	}

	public class LanguageModel
	{
		public int numberOfWords = 0; //number of defined song variations 
		public string[] words = { "999" };
		public int maxSyllables = 1;  //NOT YET USED
		public double maxSyllableGap = 0.25; //seconds  NOT YET USED
		public double SongWindow = 1.000; //seconds USED TO CALCULATE SONG POISSON STATISTICS
		public int callPeriodicity = 999;
	}
}