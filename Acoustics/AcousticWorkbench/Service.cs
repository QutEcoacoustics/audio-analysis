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
    using System.Threading.Tasks;
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

        protected Service(IAuthenticatedApi authenticatedApi)
            : this((IApi)authenticatedApi)
        {
            this.AuthenticatedApi = authenticatedApi;
            AddAuthTokenHeader(this.Client.DefaultRequestHeaders, authenticatedApi.Token);
        }

        protected IAuthenticatedApi AuthenticatedApi { get; }

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

        protected async Task<T> ProcessApiResult<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var result = this.Deserialize<T>(json);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpResponseException(response, result.Meta);
            }

            return result.Data;
        }

        public class HttpResponseException : Exception
        {
            public HttpResponseException(HttpResponseMessage response, Meta responseMeta)
            {
                this.Message = $"Http response failed (Status: {response.StatusCode}:\n" +
                               $"URI: {response.RequestMessage.RequestUri}\n" +
                               $"API meta: {responseMeta}";
            }

            public override string Message { get; }
        }
    }
}