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

    public class Tiler
    {
        private readonly DirectoryInfo outputDirectory;
        private readonly TilingProfile profile;
        private readonly SortedSet<Layer> calculatedLayers;

        public Tiler(DirectoryInfo outputDirectory, TilingProfile profile, SortedSet<decimal> scales, decimal unitScale, int unitWidth, int unitHeight)
        {
            this.outputDirectory = outputDirectory;
            this.profile = profile;

            this.calculatedLayers = this.CalculateLayers(scales, unitScale, unitWidth, unitHeight);
        }

        public void TileMany(IEnumerable<SuperTile> allSuperTiles)
        {
            foreach (var superTile in allSuperTiles)
            {

                this.Tile(superTile.Image, (decimal)superTile.Scale.TotalSeconds, new Point(0, 0) /*offset*/);
            }
        }

        /// <summary>
        /// Split one large image (a super tile) into smaller tiles
        /// </summary>
        /// <param name="image"></param>
        /// <param name="scale"></param>
        /// <param name="offsets">The (top, left) point of the tile within the full space of the layer</param>
        public void Tile(Image image, decimal scale, Point offsets)
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
            decimal paddingX = (decimal)xOffset / this.profile.TileWidth;


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
                    var subsection = new Rectangle();
                    var tileImage = (Bitmap)((Bitmap)image).Clone(subsection, image.PixelFormat);




                    // write tile to disk
                    var name = this.profile.GetFileBaseName();
                    var outputTilePath = this.outputDirectory.CombineFile(name + ".png").FullName;
                    tileImage.Save(outputTilePath);
                }
            }
        }

        private SortedSet<Layer> CalculateLayers(SortedSet<decimal> scales, decimal unitScale, int unitWidth, int unitHeight)
        {
            var results = new SortedSet<Layer>();
            int scaleIndex = 0;
            foreach (var scale in scales)
            {
                var normalizedScale = unitScale / scale;
                int layerWidth = (int)(unitWidth * normalizedScale), 
                    layerHeight = (int)(unitHeight * normalizedScale);

                decimal tilesForWidth = (decimal)layerWidth / this.profile.TileWidth,
                        tilesForHeight = (decimal)layerHeight / this.profile.TileHeight;

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
