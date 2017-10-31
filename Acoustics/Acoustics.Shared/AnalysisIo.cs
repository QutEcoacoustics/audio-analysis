// <copyright file="AnalysisIo.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Zio;

    public class AnalysisIo
    {
        public AnalysisIo(IFileSystem input, IFileSystem output, IFileSystem temp)
        {
            this.Input = input;
            this.Output = output;
            this.Temp = temp ?? input;
        }

        public IFileSystem Input { get; }

        public IFileSystem Output { get; }

        public IFileSystem Temp { get; }
    }
}
