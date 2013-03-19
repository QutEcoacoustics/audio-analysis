using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnalysisBase;
using AudioAnalysisTools;
using TowseyLib;
using System.Drawing;

namespace Dong.Felt
{
    class AcousticEventDetection
    {
  
       //  pick the highest point in the spectrogram
       //var amplitudeSpectrogram = new AmplitudeSonogram(config, recording.GetWavReader());

       /// <summary>
       /// To generate a binary spectrogram, you are required to provide an amplitudeThreshold
       /// Above the threshold, its amplitude value will be assigned to black, otherwise to white
       /// </summary>
       /// <returns></returns>
        public void GenerateBinarySpectrogram( AmplitudeSonogram amplitudeSpectrogram,double amplitudeThreshold)
        {
             var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;
             var maximumOfFrame = (int)Math.Round(amplitudeSpectrogram.FrameDuration / amplitudeSpectrogram.FramesPerSecond);
             var maximumOfFrequencyBin = (int)Math.Round(amplitudeSpectrogram.NyquistFrequency / amplitudeSpectrogram.FBinWidth);
             for (int i = 0; i < maximumOfFrame; i++)
             {
                 for (int j = 0; j < maximumOfFrequencyBin; j++)
                 {
                     if (spectrogramAmplitudeMatrix[i, j] > amplitudeThreshold) // by default it will be 0.028
                         spectrogramAmplitudeMatrix[i, j] = 1;
                     else
                         spectrogramAmplitudeMatrix[i, j] = 0;
                 }
             }
             var imageResult = new Image_MultiTrack(amplitudeSpectrogram.GetImage(false, true));
             imageResult.Save("C:\\Test recordings\\Test4.png");
        }

        /// <summary>
        /// PeakAmplitudeDetection is implemented by sliding a window along the frame and frequencybins, then picking up the point 
        /// which has the peakAmplitude value in this window. 
        /// to do this you are required to provide some parameters of slideWindow, such as slideWindowDuation,slideWindowFrequencybin
        /// </summary>
        /// <param name="amplitudeSpectrogram"></param>
        /// <param name="slideWindowDuation"></param>
        /// <param name="minFreq"></param>  // for slideWindowMinFrequencyBin
        /// <param name="maxFreq"></param>  // for slideWindowMaxFrequencyBin
        /// <param name="coustermizedDuation"></param>
        /// <returns></returns>
         public string PeakAmplitudeDetection(AmplitudeSonogram amplitudeSpectrogram, double slideWindowDuation, int minFreq, 
                                                    int maxFreq)
         {
             var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;
          
             var numberOfWindows = (int)(amplitudeSpectrogram.Duration.Seconds / slideWindowDuation);  // by default SlideWindowDuation = 2.0 sec
             //get the offset frame of SlideWindow
             var slideWindowFrameOffset = (int)Math.Round(slideWindowDuation * amplitudeSpectrogram.FramesPerSecond); 

             var slideWindowminFreqBin = (int)Math.Round(minFreq / amplitudeSpectrogram.FBinWidth); // MinFreq = 2000,MaxFreq = 4100
             var slideWindowmaxFreqBin = (int)Math.Round(maxFreq / amplitudeSpectrogram.FBinWidth);

             var peakPoints = new Tuple<Point, double>[numberOfWindows];

             for (int windowIndex = 0; windowIndex < numberOfWindows; windowIndex++)
             {
                 var currentMaximum = double.NegativeInfinity;
                 //peakPoints[windowIndex] = spectrogramAmplitudeMatrix[windowIndex * SlidewindowOffset, 46];

                 // scan along frames
                 for (int i = 0; i < (windowIndex + 1) * slideWindowFrameOffset; i++)
                 {
                      // scan through bins
                     for (int j = slideWindowminFreqBin; j < slideWindowmaxFreqBin; j++)
                      {
                          if (spectrogramAmplitudeMatrix[i, j] > currentMaximum)
                          {
                              peakPoints[windowIndex] = Tuple.Create(new Point(i, j), spectrogramAmplitudeMatrix[i, j]);
                          }
                      }
                  }
              }

              var outputPoints = string.Empty;
              foreach (var point in peakPoints)
              {
                  if (point != null)
                  {
                      outputPoints += string.Format("Point found at x:{0}, y:{1}, value: {2}\n", point.Item1.X, point.Item1.Y, point.Item2);
                  }
              }
              return outputPoints;
              //Log.Info("Found points: \n" + outputPoints);
         }
         //const double customizedTime = 15.0;
         //var events = new List<AcousticEvent>() { 
         //new AcousticEvent(peakPoints[0].Item1.X / amplitudeSpectrogram.FramesPerSecond, customizedTime / amplitudeSpectrogram.FramesPerSecond, peakPoints[0].Item1.Y * 43 - 15,peakPoints[0].Item1.Y*43),   
         //new AcousticEvent(peakPoints[1].Item1.X / amplitudeSpectrogram.FramesPerSecond, customizedTime / amplitudeSpectrogram.FramesPerSecond, peakPoints[1].Item1.Y * 43 - 15,peakPoints[1].Item1.Y*43),
         //  //new AcousticEvent(11.0,2.0,500,1000),
         //  //new AcousticEvent(14.0,2.0,500,1000),
         //  //new AcousticEvent(17.0,2.0,500,1000),
         //};
         //foreach (var e in events)
         //{
         //     e.BorderColour = AcousticEvent.DEFAULT_BORDER_COLOR;
         //}
         //var image = new Image_MultiTrack(amplitudeSpectrogram.GetImage(false, true));
         //image.AddTrack(Image_Track.GetTimeTrack(amplitudeSpectrogram.Duration, amplitudeSpectrogram.FramesPerSecond));
         //image.AddTrack(Image_Track.GetSegmentationTrack(amplitudeSpectrogram));
         //image.AddEvents(events, amplitudeSpectrogram.NyquistFrequency, amplitudeSpectrogram.Configuration.FreqBinCount, amplitudeSpectrogram.FramesPerSecond);
         //image.Save("C:\\Test recordings\\Test5.png");    
  
        // make fake acoustic events
        /// <summary>
        /// make fake acoustic events (which I set up some particular events) and draw box on these events
        /// </summary>
        /// <param name="wavFilePath"></param>
         public void MakingFakeAcousticEvents(string wavFilePath)
         {
             var recording = new AudioRecording(wavFilePath);
             var events = new List<AcousticEvent>() { 
                 new AcousticEvent(5.0,2.0,500,1000),   
                 new AcousticEvent(8.0,2.0,500,1000),
                 new AcousticEvent(11.0,2.0,500,1000),
                 new AcousticEvent(14.0,2.0,500,1000),
                 new AcousticEvent(17.0,2.0,500,1000),
             };
             foreach (var e in events)
             {
                 e.BorderColour = AcousticEvent.DEFAULT_BORDER_COLOR;
             }
             //generate spectrogram
             var config = new SonogramConfig { NoiseReductionType = NoiseReductionType.NONE };
             var spectrogram = new SpectralSonogram(config, recording.GetWavReader());

             var image = new Image_MultiTrack(spectrogram.GetImage(false, true));
             image.AddTrack(Image_Track.GetTimeTrack(spectrogram.Duration, spectrogram.FramesPerSecond));
             //image.AddTrack(Image_Track.GetWavEnvelopeTrack(sonogram, image.sonogramImage.Width));
             image.AddTrack(Image_Track.GetSegmentationTrack(spectrogram));
             image.AddEvents(events, spectrogram.NyquistFrequency, spectrogram.Configuration.FreqBinCount, spectrogram.FramesPerSecond);
             image.Save("C:\\Test recordings\\Test1.png");

         }
        /// <summary>
        /// Draw a line in the spectrogram, here the frame and bin paramters has been fixed. 
        /// </summary>
        /// <param name="wavFilePath"></param>
         public void DrawLine(string wavFilePath,double startTime,double endTime, int minFrequency,int maxFrequency)
         {
             var recording = new AudioRecording(wavFilePath);
             var config = new SonogramConfig(); 
             var amplitudeSpectrogram = new SpectralSonogram(config, recording.GetWavReader());
             var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;

             int minFrame = (int)Math.Round(startTime * amplitudeSpectrogram.FramesPerSecond );
             int maxFrame = (int)Math.Round(endTime * amplitudeSpectrogram.FramesPerSecond );

             int minFrequencyBin = (int)Math.Round(minFrequency / amplitudeSpectrogram.FBinWidth);
             int maxFrequencyBin = (int)Math.Round(maxFrequency / amplitudeSpectrogram.FBinWidth); 

             for (int i = minFrame ; i < maxFrame; i++)
             {
                 for (int j = minFrequencyBin; j < maxFrequencyBin; j++)
                 {
                     spectrogramAmplitudeMatrix[i, j] = 1;
                 }
             }
             var imageResult = new Image_MultiTrack(amplitudeSpectrogram.GetImage(false, true));
             imageResult.Save("C:\\Test recordings\\Test2.png");
         }
            //  not necessary part
            //if (recording.SampleRate != 22050)
            //{
            //    recording.ConvertSampleRate22kHz();
            //}
        /// <summary>
        /// Draw a box on a customerized frequency and time range
        /// </summary>
        /// <param name="wavFilePath"></param>
         public void DrawCostermizedBox(string wavFilePath)
         {
             var recording = new AudioRecording(wavFilePath);
             var config = new SonogramConfig();  //?????
             var amplitudeSpectrogram = new AmplitudeSonogram(config, recording.GetWavReader());
             var spectrogramAmplitudeMatrix = amplitudeSpectrogram.Data;
             const int MinFreq = 2000;
             const int MaxFreq = 3500;
             int minFreqBin = (int)Math.Round(MinFreq / amplitudeSpectrogram.FBinWidth);
             int maxFreqBin = (int)Math.Round(MaxFreq / amplitudeSpectrogram.FBinWidth);

             const int StartTime = 16;
             const int EndTime = 22;
             int minFrameNum = (int)Math.Round(StartTime * amplitudeSpectrogram.FramesPerSecond);
             int maxFrameNum = (int)Math.Round(EndTime * amplitudeSpectrogram.FramesPerSecond);

             for (int i = minFrameNum; i < maxFrameNum; i++)
             {
                 spectrogramAmplitudeMatrix[i, minFreqBin] = 1;
                 spectrogramAmplitudeMatrix[i, maxFreqBin] = 1;
             }
             for (int j = minFreqBin; j < maxFreqBin; j++)
             {
                 spectrogramAmplitudeMatrix[minFrameNum, j] = 1;
                 spectrogramAmplitudeMatrix[maxFrameNum, j] = 1;
             }

             var imageResult = new Image_MultiTrack(amplitudeSpectrogram.GetImage(false, true));
             imageResult.Save("C:\\Test recordings\\Test3.png");
         }
         // print configure dictionary
         //string printMessage = analysisSettings.ConfigDict["my_custom_setting"];
         //Log.Info(printMessage);
    }
}
