// <copyright file="RecognizerTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Shared.ConfigFile;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    public static class RecognizerTools
    {
        /// <summary>
        /// Remove events whose acoustic profile does not match that of a flying fox.
        /// </summary>
        /// <param name="events">unfiltered acoustic events.</param>
        /// <param name="sonogram">includes matrix of spectrogram values.</param>
        /// <returns>filtered acoustic events.</returns>
        public static List<AcousticEvent> FilterEventsForSpectralProfile(List<AcousticEvent> events, BaseSonogram sonogram)
        {
            double[,] spectrogramData = sonogram.Data;

            //int colCount = spectrogramData.GetLength(1);

            // The following freq bins are used to demarcate freq bands for spectral tests below.
            // The hertz values are hard coded but could be included in the config.yml file.
            int maxBin = (int)Math.Round(8000 / sonogram.FBinWidth);
            int fourKiloHzBin = (int)Math.Round(4000 / sonogram.FBinWidth);
            int oneKiloHzBin = (int)Math.Round(1000 / sonogram.FBinWidth);

            var filteredEvents = new List<AcousticEvent>();
            foreach (AcousticEvent ae in events)
            {
                int startFrame = ae.Oblong.RowTop;

                //int endFrame = ae.Oblong.RowBottom;

                // get all the frames of the acoustic event
                //var subMatrix = DataTools.Submatrix(spectrogramData, startFrame, 0, endFrame, colCount - 1);

                // get only the frames from centre of the acoustic event
                var subMatrix = DataTools.Submatrix(spectrogramData, startFrame + 1, 0, startFrame + 4, maxBin);
                var spectrum = MatrixTools.GetColumnAverages(subMatrix);
                var normalisedSpectrum = DataTools.normalise(spectrum);
                normalisedSpectrum = DataTools.filterMovingAverageOdd(normalisedSpectrum, 11);
                var maxId = DataTools.GetMaxIndex(normalisedSpectrum);

                //var hzMax = (int)Math.Ceiling(maxId * sonogram.FBinWidth);

                // Do TESTS to determine if event has spectrum matching a Flying fox.
                // Test 1: Spectral maximum should be below 4 kHz.
                bool passTest1 = maxId < fourKiloHzBin;

                // Test 2: There should be little energy in 0-1 kHz band.
                var subband1Khz = DataTools.Subarray(normalisedSpectrum, 0, oneKiloHzBin);
                double bandArea1 = subband1Khz.Sum();
                double energyRatio1 = bandArea1 / normalisedSpectrum.Sum();

                // 0.125  = 1/8.  i.e. test requires that energy in 0-1kHz band is less than average in all 8 kHz bands
                // 0.0938 = 3/32. i.e. test requires that energy in 0-1kHz band is less than 3/4 average in all 8 kHz bands
                // 0.0625 = 1/16. i.e. test requires that energy in 0-1kHz band is less than half average in all 8 kHz bands
                bool passTest2 = !(energyRatio1 > 0.1);
                
                // Test 3: There should be little energy in 4-5 kHz band.
                var subband4Khz = DataTools.Subarray(normalisedSpectrum, fourKiloHzBin, oneKiloHzBin);
                double bandArea2 = subband4Khz.Sum();
                double energyRatio2 = bandArea2 / normalisedSpectrum.Sum();
                bool passTest3 = !(energyRatio2 > 0.125);

                // TODO write method to determine similarity of spectrum to a true flying fox spectrum.
                // Problem: it is not certain how variable the FF spectra are.
                // In ten minutes of recording used so far, which include 14-15 obvious calls, there appear to be two spectral types.
                // One type has three peaks at around 1.5 kHz, 3 kHz and 6 kHz.
                // The other type have two peaks around 2.5 and 5.5 kHz.

                //if (passTest1)
                //if (true)
                if (passTest1 && passTest2 && passTest3)
                {
                    filteredEvents.Add(ae);

                    //DEBUG SPECTRAL PROFILES: UNCOMMENT following lines to get spectral profiles of the events.
                    /*
                    double startSecond = ae.EventStartSeconds - ae.SegmentStartSeconds;
                    string name = "CallSpectrum " + (ae.SegmentStartSeconds / 60) + "m" + (int)Math.Floor(startSecond) + "s hzMax" + hzMax;
                    var bmp2 = GraphsAndCharts.DrawGraph(name, normalisedSpectrum, 100);
                    bmp2.Save(Path.Combine(@"PATH\Towsey.PteropusSpecies", name + ".png"));
                    */
                }
            }

            return filteredEvents;
        }
    }
}
