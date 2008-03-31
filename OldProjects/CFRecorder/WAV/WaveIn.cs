using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.WindowsCE.Forms;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace CFRecorder
{
    /// <summary>
    /// Encapsulates Waveform Audio Interface recording functions and provides a simple
    /// interface for recording audio.
    /// </summary>
    public class WaveIn
    {
        /// <summary>
        /// Supplies an inteface for creating, recording to, and saving a .wav file.
        /// </summary>
        protected class WaveFile : IDisposable
        {
            /// <summary>
            /// Hardware interface instance for this wave file.
            /// </summary>
            protected IntPtr m_hwi = IntPtr.Zero;

            /// <summary>
            /// Instance of the WAVEFORMATEX header for this file.
            /// </summary>
            protected Wave.WAVEFORMATEX m_wfmt = null;

            /// <summary>
            /// Buffers used to store recording information.
            /// </summary>
            protected Wave.WAVEHDR[] m_whdr = null;

            /// <summary>
            /// Specifies whether the file is inited or not.
            /// </summary>
            protected bool m_inited = false;

            /// <summary>
            /// Current block, or buffer in m_whdr, being written.
            /// </summary>
            protected int m_curBlock;

            /// <summary>
            /// Maximum number of blocks to be written.
            /// </summary>
            protected int m_numBlocks;

            /// <summary>
            /// Size of each record buffer.
            /// </summary>
            protected uint m_bufferSize;

            /// <summary>
            /// Maximum size of all record buffers.
            /// </summary>
            protected uint m_maxDataLength;

            /// <summary>
            /// Specifies whether or not recording is done.
            /// </summary>
            public bool Done { get { return !m_recording; } }
            protected bool m_recording = false;

            /// <summary>
            /// Preload the buffers and prepare them for recording.
            /// </summary>
            /// <param name="curDevice">Device to use for recording</param>
            /// <param name="hwnd">Handle to a message window that will receive
            /// audio messages.</param>
            /// <param name="maxRecordLength_ms">Maximum length of recording</param>
            /// <param name="bufferSize">Size of buffers to use for recording.  New
            /// buffers are added when needed until the maximum length is reached
            /// or the recording is stopped.</param>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public Wave.MMSYSERR Preload(uint curDevice, IntPtr hwnd, int maxRecordLength_ms, int bufferSize)
            {
                // Do not allow recording to be interrupted
                if (m_recording)
                    return Wave.MMSYSERR.ERROR;

                // If this file is already initialized then start over
                if (m_inited)
                {
                    Stop();
                    FreeWaveBuffers();
                }

                // Create an instance of WAVEINCAPS to check if our desired
                // format is supported
                WAVEINCAPS caps = new WAVEINCAPS();
                waveInGetDevCaps(0, caps, caps.Size);
                if ((caps.dwFormats & Wave.WAVE_FORMAT_1S08) == 0)
                    return Wave.MMSYSERR.NOTSUPPORTED;

                // Initialize a WAVEFORMATEX structure specifying the desired
                // format
                m_wfmt = new Wave.WAVEFORMATEX();
                m_wfmt.wFormatTag = Wave.WAVEHDR.WAVE_FORMAT_PCM;
                m_wfmt.wBitsPerSample = 16;
                m_wfmt.nChannels = 1;
                m_wfmt.nSamplesPerSec = 22050;
                m_wfmt.nAvgBytesPerSec = (uint)(m_wfmt.nSamplesPerSec * m_wfmt.nChannels * (m_wfmt.wBitsPerSample / 8));
                m_wfmt.nBlockAlign = (ushort)(m_wfmt.wBitsPerSample * m_wfmt.nChannels / 8);

                // Attempt to open the specified device with the desired wave format
                Wave.MMSYSERR result = waveInOpen(ref m_hwi, curDevice, m_wfmt, hwnd, 0, Wave.CALLBACK_WINDOW);
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                if (bufferSize == 0)
                    return Wave.MMSYSERR.ERROR;

                m_bufferSize = (uint)bufferSize;

                // Force the buffers to align to nBlockAlign
                if (m_bufferSize % m_wfmt.nBlockAlign != 0)
                    m_bufferSize += m_wfmt.nBlockAlign - (m_bufferSize % m_wfmt.nBlockAlign);

                // Determine the number of buffers needed to record the maximum length
                m_maxDataLength = (uint)(m_wfmt.nAvgBytesPerSec * maxRecordLength_ms / 1000);
                m_numBlocks = (int)(m_maxDataLength / m_bufferSize);
                if (m_numBlocks * m_bufferSize < m_maxDataLength)
                    m_numBlocks++;

                // Allocate the list of buffers
                m_whdr = new Wave.WAVEHDR[m_numBlocks + 1];

                // Allocate and initialize two buffers to start with
                m_whdr[0] = new Wave.WAVEHDR();
                m_whdr[1] = new Wave.WAVEHDR();

                result = InitBuffer(0);
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                result = InitBuffer(1);
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                m_curBlock = 0;
                m_inited = true;

                return Wave.MMSYSERR.NOERROR;
            }

            /// <summary>
            /// Called when the Windows message specifying that a block has finished
            /// recording is encountered.  It is critical that the application stay
            /// one buffer ahead because at the point this function is called, the system
            /// has already started writing to the next buffer.
            /// </summary>
            public void BlockDone()
            {
                m_curBlock++;

                // If the next block is not the padding buffer at the end of the
                // recording then initialize another buffer, otherwise stop recording
                if (m_curBlock < m_numBlocks)
                {
                    InitBuffer(m_curBlock + 1);
                }
                else if (m_curBlock == m_numBlocks)
                {
                    Stop();
                }
            }

            /// <summary>
            /// Initialize a buffer for recording by allocating a data buffer,
            /// preparing the header, and adding it to the read queue.
            /// </summary>
            /// <param name="bufIndex">Index of buffer to allocate</param>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public Wave.MMSYSERR InitBuffer(int bufIndex)
            {
                // Determine the size of the buffer to create
                uint writeLength = (uint)m_bufferSize;
                if (bufIndex < m_numBlocks)
                {
                    uint remainingDataLength = (uint)(m_maxDataLength - bufIndex * m_bufferSize);
                    if (m_bufferSize > remainingDataLength)
                        writeLength = remainingDataLength;
                }

                // If the header is not already instanced then instance it
                if (m_whdr[bufIndex] == null)
                    m_whdr[bufIndex] = new Wave.WAVEHDR();

                // Allocate memory if not already allocated
                Wave.MMSYSERR result = m_whdr[bufIndex].Init(writeLength, false);
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                // Prepare the header
                result = waveInPrepareHeader(m_hwi, m_whdr[bufIndex], (uint)Marshal.SizeOf(m_whdr[bufIndex]));
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                // Put the buffer in the queue
                return waveInAddBuffer(m_hwi, m_whdr[bufIndex], (uint)Marshal.SizeOf(m_whdr[bufIndex]));
            }

            /// <summary>
            /// Start recording.
            /// </summary>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public Wave.MMSYSERR Start()
            {
                if (!m_inited || m_recording)
                    return Wave.MMSYSERR.ERROR;

                Wave.MMSYSERR result = waveInStart(m_hwi);
                if (result != Wave.MMSYSERR.NOERROR)
                    return result;

                m_recording = true;

                return Wave.MMSYSERR.NOERROR;
            }

            /// <summary>
            /// Free the buffers allocated for recording.  This is not done when
            /// recording stops because the user needs a chance to save or reject
            /// the recording before trying again.
            /// </summary>
            private void FreeWaveBuffers()
            {
                m_inited = false;

                if (m_whdr != null)
                {
                    for (int i = 0; i < m_whdr.Length; i++)
                    {
                        if (m_whdr[i] != null)
                        {
                            waveInUnprepareHeader(m_hwi, m_whdr[i], (uint)Marshal.SizeOf(m_whdr[i]));

                            m_whdr[i].Dispose();
                            m_whdr[i] = null;
                        }
                    }

                    m_whdr = null;
                }

                waveInClose(m_hwi);

                m_hwi = IntPtr.Zero;
            }

            /// <summary>
            /// Write individual characters to the BinaryWriter.  This is a helper
            /// function used by Save.  Writing strings does not work because they
            /// write a length before the characters.
            /// </summary>
            /// <param name="wrtr">Destination of write</param>
            /// <param name="text">Characters to be written</param>
            private void WriteChars(BinaryWriter wrtr, string text)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char c = (char)text[i];
                    wrtr.Write(c);
                }
            }

            /// <summary>
            /// Save the current record buffers to the specified file.
            /// </summary>
            /// <param name="fileName">Name of file to be saved</param>
            /// <returns>MMSYSERR.NOERROR if successful</returns>
            public Wave.MMSYSERR Save(string fileName)
            {
                if (!m_inited)
                    return Wave.MMSYSERR.ERROR;

                if (m_recording)
                    Stop();

                FileStream strm = null;
                BinaryWriter wrtr = null;

                try
                {
                    if (File.Exists(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if ((fi.Attributes & FileAttributes.ReadOnly) != 0)
                            fi.Attributes -= FileAttributes.ReadOnly;

                        strm = new FileStream(fileName, FileMode.Truncate);
                    }
                    else
                    {
                        strm = new FileStream(fileName, FileMode.Create);
                    }

                    if (strm == null)
                        return Wave.MMSYSERR.ERROR;

                    wrtr = new BinaryWriter(strm);
                    if (wrtr == null)
                        return Wave.MMSYSERR.ERROR;

                    // Determine the size of the data, as the total number of bytes
                    // recorded by each buffer
                    uint totalSize = 0;
                    for (int i = 0; i < m_numBlocks; i++)
                    {
                        if (m_whdr[i] != null)
                            totalSize += m_whdr[i].dwBytesRecorded;
                    }

                    int chunkSize = (int)(36 + totalSize);

                    // Write out the header information
                    WriteChars(wrtr, "RIFF");
                    wrtr.Write(chunkSize);
                    WriteChars(wrtr, "WAVEfmt ");
                    wrtr.Write((int)16);
                    m_wfmt.Write(wrtr);
                    WriteChars(wrtr, "data");
                    wrtr.Write((int)totalSize);

                    // Write the data recorded to each buffer
                    for (int i = 0; i < m_numBlocks; i++)
                    {
                        if (m_whdr[i] != null)
                        {
                            Wave.MMSYSERR result = m_whdr[i].Write(wrtr);
                            if (result != Wave.MMSYSERR.NOERROR)
                                return result;
                        }
                    }

                    return Wave.MMSYSERR.NOERROR;
                }
                finally
                {
                    FreeWaveBuffers();

                    if (strm != null)
                        strm.Close();

                    if (wrtr != null)
                        wrtr.Close();
                }
            }

            /// <summary>
            /// Stop recording.  After stopping, the buffers are kept until recording
            /// is restarted or the buffers are saved.
            /// </summary>
            public void Stop()
            {
                waveInReset(m_hwi);

                m_recording = false;
            }

            /// <summary>
            /// Clean up any allocated resources.
            /// </summary>
            public void Dispose()
            {
                Stop();

                FreeWaveBuffers();
            }
        }

        /// <summary>
        /// Defines the MessageWindow used to receive messages from the audio
        /// system.
        /// </summary>
        protected class SoundMessageWindow : MessageWindow
        {
            public const int MM_WIM_OPEN = 0x3BE;
            public const int MM_WIM_CLOSE = 0x3BF;
            public const int MM_WIM_DATA = 0x3C0;

            // Instance of a recording interface
            protected WaveIn m_wi = null;

            public SoundMessageWindow(WaveIn wi)
            {
                m_wi = wi;
            }

            protected override void WndProc(ref Message msg)
            {
                switch (msg.Msg)
                {
                    // When this message is encountered, a block is
                    // done recording, so notify the WaveIn instance.
                    case MM_WIM_DATA:
                        {
                            if (m_wi != null)
                                m_wi.BlockDone();
                        }
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
        /// An instance of WaveFile used as the destination for recording audio.
        /// </summary>
        protected WaveFile m_file = null;

        /// <summary>
        /// Create an instance of WaveIn.
        /// </summary>
        public WaveIn()
        {
            m_msgWindow = new SoundMessageWindow(this);
            m_file = new WaveFile();
        }

        /// <summary>
        ///  Determine the number of available recording devices.
        /// </summary>
        /// <returns>Number of input devices</returns>
        public uint NumDevices()
        {
            return (uint)waveInGetNumDevs();
        }

        /// <summary>
        /// Get the name of the specified recording device.
        /// </summary>
        /// <param name="deviceId">ID of the device</param>
        /// <param name="prodName">Destination string assigned the name</param>
        /// <returns>MMSYSERR.NOERROR if successful</returns>
        public Wave.MMSYSERR GetDeviceName(uint deviceId, ref string prodName)
        {
            WAVEINCAPS caps = new WAVEINCAPS();
            Wave.MMSYSERR result = waveInGetDevCaps(deviceId, caps, caps.Size);
            if (result != Wave.MMSYSERR.NOERROR)
                return result;

            prodName = caps.szPname;

            return Wave.MMSYSERR.NOERROR;
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
        public void BlockDone()
        {
            m_file.BlockDone();
        }

        /// <summary>
        /// Preload the buffers of the record file.
        /// </summary>
        /// <param name="maxRecordLength_ms">Maximum record length in milliseconds</param>
        /// <param name="bufferSize">Size of individual buffers, in bytes</param>
        /// <returns>MMSYSERR.NOERROR if successful</returns>
        public Wave.MMSYSERR Preload(int maxRecordLength_ms, int bufferSize)
        {
            if (m_file != null)
                return m_file.Preload(0, m_msgWindow.Hwnd, maxRecordLength_ms, bufferSize);

            return Wave.MMSYSERR.NOERROR;
        }

        /// <summary>
        /// Stop recording.
        /// </summary>
        public void Stop()
        {
            if (m_file != null)
                m_file.Stop();
        }

        /// <summary>
        /// Start recording.
        /// </summary>
        /// <returns>MMSYSERR.NOERROR if successful</returns>
        public Wave.MMSYSERR Start()
        {
            if (m_file != null)
                return m_file.Start();

            return Wave.MMSYSERR.NOERROR;
        }

        /// <summary>
        /// Checks if the recording time is expired.
        /// </summary>
        /// <returns>true if not currently recording</returns>
        public bool Done()
        {
            return m_file.Done;
        }

        /// <summary>
        /// Save the WaveFile buffers to the specified file.
        /// </summary>
        /// <param name="fileName">Name of destination file</param>
        /// <returns>MMSYSERR.NOERROR if successful</returns>
        public Wave.MMSYSERR Save(string fileName)
        {
            if (m_file != null)
                return m_file.Save(fileName);

            return Wave.MMSYSERR.NOERROR;
        }

        /// <summary>
        /// Clean up any resources allocated by the class.
        /// </summary>
        public void Dispose()
        {
            m_msgWindow.Dispose();

            if (m_file != null)
                m_file.Dispose();
        }

        /// <summary>
        /// This function retrieves the number of waveform input devices present in the system.
        /// </summary>
        /// <returns>The number of devices indicates success. Zero indicates that no devices are present or that an error occurred.</returns>
        [DllImport("coredll.dll")]
        protected static extern int waveInGetNumDevs();

        /// <summary>
        /// This function opens a specified waveform input device for recording.
        /// </summary>
        /// <param name="phwi">Address filled with a handle identifying the open
        /// waveform-audio input device. Use this handle to identify the device when
        /// calling other waveform-audio input functions. This parameter can be NULL
        /// if WAVE_FORMAT_QUERY is specified for fdwOpen.</param>
        /// <param name="uDeviceID">Identifier of the waveform-audio input device to open.
        /// It can be either a device identifier or a Handle to an open waveform-audio
        /// input device. Can also be WAVE_MAPPER.</param>
        /// <param name="pwfx">Pointer to a WAVEFORMATEX structure that identifies the
        /// desired format for recording waveform-audio data. You can free this structure
        /// immediately after waveInOpen returns.</param>
        /// <param name="dwCallback">Specifies the address of a fixed callback function,
        /// an event handle, a handle to a window, or the identifier of a thread to be
        /// called during waveform-audio recording to process messages related to the
        /// progress of recording. If no callback function is required, this value can be
        /// zero.</param>
        /// <param name="dwInstance">Specifies user-instance data passed to the callback
        /// mechanism. This parameter is not used with the window callback mechanism.</param>
        /// <param name="fdwOpen">Flags for opening the device.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        private static extern Wave.MMSYSERR waveInOpen(ref IntPtr phwi, uint uDeviceID, Wave.WAVEFORMATEX pwfx, IntPtr dwCallback, uint dwInstance, uint fdwOpen);

        /// <summary>
        /// This function prepares a buffer for waveform input.
        /// </summary>
        /// <param name="hwi">Handle to the waveform-audio input device.</param>
        /// <param name="pwh">Pointer to a WAVEHDR structure that identifies the buffer
        /// to be prepared. The buffer's base address must be aligned with the respect
        /// to the sample size.</param>
        /// <param name="cbwh">Size, in bytes, of the WAVEHDR structure.</param>
        /// <returns></returns>
        [DllImport("coredll.dll")]
        private static extern Wave.MMSYSERR waveInPrepareHeader(IntPtr hwi, Wave.WAVEHDR pwh, uint cbwh);

        /// <summary>
        /// This function cleans up the preparation performed by waveInPrepareHeader.
        /// The function must be called after the device driver fills a data buffer
        /// and returns it to the application. You must call this function before
        /// freeing the data buffer.
        /// </summary>
        /// <param name="hwi">Handle to the waveform-audio input device.</param>
        /// <param name="pwh">Pointer to a WAVEHDR structure identifying the buffer to
        /// be cleaned up.</param>
        /// <param name="cbwh">Size, in bytes, of the WAVEHDR structure.</param>
        /// <returns></returns>
        [DllImport("coredll.dll")]
        private static extern Wave.MMSYSERR waveInUnprepareHeader(IntPtr hwi, Wave.WAVEHDR pwh, uint cbwh);

        /// <summary>
        /// This function closes the specified waveform-audio input device.
        /// </summary>
        /// <param name="hwi">Handle to the waveform-audio input device. If the
        /// function succeeds, the handle is no longer valid after this call.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveInClose(IntPtr hwi);

        /// <summary>
        /// This function stops input on a specified waveform input device and resets
        /// the current position to 0. All pending buffers are marked as done and
        /// returned to the application.
        /// </summary>
        /// <param name="hwi">Handle to the waveform-audio input device.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveInReset(IntPtr hwi);

        /// <summary>
        /// This function starts input on the specified waveform input device.
        /// </summary>
        /// <param name="hwi">Handle to the waveform-audio input device.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveInStart(IntPtr hwi);

        /// <summary>
        /// This function stops waveform input.
        /// </summary>
        /// <param name="hwi">Handle to the waveform-audio input device.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveInStop(IntPtr hwi);

        /// <summary>
        /// This function sends an input buffer to the specified waveform-audio input
        /// device. When the buffer is filled, the application is notified.
        /// </summary>
        /// <param name="hwi">Handle to the waveform-audio input device.</param>
        /// <param name="pwh">Pointer to a WAVEHDR structure that identifies
        /// the buffer.</param>
        /// <param name="cbwh">Size, in bytes, of the WAVEHDR structure.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        private static extern Wave.MMSYSERR waveInAddBuffer(IntPtr hwi, Wave.WAVEHDR pwh, uint cbwh);

        /// <summary>
        /// This structure describes the capabilities of a waveform-audio input device.
        ///		typedef struct 
        ///		{
        ///			WORD wMid; 
        ///			WORD wPid; 
        ///			MMVERSION vDriverVersion; 
        ///			TCHAR szPname[MAXPNAMELEN]; 
        ///			DWORD dwFormats; 
        ///			WORD wChannels; 
        ///			WORD wReserved1;} 
        ///		WAVEINCAPS;
        ///	This structure has an embedded TCHAR array so the managed implementation is
        ///	a byte array with accessors.
        /// </summary>
        protected class WAVEINCAPS
        {
            const uint WAVEINCAPS_SIZE = 80;

            private byte[] m_data = null;
            public uint Size { get { return (uint)WAVEINCAPS_SIZE; } }

            /// <summary>
            /// Manufacturer identifier for the device driver for the waveform-audio
            /// input device.
            /// </summary>
            public ushort wMid { get { return BitConverter.ToUInt16(m_data, 0); } }
            /// <summary>
            /// Product identifier for the waveform-audio input device. Product
            /// identifiers are defined in Manufacturer and Product Identifiers.
            /// </summary>
            public ushort wPid { get { return BitConverter.ToUInt16(m_data, 2); } }
            /// <summary>
            /// Version number of the device driver for the waveform-audio input device.
            /// The high-order byte is the major version number, and the low-order byte
            /// is the minor version number.
            /// </summary>
            public uint vDriverVersion { get { return BitConverter.ToUInt32(m_data, 4); } }
            /// <summary>
            /// Specifies the standard formats that are supported. It is one or
            /// a combination of the following flags.
            /// </summary>
            public uint dwFormats { get { return BitConverter.ToUInt32(m_data, 72); } }
            /// <summary>
            /// Number that specifies whether the device supports mono (1) or stereo (2)
            /// input.
            /// </summary>
            public ushort wChannels { get { return BitConverter.ToUInt16(m_data, 76); } }
            /// <summary>
            /// Padding.
            /// </summary>
            public ushort wReserved1 { get { return BitConverter.ToUInt16(m_data, 78); } }

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

            public WAVEINCAPS()
            {
                m_data = new byte[WAVEINCAPS_SIZE];
            }

            public static implicit operator byte[](WAVEINCAPS caps)
            {
                return caps.m_data;
            }

        }

        /// <summary>
        /// This function retrieves the capabilities of a specified waveform-audio
        /// input device.
        /// </summary>
        /// <param name="uDeviceID">Identifier of the waveform-audio output device. It
        /// can be either a device identifier or a Handle to an open waveform-audio
        /// input device.</param>
        /// <param name="pwic">Pointer to a WAVEINCAPS structure to be filled with
        /// information about the capabilities of the device.</param>
        /// <param name="cbwic">Size, in bytes, of the WAVEINCAPS structure.</param>
        /// <returns>MMSYSERR</returns>
        [DllImport("coredll.dll")]
        protected static extern Wave.MMSYSERR waveInGetDevCaps(uint uDeviceID, byte[] pwic, uint cbwic);

        /// <summary>
        /// Run a test of the WaveIn class.
        /// </summary>
        /// <param name="showLine">Delegate called to show debug information</param>

    }
}
