// <copyright file="IConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ConfigFile
{
    using System;

    /// <summary>
    /// Represents a configuration object which at a minimum has path to the config
    /// file that was loaded and a callback for loaded events.
    /// </summary>
    public interface IConfig
    {
        event Action<IConfig> Loaded;

        string ConfigPath { get; set; }

        void InvokeLoaded();
    }
}