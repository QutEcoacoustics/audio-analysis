using System;
using System.IO;

namespace TowseyLib
{
    class WavWriter
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
        /// <param name="signal"></param>
        /// <param name="samplesPerSecond"></param>
        /// <param name="path"></param>
        public static void WriteWavFile(double[] signal, int samplesPerSecond, string path)
        {

            //int samples = 88200 * 4;
            //short[] perfect5th = Perfect5th(samples, samplesPerSecond);

            int samples = signal.Length;


            FileStream   stream = new FileStream(path, FileMode.Create);
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
            short frameSize = (short)(tracks * ((bitsPerSample + 7)/8));
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
            for (int i = 0; i < samples; i++) writer.Write((short)signal[i]);

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
            const string wavDirName = @"C:\SensorNetworks\WavFiles\";
            const string fName = "SineSignal.wav";
            string path = wavDirName + fName;

            int sampleRate = 22050;
            //int sampleRate = 44100;
            double duration = 30.245; //sig duration in seconds
            int[] harmonics = { 500, 1000, 2000, 4000 };
            double[] signal = DSP.GetSignal(sampleRate, duration, harmonics);
            WriteWavFile(signal, sampleRate, path);
            Console.WriteLine("FINISHED!");
            Console.ReadLine();

      } //end Main method


   }// end class
}


