using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;
using TowseyLib;
using System.Reflection;

namespace AudioAnalysis
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
            ChooseWavFile(out wavDirName, out wavFileName);  //WARNING! MUST CHOOSE WAV FILE IF CREATING NEW TEMPLATE
            //#######################################################################################################

            string appConfigPath = args[0];
            string wavPath       = args[1];
            string outputFolder  = args[2];

            wavPath = wavDirName + wavFileName + ".wav"; //set the .wav file in method ChooseWavFile()
            outputFolder = @"C:\SensorNetworks\Output\";  //default 

            Log.WriteIfVerbose("appConfigPath =" + appConfigPath);
            Log.WriteIfVerbose("wav File Path =" + wavPath);
            Log.WriteIfVerbose("output folder =" + outputFolder);
            Console.WriteLine();

            BaseSonogram sonogram = new SpectralSonogram(appConfigPath, new WavReader(wavPath));

            Console.WriteLine("Signal Duration =" + sonogram.Duration);
            Console.WriteLine("Sample Rate     =" + sonogram.SampleRate);
            Console.WriteLine("Window Size     =" + sonogram.Configuration.WindowSize);

            Console.WriteLine("\nFRAME PARAMETERS");
            Console.WriteLine("Frame Count     =" + sonogram.FrameCount);
            Console.WriteLine("Frame Duration  =" + (sonogram.FrameDuration   * 1000).ToString("F1") + " ms");
            Console.WriteLine("Frame Offset    =" + (sonogram.FrameOffset     * 1000).ToString("F1") + " ms");
            Console.WriteLine("Frames Per Sec  =" + sonogram.FramesPerSecond.ToString("F1"));

            Console.WriteLine("\nFREQUENCY PARAMETERS");
            Console.WriteLine("Nyquist Freq    =" + sonogram.NyquistFrequency + " Hz");
            Console.WriteLine("Freq Bin Width  =" + sonogram.FBinWidth.ToString("F2") + " Hz");

            Console.WriteLine("\nENERGY PARAMETERS");
            Console.WriteLine("Signal Max Amplitude  = " + sonogram.MaxAmplitude.ToString("F3"));
            Console.WriteLine("\tExplanation: Signal samples take values between -1.0 and +1.0");
            Console.WriteLine("\tExplanation: Signal energy is calculated frame by frame. The average value of the signal energy in a frame");
            Console.WriteLine("\t             equals the average of the amplitude squared of all 512 values in a frame.");
            Console.WriteLine("\t             The Log Energy of a frame is the log(10) of its signal energy as calculated above.");
            Console.WriteLine("\t             For calculation of SNR and to isolate acoustic events, it is useful to normalise the frame log energies with");
            Console.WriteLine("\t             reference to maximum and minimum frame log energies defined as CONSTANTS.");
            Console.WriteLine("\t             The following two constants are defined:");
            Console.WriteLine("Minimum Frame Log Energy =" + BaseSonogram.MinLogEnergy.ToString("F3"));
            Console.WriteLine("\tExplanation: A typical background noise value for Brisbane Airport (BAC2) recordings: Frame Log Energy = -4.5");
            Console.WriteLine("\t             A value of -7.0 allows for a very quiet background!");
            Console.WriteLine("Maximum Frame Log Energy =" + BaseSonogram.MaxLogEnergy.ToString("F3"));
            Console.WriteLine("\tExplanation: A value of Max Log Energy = -0.60206 is obtained by assuming the max average frame amplitude = 0.5");
            Console.WriteLine("\t             That is -0.60206 = Math.Log10(0.5 * 0.5)");
            Console.WriteLine("\t             Note that we have cicada recordings where the max average frame amplitude = 0.55");
            Console.WriteLine("\t             When normalising the log energy, any value < min is set = min and then");
            Console.WriteLine("\t             normalised log energy = logE - maxLogEnergy = log(E / maxE).");
            Console.WriteLine("\t             Positive values of normalised log energy occur only when log energy exceeds the maximum.");
            Console.WriteLine("\t             Log energy values are converted to decibels by multiplying by 10. Here are the minimum and maximum dB values");
            Console.WriteLine("Minimum frame dB         =" + sonogram.FrameNoise_dB.ToString("F3"));
            Console.WriteLine("Maximum frame dB         =" + sonogram.FrameMax_dB.ToString("F3"));

            Console.WriteLine("\ndB NOISE SUBTRACTION");
            Console.WriteLine("Noise Subtracted from each frame =" + sonogram.NoiseSubtracted.ToString("F3")+ " dB");
            Console.WriteLine("\tExplanation: The average background noise per frame is calculated using an algorithm of Lamel et al, 1981. ");
            Console.WriteLine("\t             They call it 'Adaptive Level Equalisatsion'. It effectively sets background noise level to 0 dB.");
            Console.WriteLine("\t             After removal of noise the min and max reference dB levels are as follows:");
            Console.WriteLine("Max DecibelReference =" + sonogram.MaxDecibelReference.ToString("F3") + " dB");
            Console.WriteLine("Min DecibelReference =" + sonogram.MinDecibelReference.ToString("F3") + " dB");
            Console.WriteLine("\tExplanation: The modal frame noise level is now 0 dB but the minimum level = "+ sonogram.MinDecibelReference.ToString("F3"));
            Console.WriteLine("\t             The maximum dB reference level is that obtained if the log energy = maximum.");
            Console.WriteLine("Max-min   frame SNR            = " + sonogram.Frame_SNR.ToString("F3") + " dB");
            double snr = sonogram.FrameMax_dB - sonogram.NoiseSubtracted;
            Console.WriteLine("Max-noise frame SNR            = " + snr.ToString("F3")+" dB       <<<<<<<<<<<< THIS IS THE KEY FIGURE");
            Console.WriteLine("\tExplanation: Here are some dB comparisons. They are with reference to the auditory threshold at 1 kHz.");
            Console.WriteLine("\t             Leaves rustling, calm breathing:  10 dB");
            Console.WriteLine("\t             Very calm room:                   20 - 30 dB");
            Console.WriteLine("\t             Normal talking at 1 m:            40 - 60 dB");
            Console.WriteLine("\t             Major road at 10 m:               80 - 90 dB");
            Console.WriteLine("\t             Jet at 100 m:                    110 -140 dB");


            Console.WriteLine("\nSEGMENTATION PARAMETERS");
            Console.WriteLine("sonogram.SegmentationThresholdK1 =" + sonogram.SegmentationThresholdK1.ToString("F3"));
            Console.WriteLine("sonogram.SegmentationThresholdK2 =" + sonogram.SegmentationThresholdK2.ToString("F3"));

//            Console.ReadLine();
            var recording = new AudioRecording() { FileName = wavPath };
            bool doHighlightSubband = false; bool add1kHzLines = true;
			var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image.Image.Width));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.Save(outputFolder + wavFileName + ".png");

            int imageWidth = 284;
            int imageHeight = 60;
            var image2 = new Image_MultiTrack(recording.GetImageOfWaveForm(imageWidth, imageHeight));
            image2.Save(outputFolder + wavFileName + "_waveform.png");

            int factor = 10;
            var image3 = new Image_MultiTrack(sonogram.GetImage_ReducedSonogram(factor));
            image3.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            image3.AddTrack(Image_Track.GetWavEnvelopeTrack(recording, image3.Image.Width));
            //image3.AddTrack(Image_Track.GetDecibelTrack(sonogram));
            image3.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            double[] scores = new double[image3.Image.Width*4];
            for (int n = 0; n < scores.Length; n++) scores[n] = (0.1 * n) % 1.0;
            double scoreMax = 1.0; 
            double scoreThreshold = 0.5;
            //image3.AddTrack(Image_Track.GetScoreTrack(scores, scoreMax, scoreThreshold));
            image3.Save(outputFolder + wavFileName + "_reduced.png");


            
            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
		}


        static void ChooseWavFile(out string wavDirName, out string wavFileName)
        {
            //BRISBANE AIRPORT CORP
            //wavDirName = @"C:\SensorNetworks\WavFiles\";
            //wavFileName = "sineSignal";
            //wavFileName = "golden-whistler";
            //wavFileName = "BAC2_20071008-085040";           //Lewin's rail kek keks used for obtaining kek-kek template.
            //string wavFileName = "BAC1_20071008-084607";             //faint kek-kek call
            //string wavFileName = "BAC2_20071011-182040_cicada";    //repeated cicada chirp 5 hz bursts of white noise
            //string wavFileName = "dp3_20080415-195000";            //ZERO SIGNAL silent room recording using dopod
            //string wavFileName = "BAC2_20071010-042040_rain";      //contains rain and was giving spurious results with call template 2
            //string wavFileName = "BAC2_20071018-143516_speech";
            //string wavFileName = "BAC2_20071014-022040nightnoise"; //night with no signal in Kek-kek band.
            //string wavFileName = "BAC2_20071008-195040";           //kek-kek track completely clear
            //string wavFileName = "BAC3_20070924-153657_wind";
            //string wavFileName = "BAC3_20071002-070657";
            //string wavFileName = "BAC3_20071001-203657";
            //string wavFileName = "BAC5_20080520-040000_silence";
            //string wavFileName = "Samford13Pre-Deploy_20081004-061500";
            //String wavFileName = "BAC2_20071008-062040"; //kek-kek @ 33sec
            //String wavFileName = "BAC2_20071008-075040"; //kek-kek @ 17sec
            //String wavFileName = "BAC1_20071008-081607";//false positive or vague kek-kek @ 19.3sec
            //String wavFileName = "BAC1_20071008-084607";   //faint kek-kek @ 1.7sec

            //SAMFORD
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Samford02\";
            //string wavFileName = "SA0220080221-022657";
            //string wavFileName = "SA0220080222-015657";
            //string wavFileName = "SA0220080223-215657";

            //SAMFORD 24
            //wavDirName = @"C:\SensorNetworks\WavFiles\\Samford24\";
            //wavFileName = "Samford_24_20090313-123000";

            //AUSTRALIAN BIRD CALLS
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\VoicesOfSubtropicalRainforests\";
            //string wavFileName = "06 Logrunner";

            //WEBSTER
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Websters\";
            //string wavFileName = "BOOBOOK";
            //string wavFileName = "CAPPRE";
            //string wavFileName = "KINGPAR";

            //JINHAI
            //const string wavDirName = @"C:\SensorNetworks\WavFiles\Jinhai\";
            //string wavFileName = "vanellus-miles";
            //string wavFileName = "En_spinebill";
            //string wavFileName = "kookaburra";
            //string wavFileName = "magpie";
            //string wavFileName = "raven";

            //KOALA recordings  - training files etc
            //const string wavDirName = @"C:\SensorNetworks\Koala\";
            //const string opDirName  = @"C:\SensorNetworks\Koala\";
            //string wavFileName = "Jackaroo_20080715-103940";  //recording from Bill Ellis.

            //ST BEES
            //wavDirName = @"C:\SensorNetworks\WavFiles\StBees\";
            //wavFileName = "West_Knoll_-_St_Bees_KoalaBellow20080919-073000"; //source file for template
            //wavFileName = "Honeymoon_Bay_St_Bees_KoalaBellow_20080905-001000";
            //wavFileName = "West_Knoll_St_Bees_WindRain_20080917-123000";
            //wavFileName = "West_Knoll_St_Bees_FarDistantKoala_20080919-000000";
            //wavFileName = "West_Knoll_St_Bees_fruitBat1_20080919-030000";
            //wavFileName = "West_Knoll_St_Bees_KoalaBellowFaint_20080919-010000";
            //wavFileName = "West_Knoll_St_Bees_FlyBirdCicada_20080917-170000";
            //wavFileName = "West_Knoll_St_Bees_Currawong1_20080923-120000";
            //wavFileName = "West_Knoll_St_Bees_Currawong2_20080921-053000";
            //wavFileName = "West_Knoll_St_Bees_Currawong3_20080919-060000";
            //wavFileName = "Top_Knoll_St_Bees_Curlew1_20080922-023000";
            //wavFileName = "Top_Knoll_St_Bees_Curlew2_20080922-030000";
            //wavFileName = "Honeymoon_Bay_St_Bees_Curlew3_20080914-003000";
            //wavFileName = "West_Knoll_St_Bees_RainbowLorikeet1_20080918-080000";
            //wavFileName = "West_Knoll_St_Bees_RainbowLorikeet2_20080916-160000";
            //wavFileName = "Honeymoon_Bay_St_Bees_20090312-060000_PheasantCoucal";

            //JENNIFER'S CD
            //string wavDirName = @"C:\SensorNetworks\WavFiles\JenniferCD\";
            //string wavFileName = "Track02";           //Lewin's rail kek keks.

            //JENNIFER'S DATA
            //wavDirName = @"C:\SensorNetworks\WavFiles\Jennifer_BAC10\BAC10\";
            //wavFileName = "BAC10_20081101-045000";

            //TEST DATA
            wavDirName = @"C:\SensorNetworks\WavFiles\Test_12March2009\";
            //wavFileName = "file0031_selection";
            //wavFileName = "daphne-151000_selection";
            //wavFileName = "jb1-161000_selection";
            wavFileName = "jb3-151000_selection";

        } //end ChooseWavFile()


	} //end class
}