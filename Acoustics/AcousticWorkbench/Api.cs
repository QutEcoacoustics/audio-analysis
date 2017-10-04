// <copyright file="Api.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.Text.RegularExpressions;

    public class Api : IApi
    {
        private const string DefaultVersion = "v1";
        private static readonly Regex VersionRegex = new Regex(@"^v[\d\.]+$");

        static Api()
        {
            Default = new Api()
            {
                Host = "www.ecosounds.org",
                Protocol = "https",
                Version = DefaultVersion,
            };
        }

        internal Api()
        {
        }

        public static Api Default { get; }

        public string Host { get; protected set; }

        public string Version { get; protected set; }

        public string Protocol { get; protected set; }

        //public string Uri { get; protected set; }

        public static Api Parse(string apiString)
        {
            var (success, message) = TryParse(apiString, out Api api);

            if (!success)
            {
                throw new ArgumentException("Cannot parse Acoustic Workbench API string: " + message);
            }

            return api;
        }

        public static (bool, string) TryParse(string apiString, out Api api)
        {
            api = null;
            if (string.IsNullOrWhiteSpace(apiString))
            {
                return (false, "Cannot be null or empty");
            }

            var success = Uri.TryCreate(apiString, UriKind.Absolute, out Uri uri);

            if (!success)
            {
                return (false, "Uri could not be parsed (ensure it is absolute and valid)");
            }

            // validate the URI
            if (uri.Scheme != "https")
            {
                return (false, "Only https is supported");
            }

            if (!uri.IsDefaultPort)
            {
                return (false, "Port specifications are not supported");
            }

            string version = DefaultVersion;
            string path = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (!VersionRegex.IsMatch(path))
                {
                    return (false, "Invalid version");
                }

                version = path.TrimStart('/');
            }

            api = new Api()
            {
                Host = uri.Host,
                Protocol = uri.Scheme,
                Version = version,
                //Uri = uri
            };

            return (true, null);
        }

        public override string ToString()
        {
            return $"{{Protocol=\"{this.Protocol}\", Host=\"{this.Host}\", Version=\"{this.Version}\"}}";
        }
    }
}