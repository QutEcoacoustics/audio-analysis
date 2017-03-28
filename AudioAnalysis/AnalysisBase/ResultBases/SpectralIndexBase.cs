// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectrumBase.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SpectrumBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase.ResultBases
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public abstract class SpectralIndexBase : ResultBase
    {
        public abstract Dictionary<string, Func<SpectralIndexBase, double[]>> GetSelectors();

    }


}