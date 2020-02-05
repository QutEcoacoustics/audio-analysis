// <copyright file="RemoteSegment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.AcousticWorkbench.Orchestration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using AnalysisBase.Segment;
    using global::AcousticWorkbench;
    using global::AcousticWorkbench.Models;

    /// <inheritdoc />
    /// <summary>
    /// Represents a segment of a remote target audio file.
    /// This is not a hierarchical structure.
    /// </summary>
    public class RemoteSegment : ISegment<AudioRecording>
    {
        private readonly AudioRecording recording;

        public RemoteSegment(AudioRecording source, Range<double> offsets)
        {
            this.Source = source;
            this.Offsets = offsets;

            this.SourceMetadata = new SourceMetadata(
                source.DurationSeconds.Seconds(),
                source.SampleRateHertz,
                source.Uuid,
                source.RecordedDate);
        }

        public RemoteSegment(AudioRecording source, double startOffsetSeconds, double endOffsetSeconds)
            : this(source, new Range<double>(startOffsetSeconds, endOffsetSeconds))
        {
        }

        public RemoteSegment(AudioRecording source)
            : this(source, 0, source.DurationSeconds)
        {
        }

        public Range<double> Offsets { get; }

        public AudioRecording Source { get; }

        public SourceMetadata SourceMetadata { get; }

        public double StartOffsetSeconds => this.Offsets.Minimum;

        public double EndOffsetSeconds => this.Offsets.Maximum;

        public bool Equals(ISegment<AudioRecording> other)
        {
            if (other == null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Source.Uuid == other.Source.Uuid &&
                   this.StartOffsetSeconds == other.StartOffsetSeconds &&
                   this.EndOffsetSeconds == other.EndOffsetSeconds;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Source?.Uuid.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.StartOffsetSeconds.GetHashCode();
                hashCode = (hashCode * 397) ^ this.EndOffsetSeconds.GetHashCode();
                return hashCode;
            }
        }

        public ISegment<AudioRecording> SplitSegment(double newStart, double newEnd)
        {
            return new RemoteSegment(this.Source, newStart, newEnd);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is RemoteSegment objSegment))
            {
                return false;
            }

            return this.Equals(objSegment);
        }
    }

    public class RemoteSegmentWithData : RemoteSegment
    {
        public RemoteSegmentWithData(AudioRecording source, Range<double> offsets, object[] data)
            : this(source, offsets, (IList<object>)data)
        {
        }

        public RemoteSegmentWithData(AudioRecording source, Range<double> offsets, IList<object> data)
            : base(source, offsets.Minimum, offsets.Maximum)
        {
            this.Data = new ReadOnlyCollection<object>(data);
        }

        public IReadOnlyCollection<object> Data { get; }

        public new ISegment<AudioRecording> SplitSegment(double newStart, double newEnd)
        {
            return new RemoteSegmentWithData(this.Source, (newStart, newEnd).AsRange(), this.Data.ToList());
        }

        public override string ToString()
        {
            return "[RemoteSegmentWithData: " + Json.SerializeToString(this) + "]";
        }
    }
}
