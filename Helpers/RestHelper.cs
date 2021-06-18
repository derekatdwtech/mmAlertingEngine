using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace tempaast.alerting.helpers
{
    public class RestHelper
    {
        private RestClient _client;
        private ILogger log;
        public RestHelper(string uri, ILogger logger)
        {
            _client = new RestClient(uri);
            log = logger;
        }

        public IRestResponse Get(string route)
        {
            var request = new RestRequest(route);
            return _client.Get<IRestResponse>(request);
        }

        public IRestResponse Post(string route, string body = null)
        {
            if (body != null)
            {
                var request = new RestRequest(route).AddJsonBody(body);
                var result = _client.ExecutePostAsync<IRestResponse>(request);
                return result.Result;

            }
            else
            {
                var request = new RestRequest(route);
                var result = _client.ExecutePostAsync<RestResponse>(request);
                return result.Result;
            }

        }
    }
}