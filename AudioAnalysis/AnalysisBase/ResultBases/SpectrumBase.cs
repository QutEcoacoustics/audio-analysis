// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectrumBase.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the SpectrumBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace AnalysisBase.ResultBases
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public abstract class SpectrumBase : ResultBase
    {
        public abstract Dictionary<string, Func<SpectrumBase, double[]>> GetSelectors();

    }


}