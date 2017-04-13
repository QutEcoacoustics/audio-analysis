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
