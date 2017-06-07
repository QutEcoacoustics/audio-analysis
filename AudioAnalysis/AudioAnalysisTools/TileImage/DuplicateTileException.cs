namespace AudioAnalysisTools.TileImage
{
    using System;

    public class DuplicateTileException : Exception
    {
        public string Name { get; private set; }

        public ISuperTile Current { get; private set; }

        public DuplicateTileException(string name, ISuperTile current)
        {
            this.Name = name;
            this.Current = current;
        }

        public override string Message => $"Tile '{this.Name}' has already been created. SuperTile: {this.Current.OffsetX},{this.Current.OffsetY},{this.Current.Scale}";
    }
}