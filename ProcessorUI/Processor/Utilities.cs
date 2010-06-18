// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Memory utilities.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.Processor
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Memory utilities.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Minimise Memory.
        /// </summary>
        public static void MinimizeMemory()
        {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(
                Process.GetCurrentProcess().Handle,
                (UIntPtr)0xFFFFFFFF,
                (UIntPtr)0xFFFFFFFF);

            IntPtr heap = GetProcessHeap();

            if (HeapLock(heap))
            {
                try
                {
                    if (HeapCompact(heap, 0) == 0)
                    {
                        // error condition ignored
                    }
                }
                finally
                {
                    HeapUnlock(heap);
                }
            }
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetProcessWorkingSetSize(
            IntPtr process,
            UIntPtr minimumWorkingSetSize,
            UIntPtr maximumWorkingSetSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcessHeap();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HeapLock(IntPtr heap);

        [DllImport("kernel32.dll")]
        internal static extern uint HeapCompact(IntPtr heap, uint flags);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HeapUnlock(IntPtr heap);
    }
}