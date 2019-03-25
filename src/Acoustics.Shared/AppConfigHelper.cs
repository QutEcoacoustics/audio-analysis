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
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using ConfigFile;
    using log4net;
    using Microsoft.Extensions.Configuration;

    public static class AppConfigHelper
    {
        public const string DefaultTargetSampleRateKey = "DefaultTargetSampleRate";

        /// <summary>
        /// Warning: do not use this format to print dates as strings - it will include a colon in the time zone offset :-(
        /// </summary>
        public const string Iso8601FileCompatibleDateFormat = "yyyyMMddTHHmmsszzz";
        public const string Iso8601FileCompatibleDateFormatUtcWithFractionalSeconds = "yyyyMMddTHHmmss.FFF\\Z";
        public const string StandardDateFormatUtc = "yyyyMMdd-HHmmssZ";
        public const string StandardDateFormatUtcWithFractionalSeconds = "yyyyMMdd-HHmmss.FFFZ";
        public const string StandardDateFormat = "yyyyMMdd-HHmmsszzz";
        public const string StandardDateFormatNoTimeZone = "yyyyMMdd-HHmmss";
        public const string StandardDateFormatUnderscore = "yyyyMMdd_HHmmsszzz";
        public const string StandardDateFormatSm2 = "yyyyMMdd_HHmmss";

        private static readonly string ExecutingAssemblyPath =
            (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;

        private static readonly Lazy<IConfigurationSection> SharedSettings = new Lazy<IConfigurationSection>(() =>
        {
            var settings = AppConfiguration.Value.GetSection("appSettings");
            if (!settings.AsEnumerable().Any())
            {
                throw new ConfigurationErrorsException("Could not read AP.Settings.json - no values were found in the config file");
            }

            return settings;
        });

        private static readonly ILog Log = LogManager.GetLogger(nameof(AppConfigHelper));
        private static readonly bool IsLinuxValue;
        private static readonly bool IsWindowsValue;
        private static readonly bool IsMacOsXValue;

        static AppConfigHelper()
        {
            IsMono = Type.GetType("Mono.Runtime") != null;
            CheckOs(ref IsWindowsValue, ref IsLinuxValue, ref IsMacOsXValue);
        }

        public static Lazy<IConfigurationRoot> AppConfiguration { get; } = new Lazy<IConfigurationRoot>(() =>
            new ConfigurationBuilder()
                .AddJsonFile("AP.Settings.json")
                .Build());

        public static int DefaultTargetSampleRate => GetInt(DefaultTargetSampleRateKey);

        public static string ExecutingAssemblyDirectory { get; } = Path.GetDirectoryName(ExecutingAssemblyPath);

        public static string FileDateFormatUtc
        {
            get
            {
                var dateFormat = GetString("StandardFileDateFormatUtc");
                return dateFormat.IsNotWhitespace() ? dateFormat : StandardDateFormatUtc;
            }
        }

        public static string FileDateFormat
        {
            get
            {
                var dateFormat = GetString("StandardFileDateFormat");
                return dateFormat.IsNotWhitespace() ? dateFormat : StandardDateFormat;
            }
        }

        public static string FileDateFormatSm2
        {
            get
            {
                var dateFormat = GetString("StandardFileDateFormatSm2");
                return dateFormat.IsNotWhitespace() ? dateFormat : StandardDateFormatSm2;
            }
        }

        /// <summary>
        /// Gets FfmpegExe.
        /// </summary>
        public static string FfmpegExe => GetExeFile("AudioUtilityFfmpegExe");

        /// <summary>
        /// Gets FfmpegExe.
        /// </summary>
        public static string FfprobeExe => GetExeFile("AudioUtilityFfprobeExe");

        /// <summary>
        /// Gets WvunpackExe.
        /// </summary>
        public static string WvunpackExe => GetExeFile("AudioUtilityWvunpackExe");

        /// <summary>
        /// Gets Mp3SpltExe.
        /// </summary>
        public static string Mp3SpltExe => GetExeFile("AudioUtilityMp3SpltExe", false);

        /// <summary>
        /// Gets ShntoolExe.
        /// </summary>
        public static string ShntoolExe => GetExeFile("AudioUtilityShntoolExe", false);

        /// <summary>
        /// Gets SoxExe.
        /// </summary>
        public static string SoxExe => GetExeFile("AudioUtilitySoxExe");

        /// <summary>
        /// Gets Wav2PngExe.
        /// </summary>
        public static string Wav2PngExe => GetExeFile("AudioUtilityWav2PngExe");

        /// <summary>
        /// Gets the directory of the QutSensors.Shared.dll assembly.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">
        /// Could not get directory.
        /// </exception>
        public static DirectoryInfo AssemblyDir
        {
            get
            {
                var assemblyDirString = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (!string.IsNullOrEmpty(assemblyDirString))
                {
                    var assemblyDir = new DirectoryInfo(assemblyDirString);

                    if (!Directory.Exists(assemblyDir.FullName))
                    {
                        throw new DirectoryNotFoundException("Could not find assembly directory: " + assemblyDir.FullName);
                    }

                    return assemblyDir;
                }

                throw new Exception("Cannot get assembly directory.");
            }
        }

        public static bool IsMono { get; }

        public static bool IsLinux => IsLinuxValue;

        public static bool IsWindows => IsWindowsValue;

        public static bool IsMacOsX => IsMacOsXValue;

        public static string GetString(string key)
        {
            var value = SharedSettings.Value[key];

            if (string.IsNullOrEmpty(value))
            {
                throw new ConfigurationErrorsException("Could not find appSettings key or it did not have a value: " + key);
            }

            return value;
        }

        public static bool GetBool(string key)
        {
            var value = GetString(key);

            if (bool.TryParse(value, out var valueParsed))
            {
                return valueParsed;
            }

            throw new ConfigurationErrorsException(
                "Key " + key + " exists but could not be converted to a bool: " + value);
        }

        public static int GetInt(string key)
        {
            var value = GetString(key);

            if (int.TryParse(value, out var valueParsed))
            {
                return valueParsed;
            }

            throw new ConfigurationErrorsException(
                "Key " + key + " exists but could not be converted to a int: " + value);
        }

        public static double GetDouble(string key)
        {
            var value = GetString(key);

            if (double.TryParse(value, out var valueParsed))
            {
                return valueParsed;
            }

            throw new ConfigurationErrorsException(
                "Key " + key + " exists but could not be converted to a double: " + value);
        }

        public static DirectoryInfo GetDir(string key, bool checkExists)
        {
            var value = GetString(key);

            if (checkExists && !Directory.Exists(value))
            {
                throw new DirectoryNotFoundException($"Could not find directory: {key} = {value}");
            }

            return new DirectoryInfo(value);
        }

        public static FileInfo GetFile(string key, bool checkExists)
        {
            var value = GetString(key);

            if (checkExists && !File.Exists(value))
            {
                throw new FileNotFoundException($"Could not find file: {key} = {value}");
            }

            return new FileInfo(value);
        }

        /// <summary>
        /// Get the value for a key as one or more files.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="checkAnyExist">
        /// The check any exist.
        /// </param>
        /// <param name="separators">
        /// The separators.
        /// </param>
        /// <returns>
        /// The specified file, if it exists
        /// </returns>
        public static IEnumerable<FileInfo> GetFiles(string key, bool checkAnyExist, params string[] separators)
        {
            var value = GetString(key);
            var values = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            var files = values.Where(v => !string.IsNullOrEmpty(v)).Select(v => new FileInfo(v)).ToList();

            if (checkAnyExist && files.All(f => !File.Exists(f.FullName)))
            {
                throw new FileNotFoundException("None of the given files exist: " + string.Join(", ", files.Select(f => f.FullName)));
            }

            return files;
        }

        public static long GetLong(string key)
        {
            var value = GetString(key);

            if (long.TryParse(value, out var valueParsed))
            {
                return valueParsed;
            }

            throw new ConfigurationErrorsException(
                "Key " + key + " exists but could not be converted to a long: " + value);
        }

        /// <summary>
        /// Adapted from https://stackoverflow.com/a/38795621/224512
        /// </summary>
        private static void CheckOs(ref bool isWindows, ref bool isLinux, ref bool isMacOsX)
        {
            var windir = Environment.GetEnvironmentVariable("windir");
            if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
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
                throw new PlatformNotSupportedException("Unkown platform");
            }
        }

        private static string GetExeFile(string appConfigKey, bool required = true)
        {
            string key;

            if (IsMacOsX)
            {
                key = appConfigKey + "MacOsX";
            }
            else if (IsLinux)
            {
                key = appConfigKey + "Linux";
            }
            else
            {
                key = appConfigKey;
            }

            var path = SharedSettings.Value[key];

            Log.Verbose($"Attempted to get exe path `{appConfigKey}`. Value: '{path}'");

            if (!path.IsNullOrEmpty())
            {
                return Path.IsPathRooted(path) ? path : Path.Combine(AssemblyDir.FullName, path);
            }

            if (required)
            {
                throw new ConfigFileException($"An exe path for `{key}` was not found or it's value is empty in AP.Settings.json");
            }

            Log.Debug($"No key found for `{key}` in the AP.Settings.config. This program may fail if this binary is needed.");
            return null;
        }
    }
}
