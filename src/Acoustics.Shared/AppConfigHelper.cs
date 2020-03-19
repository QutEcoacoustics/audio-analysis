// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppConfigHelper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the AppConfigHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using log4net;
    using Mono.Unix.Native;
    using static System.Runtime.InteropServices.OSPlatform;
    using static System.Runtime.InteropServices.RuntimeInformation;

    public static class AppConfigHelper
    {
        /// <summary>
        /// Warning: do not use this format to print dates as strings - it will include a colon in the time zone offset :-(.
        /// </summary>
        public const string Iso8601FileCompatibleDateFormat = "yyyyMMddTHHmmsszzz";
        public const string Iso8601FileCompatibleDateFormatUtcWithFractionalSeconds = "yyyyMMddTHHmmss.FFF\\Z";
        public const string Iso8601FormatNoFractionalSeconds = "yyyy-MM-ddTHH:mm:sszzz";
        public const string StandardDateFormatUtc = "yyyyMMdd-HHmmssZ";
        public const string StandardDateFormatUtcWithFractionalSeconds = "yyyyMMdd-HHmmss.FFFZ";
        public const string StandardDateFormat = "yyyyMMdd-HHmmsszzz";
        public const string StandardDateFormatNoTimeZone = "yyyyMMdd-HHmmss";
        public const string StandardDateFormatUnderscore = "yyyyMMdd_HHmmsszzz";
        public const string StandardDateFormatSm2 = "yyyyMMdd_HHmmss";
        public const string RenderedDateFormatShort = "yyyy-MM-dd HH:mm";

        public const string WinX64 = "win-x64";
        public const string WinArm64 = "win-arm64";
        public const string OsxX64 = "osx-x64";
        public const string LinuxX64 = "linux-x64";
        public const string LinuxMuslX64 = "linux-musl-x64";
        public const string LinuxArm = "linux-arm";
        public const string LinuxArm64 = "linux-arm64";

        public const int DefaultTargetSampleRate = 22050;

        public const bool WasBuiltAgainstMusl =
#if BUILT_AGAINST_MUSL
            true;
#else
            false;
#endif

        public static readonly IImmutableSet<string> WellKnownRuntimeIdentifiers = ImmutableHashSet.Create(
            WinX64,
            WinArm64,
            OsxX64,
            LinuxX64,
            LinuxMuslX64,
            LinuxArm,
            LinuxArm64);

        private const string Ffmpeg = "ffmpeg";
        private const string Ffprobe = "ffprobe";
        private const string Wvunpack = "wvunpack";
        private const string Sox = "sox";

        private static readonly string ExecutingAssemblyPath =
            Assembly.GetAssembly(typeof(AppConfigHelper)).Location;

        private static readonly ILog Log = LogManager.GetLogger(typeof(AppConfigHelper));
        private static readonly bool IsLinuxValue;
        private static readonly bool IsWindowsValue;
        private static readonly bool IsMacOsXValue;
        private static string resolvedFfmpegExe;
        private static string resolvedFfprobeExe;
        private static string resolvedSoxExe;
        private static string resolvedWvunpackExe;

        static AppConfigHelper()
        {
            CheckOs(ref IsWindowsValue, ref IsLinuxValue, ref IsMacOsXValue);
        }

        public static string ExecutingAssemblyDirectory { get; } = Path.GetDirectoryName(ExecutingAssemblyPath);

        /// <summary>
        /// Gets FfmpegExe.
        /// </summary>
        public static string FfmpegExe => resolvedFfmpegExe ??= GetExeFile(Ffmpeg);

        /// <summary>
        /// Gets FfmpegExe.
        /// </summary>
        public static string FfprobeExe => resolvedFfprobeExe ??= GetExeFile(Ffprobe);

        /// <summary>
        /// Gets WvunpackExe.
        /// </summary>
        public static string WvunpackExe => resolvedWvunpackExe ??= GetExeFile(Wvunpack, required: false);

        /// <summary>
        /// Gets SoxExe.
        /// </summary>
        public static string SoxExe => resolvedSoxExe ??= GetExeFile(Sox);

        /// <summary>
        /// Gets a value indicating whether we are running on the Mono platform.
        /// This should always be false.
        /// </summary>
        public static bool IsMono { get; } = Type.GetType("Mono.Runtime") != null;

        /// <summary>
        /// Gets a value indicating whether the current operating system is a linux variant.
        /// Note: this property actually tests what operating system we're using, unlike
        /// <see cref="RuntimeInformation.IsOSPlatform"/> which uses values from the ***build**.
        /// </summary>
        public static bool IsLinux => IsLinuxValue;

        /// <summary>
        /// Gets a value indicating whether the current operating system is a Windows variant.
        /// Note: this property actually tests what operating system we're using, unlike
        /// <see cref="RuntimeInformation.IsOSPlatform"/> which uses values from the ***build**.
        /// </summary>
        public static bool IsWindows => IsWindowsValue;

        /// <summary>
        /// Gets a value indicating whether the current operating system is a OSX variant.
        /// Note: this property actually tests what operating system we're using, unlike
        /// <see cref="RuntimeInformation.IsOSPlatform"/> which uses values from the ***build**.
        /// </summary>
        public static bool IsMacOsX => IsMacOsXValue;

        /// <summary>
        /// Gets a pseudo (fake) runtime identifier from the information available to us.
        /// This value represents a ***build** time construct.
        /// </summary>
        /// <remarks>
        /// Note: these are not real .NET RIDs... but they're meant to simulate them.
        /// </remarks>
        /// <returns>An rid-like string.</returns>
        public static string PseudoRuntimeIdentifier { get; } =
            OSArchitecture switch
            {
                Architecture.X64 when IsOSPlatform(Windows) => WinX64,
                Architecture.X64 when IsOSPlatform(Linux) => LinuxX64,
                Architecture.X64 when IsOSPlatform(OSX) => OsxX64,
                Architecture.Arm64 when IsOSPlatform(Windows) => WinArm64,

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable 162
                Architecture.Arm64 when IsOSPlatform(Linux) && WasBuiltAgainstMusl => LinuxMuslX64,
#pragma warning restore 162
                Architecture.Arm64 when IsOSPlatform(Linux) => LinuxArm64,
                Architecture.Arm when IsOSPlatform(Linux) => LinuxArm,

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // ReSharper disable once UnreachableCode
#pragma warning disable 162
                _ => throw new PlatformNotSupportedException($"{Meta.Name} has not been configured to work with {OSDescription} {OSArchitecture}{(WasBuiltAgainstMusl ? " musl" : string.Empty)}"),
#pragma warning restore 162
            };

        internal static string GetExeFile(string name, bool required = true)
        {
            var isWindows = IsOSPlatform(Windows);

            (string directory, string osName) = name switch
            {
                Ffmpeg => (name, isWindows ? $"{name}.exe" : name),
                Ffprobe => (Ffmpeg, isWindows ? $"{name}.exe" : name),
                Sox => (name, isWindows ? $"{name}.exe" : name),
                Wvunpack => ("wavpack", isWindows ? $"{name}.exe" : name),
                _ => throw new ArgumentException("Executable not supported", nameof(name)),
            };

            string rid = PseudoRuntimeIdentifier;

            string executablePath = Path.Join(ExecutingAssemblyDirectory, "audio-utils", rid, directory, osName);

            if (!File.Exists(executablePath))
            {
                Log.Verbose($"Attempted to get exe path `{executablePath}` but it was not found");

                executablePath = FindProgramInPath(name);
            }

            if (executablePath != null)
            {
                if (Log.IsVerboseEnabled())
                {
                    Log.Verbose($"Found and using exe {executablePath}");
                }

                CheckForExecutePermission(executablePath);

                return executablePath;
            }

            if (required)
            {
                throw new FileNotFoundException($"Could not find {name} audio-utils or in the system. Please install {name}.");
            }

            Log.Verbose($"No audio tool found for {name}. This program may fail if this binary is needed.");
            return null;
        }

        internal static void CheckForExecutePermission(string executablePath)
        {
            // check we have required permissions
            if (!IsOSPlatform(Windows))
            {
                Syscall.stat(executablePath, out var stat);
                FilePermissions filePerms = stat.st_mode & FilePermissions.ALLPERMS;
                const FilePermissions execute = FilePermissions.S_IXUSR | FilePermissions.S_IXGRP | FilePermissions.S_IXOTH;
                if ((filePerms & execute) == 0)
                {
                    throw new UnauthorizedAccessException(
                        $"The executable file `{executablePath}` does not have any execute permissions set. Please `chmod a+x {executablePath}`");
                }
            }
        }

        /// <summary>
        /// Gets the path to the program.
        /// </summary>
        /// <remarks>
        /// Copied from https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.Process/src/System/Diagnostics/Process.Unix.cs#L727.
        /// </remarks>
        /// <param name="program">The program to search for.</param>
        /// <returns>The path if the file is found, otherwise null.</returns>
        internal static string FindProgramInPath(string program)
        {
            string pathEnvVar = Environment.GetEnvironmentVariable(IsWindows ? "Path" : "PATH");
            if (pathEnvVar != null)
            {
                var pathParser = pathEnvVar.Split(':', StringSplitOptions.RemoveEmptyEntries);
                foreach (var subPath in pathParser)
                {
                    var path = Path.Combine(subPath, program);
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            Log.Verbose($"Attempted to search for exe on system Path but it was not found. System path: \n{pathEnvVar}");
            return null;
        }

        /// <summary>
        /// Adapted from https://stackoverflow.com/a/38795621/224512.
        /// </summary>
        private static void CheckOs(ref bool isWindows, ref bool isLinux, ref bool isMacOsX)
        {
            var winDir = Environment.GetEnvironmentVariable("windir");
            if (!string.IsNullOrEmpty(winDir) && winDir.Contains(@"\") && Directory.Exists(winDir))
            {
                isWindows = true;
            }
            else if (File.Exists(@"/proc/sys/kernel/ostype"))
            {
                var osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
                {
                    // Note: Android gets here too
                    isLinux = true;
                }
                else
                {
                    throw new PlatformNotSupportedException(osType);
                }
            }
            else if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
            {
                // Note: iOS gets here too
                isMacOsX = true;
            }
            else
            {
                throw new PlatformNotSupportedException("Unknown platform");
            }
        }
    }
}
