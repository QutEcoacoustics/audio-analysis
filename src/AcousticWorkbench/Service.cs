// <copyright file="Service.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using AcousticWorkbench.Models;
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

        public static readonly NamingStrategy NamingStrategy = new SnakeCaseNamingStrategy();

        private const string ApplicationJson = "application/json";

        private readonly DefaultContractResolver defaultContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = NamingStrategy,
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
            this.HttpClient = new HttpClient
            {
                Timeout = ClientTimeout,
            };
            this.HttpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ApplicationJson));
            this.HttpClient.BaseAddress = api.Base();

            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = this.defaultContractResolver,
                NullValueHandling = NullValueHandling.Ignore,
            };
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

        protected AcousticWorkbenchSingleResponse<T> DeserializeSingle<T>(string json)
        {
            return JsonConvert.DeserializeObject<AcousticWorkbenchSingleResponse<T>>(json, this.jsonSerializerSettings);
        }

        protected AcousticWorkbenchListResponse<T> DeserializeList<T>(string json)
        {
            return JsonConvert.DeserializeObject<AcousticWorkbenchListResponse<T>>(json, this.jsonSerializerSettings);
        }

        protected async Task<T> ProcessApiResult<T>(HttpResponseMessage response, string requestBody = "")
        {
            var json = await response.Content.ReadAsStringAsync();

            AcousticWorkbenchSingleResponse<T> result = null;
            try
            {
                result = this.DeserializeSingle<T>(json);
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

            // tag the models with Meta if possible
            if (result.Data is IModelWithMeta model)
            {
                model.Meta = result.Meta;
            }

            return result.Data;
        }

        protected async Task<IReadOnlyCollection<T>> ProcessApiResults<T>(HttpResponseMessage response, string requestBody = "")
        {
            var json = await response.Content.ReadAsStringAsync();

            AcousticWorkbenchListResponse<T> result = null;
            try
            {
                result = this.DeserializeList<T>(json);
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

            // tag the models with Meta if possible
            foreach (var item in result.Data)
            {
                if (item is IModelWithMeta model)
                {
                    model.Meta = result.Meta;
                }
            }

            return result.Data;
        }

        public class HttpResponseException : Exception
        {
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

            public HttpResponseMessage Response { get; }

            public Meta ResponseMeta { get; }

            public override string Message { get; }
        }
    }
}