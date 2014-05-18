namespace Acoustics.Shared
{
    using System;

    /// <summary>
    /// Segment policy interface.
    /// </summary>
    public interface ISegmentPolicy
    {
        /// <summary>
        /// Gets TargetSegmentSize.
        /// </summary>
        TimeSpan TargetSegmentSize { get; }

        /// <summary>
        /// Gets TargetSegmentSizeMs.
        /// </summary>
        long TargetSegmentSizeMs { get; }

        /// <summary>
        /// Gets MinimumSegmentSize.
        /// </summary>
        TimeSpan MinimumSegmentSize { get; }

        /// <summary>
        /// Gets MinimumSegmentSizeMs.
        /// </summary>
        long MinimumSegmentSizeMs { get; }
    }

    /// <summary>
    /// Default segment policy implementation.
    /// </summary>
    public class SegmentPolicy : ISegmentPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentPolicy"/> class.
        /// </summary>
        /// <param name="targetSegmentSize">
        /// The target segment size.
        /// </param>
        /// <param name="minimumSegmentSize">
        /// The minimum segment size.
        /// </param>
        public SegmentPolicy(TimeSpan targetSegmentSize, TimeSpan minimumSegmentSize)
        {
            this.TargetSegmentSize = targetSegmentSize;
            this.TargetSegmentSizeMs = (long)targetSegmentSize.TotalMilliseconds;
            this.MinimumSegmentSize = minimumSegmentSize;
            this.MinimumSegmentSizeMs = (long)minimumSegmentSize.TotalMilliseconds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentPolicy"/> class.
        /// </summary>
        /// <param name="targetSegmentSize">
        /// The target segment size.
        /// </param>
        /// <param name="minimumSegmentSize">
        /// The minimum segment size.
        /// </param>
        public SegmentPolicy(long targetSegmentSize, long minimumSegmentSize)
        {
            this.TargetSegmentSize = TimeSpan.FromMilliseconds(targetSegmentSize);
            this.TargetSegmentSizeMs = targetSegmentSize;
            this.MinimumSegmentSize = TimeSpan.FromMilliseconds(minimumSegmentSize);
            this.MinimumSegmentSizeMs = minimumSegmentSize;
        }

        /// <summary>
        /// Gets TargetSegmentSize.
        /// </summary>
        public TimeSpan TargetSegmentSize { get; private set; }

        /// <summary>
        /// Gets TargetSegmentSizeMs.
        /// </summary>
        public long TargetSegmentSizeMs { get; private set; }

        /// <summary>
        /// Gets MinimumSegmentSize.
        /// </summary>
        public TimeSpan MinimumSegmentSize { get; private set; }

        /// <summary>
        /// Gets MinimumSegmentSizeMs.
        /// </summary>
        public long MinimumSegmentSizeMs { get; private set; }
    }
}
