using AudioAnalysisTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt.Features
{
    public class Ridge
    {
        #region Public Properties

        public int RowIndex { get; set; }

        public int ColIndex { get; set; }

        /// <summary>
        /// Gets or sets the Ridge Magnitude at the given point.
        /// </summary>
        public double RidgeMagnitude { get; set; }

        
        /// <summary>
        /// Gets or sets the Local Ridge Orientation.
        /// </summary>
        public int OrientationCategory { get; set; }

        #endregion

        public static List<Ridge> FromPointOfInterest(List<PointOfInterest> poiList)
        {
            var result = new List<Ridge>();
            foreach (var p in poiList)
            {
            var ridge = new Ridge();
            ridge.RowIndex = p.Point.X;
            ridge.ColIndex = p.Point.Y;
            ridge.RidgeMagnitude = p.RidgeMagnitude;
                ridge.OrientationCategory = p.OrientationCategory;
           result.Add(ridge);
            }
            return result;
        }



    }
}
