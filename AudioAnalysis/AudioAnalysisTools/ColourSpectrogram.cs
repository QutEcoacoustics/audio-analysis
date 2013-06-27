using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

//using Acoustics.Shared;
//using Acoustics.Tools;
//using Acoustics.Tools.Audio;

//using AnalysisBase;

using TowseyLib;





namespace AudioAnalysisTools
{
    public class ColourSpectrogram
    {

        int X_interval = 60; // assume one minute spectra and hourly time lines
        int frameWidth = 512;   // default value - from which spectrogram was derived
        int sampleRate = 17640; // default value - after resampling




            public static void Sandpit()
            {
                string cvrCsvPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.cvrSpectrum.csv";
                string avgCsvPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.avgSpectrum.csv";
                string csvAciPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.aciSpectrum.csv";
                string csvTenPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.tenSpectrum.csv";
                string imagePath  = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.colSpectrumTest10.png";
                // colour scheme IDs for RGB plus reverse
                // Need to add new ones into AcousticFeatures.DrawFalseColourSpectrogramOfIndices()
                //string colorSchemeID = "DEFAULT"; //R-G-B
                //string colorSchemeID = "ACI-TEN-AVG-REV"; //R-G-B
                //string colorSchemeID = "ACI-TEN-CVR"; //R-G-B
                //string colorSchemeID = "ACI-TEN-CVR-REV";
                //string colorSchemeID = "ACI-CVR-TEN";
                //string colorSchemeID = "ACI-TEN-CVR_AVG-REV";
                string colorSchemeID = "ACI-TEN-CVR_AVG";



                // set the X and Y axis scales for the spectrograms 
                int X_interval = 60; // assume one minute spectra and hourly time lines
                int frameWidth = 512;   // default value - from which spectrogram was derived
                int sampleRate = 17640; // default value - after resampling
                double freqBinWidth = sampleRate / (double)frameWidth;
                int Y_interval = (int)Math.Round(1000 / freqBinWidth); // mark 1 kHz intervals
                //AcousticFeatures.DrawColourSpectrogramsOfIndices(avgCsvPath, cvrCsvPath, csvAciPath, csvTenPath, imagePath, colorSchemeID, X_interval, Y_interval);
            }


    }
}
