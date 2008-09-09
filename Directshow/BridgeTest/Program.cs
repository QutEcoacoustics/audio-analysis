using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DirectShowLib;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using AudioTools;
using System.IO;
using System.Threading;
using DirectShowLib.Utils;
using AudioTools.DirectShow;
using Microsoft.Win32;
using System.Globalization;
using DirectShowLib.DMO;
using System.Security;
using DirectShowLib.DES;

namespace BridgeTest
{
	public class Program
	{
		[MTAThread]
		public static void Main()
		{
			var audioReadingID = new Guid("AE88D25B-B86C-4130-89B1-10202045E5F9");
			using (var file = new SqlFilestreamHandle("AudioReadings", "Data", "AudioReadingID = @0", audioReadingID))
			{
				//ToWavFile(file, MimeTypes.WavpackMimeType, @"C:\Users\masonr\Desktop\Joe.wav");
				ToMp3File(file, MimeTypes.WavpackMimeType, @"C:\Users\masonr\Desktop\Joe2.mp3");
				//ToWmaFile(file, MimeTypes.WavpackMimeType, @"C:\Users\masonr\Desktop\Joe.wma");
				Console.WriteLine("DONE");
			}
		}

		static LameEncoder enc = new LameEncoder();

		public static void ToWavFile(SqlFilestreamHandle file, string sourceType, string target)
		{
			var graphBuilder = (IFilterGraph2)new FilterGraph();
			var decoderFilters = DShowConverter.GetDecoderFilters(sourceType);
			// Add WavDest
			var wavDest = FilterGraphTools.AddFilterFromClsid(graphBuilder, new Guid(DShowConverter.WavDestClsid), "WAV Dest");
			if (wavDest == null)
				throw new Exception("WavDest filter not found");
			DsROTEntry rot = new DsROTEntry(graphBuilder);

			// Add source
			IBaseFilter sourceFilter = DSUtilities.CreateSqlSource(graphBuilder);
			var sqlFileSource = sourceFilter as ISqlFileSource;
			sqlFileSource.set_TransactionContext(file.Context, file.Context.Length);
			var fileSource = sourceFilter as IFileSourceFilter;
			var mt = GetMediaType(file);
			fileSource.Load(file.FileName, mt);

			if (decoderFilters != null && decoderFilters.Count > 0)
			{
				int i = 0;
				IBaseFilter previousFilter = sourceFilter;
				foreach (var filter in decoderFilters)
				{
					DsError.ThrowExceptionForHR(graphBuilder.AddFilter(filter, "Audio Decoder " + (i++).ToString()));
					FilterGraphTools.ConnectFilters(graphBuilder, previousFilter, 0, filter, 0, true);
					previousFilter = filter;
				}
				FilterGraphTools.ConnectFilters(graphBuilder, previousFilter, 0, wavDest, 0, true);
			}
			else
				FilterGraphTools.ConnectFilters(graphBuilder, sourceFilter, 0, wavDest, 0, true);

			// Add sink
			/*var bridgeSink = DSUtilities.CreateBridgeSink(graphBuilder);
			FilterGraphTools.ConnectFilters(graphBuilder, encoderFilter, 0, (IBaseFilter)bridgeSink, 0, true);*/
			var writer = new FileWriter() as IFileSinkFilter;
			DsError.ThrowExceptionForHR(graphBuilder.AddFilter((IBaseFilter)writer, "File Writer"));
			DsError.ThrowExceptionForHR(writer.SetFileName(target, null));
			FilterGraphTools.ConnectFilters(graphBuilder, wavDest, 0, (IBaseFilter)writer, 0, true);

			var mediacontrol = graphBuilder as IMediaControl;
			DsError.ThrowExceptionForHR(mediacontrol.Run());
			IMediaEvent events = graphBuilder as IMediaEvent;
			EventCode evCode;
			DsError.ThrowExceptionForHR(events.WaitForCompletion(-1, out evCode));
			FilterGraphTools.DisconnectAllPins(graphBuilder);
			FilterGraphTools.RemoveAllFilters(graphBuilder);
		}

		const int OneSecond = 10000000;
		static IRenderEngine SetupDES(int start, int stop)
		{
			// we'll have one timeline object
			IAMTimeline tl = (IAMTimeline)new AMTimeline();
			// with one groupIAMTimelineGroup 
			IAMTimelineObj obj;
			DsError.ThrowExceptionForHR(tl.CreateEmptyNode(out obj, TimelineMajorType.Group));
			var tg = obj as IAMTimelineGroup;
			DsError.ThrowExceptionForHR(tg.SetMediaTypeForVB(1));
			DsError.ThrowExceptionForHR(tl.AddGroup(obj));
			// and one track
			tl.CreateEmptyNode(out obj, TimelineMajorType.Track);
			var tt = (IAMTimelineTrack)obj;
			DsError.ThrowExceptionForHR(((IAMTimelineComp)tg).VTrackInsBefore(obj, 0));
			// and two sources
			DsError.ThrowExceptionForHR(tl.CreateEmptyNode(out obj, TimelineMajorType.Source));
			var src = (IAMTimelineSrc)obj;
			DsError.ThrowExceptionForHR(obj.SetStartStop(0, (stop - start) * OneSecond));
			DsError.ThrowExceptionForHR(src.SetMediaTimes2(start * OneSecond, stop * OneSecond));
			tt.SrcAdd(obj);

			var re = (IRenderEngine)new RenderEngine();
			re.SetTimelineObject(tl);
			re.ConnectFrontEnd();
			return re;
		}

		public static void ToMp3File(SqlFilestreamHandle file, string sourceType, string target)
		{
			//var re = SetupDES(5, 10);

			IGraphBuilder graphBuilder = (IGraphBuilder)new FilterGraph();
			DsROTEntry rot = new DsROTEntry(graphBuilder);
			//DsError.ThrowExceptionForHR(re.GetFilterGraph(out graphBuilder));

			var decoderFilters = DShowConverter.GetDecoderFilters(sourceType);

			// Add source
			IBaseFilter sourceFilter = DSUtilities.CreateSqlSource(graphBuilder);
			var sqlFileSource = sourceFilter as ISqlFileSource;
			sqlFileSource.set_TransactionContext(file.Context, file.Context.Length);
			var fileSource = sourceFilter as IFileSourceFilter;
			var mt = GetMediaType(file);
			fileSource.Load(file.FileName, mt);

			// Add LAME
			var lame = FilterGraphTools.AddFilterFromClsid(graphBuilder, new Guid("{B8D27088-FF5F-4B7C-98DC-0E91A1696286}"/*DShowConverter.LameClsid*/), "MP3 Encoder");
			if (lame == null)
				throw new Exception("LAME filter not found");

			if (decoderFilters != null && decoderFilters.Count > 0)
			{
				int i = 0;
				IBaseFilter previousFilter = sourceFilter;
				foreach (var filter in decoderFilters)
				{
					DsError.ThrowExceptionForHR(graphBuilder.AddFilter(filter, "Audio Decoder " + (i++).ToString()));
					FilterGraphTools.ConnectFilters(graphBuilder, previousFilter, 0, filter, 0, true);
					//FilterGraphTools.ConnectFilters(graphBuilder, DsFindPin.ByDirection(previousFilter, PinDirection.Output, 0), reInputPin, false);
					previousFilter = filter;
				}
				FilterGraphTools.ConnectFilters(graphBuilder, previousFilter, 0, lame, 0, true);
				//FilterGraphTools.ConnectFilters(graphBuilder, DsFindPin.ByDirection(previousFilter, PinDirection.Output, 0), reInputPin, false);
			}
			else
				FilterGraphTools.ConnectFilters(graphBuilder, sourceFilter, 0, lame, 0, true);
				//FilterGraphTools.ConnectFilters(graphBuilder, DsFindPin.ByDirection(sourceFilter, PinDirection.Output, 0), reInputPin, false);

			//IPin reOutputPin;
			//DsError.ThrowExceptionForHR(re.GetGroupOutputPin(0, out reOutputPin));
			//FilterGraphTools.ConnectFilters(graphBuilder, reOutputPin, DsFindPin.ByDirection(lame, PinDirection.Input, 0), false);

			// Add sink
			var writer = FilterGraphTools.AddFilterFromClsid(graphBuilder, new Guid("{276806CD-1D0F-4BFD-B931-105D8199AD0F}"), null) as IFileSinkFilter;
			DsError.ThrowExceptionForHR(graphBuilder.AddFilter((IBaseFilter)writer, "File Writer"));
			DsError.ThrowExceptionForHR(writer.SetFileName(target, null));
			Console.ReadLine();
			FilterGraphTools.ConnectFilters(graphBuilder, lame, 0, (IBaseFilter)writer, 0, false);

			ConfigureMp3(lame);

			var mediacontrol = graphBuilder as IMediaControl;
			DsError.ThrowExceptionForHR(mediacontrol.Run());
			IMediaEvent events = graphBuilder as IMediaEvent;
			EventCode evCode;
			DsError.ThrowExceptionForHR(events.WaitForCompletion(-1, out evCode));
			FilterGraphTools.DisconnectAllPins(graphBuilder);
			FilterGraphTools.RemoveAllFilters(graphBuilder);
		}

		private static void ConfigureMp3(IBaseFilter lame)
		{
			var props = (ILameAudioEncoderProperties)lame;
			int bitrate;
			props.get_Bitrate(out bitrate);
			Console.WriteLine("Bitrate: {0}", bitrate);
			int channels;
			props.get_SourceChannels(out channels);
			Console.WriteLine("Channels: {0}", channels);

			props.set_Bitrate(96);
		}

		[DllImport("WMVCore.dll", EntryPoint = "WMCreateProfileManager", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern int WMCreateProfileManager([Out, MarshalAs(UnmanagedType.Interface)] out BridgeTest.WMInterfaces.IWMProfileManager ppProfileManager);

		[SuppressUnmanagedCodeSecurity]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("45086030-F7E4-486a-B504-826BB5792A3B")]
		interface MyIConfigAsfWriter
		{
			[Obsolete("This method requires IWMProfile, which in turn requires several other interfaces.  Rather than duplicate all those interfaces here, it is recommended that you use the WindowsMediaLib from http://DirectShowNet.SourceForge.net", false)]
			int ConfigureFilterUsingProfile(WMInterfaces.IWMProfile profile);
			[Obsolete("Using Guids is considered obsolete by MS.  The preferred approach is using an IWMProfile.  See ConfigureFilterUsingProfile", false)]
			int ConfigureFilterUsingProfileGuid(Guid guidProfile);
			[Obsolete("This method is now obsolete because it assumes version 4.0 Windows Media Format SDK profiles. Use GetCurrentProfile or GetCurrentProfileGuid instead to correctly identify a profile.", false)]
			int ConfigureFilterUsingProfileId(int dwProfileId);
			[Obsolete("This method requires IWMProfile, which in turn requires several other interfaces.  Rather than duplicate all those interfaces here, it is recommended that you use the WindowsMediaLib from http://DirectShowNet.SourceForge.net", false)]
			int GetCurrentProfile(out IntPtr ppProfile);
			[Obsolete("Using Guids is considered obsolete by MS.  The preferred approach is using an IWMProfile.  See GetCurrentProfile", false)]
			int GetCurrentProfileGuid(out Guid pProfileGuid);
			[Obsolete("This method is now obsolete because it assumes version 4.0 Windows Media Format SDK profiles. Use GetCurrentProfile or GetCurrentProfileGuid instead to correctly identify a profile.", false)]
			int GetCurrentProfileId(out int pdwProfileId);
			int GetIndexMode(out bool pbIndexFile);
			int SetIndexMode(bool bIndexFile);
		}

		public static void ToWmaFile(SqlFilestreamHandle file, string sourceType, string target)
		{
			var graphBuilder = (IFilterGraph2)new FilterGraph();
			var decoderFilters = DShowConverter.GetDecoderFilters(sourceType);
			
			DsROTEntry rot = new DsROTEntry(graphBuilder);

			// Add source
			IBaseFilter sourceFilter = DSUtilities.CreateSqlSource(graphBuilder);
			var sqlFileSource = sourceFilter as ISqlFileSource;
			sqlFileSource.set_TransactionContext(file.Context, file.Context.Length);
			var fileSource = sourceFilter as IFileSourceFilter;
			var mt = GetMediaType(file);
			fileSource.Load(file.FileName, mt);

			// Add WMA Encoder
			var encoder = (IBaseFilter)new DMOWrapperFilter();
			var dmoWrapper = (IDMOWrapperFilter)encoder;
			dmoWrapper.Init(new Guid("{70F598E9-F4AB-495A-99E2-A7C4D3D89ABF}"), DMOCategory.AudioEncoder);
			
			ConfigureWMA(encoder);
			DsError.ThrowExceptionForHR(graphBuilder.AddFilter(encoder, "WMA Encoder"));

			var writer = new WMAsfWriter() as IBaseFilter;
			DsError.ThrowExceptionForHR(graphBuilder.AddFilter((IBaseFilter)writer, "File Writer"));

			if (decoderFilters != null && decoderFilters.Count > 0)
			{
				int i = 0;
				IBaseFilter previousFilter = sourceFilter;
				foreach (var filter in decoderFilters)
				{
					DsError.ThrowExceptionForHR(graphBuilder.AddFilter(filter, "Audio Decoder " + (i++).ToString()));
					FilterGraphTools.ConnectFilters(graphBuilder, previousFilter, 0, filter, 0, false);
					previousFilter = filter;
				}
				FilterGraphTools.ConnectFilters(graphBuilder, previousFilter, 0, encoder, 0, false);
			}
			else
				FilterGraphTools.ConnectFilters(graphBuilder, sourceFilter, 0, encoder, 0, false);

			// Add file sink
			//var writer = new FileWriter() as IFileSinkFilter;
			
			DsError.ThrowExceptionForHR(((IFileSinkFilter)writer).SetFileName(target, null));
			ConfigureASFWriter((IConfigAsfWriter2)writer);
			FilterGraphTools.ConnectFilters(graphBuilder, encoder, 0, (IBaseFilter)writer, 0, true);

			Console.ReadLine();
			var mediacontrol = graphBuilder as IMediaControl;
			DsError.ThrowExceptionForHR(mediacontrol.Run());
			
			IMediaEvent events = graphBuilder as IMediaEvent;
			EventCode evCode;
			DsError.ThrowExceptionForHR(events.WaitForCompletion(-1, out evCode));
			FilterGraphTools.DisconnectAllPins(graphBuilder);
			FilterGraphTools.RemoveAllFilters(graphBuilder);
		}

		public static Guid WMMEDIATYPE_Audio = new Guid("73647561-0000-0010-8000-00AA00389B71");
		static void ConfigureASFWriter(IConfigAsfWriter2 writer)
		{
			DsError.ThrowExceptionForHR(writer.SetParam(ASFWriterConfig.DontCompress, 1, 0));
			//DsError.ThrowExceptionForHR(writer.ConfigureFilterUsingProfileId(0));
			/*WMInterfaces.IWMProfileManager manager;
			DsError.ThrowExceptionForHR(WMCreateProfileManager(out manager));
			WMInterfaces.IWMProfile profile;
			manager.CreateEmptyProfile(WMInterfaces.WMT_VERSION.WMT_VER_9_0, out profile);
			WMInterfaces.IWMStreamConfig streamConfig;
			profile.CreateNewStream(ref WMMEDIATYPE_Audio, out streamConfig);
			streamConfig.SetBitrate(64000);
			streamConfig.SetStreamName("Audio");
			profile.AddStream(streamConfig);
			writer.ConfigureFilterUsingProfile(profile);*/
		}

		[SuppressUnmanagedCodeSecurity]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("89c31040-846b-11ce-97d3-00aa0055595a")]
		public interface MyIEnumMediaTypes
		{
			int Clone(out IEnumMediaTypes ppEnum);
			int Next(int cMediaTypes, AMMediaType[] ppMediaTypes, ref int pcFetched);
			int Reset();
			int Skip(int cMediaTypes);
		}

		static void ConfigureWMA(IBaseFilter encoder)
		{
			var mo = (IMediaObject)encoder;
			bool found = false;
			int i = 0;
			while (!found)
			{
				AMMediaType mt = new AMMediaType();
				int hr = mo.GetOutputType(0, i++, mt);
				WaveFormatEx f = new WaveFormatEx();
				Marshal.PtrToStructure(mt.formatPtr, f);
				if (hr != 0)
					found = true;
				else if (f.nChannels == 1 &&// Search for 16-bit mono 22KHz 64kBps
					f.wBitsPerSample == 16 &&
					f.nSamplesPerSec == 22050/* &&
					f.nAvgBytesPerSec == 96000*/)
				{
					//if (f.nAvgBytesPerSec > 5000)
						mo.SetOutputType(0, mt, DMOSetType.None);
					Console.Write("{0} - {1}Hz {2}bit\t", f.nChannels, f.nSamplesPerSec, f.wBitsPerSample);
					Console.WriteLine("{0}/{1}bps", f.nSamplesPerSec, f.nAvgBytesPerSec);
				}
			}

			found = false;
			i = 0;
			while (!found)
			{
				AMMediaType mt = new AMMediaType();
				int hr = mo.GetInputType(0, i++, mt);
				WaveFormatEx f = new WaveFormatEx();
				Marshal.PtrToStructure(mt.formatPtr, f);
				if (hr != 0)
					found = true;
				else/* if (f.nChannels == 1 &&// Search for 16-bit mono 22KHz 64kBps
					f.wBitsPerSample == 16/* &&
					f.nSamplesPerSec == 22050/* &&
					f.nAvgBytesPerSec == 96000
												)*/
				{
					//if (f.nAvgBytesPerSec > 5000)
					//mo.SetOutputType(0, mt, DMOSetType.None);
					Console.WriteLine("{0} - {1}Hz {2}bit", f.nChannels, f.nSamplesPerSec, f.wBitsPerSample);
				}
			}
		}

		const int MaxCheckBytes = 16;
		public static AMMediaType GetMediaType(SqlFilestreamHandle file)
		{
			var retVal = new AMMediaType();
			retVal.majorType = MediaType.Null;
			retVal.subType = MediaSubType.Null;
			file.Open();
			try
			{
				byte[] buffer = new byte[MaxCheckBytes];
				int read;
				using (var stream = new FileStream(file.Handle, FileAccess.Read))
					read = stream.Read(buffer, 0, MaxCheckBytes);

				// Iterate through available types
				var key = Registry.ClassesRoot.OpenSubKey("Media Type");
				foreach (var majorType in key.GetSubKeyNames())
				{
					if (majorType == "Extensions")
						continue;

					var mtKey = key.OpenSubKey(majorType);
					foreach (var subType in mtKey.GetSubKeyNames())
					{
						var stKey = mtKey.OpenSubKey(subType);
						foreach (var match in stKey.GetValueNames())
						{
							int temp;
							if (int.TryParse(match, out temp))
							{
								var val = stKey.GetValue(match).ToString();
								var parts = val.Split(',');
								if (parts.Length == 4)
								{
									try
									{
										int offset = int.Parse(parts[0]);
										int length = int.Parse(parts[1]);
										// We only support offsets from the start of the file
										if (CheckBytes(buffer, read, offset, length, parts[2], parts[3]))
										{
											retVal.majorType = new Guid(majorType);
											retVal.subType = new Guid(subType);
											return retVal;
										}
									}
									catch
									{
									}
								}
							}
						}
					}
				}
			}
			finally
			{
				file.Close();
			}
			return retVal;
		}

		static bool CheckBytes(byte[] buffer, int bufferLength, int offset, int length, string mask, string val)
		{
			if (offset >= 0 && offset + length <= bufferLength)
			{
				byte[] checkBytes = new byte[length];
				for (int i = 0; i < length; i++)
					checkBytes[i] = byte.Parse(val.Substring(i * 2, 2), NumberStyles.HexNumber);

				byte[] testBytes = new byte[length];
				Array.Copy(buffer, offset, testBytes, 0, length);

				if (!string.IsNullOrEmpty(mask))
				{
					byte[] maskBytes = new byte[length];
					for (int i = 0; i < length; i++)
						maskBytes[i] = byte.Parse(mask.Substring(i * 2, 2), NumberStyles.HexNumber);

					// Apply mask
					for (int i = 0; i < length; i++)
						testBytes[i] &= maskBytes[i];
				}

				for (int i = 0; i < length; i++)
					if (testBytes[i] != checkBytes[i])
						return false;
				return true;
			}
			return false;
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static IBaseFilter AddFilterFromClsid(IGraphBuilder graphBuilder, Guid clsid, string name)
		{
			int hr = 0;
			IBaseFilter filter = null;

			if (graphBuilder == null)
			{
				throw new ArgumentNullException("graphBuilder");
			}

			try
			{
				Type type = Type.GetTypeFromCLSID(clsid);
				var j = Activator.CreateInstance(type);
				filter = (IBaseFilter)j;

				hr = graphBuilder.AddFilter(filter, name);
				DsError.ThrowExceptionForHR(hr);
			}
			catch
			{
				if (filter != null)
				{
					Marshal.ReleaseComObject(filter);
					filter = null;
				}
			}

			return filter;
		}
	}
}
