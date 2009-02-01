using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using System.IO;

namespace AudioStuff
{
	[Serializable]
	public class MMResult : BaseResult
	{
		public MMResult(MMTemplate template)
		{
			Template = template;
		}

		#region Properties
		public string ID { get; set; }
		public MMTemplate Template { get; private set; }

		public double[,] AcousticMatrix { get; set; }	// matrix of fv x time frames
		public string SyllSymbols { get; set; }			// array of symbols  representing winning user defined feature templates
		public int[] SyllableIDs { get; set; }			// array of integers representing winning user defined feature templates
		public double[] VocalScores { get; set; }		// array of scores for user defined call templates
		public int? VocalCount { get; set; }			// number of hits whose score exceeds some threshold
		public double? VocalBest { get; set; }			// the best score in recording, and .....
		public double? VocalBestLocation { get; set; }	// its location in seconds from beginning of recording

		public int? CallPeriodicity_frames { get; set; }
		public int? CallPeriodicity_ms { get; set; }
		public int? NumberOfPeriodicHits { get; set; }
		#endregion

		#region Symbol Sequence Formatting
		public void SaveSymbolSequences(string path, bool includeUserDefinedVocabulary)
		{
			Validation.Begin()
						.IsStateNotNull(SyllSymbols, "SyllSymbols has not been provided. Ensure you have generated the symbol sequence.")
						.IsStateNotNull(SyllableIDs, "SyllableIDs has not been provided. Ensure you have generated the symbol sequence.")
						.IsNotNull(path, "pathName")
						.Check();

			using (TextWriter writer = new StreamWriter(path))
			{
				writer.Write("\n==================================RESULTS TRACK==============================================================\n\n");
				writer.Write(FormatSymbolSequence());
				if (includeUserDefinedVocabulary)
				{
					//writer.Write(DisplayUserDefinedVocabulary(i));
				}
			}
		}

		public string FormatSymbolSequence()
		{
			Validation.Begin()
						.IsStateNotNull(SyllSymbols, "SyllSymbols has not been provided. Ensure you have generated the symbol sequence.")
						.IsStateNotNull(SyllableIDs, "SyllableIDs has not been provided. Ensure you have generated the symbol sequence.")
						.Check();

			StringBuilder sb = new StringBuilder();

			// display the symbol sequence, one second per line
			sb.Append("\n################## THE SYMBOL SEQUENCE DERIVED FROM TEMPLATE ");// + templateID);
			sb.Append("\n################## Number of user defined symbols/feature vectors =" + Template.FeatureVectorParameters.FeatureVectorCount);
			sb.Append("\n################## n=noise.   x=garbage i.e. frame has unrecognised acoustic energy.\n");
			sb.Append(FormatSymbolSequence(SyllSymbols));

			//display N-grams
			int N = 2;
			var _2grams = ExtractNgramSequences(SyllSymbols, N);
			var ht2 = DataTools.WordsHisto(_2grams);
			sb.Append("\n################# Number of 2grams=" + _2grams.Count + ".  Distinct=" + ht2.Count + ".\n\t# 2gram (count,RF)\n");
			int count = 0;
			foreach (string str in ht2.Keys)
			{
				double rf = ht2[str] / (double)_2grams.Count;
				sb.Append("\t" + ((++count).ToString("D2")) + " " + str + " (" + ((int)ht2[str]).ToString("D2") + "," + rf.ToString("F3") + ")\n");
			}

			N = 3;
			var _3grams = ExtractNgramSequences(SyllSymbols, N);
			var ht3 = DataTools.WordsHisto(_3grams);
			sb.Append("\n################# Number of 3grams=" + _3grams.Count + ".  Distinct=" + ht3.Count + ".\n\t# 3gram (count,RF)\n");

			count = 0;
			foreach (string str in ht3.Keys)
				sb.Append("\t" + ((++count).ToString("D2")) + " " + str + " (" + ht3[str] + ")\n");

			//display the sequences of valid syllables
			var list = ExtractWordSequences(SyllSymbols);
			var ht = DataTools.WordsHisto(list);
			sb.Append("\n################# Number of Words = " + list.Count + "  Number of Distinct Words = " + ht.Count + "\n");

			count = 0;
			foreach (string str in ht.Keys)
				sb.Append((++count).ToString("D2") + "  " + str + " \t(" + ht[str] + ")\n");

			int maxGap = 80;
			double durationMS = Template.LanguageModel.FrameOffset * 1000;
			sb.Append("\n################# Distribution of Gaps between Detected : (Max gap=" + maxGap + " frames)\n");
			sb.Append("                   Duration of each frame = " + durationMS.ToString("F1") + " ms\n");
			int[] gaps = CalculateGaps(SyllSymbols, maxGap); //lengths of 'n' and 'x' - noise and garbage
			for (int i = 0; i < maxGap; i++) if (gaps[i] > 0)
					sb.Append("Frame Gap=" + i + " count=" + gaps[i] + " (" + (i * durationMS).ToString("F1") + "ms)\n");
			sb.Append("\n");

			return sb.ToString();
		} // end FormatSymbolSequence()

		string FormatSymbolSequence(string sequence)
		{
			StringBuilder sb = new StringBuilder("sec\tSEQUENCE\n");
			int L = sequence.Length;
			int symbolRate = (int)Math.Round(Template.LanguageModel.FramesPerSecond);
			int secCount = L / symbolRate;
			int tail = L % symbolRate;
			for (int i = 0; i < secCount; i++)
			{
				int start = i * symbolRate;
				sb.Append(i.ToString("D3") + "\t" + sequence.Substring(start, symbolRate) + "\n");
			}
			sb.Append(secCount.ToString("D3") + "\t" + sequence.Substring(secCount * symbolRate) + "\n");
			return sb.ToString();
		}

		List<string> ExtractNgramSequences(string sequence, int N)
		{
			var list = new List<string>();
			int L = sequence.Length;

			for (int i = 0; i < L - N; i++)
			{
				if (IsSyllable(sequence[i]) && IsSyllable(sequence[i + N - 1]))
					list.Add(sequence.Substring(i, N));
			}

			return list;
		}

		List<string> ExtractWordSequences(string sequence)
		{
			var list = new List<string>();
			bool inWord = false;
			int L = sequence.Length;
			int wordStart = 0;
			int buffer = 3;

			for (int i = 0; i < L - buffer; i++)
			{
				bool endWord = true;
				char c = sequence[i];
				if (IsSyllable(c))
				{
					if (!inWord)
						wordStart = i;
					inWord = true;
					endWord = false;
				}
				else if (ContainsSyllable(sequence.Substring(i, buffer)))
					endWord = false;

				if ((inWord) && (endWord))
				{
					list.Add(sequence.Substring(wordStart, i - wordStart));
					inWord = false;
				}
			}//end loop over sequence 

			return list;
		}

		int[] CalculateGaps(string sequence, int maxGap)
		{
			int[] gaps = new int[maxGap];
			bool inGap = false;
			int L = sequence.Length;
			int gapStart = 0;

			for (int i = 0; i < L; i++)
			{
				bool endGap = true;
				char c = sequence[i];
				if (!IsSyllable(c)) //ie is noise or garbage frame
				{
					if (!inGap) gapStart = i;
					inGap = true;
					endGap = false;
				}

				if ((inGap) && (endGap))
				{
					int gap = i - gapStart;
					if (gap >= maxGap) gaps[maxGap - 1]++; else gaps[gap]++;
					inGap = false;
				}
			}
			return gaps;
		} //end of CalculateGaps()

		bool IsSyllable(char c)
		{
			return (c != 'n') && (c != 'x');
		}

		bool ContainsSyllable(string str)
		{
			// NOTE: from Richard - this doesn't seem correct, but it's what was written.
			return !string.IsNullOrEmpty(str) && IsSyllable(str[0]);
		}
		#endregion

		public Track GetSyllablesTrack()
		{
			var track = new Track(TrackType.syllables, SyllableIDs);
			track.GarbageID = Template.FeatureVectorParameters.FeatureVectorCount + 2 - 1;
			return track;
		}

		#region Comma Separated Summary Methods
		public static string GetSummaryHeader()
		{
			return "ID,Hits,MaxScr,MaxLoc";
		}

		public string GetOneLineSummary()
		{
			return string.Format("{0},{1},{2:F1},{3:F1}", ID, NumberOfPeriodicHits, VocalBest, VocalBestLocation);
		}
		#endregion
	}
}