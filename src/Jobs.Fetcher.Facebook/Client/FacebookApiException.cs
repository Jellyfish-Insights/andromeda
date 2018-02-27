using System;
using Newtonsoft.Json.Linq;
using Common.Logging;
using Serilog;

namespace Jobs.Fetcher.Facebook {

    class FacebookApiException : Exception {

        public JObject Error { get; set; }

        public FacebookApiException(string message): base(message) {}

        public FacebookApiException(string message, Exception error): base(message) {
            Log.ForContext<FacebookApiException>().Error(error, "Error fetching Facebook API");
        }

        public FacebookApiException(string message, JObject error): base(message) {
            Error = error;
            Log.ForContext<FacebookApiException>().Warning(this, "Error: {Error}", error.ToString());
        }
    }

    class FacebookApiUnreachable : FacebookApiException {
        public FacebookApiUnreachable(): base("API unreachable") {}
    }
}
