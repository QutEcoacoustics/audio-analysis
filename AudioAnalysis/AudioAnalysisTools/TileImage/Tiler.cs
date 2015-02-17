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
    using System.IO;
    using System.Linq;
    using System.Text;

    using AudioAnalysisTools.LongDurationSpectrograms;

    public class Tiler
    {
        public Tiler(DirectoryInfo outputDirectory, TileNamingPattern namingPattern)
        {
            
        }



        public void Tile(IEnumerable<SuperTile> allSuperTiles)
        {
            // order results, output to array

            // get layer names

            // loop thrrough layers

                // get indexed names

                // write subset of image
        }
    }
}
