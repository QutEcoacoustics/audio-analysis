// <copyright file="NotExistingFileAttribute.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Validation
{
    public class NotExistingFileAttribute : ExistingFileAttribute
    {
        public NotExistingFileAttribute()
            : base(false, false)
        {
        }
    }
}