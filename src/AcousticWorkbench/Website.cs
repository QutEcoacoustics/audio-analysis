// <copyright file="Website.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;

    public record Website : IWebsite
    {
        private string protocol;

        public string Host { get; init; }

        public Website(string host, string protocol)
        {
            this.Host = host;
            this.Protocol = protocol;
        }

        public static Website Parse(string uri)
        {
            var (success, message) = Api.TryParse(uri, out var api);
            if (success)
            {
                return new Website(api.Host, api.Protocol);
            }

            throw new ArgumentException("Cannot parse Acoustic Workbench API string: " + message);
        }

        public string Protocol
        {
            get => this.protocol; init
            {
                if (value != "https")
                {
                    throw new ArgumentException($"{nameof(this.Protocol)} only supports https");
                }

                this.protocol = value;
            }
        }
    }
}