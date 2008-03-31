using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.WindowsCE.Forms;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace CFRecorder
{
    /// <summary>
    /// Encapsulates Waveform Audio Interface playback functions and provides a simple
    /// interface for playing audio.
    /// </summary>
    public class WaveOut : IDisposable
    {
        /// <summary>
        /// Supplies an inteface for loading and playing a .wav file.
        /// </summary>
        public class WaveFile : IDisposable
        {
            /// <summary>
            /// Hardware interface instance for this wave file.
            /// </summary>
            protected IntPtr m_hwo = IntPtr.Zero;

            /// <summary>
            /// Instance of WAVEFORMATEX associated with this file.
            /// </summary>
            protected Wave.WAVEFORMATEX m_wfmt = null;

            /// <summary>
            /// Buffers used to read and play wave data.
            /// </summary>
            protected Wave.WAVEHDR[] m_whdr = null;

            /// <summary>
            /// Size of each buffer used for playback
            /// </summary>
            protected int m_bufferSize;

            /// <summary>
            /// Number of buffers needed for playback
            /// </summary>
            protected int m_numBlocks;

            /// <summary>
            /// Current buffer being read
            /// </summary>
            protected int m_curBlock;

            /// <summary>
            /// BinaryRead used to access the streaming audio file.
            /// </summary>
            protected BinaryReader m_rdr = null;

            /// <summary>
            /// Determines if file is done playing.
            /// </summary>
            public bool Done { get { return !m_playing; } }
            protected bool m_playing = false;

            /// <summary>
            /// Length, in milliseconds, of the audio file.
            /// </summary>
            public uint Milliseconds { get { return 1000 * m_dataLength / m_wfmt.nAvgBytesPerSec; } }
            protected uint m_dataLength;

            /// <summary>
            /// Create an instance of a wave file.  Allocate the two buffers that will
            /// be used for streaming audio.
            /// </summary>
            public WaveFile()
            {
                m_whdr = new Wave.WAVEHDR[2];
            }

            /// <summary>
            /// Play a wave file.
            /// </summary>
            /// <param name="curDevice">Hardware device to use for playback</param>
            /// <param name="fileName">Name of file to play</param>
            /// <param name="hwnd">Handle to a message window to use for messaging</param>
            /// <param name="bufferSize">Size of streaming buffers, a 0 value specifies
            /// that the buffer should be created big enough to fit the entire file</param>
            /// <param name="volLeft">Left channel volume level</param>
            /// <param name="volRight">Right channel volume level</param>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public Wave.MMSYSERR Play(uint curDevice, String fileName, IntPtr hwnd, int bufferSize, ushort volLeft, ushort volRight)
            {
                if (m_playing)
                    return Wave.MMSYSERR.NOERROR;

                if (!File.Exists(fileName))
                    return Wave.MMSYSERR.ERROR;

                FileInfo fi = new FileInfo(fileName);
                if ((fi.Attributes & FileAttributes.ReadOnly) != 0)
                    fi.Attributes -= FileAttributes.ReadOnly;

                FileStream strm = new FileStream(fileName, FileMode.Open);
                if (strm == null)
                    return Wave.MMSYSERR.ERROR;

                m_rdr = new BinaryReader(strm);
                if (m_rdr == null)
                    return Wave.MMSYSERR.ERROR;

                m_wfmt = new Wave.WAVEFORMATEX();
                m_wfmt.SeekTo(strm);

                // Read in the WAVEFORMATEX structure and attempt to open the
                // device for playback.
                m_wfmt.Read(m_rdr);

                Wave.MMSYSERR result = waveOutOpen(ref m_hwo, curDevice, m_wfmt, hwnd, 0, Wave.CALLBACK_WINDOW);
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                m_dataLength = (uint)(m_rdr.BaseStream.Length - Wave.WAVEFORMATEX.WF_OFFSET_DATA);

                if (bufferSize == 0)
                    m_bufferSize = (int)m_dataLength;
                else
                    m_bufferSize = bufferSize / 2;

                if (m_bufferSize % m_wfmt.nBlockAlign != 0)
                    m_bufferSize += m_wfmt.nBlockAlign - (m_bufferSize % m_wfmt.nBlockAlign);

                // Determine the number of buffer reads required to play the entire
                // file
                m_numBlocks = (int)(m_dataLength / m_bufferSize);
                if ((m_numBlocks * m_bufferSize) < m_dataLength)
                    m_numBlocks++;

                m_whdr[0] = new Wave.WAVEHDR();
                m_whdr[1] = new Wave.WAVEHDR();

                // Read in the first buffer
                result = ReadBuffer(0);
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                // If the entire file fits in the buffer then close the file
                if (m_numBlocks == 1)
                {
                    m_rdr.BaseStream.Close();
                    m_rdr.Close();
                    m_rdr = null;
                }

                SetVolume(volLeft, volRight);

                // Start playback of the first buffer
                result = waveOutWrite(m_hwo, m_whdr[0], (uint)Marshal.SizeOf(m_whdr[0]));
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                m_curBlock = 0;

                // Create the second buffer.  If the audio is being streamed, this will
                // be the next audio block, otherwise it will be padding
                Thread loadThread = new Thread(new ThreadStart(LoadBuffer));
                loadThread.Start();

                m_playing = true;

                return Wave.MMSYSERR.NOERROR;
            }

            /// <summary>
            /// Read in the specified buffer.
            /// </summary>
            /// <param name="bufIndex">Index of buffer to use for the read</param>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            protected Wave.MMSYSERR ReadBuffer(int bufIndex)
            {
                uint readLength = (uint)m_bufferSize;
                if (bufIndex < m_numBlocks)
                {
                    uint remainingDataLength = (uint)(m_rdr.BaseStream.Length - m_rdr.BaseStream.Position);
                    if (m_bufferSize > remainingDataLength)
                        readLength = remainingDataLength;
                }

                // Read in the next block of data
                Wave.MMSYSERR result = m_whdr[bufIndex].Read(m_rdr, readLength, m_wfmt.nBlockAlign);
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                // If the header is not prepared then prepare it
                if ((m_whdr[bufIndex].dwFlags & Wave.WAVEHDR.WHDR_PREPARED) == 0)
                {
                    return waveOutPrepareHeader(m_hwo, m_whdr[bufIndex], (uint)Marshal.SizeOf(m_whdr[bufIndex]));
                }

                return Wave.MMSYSERR.NOERROR;
            }

            /// <summary>
            /// Create an empty buffer to append to the end of the sound.  This protects
            /// the playback because the system will sometimes continue reading after the
            /// BlockDone method is called.
            /// </summary>
            /// <param name="bufIndex">Index of buffer to be created</param>
            /// <returns>MMSYSERR.NOERROR is successful</returns>
            protected Wave.MMSYSERR CreateBuffer(int bufIndex)
            {
                Wave.MMSYSERR result = m_whdr[bufIndex].Init((uint)m_bufferSize, true);
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                if ((m_whdr[bufIndex].dwFlags & Wave.WAVEHDR.WHDR_PREPARED) == 0)
                {
                    return waveOutPrepareHeader(m_hwo, m_whdr[bufIndex], (uint)Marshal.SizeOf(m_whdr[bufIndex]));
                }

                return Wave.MMSYSERR.NOERROR;
            }

            /// <summary>
            /// Load a buffer.  If there are blocks left to be read, then data will
            /// be read into the buffer that is not being played.  Otherwise, the buffer
            /// will be filled with 0's for padding to detect the end of playback.
            /// </summary>
            public void LoadBuffer()
            {
                int readBuffer = (m_curBlock + 3) % 2;

                lock (m_whdr[readBuffer])
                {
                    if (m_curBlock == m_numBlocks - 1)
                        CreateBuffer(readBuffer);
                    else
                        ReadBuffer(readBuffer);

                    waveOutWrite(m_hwo, m_whdr[readBuffer], (uint)Marshal.SizeOf(m_whdr[readBuffer]));
                }
            }

            /// <summary>
            /// Called by the MessageWindow when the buffer currently being played has
            /// finished.  This method starts the loading of the next block on a
            /// separate thread.  If the current block is the last one then playback is
            /// stopped.
            /// </summary>
            public void BlockDone()
            {
                m_curBlock++;

                if (m_curBlock < m_numBlocks)
                {
                    Debug.Assert((m_whdr[(m_curBlock + 2) % 2].dwFlags & Wave.WAVEHDR.WHDR_INQUEUE) != 0,
                        "ERROR: A sound block finished before the subsequent block was written.");

                    Thread loadThread = new Thread(new ThreadStart(LoadBuffer));
                    loadThread.Start();
                }
                else if (m_curBlock == m_numBlocks)
                {
                    Stop();
                }
            }

            /// <summary>
            /// Stop playing the current file and clean up all resources.
            /// </summary>
            public void Stop()
            {
                waveOutReset(m_hwo);

                m_playing = false;

                if (m_rdr != null)
                {
                    m_rdr.BaseStream.Close();
                    m_rdr.Close();
                    m_rdr = null;
                }

                for (int i = 0; i < 2; i++)
                {
                    if (m_whdr[i] != null)
                    {
                        lock (m_whdr[i])
                        {
                            if (m_hwo != IntPtr.Zero)
                                waveOutUnprepareHeader(m_hwo, m_whdr[i], (uint)Marshal.SizeOf(m_whdr[i]));

                            m_whdr[i].Dispose();
                            m_whdr[i] = null;
                        }
                    }
                }

                if (m_hwo != IntPtr.Zero)
                    waveOutClose(m_hwo);

                m_hwo = IntPtr.Zero;
                m_wfmt = null;

            }

            /// <summary>
            /// Resume the playback of a paused sound.
            /// </summary>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public Wave.MMSYSERR Resume()
            {
                return waveOutRestart(m_hwo);
            }

            /// <summary>
            /// Pause the playback of a sound.
            /// </summary>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public Wave.MMSYSERR Pause()
            {
                return waveOutPause(m_hwo);
            }

            /// <summary>
            /// Get the volume of this sound.
            /// </summary>
            /// <param name="volLeft">Left channel volume level</param>
            /// <param name="volRight">Right channel volume level</param>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public Wave.MMSYSERR GetVolume(ref ushort volLeft, ref ushort volRight)
            {
                uint vol = 0;

                Wave.MMSYSERR result = waveOutGetVolume(m_hwo, ref vol);
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                volLeft = (ushort)(vol & 0x0000ffff);
                volRight = (ushort)(vol >> 16);

                return Wave.MMSYSERR.NOERROR;
            }

            /// <summary>
            /// Sets the volume of this sound.
            /// </summary>
            /// <param name="volLeft">Left channel volume level</param>
            /// <param name="volRight">Right channel volume level</param>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public Wave.MMSYSERR SetVolume(ushort volLeft, ushort volRight)
            {
                uint vol = ((uint)volLeft & 0x0000ffff) | ((uint)volRight << 16);
                return waveOutSetVolume(m_hwo, vol);
            }

            /// <summary>
            /// Clean up all resources.
            /// </summary>
            public void Dispose()
            {
                Stop();
            }
        }

        /// <summary>
        /// Defines the MessageWindow used to receive messages from the audio
        /// system.
        /// </summary>
        public class SoundMessageWindow : MessageWindow
        {
            public const int MM_WOM_OPEN = 0x03BB;
            public const int MM_WOM_CLOSE = 0x03BC;
            public const int MM_WOM_DONE = 0x03BD;

            // Instance of a playback interface
            protected WaveOut m_wo = null;

            public SoundMessageWindow(WaveOut wo)
            {
                m_wo = wo;
            }

            protected override void WndProc(ref Message msg)
            {
                switch (msg.Msg)
                {
                    // When this message is encountered, a block is
                    // done playing, so notify the WaveOut instance.
                    case MM_WOM_DONE:
                        m_wo.BlockDone(msg.WParam);
                        break;

                }
                base.WndProc(ref msg);
            }
        }

        /// <summary>
        /// Maintain an instance of a MessageWindow that handles audio messages.
        /// </summary>
        protected SoundMessageWindow m_msgWindow = null;

        /// <summary>
        /// An instance of WaveFile used as the source for playing audio.
        /// </summary>
        protected WaveFile m_file = null;

        /// <summary>
        /// Create an instance of WaveOut.
        /// </summary>
        public WaveOut()
        {
            m_msgWindow = new SoundMessageWindow(this);
            m_file = new WaveFile();
        }

        /// <summary>
        ///  Determine the number of available playback devices.
        /// </summary>
        /// <returns>Number of output devices</returns>
        public uint NumDevices()
        {
            return (uint)waveOutGetNumDevs();
        }

        /// <summary>
        /// Get the name of the specified playback device.
        /// </summary>
        /// <param name="deviceId">ID of the device</param>
        /// <param name="prodName">Destination string assigned the name</param>
        /// <returns>MMSYSERR.NOERROR if successful</returns>
        public Wave.MMSYSERR GetDeviceName(uint deviceId, ref string prodName)
        {
            WAVEOUTCAPS caps = new WAVEOUTCAPS();
            Wave.MMSYSERR result = waveOutGetDevCaps(deviceId, caps, caps.Size);
            if (result != Wave.MMSYSERR.NOERROR)
                return result;

            prodName = caps.szPname;

            return Wave.MMSYSERR.NOERROR;
        }

        /// <summary>
        /// Specifies if playback has finished.
        /// </summary>
        /// <returns>true if no playback is in progress.</returns>
        public bool Done()
        {
            if (m_file != null)
                return m_file.Done;

            return true;
        }

        /// <summary>
        /// Pause playback.
        /// </summary>
        /// <returns>MMSYSERR.NOERROR if successful</returns>
        public Wave.MMSYSERR Pause()
        {
            if (m_file != null)
                return m_file.Pause();

            return Wave.MMSYSERR.NOERROR;
        }

        /// <summary>
        /// Resume playback of a paused sound.
        /// </summary>
        /// <returns>MMSYSERR.NOERROR if successful</returns>
        public Wave.MMSYSERR Resume()
        {
            if (m_file != null)
                return m_file.Resume();

            return Wave.MMSYSERR.NOERROR;
        }

        /// <summary>
        /// Determines the length of the audio file, in milliseconds.
        /// </summary>
        /// <returns>Millieconds</returns>
        public uint Milliseconds()
        {
            if (m_file != null)
                return m_file.Milliseconds;

            return 0;
        }

        /// <summary>
        /// Play a file.
        /// </summary>
        /// <param name="fileName">Name of file to play</param>
        /// <param name="bufferSize">Size of playback buffers</param>
        /// <param name="volLeft">Volume of left channel</param>
        /// <param name="volRight">Volume of right channel</param>
        /// <returns>MMSYSERR.NOERROR if successful</returns>
        public Wave.MMSYSERR Play(string fileName, int bufferSize, ushort volLeft, ushort volRight)
        {
            if (m_file != null)
                return m_file.Play(0, fileName, m_msgWindow.Hwnd, bufferSize, volLeft, volRight);

            return Wave.MMSYSERR.ERROR;
        }

        /// <summary>
        /// A block has finished so notify the WaveFile.  In a more complicated example,
        /// this class might maintain an array of WaveFile instances.  In such a case, the
        /// wParam of the message could be passed from the MM_WIM_DATA message.  This
        /// value represents the m_hwi member of the file that caused the message.
        /// The code might look something like:
        /// foreach (WaveFile f in m_files)
        /// {
        ///		if (f.m_hwi.ToInt32() == wParam.ToInt32())
        ///		{
        ///			f.BlockDone();
        ///			break;
        ///		}
        /// }
        /// </summary>
        public void BlockDone(IntPtr hwo)
        {
            if (m_file != null)
                m_file.BlockDone();
        }

        /// <summary>
        /// Clean up an allocated resources.
        /// </summary>
        public void Dispose()
        {
            if (m_msgWindow != null)
                m_msgWindow.Dispose();

            if (m_file != null)
                m_file.Dispose();
        }

        /// <summary>
        /// This function retrieves the number of waveform output devices present
        /// in the system.
        /// </summary>
        /// <returns>The number of devices indicates success. Zero indicates that
        /// no devices are present or that an error occurred.</returns>
        [DllImport("coredll.dll")]
        protected static extern int waveOutGetNumDevs();

        /// <summary>
        /// This function opens a specified waveform output device for playback.
        /// </summary>
        /// <param name="phwo">Address filled with a handle identifying the open
        /// waveform-audio output device. Use the handle to identify the device
        /// when calling other waveform-audio output functions. This parameter might
        /// be NULL if the WAVE_FORMAT_QUERY flag is specified for fdwOpen.</param>
        /// <param name="uDeviceID">Identifier of the waveform-audio output device to
        /// open. It can be either a device identifier or a Handle to an open
        /// waveform-audio input device.</param>
        /// <param name="pwfx">Pointer to a WAVEFORMATEX structure that identifies
        /// the format of the waveform-audio data to be sent to the device. You can
        /// free this structure immediately after passing it to waveOutOpen.</param>
        /// <param name="dwCallback">Specifies the address of a fixed callback function,
        /// an event handle, a handle to a window, or the identifier of a thread to be
        /// called during waveform-audio playback to process messages related to the
        /// progress of the playback. If no callback function is required, this value
        /// can be zero.</param>
        /// <param name="dwInstance">Specifies user-instance data passed to the
        /// callback mechanism. This parameter is not used with the window callback
        /// mechanism.</param>
        /// <param name="fdwOpen">Flags for opening the device.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        private static extern Wave.MMSYSERR waveOutOpen(ref IntPtr phwo, uint uDeviceID, Wave.WAVEFORMATEX pwfx, IntPtr dwCallback, uint dwInstance, uint fdwOpen);

        /// <summary>
        /// This function queries the current volume setting of a waveform output device.
        /// </summary>
        /// <param name="hwo">Handle to an open waveform-audio output device. This
        /// parameter can also be a device identifier.</param>
        /// <param name="pdwVolume">Pointer to a variable to be filled with the current
        /// volume setting. The low-order word of this location contains the left-channel
        /// volume setting, and the high-order word contains the right-channel setting.
        /// A value of 0xFFFF represents full volume, and a value of 0x0000 is silence. 
        /// If a device does not support both left and right volume control, the low-order
        /// word of the specified location contains the mono volume level.
        /// The full 16-bit setting(s) set with the waveOutSetVolume function is returned,
        /// regardless of whether the device supports the full 16 bits of volume-level
        /// control.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveOutGetVolume(IntPtr hwo, ref uint pdwVolume);

        /// <summary>
        /// This function sets the volume of a waveform output device.
        /// </summary>
        /// <param name="hwo">Handle to an open waveform-audio output device. This
        /// parameter can also be a device identifier.</param>
        /// <param name="dwVolume">Specifies a new volume setting. The low-order word
        /// contains the left-channel volume setting, and the high-order word contains
        /// the right-channel setting. A value of 0xFFFF represents full volume, and a
        /// value of 0x0000 is silence. 
        /// If a device does not support both left and right volume control, the low-
        /// order word of dwVolume specifies the volume level, and the high-order word
        /// is ignored.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveOutSetVolume(IntPtr hwo, uint dwVolume);

        /// <summary>
        /// This function prepares a waveform data block for playback.
        /// </summary>
        /// <param name="hwo">Handle to the waveform-audio output device.</param>
        /// <param name="pwh">Pointer to a WAVEHDR structure that identifies the data
        /// block to be prepared. The buffer's base address must be aligned with the
        /// respect to the sample size.</param>
        /// <param name="cbwh">Size, in bytes, of the WAVEHDR structure.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        private static extern Wave.MMSYSERR waveOutPrepareHeader(IntPtr hwo, Wave.WAVEHDR pwh, uint cbwh);

        /// <summary>
        /// This function sends a data block to the specified waveform output device.
        /// </summary>
        /// <param name="hwo">Handle to the waveform-audio output device.</param>
        /// <param name="pwh">Pointer to a WAVEHDR structure containing information
        /// about the data block.</param>
        /// <param name="cbwh">Size, in bytes, of the WAVEHDR structure.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        private static extern Wave.MMSYSERR waveOutWrite(IntPtr hwo, Wave.WAVEHDR pwh, uint cbwh);

        /// <summary>
        /// This function cleans up the preparation performed by waveOutPrepareHeader.
        /// The function must be called after the device driver is finished with a data
        /// block. You must call this function before freeing the data buffer.
        /// </summary>
        /// <param name="hwo">Handle to the waveform-audio output device.</param>
        /// <param name="pwh">Pointer to a WAVEHDR structure identifying the data block
        /// to be cleaned up.</param>
        /// <param name="cbwh">Size, in bytes, of the WAVEHDR structure.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        private static extern Wave.MMSYSERR waveOutUnprepareHeader(IntPtr hwo, Wave.WAVEHDR pwh, uint cbwh);

        /// <summary>
        /// This function closes the specified waveform output device.
        /// </summary>
        /// <param name="hwo">Handle to the waveform-audio output device. If the function
        /// succeeds, the handle is no longer valid after this call.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveOutClose(IntPtr hwo);

        /// <summary>
        /// This function stops playback on a specified waveform output device and
        /// resets the current position to 0. All pending playback buffers are marked
        /// as done and returned to the application.
        /// </summary>
        /// <param name="hwo">Handle to the waveform-audio output device.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveOutReset(IntPtr hwo);

        /// <summary>
        /// This function pauses playback on a specified waveform output device. The
        /// current playback position is saved. Use waveOutRestart to resume playback
        /// from the current playback position.
        /// </summary>
        /// <param name="hwo">Handle to the waveform-audio output device.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveOutPause(IntPtr hwo);

        /// <summary>
        /// This function restarts a paused waveform output device.
        /// </summary>
        /// <param name="hwo">Handle to the waveform-audio output device.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveOutRestart(IntPtr hwo);

        /// <summary>
        /// This structure contains timing information for different types of
        /// multimedia data.
        ///		typedef struct mmtime_tag 
        ///		{
        ///			UINT wType; 
        ///			union 
        ///			{
        ///				DWORD ms; 
        ///				DWORD sample; 
        ///				DWORD cb; 
        ///				DWORD ticks; 
        ///				struct 
        ///				{
        ///					BYTE hour; 
        ///					BYTE min; 
        ///					BYTE sec; 
        ///					BYTE frame; 
        ///					BYTE fps; 
        ///					BYTE dummy; 
        ///					BYTE pad[2]
        ///				} smpte; 
        ///				struct 
        ///				{
        ///					DWORD songptrpos;
        ///				} midi; 
        ///			} u; 
        ///		} MMTIME;
        /// </summary>
        protected class MMTIME
        {
            /// <summary>
            /// Time format.
            /// </summary>
            public uint wType = 0;
            /// <summary>
            /// Byte count. Used when wType is TIME_BYTES.
            /// </summary>
            public uint cb = 0;

            // Padding because this is actually a union
            public uint pad = 0;
        }

        // Used by MMTIME.wType
        protected const uint TIME_MS = 0x0001;
        protected const uint TIME_SAMPLES = 0x0002;
        protected const uint TIME_BYTES = 0x0004;
        protected const uint TIME_TICKS = 0x0020;

        /// <summary>
        /// This function retrieves the current playback position of the specified
        /// waveform output device.
        /// </summary>
        /// <param name="hwo">Handle to the waveform-audio output device.</param>
        /// <param name="pmmt">Pointer to an MMTIME structure.</param>
        /// <param name="cbmmt">Size, in bytes, of the MMTIME structure.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveOutGetPosition(IntPtr hwo, MMTIME pmmt, uint cbmmt);

        /// <summary>
        /// This structure describes the capabilities of a waveform-audio output device.
        ///		typedef struct 
        ///		{
        ///			WORD wMid; 
        ///			WORD wPid; 
        ///			MMVERSION vDriverVersion; 
        ///			TCHAR szPname[MAXPNAMELEN]; 
        ///			DWORD dwFormats; 
        ///			WORD wChannels; 
        ///			WORD wReserved1; 
        ///			DWORD dwSupport;} 
        ///		WAVEOUTCAPS;
        ///	This structure has an embedded TCHAR array so the managed implementation is
        ///	a byte array with accessors.
        /// </summary>
        protected class WAVEOUTCAPS
        {
            const uint WAVEOUTCAPS_SIZE = 84;

            private byte[] m_data = null;
            public uint Size { get { return (uint)WAVEOUTCAPS_SIZE; } }

            /// <summary>
            /// Used by dwSupport in WAVEOUTCAPS
            /// Supports pitch control
            /// </summary>
            public const uint WAVECAPS_PITCH = 0x0001;
            /// <summary>
            /// Used by dwSupport in WAVEOUTCAPS
            /// Supports playback rate control
            /// </summary>
            public const uint WAVECAPS_PLAYBACKRATE = 0x0002;
            /// <summary>
            /// Used by dwSupport in WAVEOUTCAPS
            /// Supports volume control
            /// </summary>
            public const uint WAVECAPS_VOLUME = 0x0004;
            /// <summary>
            /// Used by dwSupport in WAVEOUTCAPS
            /// Supports separate left-right volume control
            /// </summary>
            public const uint WAVECAPS_LRVOLUME = 0x0008;

            /// <summary>
            /// Manufacturer identifier for the device driver for the device.
            /// Manufacturer identifiers are defined in Manufacturer and Product
            /// Identifiers.
            /// </summary>
            public ushort wMid { get { return BitConverter.ToUInt16(m_data, 0); } }
            /// <summary>
            /// Product identifier for the device. Product identifiers are defined in
            /// Manufacturer and Product Identifiers.
            /// </summary>
            public ushort wPid { get { return BitConverter.ToUInt16(m_data, 2); } }
            /// <summary>
            /// Version number of the device driver for the device. The high-order byte
            /// is the major version number, and the low-order byte is the minor version
            /// number.
            /// </summary>
            public uint vDriverVersion { get { return BitConverter.ToUInt32(m_data, 4); } }
            /// <summary>
            /// Specifies the standard formats that are supported.
            /// </summary>
            public uint dwFormats { get { return BitConverter.ToUInt32(m_data, 72); } }
            /// <summary>
            /// Number specifying whether the device supports mono (1) or stereo (2)
            /// output.
            /// </summary>
            public ushort wChannels { get { return BitConverter.ToUInt16(m_data, 76); } }
            /// <summary>
            /// Packing.
            /// </summary>
            public ushort wReserved1 { get { return BitConverter.ToUInt16(m_data, 78); } }
            /// <summary>
            /// Specifies the optional functionality supported by the device.
            /// </summary>
            public uint dwSupport { get { return BitConverter.ToUInt16(m_data, 80); } }
            /// <summary>
            /// Null-terminated string that contains the product name.
            /// </summary>
            public string szPname
            {
                get
                {
                    char[] bytes = new char[32];
                    for (int i = 0; i < 32; i++)
                    {
                        bytes[i] = (char)BitConverter.ToUInt16(m_data, i * 2 + 8);
                    }

                    return new string(bytes);
                }
            }

            public WAVEOUTCAPS()
            {
                m_data = new byte[WAVEOUTCAPS_SIZE];
            }

            public static implicit operator byte[](WAVEOUTCAPS caps)
            {
                return caps.m_data;
            }
        }

        /// <summary>
        /// This function queries a specified waveform device to determine its
        /// capabilities.
        /// </summary>
        /// <param name="uDeviceID">Identifier of the waveform-audio output device.
        /// It can be either a device identifier or a Handle to an open waveform-audio
        /// output device.</param>
        /// <param name="pwoc">Pointer to a WAVEOUTCAPS structure to be filled with
        /// information about the capabilities of the device.</param>
        /// <param name="cbwoc">Size, in bytes, of the WAVEOUTCAPS structure.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveOutGetDevCaps(uint uDeviceID, byte[] pwoc, uint cbwoc);

        /// <summary>
        /// Run a test of the WaveOut class.
        /// </summary>
        /// <param name="showLine">Delegate called to show debug information</param>
        
    }
}
