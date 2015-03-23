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
    using System.Text;

    using AudioAnalysisTools.LongDurationSpectrograms;

    using MathNet.Numerics.NumberTheory;

    public class Tiler
    {
        private readonly DirectoryInfo outputDirectory;
        private readonly TilingProfile profile;
        private readonly SortedSet<Layer> calculatedLayers;

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

            this.calculatedLayers = this.CalculateLayers(xScales, xUnitScale, unitWidth, yScales, yUnitScale, unitHeight);
        }

        public DirectoryInfo OutputDirectory
        {
            get
            {
                return this.outputDirectory;
            }
        }

        public SortedSet<Layer> CalculatedLayers
        {
            get
            {
                return this.calculatedLayers;
            }
        }

        public void TileMany(IEnumerable<ISuperTile> allSuperTiles)
        {

            var windowed = allSuperTiles.Windowed(2);
            foreach (var superTiles in windowed)
            {
                this.Tile(superTiles[0], superTiles[1]);
            }
        }

        /// <summary>
        /// Split one large image (a super tile) into smaller tiles.
        /// The super tile needs to be aligned within the layer first
        /// </summary>
        /// <param name="current">
        /// </param>
        /// <param name="next">
        /// The next.
        /// </param>
        public void Tile(ISuperTile current, ISuperTile next)
        {
            Contract.Ensures(current.Image != null);

            var layer = this.CalculatedLayers.First(x => x.XScale == current.Scale);

            int width = current.Image.Width, height = current.Image.Height, xOffset = current.OffsetX, yOffset = current.OffsetY;


            // either positive or negative
            int paddingX, paddingY;
            int tileOffsetInLayerX = this.AlignSuperTileInLayer(layer.Width, layer.XTiles, this.profile.TileWidth, current.OffsetX, current.Image.Width, out paddingX);
            int tileOffsetInLayerY = this.AlignSuperTileInLayer(layer.Height, layer.YTiles, this.profile.TileHeight, current.OffsetY, current.Image.Height, out paddingY); 
            

            // drawable tiles in the current super tile
            // as a rule only draw the sections that are available in the current tile
            // and as much as we need from the next tile
            var tilesInSuperTileX = 0;
            var tilesInSuperTileY = 0;

            // determine padding needed

            // start producing tiles
            for (var i = 0; i < tilesInSuperTileX; i++)
            {
                for (var j = 0; j < tilesInSuperTileY; j++)
                {
                    // clone a segment of the super tile
                    // NOTE: At the moment it may sometimes pull regions outside of the original image
                    // unclear what beaviour will occur, either:
                    // a) excpetion - :-(
                    // b) trimmed segment - not ideal, add empty space manually
                    // c) full segment, with empty space - ideal! do nothing

                    // supertile relative
                    var top = (int)((j * this.profile.TileHeight) - paddingX);

                    // supertile relative
                    var left = (int)((i * this.profile.TileWidth) - paddingY);
                    var subsection = new Rectangle()
                                         {
                                             X = top, 
                                             Y = left, 
                                             Width = this.profile.TileWidth, 
                                             Height = this.profile.TileHeight
                                         };
                    var tileImage = ((Bitmap)current.Image).Clone(subsection, current.Image.PixelFormat);

                    // convert co-ordinates to layer relative
                    var layerTop = 0;
                    var layerLeft = 0;

                    // write tile to disk
                    var name = this.profile.GetFileBaseName(layer, new Point(layerTop, layerLeft));
                    var outputTilePath = this.OutputDirectory.CombineFile(name + ".png").FullName;
                    tileImage.Save(outputTilePath);
                }
            }
        }

        /// <summary>
        /// Aligns a supertile into a layer for one dimension only
        /// 
        /// tiles are aligned to the center of the layer
        /// if odd number of tiles required, then middle tile is offset by half tile size
        /// <para>
        /// <![CDATA[ 
        ///    |-----------------|====‖========|---------------|
        ///    l1                s1   ‖        s2              l2
        ///    |_|_|_|_|_|_|_|_|_|_|_|‖|_|_|_|_|_|_|_|_|_|_|_|_|       <- odd
        ///    tmin                   ‖                        tmax
        ///                           middle     
        ///                           ‖  
        ///     |----------------|====‖========|--------------|
        ///     l1               s1   ‖        s2             l2
        ///     |_|_|_|_|_|_|_|_|_|_|_‖_|_|_|_|_|_|_|_|_|_|_|_|        <- even
        ///     tmin                  ‖                       tmax
        ///                           middle      
        /// tileSize = 2 characters
        /// ]]>
        /// </para>
        /// </summary>
        /// <param name="layerLength">
        ///     The layer Width.
        /// </param>
        /// <param name="layerTileCount">
        /// </param>
        /// <param name="layerTileLength">
        /// </param>
        /// <param name="superTileOffset">
        ///     The super Tile Offset. This is a top/left coordinate 
        ///     relative to the start of the data
        ///     that needs to be converted to a middle coordinate
        ///     that is relative to the layer.
        /// </param>
        /// <param name="superTileWidth">
        ///     The super Tile Width.
        /// </param>
        /// <param name="padding"></param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int AlignSuperTileInLayer(int layerLength, int layerTileCount, int layerTileLength, int superTileOffset, int superTileWidth, out int padding)
        {
            // the layer should fit a whole number of tiles
            Contract.Assert(Math.Abs(layerLength / ((double)layerTileLength - layerTileCount)) < 0.001);
            
            // first determine padding required by the layer
            var tilesInLayer = (double)layerLength / layerTileCount;
            var overlap = tilesInLayer - Math.Floor(tilesInLayer);
            
            // padding is split either side
            int overlapInPx = (int)Math.Round((overlap / 2.0) * tilesInLayer, MidpointRounding.AwayFromZero);
            padding = layerTileLength - overlapInPx;

            // convert superTileOddset to coordinates relative to layer
            var superTileOffsetInLayer = padding + superTileOffset;

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
            var xEnumerator = xScales.Reverse().GetEnumerator();
            var yEnumerator = yScales.Reverse().GetEnumerator();

            double xScale = double.NaN, yScale = double.NaN;
            bool xMore, yMore;
            while ((xMore = xEnumerator.MoveNext()) | (yMore = yEnumerator.MoveNext()))
            {
                xScale = xMore ? xEnumerator.Current : xScale;
                yScale = yMore ? yEnumerator.Current : yScale;

                int xLayerLength, yLayerLength;
                int xTiles, yTiles;
                double xNormalizedScale, yNormalizedScale;

                CalculateScaleStats(xUnitScale, unitWidth, this.profile.TileWidth, xScale, out xNormalizedScale, out xLayerLength, out xTiles);
                CalculateScaleStats(yUnitScale, unitHeight, this.profile.TileHeight, yScale, out yNormalizedScale, out yLayerLength, out yTiles);

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

            var tilelength = (double)layerLength / tileLength;

            // if the tiles fit exactly within, then that exact number of tiles
            // otherwise, plus 2 tiles (to pad either side)
            tiles = PadIfNotRounded(tilelength);
        }

        private static int PadIfNotRounded(double value)
        {
            var floored = Math.Floor(value);
            return (int)(Math.Abs(floored - value) < 0.001 ? floored : floored + 2);
        }
    }
}