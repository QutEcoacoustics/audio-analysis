namespace Dong.Felt.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CompressSpectrogramConfig
    {
        /// <summary>
        /// take 1 pixel every compressStep according to some rule.
        /// </summary>
        public double TimeCompressRate { get; set; }

        public double FreqCompressRate { get; set; }

    }
}
