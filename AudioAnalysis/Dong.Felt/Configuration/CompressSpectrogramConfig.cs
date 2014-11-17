using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt.Configuration
{
    public class CompressSpectrogramConfig
    {
        /// <summary>
        /// take 1 pixel every compressStep according to some rule.
        /// </summary>
        public double CompressRate { get; set; }
    }
}
