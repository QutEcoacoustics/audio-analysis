// <copyright file="INamedProfiles.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ConfigFile
{
    using System.Collections.Generic;

    /// <summary>
    /// Allows a <see cref="Config"/> class to have profiles.
    /// </summary>
    /// <typeparam name="T">The type of each expected profile.</typeparam>
    public interface INamedProfiles<T>
    {
        /// <summary>
        /// Gets a collection of named profiles that allow for
        /// variable and configurable algorithms to be used.
        /// </summary>
        Dictionary<string, T> Profiles { get; }
    }
}