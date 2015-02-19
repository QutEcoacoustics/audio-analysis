// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tiler.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the Tiler type.
// </summary>
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

        public Tiler(DirectoryInfo outputDirectory, TilingProfile profile, SortedSet<double> scales, double unitScale, int unitWidth, int unitHeight)
        {
            this.outputDirectory = outputDirectory;
            this.profile = profile;

            this.calculatedLayers = this.CalculateLayers(scales, unitScale, unitWidth, unitHeight);
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
        /// <param name="image"></param>
        /// <param name="scale"></param>
        /// <param name="offsets">The (top, left) point of the tile within the full space of the layer</param>
        public void Tile(Image image, double scale, Point offsets)
        {
            Contract.Ensures(image != null);

            var layer = this.calculatedLayers.First(x => x.Scale == scale);

            int width = image.Width,
            height = image.Height,
            xOffset = offsets.X,
            yOffset = offsets.Y;

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
                    //       unclear what beaviour will occur, either:
                    //      a) excpetion - :-(
                    //      b) trimmed segment - not ideal, add empty space manually
                    //      c) full segment, with empty space - ideal! do nothing

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
                    var outputTilePath = this.outputDirectory.CombineFile(name + ".png").FullName;
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
        /// <param name="numberOfTilesNeededForLayer"></param>
        /// <param name="tileSize"></param>
        private int AlignSuperTileInLayer(int layerWidth, int numberOfTilesNeededForLayer, int tileSize, int superTileOffset, int superTileWidth)
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
                var tilesInSuperTile = Double.NaN; // TODO:
                if (false) // TODO;
                {

                }
                else
                {
                    throw new NotSupportedException(
                        "The super tile is not aligned to a a tile boundrary, don't know how to proceed");
                }
            }
        }

        private SortedSet<Layer> CalculateLayers(SortedSet<double> scales, double unitScale, int unitWidth, int unitHeight)
        {
            var results = new SortedSet<Layer>();
            int scaleIndex = 0;
            foreach (var scale in scales)
            {
                var normalizedScale = unitScale / scale;
                int layerWidth = (int)(unitWidth * normalizedScale), 
                    layerHeight = (int)(unitHeight * normalizedScale);

                double tilesForWidth = (double)layerWidth / this.profile.TileWidth,
                        tilesForHeight = (double)layerHeight / this.profile.TileHeight;

                int tileCountX = (int)Math.Ceiling(tilesForWidth), tileCountY = (int)Math.Ceiling(tilesForHeight);

                results.Add(
                    new Layer()
                        {
                            NormalisedScale = normalizedScale,
                            Scale = scale,
                            Width = layerWidth,
                            Height = layerHeight,
                            ScaleIndex = scaleIndex,
                            XTiles = tileCountX,
                            YTiles = tileCountY
                        });

                scaleIndex++;
            }

            return results;
        }


    }
}
