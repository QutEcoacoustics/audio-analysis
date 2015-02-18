namespace AudioAnalysisTools.TileImage
{
    using System.Drawing;

    public interface ISuperTile
    {
        double Scale { get; }

        int OffsetX { get; }

        int OffsetY { get; }

        Image Image { get; set; }
    }
}