using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisBase.ResultBases
{
    public class SpectrumBase : ResultBase
    {

        public double[] DummySpectrum { get; set; }

        public string[] Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }


}