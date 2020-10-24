// <copyright file="BlobEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    /// <summary>
    /// An acoustic event that also includes data about the content identified by the event.
    /// </summary>
    public class BlobEvent : SpectralEvent, IPointData
    {
        public BlobEvent()
        {
            // ################################################################ TODO TODO TODO
        }

        public ICollection<ISpectralPoint> Points { get; } = new HashSet<ISpectralPoint>();

        public static (List<EventCommon> Events, List<Plot> DecibelPlots) GetBlobEvents(
            SpectrogramStandard spectrogram,
            BlobParameters bp,
            double? decibelThreshold,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            var spectralEvents = new List<EventCommon>();
            var plots = new List<Plot>();

            //get the array of intensity values minus intensity in side/buffer bands.
            //i.e. require silence in side-bands. Otherwise might simply be getting part of a broader band acoustic event.
            var decibelArray = SNR.CalculateFreqBandAvIntensityMinusBufferIntensity(
                spectrogram.Data,
                bp.MinHertz.Value,
                bp.MaxHertz.Value,
                bp.BottomHertzBuffer.Value,
                bp.TopHertzBuffer.Value,
                spectrogram.NyquistFrequency);

            // prepare plot of resultant blob decibel array.
            var plot = Plot.PreparePlot(decibelArray, $"{profileName} (Blobs:{decibelThreshold.Value:F0}dB)", decibelThreshold.Value);
            plots.Add(plot);

            // iii: CONVERT blob decibel SCORES TO ACOUSTIC EVENTS.
            // Note: This method does NOT do prior smoothing of the dB array.
            var acEvents = AcousticEvent.GetEventsAroundMaxima(
                decibelArray,
                segmentStartOffset,
                bp.MinHertz.Value,
                bp.MaxHertz.Value,
                decibelThreshold.Value,
                TimeSpan.FromSeconds(bp.MinDuration.Value),
                TimeSpan.FromSeconds(bp.MaxDuration.Value),
                spectrogram.FramesPerSecond,
                spectrogram.FBinWidth);

            var events = acEvents.ConvertAcousticEventsToSpectralEvents();
            spectralEvents.AddRange(events);
            plots.Add(plot);

            return (spectralEvents, plots);
        }

        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            ((IPointData)this).DrawPointsAsFill(graphics, options);

            //  base drawing (border)
            base.Draw(graphics, options);
        }
    }
}