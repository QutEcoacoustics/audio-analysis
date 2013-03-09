namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;

    public static class AppConfigHelper
    {
        /// <summary>
        /// Gets FfmpegExe.
        /// </summary>
        public static string FfmpegExe
        {
            get
            {
                return GetString("AudioUtilityFfmpegExe");
            }
        }

        /// <summary>
        /// Gets FfmpegExe.
        /// </summary>
        public static string FfprobeExe
        {
            get
            {
                return GetString("AudioUtilityFfprobeExe");
            }
        }

        /// <summary>
        /// Gets WvunpackExe.
        /// </summary>
        public static string WvunpackExe
        {
            get
            {
                return GetString("AudioUtilityWvunpackExe");
            }
        }

        /// <summary>
        /// Gets Mp3SpltExe.
        /// </summary>
        public static string Mp3SpltExe
        {
            get
            {
                return GetString("AudioUtilityMp3SpltExe");
            }
        }

        /// <summary>
        /// Gets ShntoolExe.
        /// </summary>
        public static string ShntoolExe
        {
            get
            {
                return GetString("AudioUtilityShntoolExe");
            }
        }

        /// <summary>
        /// Gets SoxExe.
        /// </summary>
        public static string SoxExe
        {
            get
            {
                return GetString("AudioUtilitySoxExe");
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
        /// Gets OriginalAudioStorageDirs.
        /// </summary>
        public static IEnumerable<DirectoryInfo> OriginalAudioStorageDirs
        {
            get
            {
                return GetDirs(WebsiteBasePath, "OriginalAudioStorageDirs", true, ",");
            }
        }

        /// <summary>
        /// Gets SegmentedAudioStorageDirs.
        /// </summary>
        public static IEnumerable<DirectoryInfo> SegmentedAudioStorageDirs
        {
            get
            {
                return GetDirs(WebsiteBasePath, "SegmentedAudioStorageDirs", true, ",");
            }
        }

        /// <summary>
        /// Gets SpectrogramStorageDirs.
        /// </summary>
        public static IEnumerable<DirectoryInfo> SpectrogramStorageDirs
        {
            get
            {
                return GetDirs(WebsiteBasePath, "SpectrogramStorageDirs", true, ",");
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

        /// <summary>
        /// Gets a value indicating whether is asp net.
        /// </summary>
        public static bool IsAspNet
        {
            get
            {
                try
                {
                    var appDomainPath = HttpRuntime.AppDomainAppVirtualPath;
                    var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                    var interactive = Environment.UserInteractive;
                    var entryAssembly = Assembly.GetEntryAssembly();
                    var currentContext = HttpContext.Current;

                    // process name might be one of these
                    if (processName == "w3wp"
                        || processName == "iisexpress"
                        || processName == "aspnet_wp"
                        || processName.StartsWith("WebDev.WebServer"))
                    {
                        return true;
                    }

                    // app virtual path should not be null and current context usually not null
                    if (!string.IsNullOrEmpty(appDomainPath) || currentContext != null)
                    {
                        return true;
                    }

                    // might not be interactive, and have a null entry asebmly
                    if (!interactive && entryAssembly == null)
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }

                return false;
            }
        }

        public static string WebsiteBasePath
        {
            get
            {
                var appDomainPath = HttpRuntime.AppDomainAppPath;
                var appBasePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                var hostingEnvironmentRoot = HostingEnvironment.MapPath("/");
                return hostingEnvironmentRoot;
            }
        }

        public static IEnumerable<DirectoryInfo> SuggestionAnalysisCacheDirectoy
        {
            get
            {
                return GetDirs(WebsiteBasePath, "SuggestionAnalysisCacheDirectoy", true, ",");
            }
        }

        public static string GetString(string key)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Any(k => k == key))
            {
                //throw new ConfigurationErrorsException("Could not find key: " + key);
                return string.Empty;
            }

            var value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrEmpty(value))
            {
                throw new ConfigurationErrorsException("Found key, but it did not have a value: " + key);
            }

            return value;
        }

        public static bool Contains(string key)
        {
            return ConfigurationManager.AppSettings.AllKeys.Any(k => k == key);
        }

        //        public static IEnumerable<string> GetStrings(string key, params char[] separators)
        public static string[] GetStrings(string key, params char[] separators)
        {
            var value = GetString(key);
            var values = value
                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(v => !string.IsNullOrEmpty(v));

            if (!values.Any() || values.All(s => string.IsNullOrEmpty(s)))
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
                throw new DirectoryNotFoundException(String.Format("Could not find directory: {0} = {1}", key, value));
            }

            return new DirectoryInfo(value);
        }

        public static FileInfo GetFile(string key, bool checkExists)
        {
            var value = GetString(key);

            if (checkExists && !File.Exists(value))
            {
                throw new FileNotFoundException(String.Format("Could not find file: {0} = {1}", key, value));
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
    }
}
