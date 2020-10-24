// <copyright file="BlobParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using Acoustics.Shared;

    /// <summary>
    /// Parameters needed from a config file to detect blob components.
    /// The following parameters worked well on a ten minute recording containing 14-16 calls.
    /// Note: if you lower the dB threshold, you need to increase maxDurationSeconds.
    /// </summary>
    [YamlTypeTag(typeof(BlobParameters))]
    public class BlobParameters : CommonParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobParameters"/> class.
        /// </summary>
        public BlobParameters()
        {
            this.MinHertz = 800;
            this.MaxHertz = 8000;
        }
    }
}