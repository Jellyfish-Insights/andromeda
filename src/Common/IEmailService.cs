using System;
using System.IO;
using RestSharp;
using RestSharp.Authenticators;

namespace Common {
    public interface IEmailService {
        IRestResponse SendMessage(string to, string subject, string text);
    }
}
