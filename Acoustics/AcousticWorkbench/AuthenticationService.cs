// <copyright file="AuthenticationService.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Security.Authentication;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class AuthenticationService : Service
    {
        private readonly IApi api;

        public AuthenticationService(IApi api)
            : base(api)
        {
            this.api = api;
        }

        public async Task<IAuthenticatedApi> CheckLogin(string token)
        {
            var uri = this.api.GetSessionValidateUri();

            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get,
            };
            AddAuthTokenHeader(request.Headers, token);

            var result = await this.Client.SendAsync(request);

            return await this.ProcessResult(result);
        }

        public async Task<IAuthenticatedApi> Login(string username, string password)
        {
            var uri = this.api.GetLoginUri();

            StringContent body;
            if (username.Contains("@"))
            {
                body = this.SerializeContent(new EmailLoginRequest() { Email = username, Password = password });
            }
            else
            {
                body = this.SerializeContent(new LoginRequest() { Login = username, Password = password });
            }

            var result = await this.Client.PostAsync(uri, body);

            return await this.ProcessResult(result);
        }

        private async Task<IAuthenticatedApi> ProcessResult(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var result = this.Deserialize<LoginResponse>(json);

            if (!response.IsSuccessStatusCode)
            {
                throw new AuthenticationException("Could not login in to the acoustic workbench: " + result.Meta);
            }

            return AcousticWorkbench.AuthenticatedApi.Merge(this.api, result.Data.UserName, result.Data.AuthToken);
        }

        public class LoginRequest
        {
            public string Login { get; set; }

            public string Password { get; set; }
        }

        public class EmailLoginRequest
        {
            public string Email { get; set; }

            public string Password { get; set; }
        }

        public class LoginResponse
        {
            public string AuthToken { get; set; }

            public string UserName { get; set; }

            public string Message { get; set; }
        }
    }
}