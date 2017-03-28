namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public struct Span<T> : IEquatable<Span<T>>, IComparable<Span<T>> where T : struct, IComparable<T>
    {
        public Span(T lower, T upper)
        {
            this.Lower = lower;
            this.Upper = upper;
        }

        public T Lower { get; }
        public T Upper { get; }

        public bool Equals(Span<T> other)
        {
            return this.Lower.Equals(other.Lower) && this.Upper.Equals(other.Upper);
        }

        public int CompareTo(Span<T> other)
        {
            var lower = this.Lower.CompareTo(other.Lower);
            var upper = this.Upper.CompareTo(other.Upper);

            return lower == 0 ? upper : lower;
        }
    }

    public static class Span
    {
        public static Span<T> Create<T>(T minimum, T maximum) where T : struct, IComparable<T>
        {
            return new Span<T>(minimum, maximum);
        }
    }
}
