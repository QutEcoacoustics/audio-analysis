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
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using log4net;

    public static class AppConfigHelper
    {
        public const string DefaultTargetSampleRateKey = "DefaultTargetSampleRate";

        public static int DefaultTargetSampleRate => GetInt(DefaultTargetSampleRateKey);

        /// <summary>
        /// Warning: do not use this format to print dates as strings - it will include a colon in the time zone offset :-(
        /// </summary>
        public const string Iso8601FileCompatibleDateFormat = "yyyyMMddTHHmmsszzz";

        public const string Iso8601FileCompatibleDateFormatUtcWithFractionalSeconds = "yyyyMMddTHHmmss.FFF\\Z";

        public const string StandardDateFormatUtc = "yyyyMMdd-HHmmssZ";

        public const string StandardDateFormatUtcWithFractionalSeconds = "yyyyMMdd-HHmmss.FFFZ";

        private static readonly string ExecutingAssemblyPath =
            (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;


        private static readonly KeyValueConfigurationCollection SharedSettings;

        private static readonly ILog Log = LogManager.GetLogger(nameof(AppConfigHelper));

        private static readonly bool IsLinuxValue;
        private static readonly bool IsWindowsValue;
        private static readonly bool IsMacOsXValue;

        static AppConfigHelper()
        {
            ExeConfigurationFileMap exeConfigurationFileMap = new ExeConfigurationFileMap();
            exeConfigurationFileMap.ExeConfigFilename = Path.Combine(ExecutingAssemblyDirectory, "AP.Settings.Config");
            var sharedConfig = ConfigurationManager.OpenMappedExeConfiguration(exeConfigurationFileMap, ConfigurationUserLevel.None);
            SharedSettings = sharedConfig.AppSettings.Settings;

            IsMono = Type.GetType("Mono.Runtime") != null;
            CheckOs(ref IsWindowsValue, ref IsLinuxValue, ref IsMacOsXValue);

        }

        public static string ExecutingAssemblyDirectory { get; } = Path.GetDirectoryName(ExecutingAssemblyPath);

        /// <summary>
        /// Adapted from https://stackoverflow.com/a/38795621/224512
        /// </summary>
        private static void CheckOs(ref bool isWindows, ref bool isLinux, ref bool isMacOsX)
        {
            string windir = Environment.GetEnvironmentVariable("windir");
            if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
            {
                isWindows = true;
            }
            else if (File.Exists(@"/proc/sys/kernel/ostype"))
            {
                string osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
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

        public static string FileDateFormatUtc
        {
            get
            {
                var dateFormat = GetString("StandardFileDateFormatUtc");
                if (dateFormat.IsNotWhitespace())
                {
                    return dateFormat;
                }
                else
                {
                    return StandardDateFormatUtc;
                }
            }
        }

        public const string StandardDateFormat = "yyyyMMdd-HHmmsszzz";
        public const string StandardDateFormatNoTimeZone = "yyyyMMdd-HHmmss";
        public const string StandardDateFormatUnderscore = "yyyyMMdd_HHmmsszzz";

        public static string FileDateFormat
        {
            get
            {
                var dateFormat = GetString("StandardFileDateFormat");
                if (dateFormat.IsNotWhitespace())
                {
                    return dateFormat;
                }
                else
                {
                    return StandardDateFormat;
                }
            }
        }

        public const string StandardDateFormatSm2 = "yyyyMMdd_HHmmss";

        public static string FileDateFormatSm2
        {
            get
            {
                var dateFormat = GetString("StandardFileDateFormatSm2");
                if (dateFormat.IsNotWhitespace())
                {
                    return dateFormat;
                }
                else
                {
                    return StandardDateFormatSm2;
                }
            }
        }

        /// <summary>
        /// Gets FfmpegExe.
        /// </summary>
        public static string FfmpegExe
        {
            get
            {
                return GetExeFile("AudioUtilityFfmpegExe");
            }
        }

        /// <summary>
        /// Gets FfmpegExe.
        /// </summary>
        public static string FfprobeExe
        {
            get
            {
                return GetExeFile("AudioUtilityFfprobeExe");
            }
        }

        /// <summary>
        /// Gets WvunpackExe.
        /// </summary>
        public static string WvunpackExe
        {
            get
            {
                return GetExeFile("AudioUtilityWvunpackExe");
            }
        }

        /// <summary>
        /// Gets Mp3SpltExe.
        /// </summary>
        public static string Mp3SpltExe
        {
            get
            {
                return GetExeFile("AudioUtilityMp3SpltExe");
            }
        }

        /// <summary>
        /// Gets ShntoolExe.
        /// </summary>
        public static string ShntoolExe
        {
            get
            {
                return GetExeFile("AudioUtilityShntoolExe");
            }
        }

        /// <summary>
        /// Gets SoxExe.
        /// </summary>
        public static string SoxExe
        {
            get
            {
                return GetExeFile("AudioUtilitySoxExe");
            }
        }

        /// <summary>
        /// Gets Wav2PngExe.
        /// </summary>
        public static string Wav2PngExe
        {
            get
            {
                return GetExeFile("AudioUtilityWav2PngExe");
            }
        }

        /// <summary>
        /// Gets AnalysisProgramBaseDir.
        /// </summary>
        public static DirectoryInfo AnalysisProgramBaseDir
        {
            get
            {
                return GetDir("AnalysisProgramBaseDirectory", true);
            }
        }

        /// <summary>
        /// Gets AnalysisRunDir.
        /// </summary>
        public static DirectoryInfo AnalysisRunDir
        {
            get
            {
                return GetDir("AnalysisRunDirectory", true);
            }
        }

        /// <summary>
        /// Gets TargetSegmentSize.
        /// </summary>
        public static TimeSpan TargetSegmentSize
        {
            get
            {
                return TimeSpan.FromMilliseconds(GetDouble("TargetSegmentSizeMs"));
            }
        }

        /// <summary>
        /// Gets MinSegmentSize.
        /// </summary>
        public static TimeSpan MinSegmentSize
        {
            get
            {
                return TimeSpan.FromMilliseconds(GetDouble("MinSegmentSizeMs"));
            }
        }

        /// <summary>
        /// Gets LogDir.
        /// </summary>
        public static DirectoryInfo LogDir
        {
            get
            {
                return GetDir("LogDir", true);
            }
        }

        /// <summary>
        /// Gets UploadFolder.
        /// </summary>
        public static DirectoryInfo UploadFolder
        {
            get
            {
                return GetDir("UploadFolder", true);
            }
        }

        /// <summary>
        /// Gets the directory of the QutSensors.Shared.dll assembly.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">
        /// Could not get directory.
        /// </exception>
        /// <exception cref="Exception">
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
            if (!Contains(key))
            {
                //throw new ConfigurationErrorsException("Could not find key: " + key);
                return null;
            }

            var value = SharedSettings[key].Value;

            if (string.IsNullOrEmpty(value))
            {
                throw new ConfigurationErrorsException("Found key, but it did not have a value: " + key);
            }

            return value;
        }

        public static bool Contains(string key)
        {
            return SharedSettings.AllKeys.Contains(key);
        }

        //        public static IEnumerable<string> GetStrings(string key, params char[] separators)
        public static string[] GetStrings(string key, params char[] separators)
        {
            var value = GetString(key);
            var values = value
                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(v => !string.IsNullOrEmpty(v));

            if (!values.Any() || values.All(string.IsNullOrEmpty))
            {
                throw new ConfigurationErrorsException("Key " + key + " exists but does not have a value");
            }

            return values.ToArray();
        }

        public static bool GetBool(string key)
        {
            var value = GetString(key);

            bool valueParsed;
            if (bool.TryParse(value, out valueParsed))
            {
                return valueParsed;
            }

            throw new ConfigurationErrorsException(
                "Key " + key + " exists but could not be converted to a bool: " + value);
        }

        public static int GetInt(string key)
        {
            var value = GetString(key);

            int valueParsed;
            if (int.TryParse(value, out valueParsed))
            {
                return valueParsed;
            }

            throw new ConfigurationErrorsException(
                "Key " + key + " exists but could not be converted to a int: " + value);
        }

        public static double GetDouble(string key)
        {
            var value = GetString(key);

            double valueParsed;
            if (double.TryParse(value, out valueParsed))
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
                throw new DirectoryNotFoundException(string.Format("Could not find directory: {0} = {1}", key, value));
            }

            return new DirectoryInfo(value);
        }

        public static FileInfo GetFile(string key, bool checkExists)
        {
            var value = GetString(key);

            if (checkExists && !File.Exists(value))
            {
                throw new FileNotFoundException(string.Format("Could not find file: {0} = {1}", key, value));
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
        /// </returns>
        /// <exception cref="DirectoryNotFoundException">
        /// </exception>
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

            long valueParsed;
            if (long.TryParse(value, out valueParsed))
            {
                return valueParsed;
            }

            throw new ConfigurationErrorsException(
                "Key " + key + " exists but could not be converted to a long: " + value);
        }

        /// <summary>
        /// Get the cleaned path for a directory.
        /// </summary>
        /// <param name="webConfigRealDirectory">
        /// The web config real directory.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="checkAnyExist">
        /// The check and exist.
        /// </param>
        /// <param name="separators">
        /// The separators.
        /// </param>
        /// <returns>
        /// Enumerable of directories.
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// Directory was not found.
        /// </exception>
        public static IEnumerable<DirectoryInfo> GetDirs(string webConfigRealDirectory, string key, bool checkAnyExist, params string[] separators)
        {
            var value = GetString(key);

            var values = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            var dirs =
                values.Where(v => !string.IsNullOrEmpty(v)).Select(
                    v => v.StartsWith("..") ? new DirectoryInfo(webConfigRealDirectory + v) : new DirectoryInfo(v))
                    .ToList();

            if (checkAnyExist && dirs.All(d => !Directory.Exists(d.FullName)))
            {
                throw new DirectoryNotFoundException("None of the given directories exist: " + string.Join(", ", dirs.Select(a => a.FullName)));
            }

            return dirs;
        }

        public static IEnumerable<DirectoryInfo> GetDirs(string key, bool checkAnyExist, params string[] separators)
        {
            var value = GetString(key);

            var values = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            var dirs =
                values.Where(v => !string.IsNullOrEmpty(v)).Select(v => new DirectoryInfo(v)).ToList();

            if (checkAnyExist && dirs.All(d => !Directory.Exists(d.FullName)))
            {
                throw new DirectoryNotFoundException("None of the given directories exist: " + string.Join(", ", dirs.Select(a => a.FullName)));
            }

            return dirs;
        }

        private static string GetExeFile(string appConfigKey)
        {
            string path = null;
            string key = null;

            if (IsMacOsX)
            {
                key = appConfigKey + "MacOsX";
                path = GetString(key);
            }
            else if (IsLinux)
            {
                key = appConfigKey + "Linux";
                path = GetString(key);
            }
            else
            {
                key = appConfigKey;
                path = GetString(appConfigKey);
            }

            if (path.IsNullOrEmpty())
            {
                Log.Warn($"No key found for `{key}` in the App.Config. This program may fail if this binary is needed.");
                return null;
            }

            if (Path.IsPathRooted(path))
            {
                return path;
            }
            else
            {
                return Path.Combine(AssemblyDir.FullName, path);
            }
        }
    }
}
