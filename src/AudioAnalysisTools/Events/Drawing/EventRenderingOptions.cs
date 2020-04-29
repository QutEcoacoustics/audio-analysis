// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventRenderingOptions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the EventBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Events.Drawing
{
    using AudioAnalysisTools;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    public class EventRenderingOptions
    {
        public EventRenderingOptions(UnitConverters converters)
        {
            this.Converters = converters;
        }

        /// <summary>
        /// Gets the unit coverters that will be used to convert
        /// events from real units to the target image's pixels.
        /// </summary>
        public UnitConverters Converters { get; }

        /// <summary>
        /// Gets or sets the default border color for an event.
        /// </summary>
        /// <remarks>
        /// Defaults to Red, 1px.
        /// </remarks>
        public Pen Border { get; set; } = new Pen(Color.Red, 1f);

        /// <summary>
        /// Gets or sets the default fille color for an event.
        /// </summary>
        public IBrush Fill { get; set; } = new SolidBrush(Color.FromRgb(0, 255, 0));

        /// <summary>
        /// Gets or sets the graphics options that should be used with the
        /// <see cref="Fill"/> brush for rendering the contents of an event.
        /// </summary>
        public GraphicsOptions FillOptions { get; set; } = new GraphicsOptions()
        {
            ColorBlendingMode = SixLabors.ImageSharp.PixelFormats.PixelColorBlendingMode.Lighten,

            // TODO: ENABLE THIS IS NECESSARY
            // BlendPercentage = 0.5,
        };

        /// <summary>
        /// Gets or sets the Pen used to draw a "score" indicator
        /// on the left edge of the event.
        /// </summary>
        public Pen Score { get; set; } = new Pen(Color.Blue, 1f);

        /// <summary>
        /// Gets or sets the color to use when rendering labels.
        /// </summary>
        public Color Label { get; set; } = Color.DarkBlue;

        /// <summary>
        /// Gets a value indicating whether the image to draw onto represents a spectrogram.
        /// </summary>
        public bool TargetImageIsSpectral { get; } = true;

        public bool DrawBorder { get; set; } = true;

        public bool DrawFill { get; } = true;

        public bool DrawScore { get; set; } = true;

        public bool DrawLabel { get; set; } = true;
    }
}