// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectrumBase.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the SpectrumBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace AnalysisBase.ResultBases
{
    public class SpectrumBase : ResultBase
    {

        public double[] DummySpectrum { get; set; }


        /* Here's what I'm thinking:
         * Some kind of reflection magic that will allow scannign through all base classes
         * to get all properties that match a criteria.
         * 
         * The get and set spectrum methods will do dynamic casting.
         * 
         * Caching will be important.
         */

        public string[] Keys
        {
            get
            {

                throw new NotImplementedException();
            }
        }

        public T GetSpectrum<T>(string key)
        {
            
        }

        public void SetSpectrm<T>(string key, T[] spectrum)
        {
            
        }
    }


}