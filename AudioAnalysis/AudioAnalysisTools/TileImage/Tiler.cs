// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tiler.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools.TileImage
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using log4net;

    using TowseyLibrary;

    public class Tiler
    {
        #region Fields

        private const double Epsilon = 1.0 / (2.0 * TimeSpan.TicksPerSecond);

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly SortedSet<Layer> calculatedLayers;
        private readonly DirectoryInfo outputDirectory;
        private readonly TilingProfile profile;
        private readonly Dictionary<double, HashSet<Tuple<int, int>>> tileHistory = new Dictionary<double, HashSet<Tuple<int, int>>>();

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

            this.WriteImages = true;
        }

        public Tiler(
            DirectoryInfo outputDirectory,
            TilingProfile profile,
            double xUnitScale,
            int unitWidth,
            double yUnitScale,
            int unitHeight)
            : this(
                outputDirectory,
                profile,
                new SortedSet<double>() { xUnitScale },
                xUnitScale,
                unitWidth,
                new SortedSet<double>() { yUnitScale },
                yUnitScale,
                unitHeight)
        {
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

        /// <summary>
        /// Gets or sets a value indicating whether images are written.
        /// Dirty hack to short circuit Tile's functionality for unit testing
        /// </summary>
        internal bool WriteImages { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Split one large image (a super tile) into smaller tiles
        /// </summary>
        /// <param name="superTile">The super tile to be split</param>
        public virtual void Tile(ISuperTile superTile)
        {
            this.Tile(superTile, null);
        }

        /// <summary>
        /// Split one large image (a super tile) into smaller tiles.
        ///     The super tile needs to be aligned within the layer first
        /// </summary>
        /// <param name="current">
        /// The super tile currently being operated on.
        /// </param>
        /// <param name="next">
        /// The next super tile that will be processed (positive x-dimension)
        /// </param>
        public virtual void Tile(ISuperTile current, ISuperTile next)
        {
            if (current == null)
            {
                return;
            }

            this.CheckForTileDuplication(current);

            if (!this.WriteImages)
            {
                Log.Debug("Tile method skipped");
                return;
            }

            if (current.Image == null)
            {
                throw new ArgumentException("Image cannot be null");
            }

            Layer layer = this.CalculatedLayers.First(x => Math.Abs(x.XScale - current.Scale) < Epsilon);

            int width = current.Image.Width, 
                height = current.Image.Height, 
                xOffset = current.OffsetX, 
                yOffset = current.OffsetY;

            // determine padding needed
            int paddingX, paddingY;
            int startTileEdgeX, startTileEdgeY;
            int superTileOffsetInLayerX = this.AlignSuperTileInLayer(
                layer.Width, 
                layer.XTiles, 
                this.profile.TileWidth, 
                current.OffsetX, 
                current.Image.Width, 
                out paddingX,
                out startTileEdgeX);
            int superTileOffsetInLayerY = this.AlignSuperTileInLayer(
                layer.Height, 
                layer.YTiles, 
                this.profile.TileHeight, 
                current.OffsetY, 
                current.Image.Height, 
                out paddingY,
                out startTileEdgeY);

            var deltaTileEdgeSuperTileX = superTileOffsetInLayerX - startTileEdgeX;
            var deltaTileEdgeSuperTileY = superTileOffsetInLayerY - startTileEdgeY;
            var superTileRectangle = new Rectangle(xOffset, yOffset, width, height);

            // drawable tiles in the current super tile
            // as a rule only draw the sections that are available in the current tile
            // and as much as we need from the next tile
            double tilesInSuperTileX = CalculateTilesInSuperTile(current.Image.Width, this.profile.TileWidth, paddingX, deltaTileEdgeSuperTileX);
            double tilesInSuperTileY = CalculateTilesInSuperTile(current.Image.Height, this.profile.TileHeight, paddingY, deltaTileEdgeSuperTileY);

            // draw tiles
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
                    int layerLeft = (i * this.profile.TileWidth) + startTileEdgeX,
                        superTileLeft = layerLeft - (paddingX);
                    int layerTop = (j * this.profile.TileHeight) + startTileEdgeY,
                        superTileTop = layerTop - (paddingY);

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
                    Log.Debug("Saving tile: " + outputTilePath);
                    tileImage.Save(outputTilePath);
                }
            }
        }

        public void TileMany(IEnumerable<ISuperTile> allSuperTiles)
        {
            var scaleGroups = allSuperTiles.GroupBy(st => st.Scale).OrderByDescending(stg => stg.Key);
            foreach (var scaleGroup in scaleGroups)
            {
                IEnumerable<ISuperTile[]> windowed = scaleGroup.OrderBy(st => st.OffsetX).WindowedOrDefault(2);
                foreach (var superTiles in windowed)
                {
                    
                    this.Tile(superTiles[0], superTiles[1]);
                }
            }
        }

        #endregion

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
        internal static ImageComponent[] GetImageParts(Rectangle baseRectangle, Rectangle requestedRectangle)
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

        #region Methods

        private void CheckForTileDuplication(ISuperTile superTile)
        {

            if (this.tileHistory.ContainsKey(superTile.Scale))
            {
                var offsets = Tuple.Create(superTile.OffsetX, superTile.OffsetY);
                if (this.tileHistory[superTile.Scale].Contains(offsets))
                {
                    var tileDetails = "Scale: {0}, OffsetX: {1}, OffsetY: {2}".Format(
                        superTile.Scale,
                        superTile.OffsetX,
                        superTile.OffsetY);
                    throw new ArgumentException(
                        "A duplicate set of supertiles (" + tileDetails + ") has been passed into the tiler - this exception is thrown because it will most likely result int tiles being written over.");
                }
                else
                {
                    this.tileHistory[superTile.Scale].Add(offsets);
                }
            }
            else
            {
                this.tileHistory.Add(superTile.Scale, new HashSet<Tuple<int, int>>());
            }
        }

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

        private static double CalculateTilesInSuperTile(int actualSuperTileLength, int tileLength, int layerPadding, int additionalPadding)
        {
            var lengthToSplit = actualSuperTileLength + additionalPadding;
            if (lengthToSplit % tileLength == 0)
            {
                return lengthToSplit / tileLength;
            }

            if (layerPadding == 0)
            {
                return Math.Ceiling((double)lengthToSplit / tileLength);
            }
            else
            {
                return Math.Ceiling((lengthToSplit / 2.0) / tileLength) * 2.0;
            }
        }

        private static int PadIfNotRounded(double value)
        {
            double floored = Math.Floor(value);
            return (int)(Math.Abs(floored - value) < Epsilon ? floored : floored + 2);
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
        /// <param name="startTileEdge">
        /// The start Tile Edge.
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
            out int padding,
            out int startTileEdge)
        {
            // first determine padding required by the layer
            double tilesInLayer = (double)layerLength / layerTileLength;
            double overlap = tilesInLayer - Math.Floor(tilesInLayer);

            // padding is split either side
            var overlapInPx = (int)Math.Round((overlap / 2.0) * layerTileLength, MidpointRounding.AwayFromZero);
            padding = overlapInPx == 0 ? 0 : layerTileLength - overlapInPx;

            // convert superTileOddset to coordinates relative to layer
            int superTileOffsetInLayer = padding + superTileOffset;

            // does this offset lay on the edge of a tile?
            var tilesBeforeOffset = (double)superTileOffsetInLayer / layerTileLength;
            
            // ideally the number of tiles before the offset is a whole number
            var wholeTilesBeforeOffset = (int)Math.Floor(tilesBeforeOffset);

            // if it isn't though, use the edge of the nearest whole tile processing the super tile offset as a start point
            startTileEdge = wholeTilesBeforeOffset * layerTileLength;
            
            // with limited information it is really hard to do more/verify
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