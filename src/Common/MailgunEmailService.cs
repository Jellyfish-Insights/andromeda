using System;
using System.IO;
using RestSharp;
using RestSharp.Authenticators;
using Microsoft.Extensions.Configuration;

namespace Common {
    public class MailgunEmailService : IEmailService {
        private const string MailgunUri = "https://api.mailgun.net/v3";
        private const string FeeDomain = "fee.org";

        private readonly IConfiguration _configuration;
        private RestClient client;

        public MailgunEmailService(IConfiguration configuration) {
            _configuration = configuration;
            client = new RestClient();
            client.BaseUrl = new Uri(MailgunUri);
            client.Authenticator = new HttpBasicAuthenticator("api",
                                                              _configuration.GetValue<string>("MailgunKey", ""));
        }

        public MailgunEmailService(string key) {
            client = new RestClient();
            client.BaseUrl = new Uri(MailgunUri);
            client.Authenticator = new HttpBasicAuthenticator("api", key);
        }

        public IRestResponse SendMessage(string to, string subject, string text) {
            RestRequest request = new RestRequest();
            request.AddParameter("domain", FeeDomain, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "FEE <website@fee.org>");
            request.AddParameter("to", to);
            request.AddParameter("subject", subject);
            request.AddParameter("text", text);
            request.Method = Method.POST;
            return client.Execute(request);
        }

        public IRestResponse SendHTMLMessage(string to, string subject, string html) {
            RestRequest request = new RestRequest();
            request.AddParameter("domain", FeeDomain, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "FEE <website@fee.org>");
            request.AddParameter("to", to);
            request.AddParameter("subject", subject);
            request.AddParameter("text", "your email does not support html");
            request.AddParameter("html", html);
            request.Method = Method.POST;
            return client.Execute(request);
        }
    }
}
