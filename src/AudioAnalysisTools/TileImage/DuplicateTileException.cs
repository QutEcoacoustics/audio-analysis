// <copyright file="DuplicateTileException.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.TileImage
{
    using System;

    public class DuplicateTileException : Exception
    {
        public DuplicateTileException(string name, ISuperTile current)
        {
            this.Name = name;
            this.Current = current;
        }

        public string Name { get; private set; }

        public ISuperTile Current { get; private set; }

        public override string Message => $"Tile '{this.Name}' has already been created. SuperTile: {this.Current.OffsetX},{this.Current.OffsetY},{this.Current.Scale}";
    }
}