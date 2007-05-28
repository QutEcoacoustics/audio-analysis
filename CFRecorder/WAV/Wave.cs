using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CFRecorder
{
    /// <summary>
    /// Encapsulates Wave file properties used internally be WaveIn and WaveOut.
    /// </summary>
    public class Wave
    {
        // Can be used instead of a device id to open a device
        public const uint WAVE_MAPPER = unchecked((uint)(-1));

        // Flag specifying the use of a callback window for sound messages
        public const uint CALLBACK_WINDOW = 0x10000;

        // Error information...
        private const int WAVERR_BASE = 32;
        private const int MMSYSERR_BASE = 0;

        // Enum equivalent to MMSYSERR_*
        public enum MMSYSERR : int
        {
            NOERROR = 0,
            ERROR = (MMSYSERR_BASE + 1),
            BADDEVICEID = (MMSYSERR_BASE + 2),
            NOTENABLED = (MMSYSERR_BASE + 3),
            ALLOCATED = (MMSYSERR_BASE + 4),
            INVALHANDLE = (MMSYSERR_BASE + 5),
            NODRIVER = (MMSYSERR_BASE + 6),
            NOMEM = (MMSYSERR_BASE + 7),
            NOTSUPPORTED = (MMSYSERR_BASE + 8),
            BADERRNUM = (MMSYSERR_BASE + 9),
            INVALFLAG = (MMSYSERR_BASE + 10),
            INVALPARAM = (MMSYSERR_BASE + 11),
            HANDLEBUSY = (MMSYSERR_BASE + 12),
            INVALIDALIAS = (MMSYSERR_BASE + 13),
            BADDB = (MMSYSERR_BASE + 14),
            KEYNOTFOUND = (MMSYSERR_BASE + 15),
            READERROR = (MMSYSERR_BASE + 16),
            WRITEERROR = (MMSYSERR_BASE + 17),
            DELETEERROR = (MMSYSERR_BASE + 18),
            VALNOTFOUND = (MMSYSERR_BASE + 19),
            NODRIVERCB = (MMSYSERR_BASE + 20),
            LASTERROR = (MMSYSERR_BASE + 20)
        }

        // Enum equivalent to WAVERR_*
        private enum WAVERR : int
        {
            NONE = 0,
            BADFORMAT = WAVERR_BASE + 0,
            STILLPLAYING = WAVERR_BASE + 1,
            UNPREPARED = WAVERR_BASE + 2,
            SYNC = WAVERR_BASE + 3,
            LASTERROR = WAVERR_BASE + 3
        }

        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// Invalid format
        /// </summary>
        public const uint WAVE_INVALIDFORMAT = 0x00000000;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 11.025 kHz, Mono,   8-bit
        /// </summary>
        public const uint WAVE_FORMAT_1M08 = 0x00000001;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 11.025 kHz, Stereo, 8-bit
        /// </summary>
        public const uint WAVE_FORMAT_1S08 = 0x00000002;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 11.025 kHz, Mono,   16-bit
        /// </summary>
        public const uint WAVE_FORMAT_1M16 = 0x00000004;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 11.025 kHz, Stereo, 16-bit
        /// </summary>
        public const uint WAVE_FORMAT_1S16 = 0x00000008;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 22.05  kHz, Mono,   8-bit
        /// </summary>
        public const uint WAVE_FORMAT_2M08 = 0x00000010;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 22.05  kHz, Stereo, 8-bit
        /// </summary>
        public const uint WAVE_FORMAT_2S08 = 0x00000020;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 22.05  kHz, Mono,   16-bit
        /// </summary>
        public const uint WAVE_FORMAT_2M16 = 0x00000040;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 22.05  kHz, Stereo, 16-bit
        /// </summary>
        public const uint WAVE_FORMAT_2S16 = 0x00000080;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 44.1   kHz, Mono,   8-bit
        /// </summary>
        public const uint WAVE_FORMAT_4M08 = 0x00000100;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 44.1   kHz, Stereo, 8-bit
        /// </summary>
        public const uint WAVE_FORMAT_4S08 = 0x00000200;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 44.1   kHz, Mono,   16-bit
        /// </summary>
        public const uint WAVE_FORMAT_4M16 = 0x00000400;
        /// <summary>
        /// Used by dwFormats in WAVEINCAPS and WAVEOUTCAPS
        /// 44.1   kHz, Stereo, 16-bit
        /// </summary>
        public const uint WAVE_FORMAT_4S16 = 0x00000800;

        /// <summary>
        /// WAVEFORMATEX defines the format of waveform-audio data. Only format
        /// information common to all waveform-audio data formats is included in
        /// this structure. For formats requiring additional information, this
        /// structure is included as the first member in another structure, along
        /// with the additional information.
        /// </summary>
        public class WAVEFORMATEX
        {
            // Accessors specifying data positions in a .wave file
            // RIFF header up to 20 bytes in .wav file
            protected const int WF_OFFSET_FORMATTAG = 20;
            protected const int WF_OFFSET_CHANNELS = 22;
            protected const int WF_OFFSET_SAMPLESPERSEC = 24;
            protected const int WF_OFFSET_AVGBYTESPERSEC = 28;
            protected const int WF_OFFSET_BLOCKALIGN = 32;
            protected const int WF_OFFSET_BITSPERSAMPLE = 34;
            // Offset 2 for wBitsPerSample
            // + 4 for the subchunk id "data"
            // + 4 for the subchunk length
            public const int WF_OFFSET_DATA = 44;

            /// <summary>
            /// Waveform-audio format type. Format tags are registered with Microsoft
            /// Corporation for many compression algorithms. A complete list of
            /// format tags is located in the Mmsystem.h header file. 
            /// </summary>
            public ushort wFormatTag = 0;
            /// <summary>
            /// Number of channels in the waveform-audio data. Monaural data uses one
            /// channel and stereo data uses two channels.
            /// </summary>
            public ushort nChannels = 0;
            /// <summary>
            /// Sample rate, in samples per second (hertz), that each channel should
            /// be played or recorded. If wFormatTag is WAVE_FORMAT_PCM, then common
            /// values for nSamplesPerSec are 8.0 kHz, 11.025 kHz, 22.05 kHz, and
            /// 44.1 kHz. For non-PCM formats, this member must be computed according
            /// to the manufacturer's specification of the format tag.
            /// </summary>
            public uint nSamplesPerSec = 0;
            /// <summary>
            /// Required average data-transfer rate, in bytes per second, for the format
            /// tag. If wFormatTag is WAVE_FORMAT_PCM, nAvgBytesPerSec should be equal to
            /// the product of nSamplesPerSec and nBlockAlign. For non-PCM formats, this
            /// member must be computed according to the manufacturer's specification of
            /// the format tag. 
            /// Playback and record software can estimate buffer sizes by using the
            /// nAvgBytesPerSec member. 
            /// </summary>
            public uint nAvgBytesPerSec = 0;
            /// <summary>
            /// Block alignment, in bytes. The block alignment is the minimum atomic unit
            /// of data for the wFormatTag format type. If wFormatTag is WAVE_FORMAT_PCM,
            /// nBlockAlign should be equal to the product of nChannels and wBitsPerSample
            /// divided by 8 (bits per byte). For non-PCM formats, this member must be
            /// computed according to the manufacturer's specification of the format tag. 
            /// Playback and record software must process a multiple of nBlockAlign bytes
            /// of data at a time. Data written and read from a device must always start
            /// at the beginning of a block. For example, it is illegal to start playback
            /// of PCM data in the middle of a sample (that is, on a non-block-aligned
            /// boundary). 
            /// </summary>
            public ushort nBlockAlign = 0;
            /// <summary>
            /// Bits per sample for the wFormatTag format type. If wFormatTag is
            /// WAVE_FORMAT_PCM, then wBitsPerSample should be equal to 8 or 16. For
            /// non-PCM formats, this member must be set according to the manufacturer's
            /// specification of the format tag. Some compression schemes cannot define
            /// a value for wBitsPerSample, so this member can be zero.
            /// </summary>
            public ushort wBitsPerSample = 0;

            /// <summary>
            /// Seeks the provided Stream to the position at which this structure starts.
            /// Namely, the wFormatTag member.
            /// </summary>
            /// <param name="fs"></param>
            public void SeekTo(Stream fs)
            {
                fs.Seek(WF_OFFSET_FORMATTAG, SeekOrigin.Begin);
            }

            /// <summary>
            /// Seeks the provided Stream to the position immediately after this
            /// structure.
            /// </summary>
            /// <param name="fs"></param>
            public void Skip(Stream fs)
            {
                fs.Seek(WF_OFFSET_DATA, SeekOrigin.Begin);
            }

            /// <summary>
            /// Read in a WAVEFORMATEX from the given BinaryReader.
            /// </summary>
            /// <param name="rdr">BinaryReader accessing a WAVEFORMATEX.</param>
            /// <returns>The size of the data following the structure</returns>
            public void Read(BinaryReader rdr)
            {
                wFormatTag = rdr.ReadUInt16();
                nChannels = rdr.ReadUInt16();
                nSamplesPerSec = rdr.ReadUInt32();
                nAvgBytesPerSec = rdr.ReadUInt32();
                nBlockAlign = rdr.ReadUInt16();
                wBitsPerSample = rdr.ReadUInt16();

                // Unused subchunk Id and size
                uint dataId = rdr.ReadUInt32();
                uint dataLength = rdr.ReadUInt32();
            }

            /// <summary>
            /// Write out a WAVEFORMATEX to the given BinaryWriter.
            /// </summary>
            /// <param name="wrtr">BinaryWriter to receive the WAVEFORMATEX
            /// information</param>
            public void Write(BinaryWriter wrtr)
            {
                wrtr.Write(wFormatTag);
                wrtr.Write(nChannels);
                wrtr.Write(nSamplesPerSec);
                wrtr.Write(nAvgBytesPerSec);
                wrtr.Write(nBlockAlign);
                wrtr.Write(wBitsPerSample);
            }
        }

        /// <summary>
        /// This structure defines the header used to identify a waveform-audio buffer.
        ///		typedef struct 
        ///		{
        ///			LPSTR lpData;
        ///			DWORD dwBufferLength;
        ///			DWORD dwBytesRecorded;
        ///			DWORD dwUser;
        ///			DWORD dwFlags;
        ///			DWORD dwLoops;
        ///			struct wavehdr_tag *lpNext;
        ///				DWORD reserved;}
        ///			WAVEHDR;
        /// </summary>
        public class WAVEHDR : IDisposable
        {
            /// <summary>
            /// Used by dwFlags in WAVEHDR
            /// Set by the device driver to indicate that it is finished with the buffer
            /// and is returning it to the application.
            /// </summary>
            public const int WHDR_DONE = 0x00000001;
            /// <summary>
            /// Used by dwFlags in WAVEHDR
            /// Set by Windows to indicate that the buffer has been prepared with the
            /// waveInPrepareHeader or waveOutPrepareHeader function.
            /// </summary>
            public const int WHDR_PREPARED = 0x00000002;
            /// <summary>
            /// Used by dwFlags in WAVEHDR
            /// This buffer is the first buffer in a loop. This flag is used only with
            /// output buffers.
            /// </summary>
            public const int WHDR_BEGINLOOP = 0x00000004;
            /// <summary>
            /// Used by dwFlags in WAVEHDR
            /// This buffer is the last buffer in a loop. This flag is used only with
            /// output buffers.
            /// </summary>
            public const int WHDR_ENDLOOP = 0x00000008;
            /// <summary>
            /// Used by dwFlags in WAVEHDR
            /// Set by Windows to indicate that the buffer is queued for playback.
            /// </summary>
            public const int WHDR_INQUEUE = 0x00000010;

            /// <summary>
            /// Set in WAVEFORMATEX.wFormatTag to specify PCM data.
            /// </summary>
            public const int WAVE_FORMAT_PCM = 1;

            /// <summary>
            /// Long pointer to the address of the waveform buffer. This buffer must
            /// be block-aligned according to the nBlockAlign member of the
            /// WAVEFORMATEX structure used to open the device.
            /// </summary>
            public IntPtr lpData = IntPtr.Zero;
            /// <summary>
            /// Specifies the length, in bytes, of the buffer.
            /// </summary>
            public uint dwBufferLength = 0;
            /// <summary>
            /// When the header is used in input, this member specifies how much data
            /// is in the buffer.
            /// </summary>
            public uint dwBytesRecorded = 0;
            /// <summary>
            /// Specifies user data.
            /// </summary>
            public uint dwUser = 0;
            /// <summary>
            /// Specifies information about the buffer.
            /// </summary>
            public uint dwFlags = 0;
            /// <summary>
            /// Specifies the number of times to play the loop. This member is used
            /// only with output buffers.
            /// </summary>
            public uint dwLoops = 0;
            /// <summary>
            /// Reserved.
            /// </summary>
            public IntPtr lpNext = IntPtr.Zero;
            /// <summary>
            /// Reserved.
            /// </summary>
            public uint reserved = 0;

            /// <summary>
            /// Read a data buffer from the supplied BinaryReader.  This method will
            /// allocate memory for the data buffer it is not already allocated.
            /// </summary>
            /// <param name="rdr">BinaryReader containing data</param>
            /// <param name="readLength">Size, in bytes, to be read</param>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public MMSYSERR Read(BinaryReader rdr, uint readLength, int align)
            {
                uint bufferLength = readLength;

                if (bufferLength % align != 0)
                    bufferLength += (uint)(align - (bufferLength % align));

                dwBufferLength = bufferLength;
                byte[] data = new byte[readLength];
                rdr.Read(data, 0, data.Length);

                if (lpData == IntPtr.Zero)
                    lpData = Memory.LocalAlloc(Memory.LMEM_FIXED, (uint)bufferLength);

                if (lpData == IntPtr.Zero)
                    return MMSYSERR.NOMEM;

                Marshal.Copy(data, 0, lpData, data.Length);

                return MMSYSERR.NOERROR;
            }

            /// <summary>
            /// Write the contents of the recorded buffer to the supplied
            /// BinaryWriter.
            /// </summary>
            /// <param name="wrtr">BinaryWriter used as write target</param>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public Wave.MMSYSERR Write(BinaryWriter wrtr)
            {
                if (lpData == IntPtr.Zero)
                    return Wave.MMSYSERR.NOMEM;

                byte[] data = new byte[dwBytesRecorded];
                Marshal.Copy(lpData, data, 0, data.Length);
                wrtr.Write(data);

                return Wave.MMSYSERR.NOERROR;
            }

            /// <summary>
            /// Initialize an instance of a WAVEHDR with the specified buffer
            /// size.
            /// </summary>
            /// <param name="bufferLength">Size, in bytes, of buffer</param>
            /// <param name="init">true=clear data to 0</param>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public MMSYSERR Init(uint bufferLength, bool init)
            {
                if (lpData != IntPtr.Zero && dwBufferLength < bufferLength)
                {
                    Memory.LocalFree(lpData);
                    lpData = IntPtr.Zero;
                }

                if (lpData == IntPtr.Zero)
                    lpData = Memory.LocalAlloc(Memory.LMEM_FIXED, bufferLength);

                dwBufferLength = bufferLength;

                if (lpData == IntPtr.Zero)
                    return MMSYSERR.NOMEM;

                if (init)
                {
                    for (int i = 0; i < bufferLength; i++)
                    {
                        Marshal.WriteByte(lpData, i, 0);
                    }
                }

                return MMSYSERR.NOERROR;
            }

            /// <summary>
            /// Frees any memory allocated for the buffer.
            /// </summary>
            public void Dispose()
            {
                if (lpData != IntPtr.Zero)
                    Memory.LocalFree(lpData);
            }
        }
    }
}
