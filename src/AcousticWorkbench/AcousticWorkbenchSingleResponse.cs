﻿// <copyright file="AcousticWorkbenchSingleResponse.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    public class AcousticWorkbenchSingleResponse<T>
        : AcousticWorkbenchResponse<T>
    {
        public T Data { get; set; }
    }
}