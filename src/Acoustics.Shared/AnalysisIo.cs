// <copyright file="AnalysisIo.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using Acoustics.Shared.Contracts;

    public class AnalysisIo
    {
        public AnalysisIo((IFileSystem FileSystem, IDirectoryInfo Base) input, (IFileSystem FileSystem, IDirectoryInfo Base) output, (IFileSystem FileSystem, IDirectoryInfo Base)? temp)
        {
            this.Input = input.FileSystem;
            this.InputBase = input.Base;

            this.Output = output.FileSystem;
            this.OutputBase = output.Base;

            this.Temp = (temp ?? input).FileSystem;
            this.TempBase = (temp ?? input).Base;
        }

        public IDirectoryInfo InputBase { get; }

        public IDirectoryInfo OutputBase { get; }

        public IDirectoryInfo TempBase { get; }

        public IFileSystem Input { get; }

        public IFileSystem Output { get; }

        public IFileSystem Temp { get; }

        public AnalysisIoInputDirectory EnsureInputIsDirectory()
        {
            Contract.Requires(this.InputBase != null, $"{nameof(this.InputBase)} must not be null");

            if (this.Input.Directory.Exists(this.InputBase.FullName))
            {
                return new AnalysisIoInputDirectory((this.Input, this.InputBase), (this.Output, this.OutputBase), (this.Temp, this.TempBase));
            }

            throw new ArgumentException($"Expected `{this.InputBase}` to be a directory, but it is not");
        }
    }

    public class AnalysisIoInputDirectory
        : AnalysisIo
    {
        internal AnalysisIoInputDirectory((IFileSystem FileSystem, IDirectoryInfo Base) input, (IFileSystem FileSystem, IDirectoryInfo Base) output, (IFileSystem FileSystem, IDirectoryInfo Base)? temp)
            : base(input, output, temp)
        {
        }

        public new DirectoryInfo InputBase => new DirectoryInfo(base.InputBase.FullName);
    }
}