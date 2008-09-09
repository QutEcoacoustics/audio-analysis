using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace BridgeTest
{
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("ca7e9ef0-1cbe-11d3-8d29-00a0c94bbfee")]
	public interface ILameAudioEncoderProperties
	{
    //
    // Configuring MPEG audio encoder parameters with unspecified
    // input stream type may lead to misbehaviour and confusing
    // results. In most cases the specified parameters will be
    // overridden by defaults for the input media type.
    // To archive proper results use this interface on the
    // audio encoder filter with input pin connected to the valid
    // source.
    //
        // Is PES output enabled? Return TRUE or FALSE
		void get_PESOutputEnabled(out bool enabled);
		void set_PESOutputEnabled(bool enabled);
		// Get target compression bitrate in Kbits/s
		void get_Bitrate(out int bitrate);
		// Set target compression bitrate in Kbits/s
        // Not all numbers available! See spec for details!
		void set_Bitrate(int bitrate);
        // Get variable bitrate flag
		void get_Variable(out bool variable);
        // Set variable bitrate flag
		void set_Variable(bool variable);
        // Get variable bitrate in Kbits/s
		void get_VariableMin(out int min);
        // Set variable bitrate in Kbits/s
        // Not all numbers available! See spec for details!
		void set_VariableMin(int min);
        // Get variable bitrate in Kbits/s
		void get_VariableMax(out int max);
        // Set variable bitrate in Kbits/s
        // Not all numbers available! See spec for details!
		void set_VariableMax(int max);
        // Get compression quality
		void get_Quality(out int quality);
        // Set compression quality
        // Not all numbers available! See spec for details!
        void set_Quality(int quality);
        // Get VBR quality
        void get_VariableQ(out int vbrQuality);
        // Set VBR quality
        // Not all numbers available! See spec for details!
        void set_VariableQ(int vbrQuality);
        // Get source sample rate. Return E_FAIL if input pin
        // in not connected.
		void get_SourceSampleRate(out int sampleRate);
        // Get source number of channels. Return E_FAIL if
        // input pin is not connected.
        void get_SourceChannels(out int channels);
        // Get sample rate for compressed audio bitstream
		void get_SampleRate(out int sampleRate);
        // Set sample rate. See genaudio spec for details
        void set_SampleRate(int sampleRate);
        // Get channel mode. See genaudio.h for details
        void get_ChannelMode(out int channelMode);
        // Set channel mode
        void set_ChannelMode(int channelMode);
        // Is CRC enabled?
		void get_CRCFlag(out bool flag);
        // Enable/disable CRC
        void set_CRCFlag(bool flag);
        // Force mono
        void get_ForceMono(out bool flag);
        // Force mono
        void set_ForceMono(bool flag);
        // Set duration
        void get_SetDuration(out bool flag);
        // Set duration
        void set_SetDuration(bool flag);
        // Control 'original' flag
        void get_OriginalFlag(out bool flag);
        void set_OriginalFlag(bool flag);
        // Control 'copyright' flag
        void get_CopyrightFlag(out bool flag);
        void set_CopyrightFlag(bool flag);
        // Control 'Enforce VBR Minimum bitrate' flag
        void get_EnforceVBRmin(out bool flag);
        void set_EnforceVBRmin(bool flag);
        // Control 'Voice' flag
        void get_VoiceMode(out bool flag);
        void set_VoiceMode(bool flag);
        // Control 'Keep All Frequencies' flag
        void get_KeepAllFreq(out bool flag);
        void set_KeepAllFreq(bool flag);
        // Control 'Strict ISO complience' flag
        void get_StrictISO(out bool flag);
        void set_StrictISO(bool flag);
        // Control 'Disable short block' flag
        void get_NoShortBlock(out bool flag);
        void set_NoShortBlock(bool flag);
        // Control 'Xing VBR Tag' flag
        void get_XingTag(out bool flag);
        void set_XingTag(bool flag);
        // Control 'Forced mid/ side stereo' flag
        void get_ForceMS(out bool flag);
        void set_ForceMS(bool flag);
        // Control 'ModeFixed' flag
        void get_ModeFixed(out bool flag);
        void set_ModeFixed(bool flag);

        /*//Receive the block of encoder 
        //configuration parametres
        STDMETHOD(get_ParameterBlockSize) (THIS_
            BYTE *pcBlock, DWORD *pdwSize
        ) PURE;
        // Set encoder configuration parametres
        STDMETHOD(set_ParameterBlockSize) (THIS_
            BYTE *pcBlock, DWORD dwSize
        ) PURE;
        // Set default audio encoder parameters depending
        // on current input stream type
        STDMETHOD(DefaultAudioEncoderProperties) (THIS_
        ) PURE;
        // By default the modified properties are not saved to
        // registry immediately, so the filter needs to be
        // forced to do this. Omitting this steps may lead to
        // misbehavior and confusing results.
        STDMETHOD(LoadAudioEncoderPropertiesFromRegistry) (THIS_
        ) PURE;
        STDMETHOD(SaveAudioEncoderPropertiesToRegistry) (THIS_
        ) PURE;
        // Determine, whether the filter can be configured. If this
        // functions returs E_FAIL, input format hasn't been
        // specified and filter behavior unpredicated. If S_OK,
        // the filter could be configured with correct values.
        STDMETHOD(InputTypeDefined) (THIS_
        ) PURE;
        // Reconnects output pin (crucial for Fraunhofer MPEG Layer-3 Decoder)
        STDMETHOD(ApplyChanges) (THIS_
        ) PURE;*/
	}

	public class LameEncoder : IDisposable
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WAVEFORMATEX
		{
			public short wFormatTag;
			public short nChannels;
			public int nSamplesPerSec;
			public int nAvgBytesPerSec;
			public short nBlockAlign;
			public short wBitsPerSample;
			public short cbSize;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct MPEGLAYER3WAVEFORMAT
		{
			public WAVEFORMATEX wfx;
			public short wID;
			public int fdwFlags;
			public short nBlockSize;
			public short nFramesPerBlock;
			public short nCodecDelay;
		}

		List<IntPtr> allocatedMemory = new List<IntPtr>();

		public AMMediaType SetFormat(short channels, int sampleRate, int bitrate)
		{
			var format = new MPEGLAYER3WAVEFORMAT();
			format.wfx.wFormatTag = 85;
			format.wfx.nChannels = channels;
			format.wfx.nSamplesPerSec = sampleRate;
			format.wfx.nAvgBytesPerSec = bitrate;
			format.wfx.cbSize = 12;
			format.wfx.nBlockAlign = 1;

			AMMediaType retVal = new AMMediaType();
			retVal.majorType = MediaType.Audio;
			retVal.subType = new Guid("00000055-0000-0010-8000-00AA00389B71");
			retVal.formatType = FormatType.WaveEx;
			retVal.formatSize = 30;
			retVal.formatPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(format));
			Marshal.StructureToPtr(format, retVal.formatPtr, false);
			allocatedMemory.Add(retVal.formatPtr);

			return retVal;
		}

		#region IDisposable Members
		~LameEncoder()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		void Dispose(bool disposing)
		{
			foreach (var ptr in allocatedMemory)
				Marshal.FreeCoTaskMem(ptr);
			if (disposing)
				GC.SuppressFinalize(this);
		}
		#endregion
	}
}