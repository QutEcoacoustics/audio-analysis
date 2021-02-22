// <copyright file="IInterval2{TX,TY}.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ImageSharp
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IInterval2<TX, TY>
        where TX : struct, IComparable<TX>, IFormattable
        where TY : struct, IComparable<TY>, IFormattable
    {
        Interval<TX> X { get; }

        Interval<TY> Y { get; }
    }
}
