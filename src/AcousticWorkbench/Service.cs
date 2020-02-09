// <copyright file="Service.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public abstract class Service
    {
        /// <summary>
        /// The amount of time to wait for a request to be processed.
        /// </summary>
        /// <remarks>
        /// We dynamically scale this value to the number of CPU cores multiplied by 10 seconds each so that in cases
        /// where there is back pressure the async requests do not prematurely timeout. This is important because the
        /// timeout calculation for HttpClient *includes* the time the request is waiting to be sent as well as the
        /// total request time.
        /// </remarks>
        public static readonly TimeSpan ClientTimeout = TimeSpan.FromSeconds(120 + (Environment.ProcessorCount * 10));

        private const string ApplicationJson = "application/json";

        private readonly DefaultContractResolver defaultContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy(),
        };

        private readonly JsonSerializerSettings jsonSerializerSettings;

        static Service()
        {
            // control application wide HTTP concurrency. The limit is applied per host and is automatically used
            // by HttpClient.
            // https://docs.microsoft.com/en-us/dotnet/framework/network-programming/managing-connections
            // Since typically the acoustic workbench servers should be able to handle a signficant amount of work, we
            // up the number of allowed connections so we can make better use of parallel CPUs.
            ServicePointManager.DefaultConnectionLimit = Math.Min(64, Environment.ProcessorCount * 4);
        }

        protected Service(IApi api)
        {
            this.HttpClient = new HttpClient();

            this.HttpClient.Timeout = ClientTimeout;
            this.HttpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ApplicationJson));
            this.HttpClient.BaseAddress = api.Base();

            this.jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = this.defaultContractResolver };
        }

        protected Service(IAuthenticatedApi authenticatedApi)
            : this((IApi)authenticatedApi)
        {
            this.AuthenticatedApi = authenticatedApi;
            AddAuthTokenHeader(this.HttpClient.DefaultRequestHeaders, authenticatedApi.Token);
        }

        protected IAuthenticatedApi AuthenticatedApi { get; }

        protected HttpClient HttpClient { get; }

        protected static void AddAuthTokenHeader(HttpRequestHeaders headers, string token)
        {
            headers.Authorization = new AuthenticationHeaderValue("Token", "token=" + token);
        }

        protected string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, this.jsonSerializerSettings);
        }

        protected StringContent SerializeContent(object obj)
        {
            return this.SerializeContent(obj, out var _);
        }

        protected StringContent SerializeContent(object obj, out string serializedString)
        {
            serializedString = this.Serialize(obj);
            return new StringContent(serializedString, Encoding.UTF8, ApplicationJson);
        }

        protected AcousticWorkbenchResponse<T> Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<AcousticWorkbenchResponse<T>>(json, this.jsonSerializerSettings);
        }

        protected async Task<T> ProcessApiResult<T>(HttpResponseMessage response, string requestBody = "")
        {
            var json = await response.Content.ReadAsStringAsync();

            AcousticWorkbenchResponse<T> result = null;
            try
            {
                result = this.Deserialize<T>(json);
            }
            catch (JsonReaderException)
            {
                // throw if it was meant to work... if not then we're already in an error case... best effort to get to
                // error handling block below.
                if (response.IsSuccessStatusCode)
                {
                    throw;
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpResponseException(response, result?.Meta, requestBody);
            }

            if (result == null)
            {
                throw new InvalidOperationException("Service has a null data blob that was not caught by error handling");
            }

            return result.Data;
        }

        public class HttpResponseException : Exception
        {
            public HttpResponseMessage Response { get; }

            public Meta ResponseMeta { get; }

            public HttpResponseException(HttpResponseMessage response, Meta responseMeta, string requestBody = "")
            {
                this.Response = response;
                this.ResponseMeta = responseMeta;
                this.Message = $"Http response failed (Status: {response.StatusCode}):\n" +
                               $"URI: {response.RequestMessage.RequestUri}\n" +
                               $"Headers: {response.RequestMessage.Headers}\n" +
                               $"Body: {requestBody}\n" +
                               $"API meta: {responseMeta}";
            }

            public override string Message { get; }
        }
    }
}