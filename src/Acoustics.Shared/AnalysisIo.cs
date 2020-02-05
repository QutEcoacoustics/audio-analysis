// <copyright file="AnalysisIo.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.IO.Enumeration;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Acoustics.Shared.Contracts;

    using Zio;
    using FileSystemEntry = Zio.FileSystemEntry;

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

        public FileSystemEntry InputBase { get; }

        public DirectoryEntry OutputBase { get; }

        public DirectoryEntry TempBase { get; }

        public IFileSystem Input { get; }

        public IFileSystem Output { get; }

        public IFileSystem Temp { get; }

        public AnalysisIoInputDirectory EnsureInputIsDirectory()
        {
            Contract.Requires(this.InputBase != null, $"{nameof(this.InputBase)} must not be null");

            if (this.Input.DirectoryExists(this.InputBase.Path))
            {
                return new AnalysisIoInputDirectory((this.Input, this.InputBase), (this.Output, this.OutputBase), (this.Temp, this.TempBase));
            }

            throw new ArgumentException($"Expected `{this.InputBase}` to be a directory, but it is not");
        }
    }

    public class AnalysisIoInputDirectory
        : AnalysisIo
    {
        internal AnalysisIoInputDirectory((IFileSystem FileSystem, FileSystemEntry Base) input, (IFileSystem FileSystem, DirectoryEntry Base) output, (IFileSystem FileSystem, DirectoryEntry Base)? temp)
            : base(input, output, temp)
        {
        }

        public new DirectoryEntry InputBase => (DirectoryEntry)base.InputBase;
    }
}
