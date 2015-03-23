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
            foreach (var superTile in allSuperTiles)
            {
                this.Tile(superTile.Image, superTile.Scale, new Point(superTile.OffsetX, superTile.OffsetY));
            }
        }

        /// <summary>
        /// Split one large image (a super tile) into smaller tiles
        /// </summary>
        /// <param name="image">
        /// </param>
        /// <param name="scale">
        /// </param>
        /// <param name="offsets">
        /// The (top, left) point of the tile within the full space of the layer
        /// </param>
        public void Tile(Image image, double scale, Point offsets)
        {
            Contract.Ensures(image != null);

            var layer = this.CalculatedLayers.First(x => x.XScale == scale);

            int width = image.Width, height = image.Height, xOffset = offsets.X, yOffset = offsets.Y;

            // align super tile within layer
            int numtilesX;

            // either positive or negative
            double paddingX = 0; // TODO: call AlignSuperTileInLayer
            double paddingY = 0; // TODO: call AlignSuperTileInLayer

            bool isSuperTileWidthFactorOfLayerWidth = layer.Width / (double)width == 0;
            if (true)
            {
                // tiles are aligned within layer on some factor of tileWidth
                numtilesX = 0;
            }

            var numtilesY = 0;

            // determine padding needed

            // start producing tiles
            for (var i = 0; i < numtilesX; i++)
            {
                for (var j = 0; j < numtilesY; j++)
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
                    var tileImage = ((Bitmap)image).Clone(subsection, image.PixelFormat);

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
        /// <param name="layerWidth">
        /// The layer Width.
        /// </param>
        /// <param name="numberOfTilesNeededForLayer">
        /// </param>
        /// <param name="tileSize">
        /// </param>
        /// <param name="superTileOffset">
        /// The super Tile Offset.
        /// </param>
        /// <param name="superTileWidth">
        /// The super Tile Width.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int AlignSuperTileInLayer(
            int layerWidth, 
            int numberOfTilesNeededForLayer, 
            int tileSize, 
            int superTileOffset, 
            int superTileWidth)
        {
            Contract.Assert(layerWidth / (double)tileSize == numberOfTilesNeededForLayer);

            var middleOffset = layerWidth / 2.0;
            if (numberOfTilesNeededForLayer.IsOdd())
            {
                middleOffset = layerWidth - (tileSize / 2.0);
            }

            // does the provided supertile align to a tile?
            // i.e. how many tiles away before the super tile starts
            var gapInTiles = superTileOffset / (double)tileSize;
            if (gapInTiles == 0.0)
            {
                // then super tile is aligned
                return 0;
            }
            else
            {
                // super tile is not aligned

                // support cases where there is only one super tile per layer
                var tilesInSuperTile = double.NaN; // TODO:
                if (false)
                {
                    // TODO;
                }
                else
                {
                    throw new NotSupportedException(
                        "The super tile is not aligned to a a tile boundrary, don't know how to proceed");
                }
            }
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
            var scales = xScales.Zip(yScales, Tuple.Create);
            foreach (var scale in scales)
            {
                int xLayerLength, yLayerLength;
                int xTiles, yTiles;
                double xNormalizedScale, yNormalizedScale;

                CalculateScaleStats(xUnitScale, unitWidth, this.profile.TileWidth, scale.Item1, out xNormalizedScale, out xLayerLength, out xTiles);
                CalculateScaleStats(yUnitScale, unitHeight, this.profile.TileHeight, scale.Item2, out yNormalizedScale, out yLayerLength, out yTiles);

                results.Add(
                    new Layer {
                            XNormalizedScale = xNormalizedScale,
                            YNormalizedScale = yNormalizedScale,
                            XScale = scale.Item1, 
                            Width = xLayerLength, 
                            Height = yLayerLength, 
                            ScaleIndex = scaleIndex, 
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