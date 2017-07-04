// <copyright file="Service.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Security;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public abstract class Service
    {
        public static readonly TimeSpan ClientTimeout = TimeSpan.FromSeconds(120);

        protected readonly HttpClient Client;

        private const string ApplicationJson = "application/json";

        private readonly DefaultContractResolver defaultContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy(),
        };

        private readonly JsonSerializerSettings jsonSerializerSettings;

        protected Service(IApi api)
        {
            this.Client = new HttpClient();

            this.Client.Timeout = ClientTimeout;
            this.Client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ApplicationJson));
            this.Client.BaseAddress = api.Base();

            this.jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = this.defaultContractResolver };
        }

        protected static void AddAuthTokenHeader(HttpRequestHeaders headers, string token)
        {
            headers.Authorization = new AuthenticationHeaderValue("Token token", token);
        }

        protected string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, this.jsonSerializerSettings);
        }

        protected StringContent SerializeContent(object obj)
        {
            return new StringContent(this.Serialize(obj), Encoding.UTF8, ApplicationJson);
        }

        protected AcousticWorkbenchResponse<T> Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<AcousticWorkbenchResponse<T>>(json, this.jsonSerializerSettings);
        }
    }
}