// <copyright file="Binary.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    public static class Binary
    {
        public static void Serialize<T>(FileInfo destination, T obj)
        {
            using (var stream = destination.Create())
            {
                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, obj);
            }
        }

        public static T Deserialize<T>(FileInfo source)
        {
            using (var stream = source.OpenRead())
            {
                BinaryFormatter formatter = new BinaryFormatter();

                object result = formatter.Deserialize(stream);

                return (T)result;
            }
        }
    }
}
