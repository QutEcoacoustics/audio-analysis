// <copyright file="IApi.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IApi
    {
        string Host { get; }

        string Version { get; }

        string Protocol { get; }

        //string Uri { get; }
    }
}
