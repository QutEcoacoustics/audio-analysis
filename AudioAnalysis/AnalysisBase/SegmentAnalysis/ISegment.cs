// <copyright file="ISegment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase.SegmentAnalysis
{
    using System;

    /// <summary>
    /// Represents a range of audio from a source object.
    /// SEGMENTS ARE NOT HIERARCHICAL CONSTRUCTS.
    /// </summary>
    /// <typeparam name="TSource">The type of source file used.</typeparam>
    public interface ISegment<out TSource>
    {
        /// <summary>
        /// Gets the source audio object.
        /// <remarks>
        /// It is expected that <typeparamref name="TSource" /> will be a path or FileInfo for local sources.
        /// For remote sources, <typeparamref name="TSource" /> could be an arbitrary object.
        /// The <see cref="ISourcePreparer"/> for each format handles these differences.
        /// </remarks>
        /// </summary>
        TSource Source { get; }

        /// <summary>
        /// Gets information about the source.
        /// </summary>
        ISourceMetadata SourceMetadata { get; }

        /// <summary>
        /// Gets the start of the segment, represented by seconds from the start of the audio object.
        /// </summary>
        double StartOffsetSeconds { get; }

        /// <summary>
        /// Gets the end of the segment, represented by seconds from the start of the audio object.
        /// </summary>
        double EndOffsetSeconds { get; }

        /// <summary>
        /// Allows for the creation of a new segment, keeping the original source but changing the offsets.
        /// </summary>
        /// <param name="newStart">The new start offset to use for this segment.</param>
        /// <param name="newEnd">The new end offset to use for this segment.</param>
        /// <returns>A segment from the same source, with new offsets.</returns>
        ISegment<TSource> SplitSegment(double newStart, double newEnd);
    }

    /// <summary>
    /// Information about a source audio object
    /// </summary>
    public interface ISourceMetadata
    {
        /// <summary>
        /// Gets DurationSeconds - the length of the source audio object.
        /// </summary>
        TimeSpan DurationSeconds { get; }

        /// <summary>
        /// Gets SampleRate - the number of samples per second in the source audio object.
        /// </summary>
        int SampleRate { get; }
    }

    public class InvalidSegmentException : InvalidOperationException
    {
    }
}
