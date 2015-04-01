// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tiler.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools.TileImage
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    public class Tiler
    {
        #region Fields

        private readonly SortedSet<Layer> calculatedLayers;
        private readonly DirectoryInfo outputDirectory;
        private readonly TilingProfile profile;

        #endregion

        #region Constructors and Destructors

        public Tiler(
            DirectoryInfo outputDirectory, 
            TilingProfile profile, 
            SortedSet<double> scales, 
            double unitScale, 
            int unitLength)
            : this(outputDirectory, profile, scales, unitScale, unitLength, scales, unitScale, unitLength)
        {
        }

        public Tiler(
            DirectoryInfo outputDirectory, 
            TilingProfile profile, 
            SortedSet<double> xScales, 
            double xUnitScale, 
            int unitWidth, 
            SortedSet<double> yScales, 
            double yUnitScale, 
            int unitHeight)
        {
            this.outputDirectory = outputDirectory;
            this.profile = profile;

            this.calculatedLayers = this.CalculateLayers(
                xScales, 
                xUnitScale, 
                unitWidth, 
                yScales, 
                yUnitScale, 
                unitHeight);
        }

        #endregion

        #region Public Properties

        public SortedSet<Layer> CalculatedLayers
        {
            get
            {
                return this.calculatedLayers;
            }
        }

        public DirectoryInfo OutputDirectory
        {
            get
            {
                return this.outputDirectory;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns a set of rectangles that can be used to compose a baseRectangle
        /// </summary>
        /// <param name="baseRectangle">
        /// The base Rectangle.
        /// </param>
        /// <param name="requestedRectangle">
        /// The requested Rectangle.
        /// </param>
        /// <returns>
        /// The <see cref="ImageComponent[]"/>.
        /// </returns>
        public static ImageComponent[] GetImageParts(Rectangle baseRectangle, Rectangle requestedRectangle)
        {
            if (baseRectangle == requestedRectangle)
            {
                return new[] { new ImageComponent(requestedRectangle, 0, 0) };
            }

            var parts = new List<ImageComponent>(9);

            if (!baseRectangle.IntersectsWith(requestedRectangle))
            {
                throw new NotSupportedException(
                    "The premise of this function relies on the supplied rectangles intersecting");
            }

            Rectangle innerSegment = Rectangle.Intersect(baseRectangle, requestedRectangle);

            // one by one just cut out rectangles
            // 8 cases to check in the general case
            // check left side
            if (requestedRectangle.Left < baseRectangle.Left)
            {
                List<ImageComponent> rects = SplitAlongY(
                    requestedRectangle.Left, 
                    requestedRectangle.Top, 
                    baseRectangle.X - requestedRectangle.Left, 
                    requestedRectangle.Height, 
                    innerSegment.Top, 
                    innerSegment.Bottom, 
                    TileBias.Negative);
                parts.AddRange(rects);
            }

            // check middle (this check is always true since rects are required to overlap
            if (requestedRectangle.Right > baseRectangle.Left && requestedRectangle.Left < baseRectangle.Right)
            {
                List<ImageComponent> rects = SplitAlongY(
                    innerSegment.Left, 
                    requestedRectangle.Top, 
                    innerSegment.Width, 
                    requestedRectangle.Height, 
                    innerSegment.Top, 
                    innerSegment.Bottom, 
                    TileBias.Neutral);
                parts.AddRange(rects);
            }

            // check right side
            if (requestedRectangle.Right >= baseRectangle.Right)
            {
                List<ImageComponent> rects = SplitAlongY(
                    baseRectangle.Right, 
                    requestedRectangle.Top, 
                    requestedRectangle.Right - baseRectangle.Right, 
                    requestedRectangle.Height, 
                    innerSegment.Top, 
                    innerSegment.Bottom, 
                    TileBias.Positive);
                parts.AddRange(rects);
            }

            return parts.OrderBy(ic => ic.YBias).ThenBy(ic => ic.XBias).ToArray();
        }

        /// <summary>
        /// Split one large image (a super tile) into smaller tiles.
        ///     The super tile needs to be aligned within the layer first
        /// </summary>
        /// <param name="current">
        /// </param>
        /// <param name="next">
        /// The next.
        /// </param>
        public void Tile(ISuperTile current, ISuperTile next)
        {
            if (current == null)
            {
                return;
            }

            Layer layer = this.CalculatedLayers.First(x => x.XScale == current.Scale);

            int width = current.Image.Width, 
                height = current.Image.Height, 
                xOffset = current.OffsetX, 
                yOffset = current.OffsetY;

            // determine padding needed
            int paddingX, paddingY;
            int superTileOffsetInLayerX = this.AlignSuperTileInLayer(
                layer.Width, 
                layer.XTiles, 
                this.profile.TileWidth, 
                current.OffsetX, 
                current.Image.Width, 
                out paddingX);
            int superTileOffsetInLayerY = this.AlignSuperTileInLayer(
                layer.Height, 
                layer.YTiles, 
                this.profile.TileHeight, 
                current.OffsetY, 
                current.Image.Height, 
                out paddingY);

            var superTileRectangle = new Rectangle(xOffset, yOffset, width, height);

            // drawable tiles in the current super tile
            // as a rule only draw the sections that are available in the current tile
            // and as much as we need from the next tile
            double tilesInSuperTileX = Math.Abs(((double)current.Image.Width / this.profile.TileWidth) - 1.0) < 0.001
                                           ? (double)current.Image.Width / this.profile.TileWidth
                                           : Math.Ceiling((current.Image.Width / 2.0) / this.profile.TileWidth) * 2.0;
            double tilesInSuperTileY = Math.Abs(((double)current.Image.Height / this.profile.TileHeight) - 1.0) < 0.001
                                           ? (double)current.Image.Height / this.profile.TileHeight
                                           : Math.Ceiling((current.Image.Height / 2.0) / this.profile.TileHeight) * 2.0;

            // define source
            var superTileBitmap = (Bitmap)current.Image;
            using (Graphics superTileGraphics = Graphics.FromImage(superTileBitmap))
            {
                // start producing tiles
                for (int i = 0; i < tilesInSuperTileX; i++)
                {
                    for (int j = 0; j < tilesInSuperTileY; j++)
                    {
                        // clone a segment of the super tile
                        // two cases are catered for bounds that exceed the current super tile
                        // a) Negative X Bias - Paint transparency
                        // b) Positive X Bias - Pull subsection from next image, or paint transparency
                        // Note: best case: Neutral X Bias
                        // Note: no support for anything other than Neutral y Bias

                        // make destination image
                        var tileImage = new Bitmap(
                            this.profile.TileWidth, 
                            this.profile.TileHeight, 
                            PixelFormat.Format32bppArgb);

                        // determine how to paint it
                        // supertile relative
                        int layerLeft = (i * this.profile.TileWidth) + superTileOffsetInLayerX, superTileLeft = layerLeft - (2 * paddingX);
                        int layerTop = (j * this.profile.TileHeight) + superTileOffsetInLayerY, superTileTop = layerTop - (2 * paddingY);

                        using (Graphics tileGraphics = Graphics.FromImage(tileImage))
                        {
                            var subsection = new Rectangle
                                                 {
                                                     X = superTileLeft,
                                                     Y = superTileTop, 
                                                     Width = this.profile.TileWidth, 
                                                     Height = this.profile.TileHeight
                                                 };
                            ImageComponent[] fragments = GetImageParts(superTileRectangle, subsection);

                            // now paint on destination image
                            // 3 possible sources: nothing (transparent), current, next image (along X-axis)
                            foreach (ImageComponent imageComponent in fragments)
                            {
                                if (imageComponent.YBias != TileBias.Neutral)
                                {
                                    throw new NotImplementedException(
                                        "Currently no support has been implemented for drawing from supertiles that are not aligned with the current tile on the y-axis");
                                }

                                var destRect =
                                    new Rectangle(
                                        new Point(
                                            imageComponent.Fragment.X - superTileLeft, 
                                            imageComponent.Fragment.Y - superTileTop), 
                                        imageComponent.Fragment.Size);

                                var sourceRect =
                                    new Rectangle(
                                        new Point(
                                            imageComponent.Fragment.Location.X - (superTileOffsetInLayerX - paddingX),
                                            imageComponent.Fragment.Location.Y - (superTileOffsetInLayerY - paddingY)), 
                                        imageComponent.Fragment.Size);

                                if (imageComponent.XBias == TileBias.Negative)
                                {
                                    // No-op - the default background for the tile is transparent,
                                    // no need to paint that again

                                    // also remember, we do not draw from previous super tiles
                                    // thus we don't need access to previous tile image
                                }
                                else if (imageComponent.XBias == TileBias.Positive)
                                {
                                    // two cases here: edge of layer reached (paint transparent padding)
                                    // or grab next section from image
                                    if (next == null)
                                    {
                                        // end of stream, paint transparency
                                        // default background for the tile is transparent,
                                        // no need to paint that again
                                    }
                                    else
                                    {
                                        // paint a fraction from the next image
                                        tileGraphics.DrawImage(next.Image, destRect, sourceRect, GraphicsUnit.Pixel);
                                    }
                                }
                                else
                                {
                                    // neutral
                                    tileGraphics.DrawImage(current.Image, destRect, sourceRect, GraphicsUnit.Pixel);
                                }
                            }
                        }

                        // write tile to disk
                        string name = this.profile.GetFileBaseName(
                            this.calculatedLayers, 
                            layer, 
                            new Point(layerLeft, layerTop));
                        string outputTilePath = this.OutputDirectory.CombineFile(name + ".png").FullName;
                        tileImage.Save(outputTilePath);
                    }
                }
            }
        }

        public void TileMany(IEnumerable<ISuperTile> allSuperTiles)
        {
            IEnumerable<ISuperTile[]> windowed = allSuperTiles.WindowedOrDefault(2);
            foreach (var superTiles in windowed)
            {
                this.Tile(superTiles[0], superTiles[1]);
            }
        }

        #endregion

        #region Methods

        private static void CalculateScaleStats(
            double unitScale, 
            int unitLength, 
            int tileLength, 
            double scale, 
            out double normalizedScale, 
            out int layerLength, 
            out int tiles)
        {
            normalizedScale = unitScale / scale;
            layerLength = (int)(unitLength * normalizedScale);

            double tilelength = (double)layerLength / tileLength;

            // if the tiles fit exactly within, then that exact number of tiles
            // otherwise, plus 2 tiles (to pad either side)
            tiles = PadIfNotRounded(tilelength);
        }

        private static int PadIfNotRounded(double value)
        {
            double floored = Math.Floor(value);
            return (int)(Math.Abs(floored - value) < 0.001 ? floored : floored + 2);
        }

        private static List<ImageComponent> SplitAlongY(
            int x, 
            int y, 
            int width, 
            int height, 
            int ySplit1, 
            int ySplit2, 
            TileBias xBias)
        {
            Contract.Requires(height != 0);
            Contract.Requires(ySplit1 > y);
            Contract.Requires(ySplit2 > y && ySplit2 > ySplit1);

            var split = new List<ImageComponent>(3);

            if (width == 0)
            {
                return split;
            }

            // top
            if (ySplit1 - y > 0)
            {
                var top = new Rectangle(x, y, width, ySplit1 - y);
                split.Add(new ImageComponent(top, xBias, TileBias.Negative));
            }

            // middle
            if (ySplit2 - ySplit1 > 0)
            {
                var middle = new Rectangle(x, ySplit1, width, ySplit2 - ySplit1);
                split.Add(new ImageComponent(middle, xBias, TileBias.Neutral));
            }

            // bottom
            if ((y + height) - ySplit2 > 0)
            {
                var bottom = new Rectangle(x, ySplit2, width, (y + height) - ySplit2);
                split.Add(new ImageComponent(bottom, xBias, TileBias.Positive));
            }

            return split;
        }

        /// <summary>
        /// Aligns a supertile into a layer for one dimension only
        ///     tiles are aligned to the center of the layer
        /// </summary>
        /// <param name="layerLength">
        /// The layer Width.
        /// </param>
        /// <param name="layerTileCount">
        /// </param>
        /// <param name="layerTileLength">
        /// </param>
        /// <param name="superTileOffset">
        /// The super Tile Offset. This is a top/left coordinate
        ///     relative to the start of the data
        ///     that needs to be converted to a middle coordinate
        ///     that is relative to the layer.
        /// </param>
        /// <param name="superTileWidth">
        /// The super Tile Width.
        /// </param>
        /// <param name="padding">
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int AlignSuperTileInLayer(
            int layerLength, 
            int layerTileCount, 
            int layerTileLength, 
            int superTileOffset, 
            int superTileWidth, 
            out int padding)
        {
            // first determine padding required by the layer
            double tilesInLayer = (double)layerLength / layerTileLength;
            double overlap = tilesInLayer - Math.Floor(tilesInLayer);

            // padding is split either side
            var overlapInPx = (int)Math.Round((overlap / 2.0) * layerTileLength, MidpointRounding.AwayFromZero);
            padding = overlapInPx == 0 ? 0 : layerTileLength - overlapInPx;

            // convert superTileOddset to coordinates relative to layer
            int superTileOffsetInLayer = padding + superTileOffset;

            // with limite information it is really hard to do more/verify
            return superTileOffsetInLayer;
        }

        private SortedSet<Layer> CalculateLayers(
            SortedSet<double> xScales, 
            double xUnitScale, 
            int unitWidth, 
            SortedSet<double> yScales, 
            double yUnitScale, 
            int unitHeight)
        {
            var results = new SortedSet<Layer>();
            int scaleIndex = 0;
            IEnumerator<double> xEnumerator = xScales.Reverse().GetEnumerator();
            IEnumerator<double> yEnumerator = yScales.Reverse().GetEnumerator();

            double xScale = double.NaN, yScale = double.NaN;
            bool xMore, yMore;
            while ((xMore = xEnumerator.MoveNext()) | (yMore = yEnumerator.MoveNext()))
            {
                xScale = xMore ? xEnumerator.Current : xScale;
                yScale = yMore ? yEnumerator.Current : yScale;

                int xLayerLength, yLayerLength;
                int xTiles, yTiles;
                double xNormalizedScale, yNormalizedScale;

                CalculateScaleStats(
                    xUnitScale, 
                    unitWidth, 
                    this.profile.TileWidth, 
                    xScale, 
                    out xNormalizedScale, 
                    out xLayerLength, 
                    out xTiles);
                CalculateScaleStats(
                    yUnitScale, 
                    unitHeight, 
                    this.profile.TileHeight, 
                    yScale, 
                    out yNormalizedScale, 
                    out yLayerLength, 
                    out yTiles);

                results.Add(
                    new Layer(scaleIndex)
                        {
                            XNormalizedScale = xNormalizedScale, 
                            YNormalizedScale = yNormalizedScale, 
                            XScale = xScale, 
                            YScale = yScale, 
                            Width = xLayerLength, 
                            Height = yLayerLength, 
                            XTiles = xTiles, 
                            YTiles = yTiles
                        });

                scaleIndex++;
            }

            return results;
        }

        #endregion
    }
}