using System;
using System.IO;

namespace TowseyLib
{
    public class WavWriter
    {

        /// <summary>
        /// The basic WAV file format follows the Interchange File Format specification. 
        /// An IFF file consists of a series of "chunks" where chunks can contain other chunks. 
        /// Each chunk starts with an eight byte header: four bytes describing the chunk, 
        /// followed by four bytes giving the size of the chunk (not counting the eight byte header). 
        /// The header is followed by the given number of bytes of data in a chunk-specific format. 
        /// A WAV file consists of one main chunk called RIFF that contains three things: the string "WAVE", 
        /// a "format" chunk that describes the sample rate, etc, and a "data" chunk that contains the sampled waveform.
        /// This code does NOT use any advanced WAV file features like cue points or playlists or compression. 
        /// It just dumps some data.
        /// Play it with the WAV file player. It uses CD quality audio -- 44100 samples per second, each one with 16 bits per sample.
        /// Unlike a CD, do this in mono, not stereo.
        /// </summary>
        /// <param name="signal">IMPORTANT: The signal values must be in range of signed 16 bit integer ie -32768 to +32768</param>
        /// <param name="samplesPerSecond"></param>
        /// <param name="path"></param>
        public static void WriteWavFile(double[] signal, int samplesPerSecond, string path)
        {

            //int samples = 88200 * 4;
            //short[] perfect5th = Perfect5th(samples, samplesPerSecond);

            int samples = signal.Length;


            FileStream stream = new FileStream(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            //initialise the header values
            int RIFF = 0x46464952;
            int WAVE = 0x45564157;
            int formatChunkSize = 16;
            int headerSize = 8;
            int format = 0x20746D66;
            short formatType = 1;
            short tracks = 1;
            short bitsPerSample = 16;
            short frameSize = (short)(tracks * ((bitsPerSample + 7) / 8));
            int bytesPerSecond = samplesPerSecond * frameSize;
            int waveSize = 4;
            int data = 0x61746164;
            int dataChunkSize = samples * frameSize;

            //calculate size of file
            int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;

            //write the header
            writer.Write(RIFF);
            writer.Write(fileSize);
            writer.Write(WAVE);
            writer.Write(format);
            writer.Write(formatChunkSize);
            writer.Write(formatType);
            writer.Write(tracks);
            writer.Write(samplesPerSecond);
            writer.Write(bytesPerSecond);
            writer.Write(frameSize);
            writer.Write(bitsPerSample);
            writer.Write(data);
            writer.Write(dataChunkSize);

            //perfect5th tones
            //for (int i = 0; i < samples; i++) writer.Write(perfect5th[i]);
            //write the signal
            for (int i = 0; i < samples; i++) writer.Write((short)signal[i]);//converts double to signed 16 bit

            writer.Close();
            stream.Close();
        }

        public static short[] Perfect5th(int samples, int samplesPerSecond)
        {
            double aNatural = 220.0; //A below middle C, octave below concert A
            double ampl = 10000;
            double perfect = 1.5;
            double concert = 1.498307077;

            short[] data = new short[samples];
            int id = 0;

            double freq = aNatural * perfect;
            for (int i = 0; i < samples / 4; i++) //perfect 5th
            {
                double t = (double)i / (double)samplesPerSecond;
                short s = (short)(ampl * (Math.Sin(t * freq * 2.0 * Math.PI)));
                data[id++] = s;
                //writer.Write(s);
            }
            freq = aNatural * concert;
            for (int i = 0; i < samples / 4; i++)//concert 5th
            {
                double t = (double)i / (double)samplesPerSecond;
                short s = (short)(ampl * (Math.Sin(t * freq * 2.0 * Math.PI)));
                data[id++] = s;

                //                writer.Write(s);
            }
            for (int i = 0; i < samples / 4; i++)
            {
                double t = (double)i / (double)samplesPerSecond;
                short s = (short)(ampl * (Math.Sin(t * freq * 2.0 * Math.PI) + Math.Sin(t * freq * perfect * 2.0 * Math.PI)));
                data[id++] = s;

                //                writer.Write(s);
            }
            for (int i = 0; i < samples / 4; i++)
            {
                double t = (double)i / (double)samplesPerSecond;
                short s = (short)(ampl * (Math.Sin(t * freq * 2.0 * Math.PI) + Math.Sin(t * freq * concert * 2.0 * Math.PI)));
                data[id++] = s;

                //writer.Write(s);
            }
            return data;
        }//end Perfect5th();

        public static void Main()
        {
            Console.WriteLine("RUNNING FROM TowseyLib.Main()");

            const string wavDirName = @"C:\SensorNetworks\WavFiles\";
            const string fName = "SineSignal.wav";
            string path = wavDirName + fName;

            int sampleRate = 22050;
            //int sampleRate = 44100;
            double duration = 30.245; //sig duration in seconds
            int[] harmonics = { 500, 1000, 2000, 4000 };
            double[] signal = DSP_Filters.GetSignal(sampleRate, duration, harmonics);
            WriteWavFile(signal, sampleRate, path);
            Console.WriteLine("FINISHED!");
            Console.ReadLine();

        } //end Main method

        public static void Write(string path)
        {
            using (FileStream FS_Write = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {

                BinaryWriter bw = new BinaryWriter(FS_Write);

                int c = 0;

                //52 49 46 46 16 0A 04 00 57 41 56 45 66 6D 74 20
                //10 00 00 00 01 00 01 00 44 AC 00 00 88 58 01 00
                //02 00 10 00 64 61 74 61 98 09 04 00      

                Byte[] header = { 82, 73, 70, 70, 22, 10, 4, 0, 87, 65, 86, 69, 102, 109, 116, 32 };
                bw.Write(header);

                Byte[] header2 = { 16, 0, 0, 0, 1, 0, 1, 0, 68, 172, 0, 0, 136, 88, 1, 0 };
                bw.Write(header2);

                Byte[] header3 = { 2, 0, 16, 0, 100, 97, 116, 97, 152, 9, 4, 0 };
                bw.Write(header3);

                Double t_const = (1.0 / 44100.0);
                Double t_pos = 1.0;
                Double t_val = 0.0;
                while (c < 264734)
                {
                    t_val = t_const * t_pos;
                    t_pos++;

                    Double amp = 14468.0;
                    amp = amp + 0.0;

                    Double freq = 440.0;
                    freq = 440.0;

                    Double sample = amp * Math.Sin(t_val * freq * 2 * Math.PI);

                    sample = sample + amp;
                    int sample_int = (int)sample;

                    int msb = sample_int / 256;
                    int lsb = sample_int - (msb * 256);

                    bw.Write((Byte)lsb);
                    bw.Write((Byte)msb);
                    c = c + 2;
                }
            }
        }

        public static void Write(double freq, double amp, double phase, WavInfo wavInfo, string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter writer = BeginWrite(stream, wavInfo, path))
                {
                    Console.WriteLine("Writing started...");

                    // write data
                    int sampleCount = (int)Math.Floor(wavInfo.Duration.TotalSeconds * wavInfo.SampleRate);

                    Console.WriteLine("Writing samples...");
                    for (int i = 0; i < sampleCount; i++)
                    {
                        double signalItem = amp * Math.Sin(phase + 2.0 * Math.PI * freq * i / wavInfo.SampleRate);
                        writer.Write((short)signalItem);
                    }

                    Console.WriteLine("Writing size...");

                    EndWrite(writer, wavInfo, sampleCount);

                    Console.WriteLine("Writing complete.");
                }
            }
        }

        public static BinaryWriter BeginWrite(FileStream stream, WavInfo wavInfo, string path)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            /*
             * Chunk - RIFF
             */

            // RIFF
            int chunkIdRiff = 0x46464952;

            // don't know this yet.
            int chunkDataSizeRiff = 0;

            //WAVE
            int riffTypeWave = 0x45564157;

            /*
             * Chunk - format
             */

            // "fmt " Note the chunk ID string 
            // ends with the space character (0x20).
            int chunkIdFormat = 0x20746D66;

            // the size of the standard wave format data (16 bytes) 
            // plus the size of any extra format bytes needed for 
            // the specific Wave format, if it does not contain 
            // uncompressed PCM data. 
            int chunkDataSizeFormat = 16;

            // The first word of format data specifies 
            // the type of compression used on the Wave 
            // data included in the Wave chunk found in this "RIFF" chunk.
            short compressionCode = 1;

            // how many separate audio signals that are 
            // encoded in the wave data chunk. A value of 
            // 1 means a mono signal, a value of 2 means a stereo signal.
            short numberOfChannels = 1;

            // The number of sample slices per second. 
            // This value is unaffected by the number of channels.
            int sampleRate = wavInfo.SampleRate;

            // Average Bytes Per Second
            // This value indicates how many bytes of wave 
            // data must be streamed to a D/A converter per 
            // second in order to play the wave file. This 
            // information is useful when determining if
            // data can be streamed from the source fast 
            // enough to keep up with playback. This value 
            // can be easily calculated with the formula:
            // AvgBytesPerSec = SampleRate * BlockAlign
            int bytesPerSecond = wavInfo.BytesPerSecond;


            // Block Align / bytes per sample. (frame)
            // The number of bytes per sample slice. This value 
            // is not affected by the number of channels and can be
            // calculated with the formula:
            // BlockAlign = SignificantBitsPerSample / 8 * NumChannels
            // or
            // short frameSize = (short)(channels * ((wavInfo.BitsPerSample + 7) / 8));
            short blockAlign = wavInfo.BytesPerSample;


            // Significant Bits Per Sample
            // This value specifies the number of bits used to define each sample. 
            // This value is usually 8, 16, 24 or 32. If the number of bits is not 
            // byte aligned (a multiple of 8) then the number of bytes used per sample
            // is rounded up to the nearest byte size and the unused bytes are set to 0 and ignored.
            short bitsPerSample = wavInfo.BitsPerSample;

            /*
             * Chunk - data
             */

            // data
            int chunkIdData = 0x61746164;

            // don't know this yet.
            int chunkDataSizeData = 0;

            //calculate size of file
            //int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;

            //write the header
            writer.Write(chunkIdRiff);
            writer.Write(chunkDataSizeRiff);
            writer.Write(riffTypeWave);
            writer.Write(chunkIdFormat);
            writer.Write(chunkDataSizeFormat);
            writer.Write(compressionCode);
            writer.Write(numberOfChannels);
            writer.Write(sampleRate);
            writer.Write(bytesPerSecond);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
            writer.Write(chunkIdData);
            writer.Write(chunkDataSizeData);

            // ready to write data;

            return writer;
        }

        public static void EndWrite(BinaryWriter writer, WavInfo wavInfo, int sampleCount)
        {
            // need to write file size and data size.

            /*
            data chunk size == NumSamples * NumChannels * BitsPerSample/8
            This is the number of bytes in the data.
            You can also think of this as the size
            of the read of the subchunk following this 
            number.
            */

            // size = sample rate * number of channels * (bits per sample /8) * time in seconds.

            int chunkDataSizeData = sampleCount * wavInfo.Channels * (wavInfo.BitsPerSample / 8);

            int chunkDataSizeDataOffset = 40;
            writer.Seek(chunkDataSizeDataOffset, SeekOrigin.Begin);
            writer.Write(chunkDataSizeData);

            /*
           file size = 36 + SubChunk2Size, or more precisely:
            4 + (8 + SubChunk1Size) + (8 + SubChunk2Size)
            This is the size of the rest of the chunk 
            following this number.  This is the size of the 
            entire file in bytes minus 8 bytes for the
            two fields not included in this count:
            ChunkID and ChunkSize.
            */

            int chunkDataSizeRiff = 4 + (8 + 16) + (8 + chunkDataSizeData);

            int chunkDataSizeRiffOffset = 4;
            writer.Seek(chunkDataSizeRiffOffset, SeekOrigin.Begin);
            writer.Write(chunkDataSizeRiff);
        }

    }// end class
}


