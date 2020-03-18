// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the Utilities type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using static Acoustics.Shared.AppConfigHelper;

    public static class ProcessExtensions
    {
        /// <summary>
        /// A utility class to determine a process parent.
        /// http://stackoverflow.com/a/3346055.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ParentProcessUtilities
        {
#if DEBUG
            #pragma warning disable SA1310
            // These members must match PROCESS_BASIC_INFORMATION
            internal IntPtr Reserved1;

            internal IntPtr PebBaseAddress;

            internal IntPtr Reserved2_0;

            internal IntPtr Reserved2_1;

            internal IntPtr UniqueProcessId;

            internal IntPtr InheritedFromUniqueProcessId;
            #pragma warning restore

            // TODO This import is windows only, replace with .NET Core API
            [DllImport("ntdll.dll")]
            private static extern int NtQueryInformationProcess(
                IntPtr processHandle,
                int processInformationClass,
                ref ParentProcessUtilities processInformation,
                int processInformationLength,
                out int returnLength);

            #pragma warning disable SA1202
            /// <summary>
            /// !WARNING Windows OS only
            /// Gets the parent process of the current process.
            /// Linux support blocked by https://github.com/dotnet/runtime/issues/24423.
            /// </summary>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess()
            {
                return GetParentProcess(Process.GetCurrentProcess().Handle);
            }

            /// <summary>
            /// !WARNING Windows OS only
            /// Gets the parent process of specified process.
            /// </summary>
            /// <param name="id">The process id.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(int id)
            {
                Process process = Process.GetProcessById(id);
                return GetParentProcess(process.Handle);
            }

            /// <summary>
            /// !WARNING Windows OS only
            /// Gets the parent process of a specified process.
            /// </summary>
            /// <param name="handle">The process handle.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(IntPtr handle)
            {
                if (!IsWindows)
                {
                    return null;
                }

                ParentProcessUtilities pbi = default;

                int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out var _);
                if (status != 0)
                {
                    throw new Win32Exception(status);
                }

                try
                {
                    return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
                }
                catch (ArgumentException)
                {
                    // not found
                    return null;
                }
            }
            #pragma warning restore
#endif
        }
    }
}