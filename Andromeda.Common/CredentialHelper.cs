using System;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace Andromeda.Common {
    public static class CredentialHelpers {
        private static string accessTokenName = "Google.Apis.Auth.OAuth2.Responses.TokenResponse-Credentials.json";
        private static string clientId;
        private static string clientSecret;
        private static string authUrl;
        private static string tokenUrl;
        private static string returnUrl;
    
        private static void LoadClientSecrets(string SecretsFile) {
            var data = FileSystemHelpers.LoadJson(SecretsFile);
            clientId = data["installed"]["client_id"].ToString();
            clientSecret = data["installed"]["client_secret"].ToString();
            authUrl = data["installed"]["auth_uri"].ToString();
            tokenUrl = data["installed"]["token_uri"].ToString();
            returnUrl = data["installed"]["redirect_uris"][0].ToString();
        }

        private static string GetGoogleServiceUrl() {
            System.Uri uri = new System.Uri(authUrl);
            var clientAuth = new RestClient();
            clientAuth.BaseUrl = uri;

            var request = new RestRequest(Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("redirect_uri", returnUrl);
            request.AddParameter("response_type", "code");
            request.AddParameter("client_id", clientId);
            request.AddParameter("access_type", "offline");
            request.AddParameter("prompt", "consent");
            request.AddParameter("scope", "https://www.googleapis.com/auth/youtube.readonly https://www.googleapis.com/auth/yt-analytics-monetary.readonly https://www.googleapis.com/auth/yt-analytics.readonly");

            return clientAuth.Execute(request).ResponseUri.ToString();
        }

        public static dynamic GetAccessToken(string autheticationCode) {
            System.Uri uriAccessToken = new System.Uri(tokenUrl);
            var client = new RestClient();
            client.BaseUrl = uriAccessToken;

            var request = new RestRequest(Method.POST);
            request.AddParameter("code", autheticationCode);
            request.AddParameter("redirect_uri", returnUrl);
            request.AddParameter("client_id", clientId);
            request.AddParameter("client_secret", clientSecret);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("scope", "https://www.googleapis.com/auth/youtube.readonly https://www.googleapis.com/auth/yt-analytics-monetary.readonly https://www.googleapis.com/auth/yt-analytics.readonly");

            var response = client.Execute(request);
            response.ContentType = "application/x-www-urlencoded";
            return JObject.Parse(response.Content);
        }

        public static void GetCredentials(string path, string SecretsFile) {
            LoadClientSecrets(SecretsFile);
            var googleUrl = GetGoogleServiceUrl();
            Console.WriteLine($"\nPlease visit this URL to authorize this application: {googleUrl}\n");
            Console.WriteLine("Enter the authorization code:");
            var autheticationCode = Console.ReadLine();
            var accessToken = GetAccessToken(autheticationCode);
            FileSystemHelpers.DumpJson($"{path}/{accessTokenName}", accessToken);
        }
    }
}