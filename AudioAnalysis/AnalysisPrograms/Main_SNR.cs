using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;
using TowseyLib;
using System.Reflection;
using AudioAnalysisTools;

namespace AnalysisPrograms
{
	class Main_SNR
	{
		public static void Main(string[] args)
		{
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("DETERMINING SIGNAL TO NOISE RATIO IN RECORDING\n");

            Log.Verbosity = 1;

            //#######################################################################################################
            // KEY PARAMETERS TO CHANGE
            string wavDirName; string wavFileName;
            WavChooser.ChooseWavFile(out wavDirName, out wavFileName); //WARNING! CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            string wavPath = wavDirName + wavFileName + ".wav";        //set the .wav file in method ChooseWavFile()
            AudioRecording recording = new AudioRecording(wavPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

            //#######################################################################################################

            string appConfigPath = "";
            //string appConfigPath = @"C:\SensorNetworks\Templates\sonogram.ini";
            string outputFolder = @"C:\SensorNetworks\Output\temp1\"; //default 
            //min and max of required sub-band
            int minHz = 500; int maxHz = 8000;


            //reset paths if have command line arguments
            if ((args == null)||(args.Length == 0))
            {
                Console.WriteLine("=====================================================================================");
                Console.WriteLine("USAGE:");
                Console.WriteLine("snr.exe arg1 arg2 arg3 arg4 arg5");
                Console.WriteLine("\twhere arg1 = path to application ini file (string)");
                Console.WriteLine("\twhere arg2 = path to wav file             (string)");
                Console.WriteLine("\twhere arg3 = output dir (string)");
                Console.WriteLine("\twhere arg4 = min frequency      (optional integer)");
                Console.WriteLine("\twhere arg5 = max frequency (     optional integer)");
                Console.WriteLine("");
                Console.WriteLine("\tSet arg1 = \"null\" to use default parameters.");
                Console.WriteLine("=====================================================================================");
                Console.WriteLine("");
            }
            else
            {
                if (args.Length > 0) appConfigPath = args[0];
                if (args.Length > 1) wavPath       = args[1];
                if (args.Length > 2) outputFolder  = args[2];
                if (args.Length > 3) minHz = Int32.Parse(args[3]);
                if (args.Length > 4) maxHz = Int32.Parse(args[4]);
            }

            if (! File.Exists(wavPath))
            {
                Console.WriteLine("Cannot find file <" + wavPath + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(999);
            }
            if (!Directory.Exists(outputFolder))
            {
                Console.WriteLine("Cannot find output directory <" + outputFolder + ">");
                outputFolder = System.Environment.CurrentDirectory;
                Console.WriteLine("Have set output directory = <" + outputFolder + ">");
                Console.WriteLine("Press <ENTER> key to continue.");
                Console.ReadLine();
            }

            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("wav File Path =" + wavPath);
            Log.WriteIfVerbose("output folder =" + outputFolder);
            Console.WriteLine();

            BaseSonogram sonogram = new SpectralSonogram(appConfigPath, recording.GetWavReader());

            Console.WriteLine("\nSIGNAL PARAMETERS");
            Console.WriteLine("Signal Duration =" + sonogram.Duration);
            Console.WriteLine("Sample Rate     =" + sonogram.SampleRate);

            Console.WriteLine("\nFRAME PARAMETERS");
            Console.WriteLine("Window Size     =" + sonogram.Configuration.WindowSize);
            Console.WriteLine("Frame Count     =" + sonogram.FrameCount);
            Console.WriteLine("Frame Duration  =" + (sonogram.FrameDuration   * 1000).ToString("F1") + " ms");
            Console.WriteLine("Frame Offset    =" + (sonogram.FrameOffset     * 1000).ToString("F1") + " ms");
            Console.WriteLine("Frames Per Sec  =" + sonogram.FramesPerSecond.ToString("F1"));

            Console.WriteLine("\nFREQUENCY PARAMETERS");
            Console.WriteLine("Nyquist Freq    =" + sonogram.NyquistFrequency + " Hz");
            Console.WriteLine("Freq Bin Width  =" + sonogram.FBinWidth.ToString("F2") + " Hz");

            Console.WriteLine("\nENERGY PARAMETERS");
            Console.WriteLine("Signal Max Amplitude     = " + sonogram.MaxAmplitude.ToString("F3") + "  (See Note 1)");
            Console.WriteLine("Minimum Log Energy       =" + sonogram.SnrFrames.LogEnergy.Min().ToString("F2") + "  (See Note 2, 3)");
            Console.WriteLine("Maximum Log Energy       =" + sonogram.SnrFrames.LogEnergy.Max().ToString("F2"));
            Console.WriteLine("Minimum dB / frame       =" + sonogram.SnrFrames.Min_dB.ToString("F2") + "  (See Note 4)");
            Console.WriteLine("Maximum dB / frame       =" + sonogram.SnrFrames.Max_dB.ToString("F2"));

            Console.WriteLine("\ndB NOISE SUBTRACTION");
            Console.WriteLine("Noise (estimate of mode) =" + sonogram.SnrFrames.NoiseSubtracted.ToString("F3") + " dB   (See Note 5)");
            double noiseSpan = sonogram.SnrFrames.NoiseRange;
            Console.WriteLine("Noise range              =" + noiseSpan.ToString("F2") + " to +" + (noiseSpan*-1).ToString("F2") + " dB   (See Note 6)");
            Console.WriteLine("SNR (max frame-noise)    =" + sonogram.SnrFrames.Snr.ToString("F2") + " dB   (See Note 7)");


            Console.WriteLine("\nSEGMENTATION PARAMETERS");
            Console.WriteLine("SegmentationThreshold K1 =" + EndpointDetectionConfiguration.K1Threshold.ToString("F3") + " dB   (See Note 8)");
            Console.WriteLine("SegmentationThreshold K2 =" + EndpointDetectionConfiguration.K2Threshold.ToString("F3") + " dB   (See Note 8)");

            Console.WriteLine("\n\n\tNote 1:      Signal samples take values between -1.0 and +1.0");
            Console.WriteLine("\n\tNote 2:      Signal energy is calculated frame by frame. The log energy of a frame");
            Console.WriteLine("\t             equals the log(10) of the average of the amplitude squared of all 512 values in a frame.");
            Console.WriteLine("\n\tNote 3:      We normalise the frame log energies with respect to absolute max and min values. We use:");
            Console.WriteLine("\t             Minimum reference log energy = -7.0, which allowes for very quiet background noise.");
            Console.WriteLine("\t             A typical background noise log energy value for Brisbane Airport (BAC2) recordings is -4.5");
            Console.WriteLine("\t             Maximum reference log energy = 0.0, equivalent to average frame amplitude = 1.0");
            Console.WriteLine("\t             Therefore positive values of log energy cannot occur");
            Console.WriteLine("\t             In practice, the largest av. frame amplitude we have encountered = 0.55 for a cicada recording.");
            Console.WriteLine("\n\tNote 4:      Log energy values are converted to decibels by multiplying by 10.");
            Console.WriteLine("\n\tNote 5:      The modal background noise per frame is calculated using an algorithm of Lamel et al, 1981, called 'Adaptive Level Equalisatsion'.");
            Console.WriteLine("\t             This sets the modal background noise level to 0 dB.");
            Console.WriteLine("\n\tNote 6:      The modal noise level is now 0 dB but the noise ranges " + sonogram.SnrFrames.NoiseRange.ToString("F2")+" dB either side of zero.");
            Console.WriteLine("\n\tNote 7:      Here are some dB comparisons. NOTE! They are with reference to the auditory threshold at 1 kHz.");
            Console.WriteLine("\t             Our estimates of SNR are with respect to background environmental noise which is typically much higher than hearing threshold!");
            Console.WriteLine("\t             Leaves rustling, calm breathing:  10 dB");
            Console.WriteLine("\t             Very calm room:                   20 - 30 dB");
            Console.WriteLine("\t             Normal talking at 1 m:            40 - 60 dB");
            Console.WriteLine("\t             Major road at 10 m:               80 - 90 dB");
            Console.WriteLine("\t             Jet at 100 m:                    110 -140 dB");
            Console.WriteLine("\n\tNote 8:      dB above the modal noise, which has been set to zero dB. Used as thresholds to segment acoustic events. ");
            Console.WriteLine("\n");



//            Console.ReadLine();
            bool doHighlightSubband = false; bool add1kHzLines = true;
			var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.SonoImage.Width));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.Save(outputFolder + wavFileName + ".png");

            int imageWidth = 284;
            int imageHeight = 60;
            var image2 = new Image_MultiTrack(recording.GetWaveForm(imageWidth, imageHeight));
            image2.Save(outputFolder + wavFileName + "_waveform.png");
            double dBMin = -25.0; //-25 dB appear to be good value
            var image6 = new Image_MultiTrack(recording.GetWaveFormDB(imageWidth, imageHeight, dBMin));
            image6.Save(outputFolder + wavFileName + "_waveformDB.png");

            //int factor = 10;
            //var image3 = new Image_MultiTrack(sonogram.GetImage_ReducedSonogram(factor));
            //image3.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            //image3.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image3.Image.Width));
            //image3.AddTrack(Image_Track.GetDecibelTrack(sonogram));
            //image3.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            //image3.Save(outputFolder + wavFileName + "_reduced.png");


            //EXTRACT SNR DATA ABOUT SUB BAND/.
            doHighlightSubband = true;
            sonogram.CalculateSubbandSNR(recording.GetWavReader(), minHz, maxHz);
            Console.WriteLine("\ndB NOISE IN SUBBAND " + minHz + "Hz - " + maxHz + "Hz");
            //Console.WriteLine("Sub-band Min dB   =" + sonogram.SnrSubband.Min_dB.ToString("F2") + " dB");
            //Console.WriteLine("Sub-band Max dB   =" + sonogram.SnrSubband.Max_dB.ToString("F2") + " dB");
            Console.WriteLine("Modal noise       =" + sonogram.SnrSubband.NoiseSubtracted.ToString("F2") + " dB");
            noiseSpan = sonogram.SnrSubband.NoiseRange;
            Console.WriteLine("Noise range       =" + noiseSpan.ToString("F2") + " to +" + (noiseSpan * -1).ToString("F2") + " dB   (See Note 6)");
            Console.WriteLine("SNR (sub-band)    =" + sonogram.SnrSubband.Snr.ToString("F2") + " dB");

            var image4 = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image4.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image4.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image4.SonoImage.Width));
            image4.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image4.Save(outputFolder + wavFileName + "_subband.png");

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
		}

	} //end class
}