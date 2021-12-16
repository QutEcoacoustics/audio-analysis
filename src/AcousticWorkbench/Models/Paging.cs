// <copyright file="Paging.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    public class Paging
    {
        public int Page { get; set; }

        public int Items { get; set; }

        public int Total { get; set; }

        public int MaxPage { get; set; }
    }
}