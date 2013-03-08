using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisBase
{
    /// <summary>
    /// An interface for printing command line help
    /// </summary>
    public interface IUsage
    {
        /// <summary>
        /// The method that is called when help is needed
        /// </summary>
        /// <returns></returns>
        StringBuilder Usage(StringBuilder sb);
    }
}
