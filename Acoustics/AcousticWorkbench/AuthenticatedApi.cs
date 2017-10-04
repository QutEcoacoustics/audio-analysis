// <copyright file="AuthenticatedApi.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    public class AuthenticatedApi : Api, IAuthenticatedApi
    {
        public string Username { get; private set; }

        public string Token { get; private set; }

        public static AuthenticatedApi Merge(IApi api, string username, string authToken)
        {
            return new AuthenticatedApi()
            {
                Host = api.Host,
                Version = api.Version,
                Protocol = api.Protocol,
                Username = username,
                Token = authToken,
            };
        }

        public override string ToString()
        {
            string str = base.ToString().TrimEnd('}');
            return $"{str}, Username=\"{this.Username}\", Token=\"{this.Token}\"}}";
        }
    }
}