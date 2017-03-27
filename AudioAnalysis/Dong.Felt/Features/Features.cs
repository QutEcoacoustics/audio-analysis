namespace Dong.Felt.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AudioAnalysisTools.DSP;
    using Dong.Felt.Representations;
    using TowseyLibrary;

    public class Feature
    {
        /// <summary>
        /// This is feature 1, called histogram of gradients from MIML paper written by Briggs.
        /// gets or sets the orientationHistogram for a neighbourhood. The length of it can vary, depending on the number of historgram bins divided.
        /// </summary>
        public List<int> orientationHistogram { get; set; }

        /// <summary>
        /// This is feature 2, called distance matrix from partial matching written by Eli Saber.
        /// </summary>
        public double featureBlockMatch{ get; set; }

        /// <summary>
        /// This is feature 3, just orientation of neighbourhood.
        /// </summary>
        public List<double> orientations { get; set; }

        /// <summary>
        /// This is feature 4, the magnitude of neighbourhood.
        /// </summary>
        public List<double> magnitudes { get; set; }

        /// <summary>
        /// This is feature 5, the MFCC fecture vector for each spectrogram segment.
        /// </summary>
        public double[] MFCC {get; set;}

        public Feature(double[,] spectra)
        {
            //set up the matrix of cosine coefficients
            int coeffCount = 12; //only use first 12 coefficients.
            int binCount = spectra.GetLength(1);  //number of filters in filter bank
            double[,] cosines = MFCCStuff.Cosines(binCount, coeffCount + 1); //set up the cosine coefficients

            //spectra is a double array [spectra.GetLength(0), coeffCount]
            spectra = MFCCStuff.Cepstra(spectra, coeffCount, cosines);

            double[] v2 = DataTools.Matrix2Array(spectra);
            v2 = DataTools.normalise2UnitLength(v2);
            MFCC = v2;
        }


        public Feature(List<RegionRepresentation> region)
        {
            var histogram = new int[8];
            foreach (var r in region)
            {
                if(r.magnitude != 100)
                {
                if (r.orientation >= -4 * Math.PI / 8 && r.orientation < -3 * Math.PI / 8)
                {
                    histogram[0]++;
                }
                if (r.orientation >= -3 * Math.PI / 8 && r.orientation < -2 * Math.PI / 8)
                {
                    histogram[1]++;
                }
                if (r.orientation >= -2 * Math.PI / 8 && r.orientation < -Math.PI / 8)
                {
                    histogram[2]++;
                }
                if (r.orientation >= -Math.PI / 8 && r.orientation < 0)
                {
                    histogram[3]++;
                }
                if (r.orientation >= 0 && r.orientation < Math.PI / 8)
                {
                    histogram[4]++;
                }
                if (r.orientation >= Math.PI / 8 && r.orientation < 2 * Math.PI / 8)
                {
                    histogram[5]++;
                }
                if (r.orientation >= 2 * Math.PI / 8 && r.orientation < 3 * Math.PI / 8)
                {
                    histogram[6]++;
                }
                if (r.orientation >= 3 * Math.PI / 8 && r.orientation < 4 * Math.PI / 8)
                {
                    histogram[7]++;
                }
                //this.magnitudes.Add(r.magnitude);
                //this.orientations.Add(r.orientation);
                }
            }
            var histogramList = new List<int>();
            for (int i = 0; i < histogram.Count(); i++)
            {
                histogramList.Add(histogram[i]);
            }
            this.orientationHistogram = histogramList;
        }

        public Feature(List<RegionRepresentation> region, List<RegionRepresentation> secondRegion)
        {
            var regionFeatureBlockIndicator = new List<int>();
            var secondRegionFeatureBlockIndicator = new List<int>();
            for (int i = 0; i < region.Count; i++)
            {
                if (region != null)
                {
                    var poiCountThreshold = (int)(0.15 * Math.Pow(region[0].neighbourhoodSize, 2));
                    if (region[i].magnitude != 100 && region[i].POICount > poiCountThreshold)
                    {
                        regionFeatureBlockIndicator.Add(1);
                    }
                    else
                    {
                        regionFeatureBlockIndicator.Add(0);
                    }
                }
            }

            for (int i = 0; i < secondRegion.Count; i++)
            {
                if (secondRegion != null)
                {
                    var poiCountThreshold = (int)(0.15 * Math.Pow(secondRegion[0].neighbourhoodSize, 2));
                    if (secondRegion[i].magnitude != 100 && secondRegion[i].POICount > poiCountThreshold)
                    {
                        secondRegionFeatureBlockIndicator.Add(1);
                    }
                    else
                    {
                        secondRegionFeatureBlockIndicator.Add(0);
                    }
                }
            }

            // to get the match proportion.
            var listLength = region.Count;
            var blockMatchCount = 0;
            for (int k = 0; k < listLength; k++)
            {
                if (regionFeatureBlockIndicator[k] == secondRegionFeatureBlockIndicator[k])
                {
                    blockMatchCount++;
                }
            }
            this.featureBlockMatch = (double)blockMatchCount / listLength;
        }

    }
}
