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
        public AnalysisIo((IFileSystem FileSystem, FileSystemEntry Base) input, (IFileSystem FileSystem, DirectoryEntry Base) output, (IFileSystem FileSystem, DirectoryEntry Base)? temp)
        {
            this.Input = input.FileSystem;
            this.InputBase = input.Base;

            this.Output = output.FileSystem;
            this.OutputBase = output.Base;

            this.Temp = (temp ?? input).FileSystem;
            this.TempBase = (DirectoryEntry)(temp ?? input).Base;
        }

        public FileSystemEntry InputBase { get; set; }

        public DirectoryEntry OutputBase { get; set; }

        public DirectoryEntry TempBase { get; set; }

        public IFileSystem Input { get; }

        public IFileSystem Output { get; }

        public IFileSystem Temp { get; }
    }
}
