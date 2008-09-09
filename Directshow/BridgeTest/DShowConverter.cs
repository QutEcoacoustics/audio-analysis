using System;
using System.IO;
using AudioTools.DirectShow;
using DirectShowLib;
using System.Threading;
using System.Collections.Generic;
using DirectShowLib.Utils;

namespace AudioTools
{
	public class DShowConverter
	{
		public const string WavDestClsid = "3C78B8E2-6C4D-11D1-ADE2-0000F8754B99";
		public const string LameClsid = "B8D27088-FF5F-4B7C-98DC-0E91A1696286";

		public static DirectShowStream ToWav(string filename, string sourceType)
		{
			var graphBuilder = (IFilterGraph2)new FilterGraph();
			var decoderFilters = GetDecoderFilters(sourceType);
			// Add WavDest
			var wavDest = FilterGraphTools.AddFilterFromClsid(graphBuilder, new Guid(WavDestClsid), "WAV Dest");
			if (wavDest == null)
				throw new Exception("WavDest filter not found");
			var bridgeSink = CreateGraph(graphBuilder, filename, wavDest, decoderFilters.ToArray());

			var retVal = new DirectShowStream(graphBuilder, bridgeSink);
			retVal.Run();
			return retVal;
		}

		public static DirectShowStream ToMp3(string filename, string sourceType)
		{
			var graphBuilder = (IFilterGraph2)new FilterGraph();
			var decoderFilters = GetDecoderFilters(sourceType);
			// Add LAME
			var lame = FilterGraphTools.AddFilterFromClsid(graphBuilder, new Guid(LameClsid), "MP3 Encoder");
			if (lame == null)
				throw new Exception("LAME filter not found");
			var bridgeSink = CreateGraph(graphBuilder, filename, lame, decoderFilters.ToArray());

			var retVal = new DirectShowStream(graphBuilder, bridgeSink);
			retVal.Run();
			return retVal;
		}

		public static List<IBaseFilter> GetDecoderFilters(string sourceType)
		{
			var decoderFilters = new List<IBaseFilter>();
			switch (sourceType)
			{
				case MimeTypes.WavpackMimeType:
					decoderFilters.Add(DSUtilities.CreateWavpackSplitter());
					decoderFilters.Add(DSUtilities.CreateWavpackDecoder());
					break;
				case MimeTypes.Mp3MimeType:
					decoderFilters.Add(DSUtilities.CreateMp3Decoder());
					break;
			}
			return decoderFilters;
		}

		static IBridgeSink CreateGraph(IFilterGraph2 graphBuilder, string filename, IBaseFilter encoderFilter, params IBaseFilter[] decoderFilters)
		{
			DsROTEntry rot = new DsROTEntry(graphBuilder);

			// Add source
			IBaseFilter sourceFilter;
			DsError.ThrowExceptionForHR(graphBuilder.AddSourceFilter(filename, "Source", out sourceFilter));

			if (decoderFilters != null && decoderFilters.Length > 0)
			{
				int i = 0;
				IBaseFilter previousFilter = sourceFilter;
				foreach (var filter in decoderFilters)
				{
					DsError.ThrowExceptionForHR(graphBuilder.AddFilter(filter, "Audio Decoder " + (i++).ToString()));
					FilterGraphTools.ConnectFilters(graphBuilder, previousFilter, 0, filter, 0, false);
					previousFilter = filter;
					//extraFilters.Add(filter);
				}
				FilterGraphTools.ConnectFilters(graphBuilder, previousFilter, 0, encoderFilter, 0, true);
			}
			else
				FilterGraphTools.ConnectFilters(graphBuilder, sourceFilter, 0, encoderFilter, 0, true);

			// Add sink
			var bridgeSink = DSUtilities.CreateBridgeSink(graphBuilder);
			FilterGraphTools.ConnectFilters(graphBuilder, encoderFilter, 0, (IBaseFilter)bridgeSink, 0, true);

			return bridgeSink;
		}
	}

	public class DirectShowStream : IDisposable
	{
		static List<DirectShowStream> allStreams = new List<DirectShowStream>();

		IFilterGraph2 graphBuilder;
		IBridgeSink bridgeSink;
		IMediaControl mediaControl;

		public DirectShowStream(IFilterGraph2 graphBuilder, IBridgeSink bridgeSink)
		{
			this.graphBuilder = graphBuilder;
			this.bridgeSink = bridgeSink;
			mediaControl = graphBuilder as IMediaControl;
			bridgeSink.set_Target(ReceiveData);

			AssociatedObjects = new List<IDisposable>();
			allStreams.Add(this);
		}

		public void Run()
		{
			mediaControl.Run();
		}

		long sample = 0;
		void ReceiveData(int size, byte[] data)
		{
			if (size > 4 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46)
				Console.WriteLine("Detected RIFF - {0}", sample);
			sample++;
			while (ReceivedData == null)
				Thread.Sleep(50);
			ReceivedData(size, data);
		}

		public event ReceiveData ReceivedData;

		public List<IDisposable> AssociatedObjects { get; set; }

		#region IDisposable Members
		public void Dispose()
		{
			bridgeSink.set_Target(null);
			mediaControl.Stop();
			DSUtilities.RemoveAllFilters(graphBuilder);
			foreach (IDisposable d in AssociatedObjects)
				d.Dispose();
			AssociatedObjects.Clear();
		}
		#endregion
	}
}