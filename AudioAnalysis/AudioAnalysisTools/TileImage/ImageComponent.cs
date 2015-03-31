namespace AudioAnalysisTools.TileImage
{
    using System.Drawing;

    public class ImageComponent
    {
        public ImageComponent()
        {
        }

        public ImageComponent(Rectangle fragment, TileBias xBias, TileBias yBias)
        {
            this.Fragment = fragment;
            this.XBias = xBias;
            this.YBias = yBias;
        }

        public ImageComponent(Rectangle fragment, int xBias, int yBias)
            : this(fragment, (TileBias)xBias, (TileBias)yBias)
        {
        }

        public Rectangle Fragment { get; set; }

        /// <summary>
        /// Gets or sets XBias.
        /// Represents the image this rectangle needs to be drawn from.
        /// -1: image before on x axis
        /// 0: current image
        /// 1: next image on x axis
        /// </summary>
        public TileBias XBias { get; set; }

        /// <summary>
        /// Gets or sets YBias
        /// Represents the image this rectangle needs to be drawn from.
        /// -1: image before on y axis
        /// 0: current image
        /// 1: next image on y axis
        /// </summary>
        public TileBias YBias { get; set; }
    }
}