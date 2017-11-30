// <copyright file="ExceptionsExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace System
{
    using System;
    using Collections.Generic;
    using Linq;
    using Reflection;
    using Text;

    public static class ExceptionsExtensions
    {
        public static void PreserveStackTrace(this Exception exception)
        {
            MethodInfo preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace",
              BindingFlags.Instance | BindingFlags.NonPublic);
            preserveStackTrace.Invoke(exception, null);

        }
    }
}
