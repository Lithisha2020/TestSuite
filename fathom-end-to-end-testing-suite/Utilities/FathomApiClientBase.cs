using fathom_end_to_end_testing_suite.Infrastructure;
using fathom_end_to_end_testing_suite.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace fathom_end_to_end_testing_suite.Utilities
{
    public class FathomApiClientBase
    {
        public FathomApiClientBase(string environment, string accessToken)
        {
            Environment = environment;
            AccessToken = accessToken;
        }

        public void SetAccessToken(string accesstoken)
        {
            AccessToken = accesstoken;
        }

        protected string Environment { get; }
        protected string AccessToken { get; private set; }

        protected T Deserialize<T>(string message) => JsonConvert.DeserializeObject<T>(message);
        protected string GetEndpointUri(string endpoint) => $"{Environment}/{endpoint}";
        protected string GetEndPointUrilWithDataset(string datasetNameOrId, string endpoint) => $"{Environment}/dataset/{datasetNameOrId}/{endpoint}";
        protected Dictionary<string, string> GetDefaultHeaders() => new Dictionary<string, string>();

        protected Dictionary<string, string> GetFormDataHeaders()
        {

            var headers = new Dictionary<string, string>();
            headers.Add("CONTENTTYPE", "multipart/form-data");
            headers.Add("ACCEPT", "application/json");
            headers.Add("Authorization", $"bearer {AccessToken}");

            return headers;
        }

        protected Dictionary<string, string> GetDefaultQueryParams() => new Dictionary<string, string>();

        protected Result<HttpResponseInTest> Get(string uri, HttpStatusCode expectedStatus = HttpStatusCode.OK)
        {
            var response = HttpUtility.GetAsync(uri, AccessToken, GetDefaultHeaders(), GetDefaultQueryParams()).Result;

            if (response.HeaderResponseStatus != expectedStatus)
                return Result.Fail<HttpResponseInTest>($"Status: {response.HeaderResponseStatus}");

            return Result.Ok(response);
        }

        protected Result<HttpResponseInTest> Post<T>(string uri, T body, HttpStatusCode expectedStatus = HttpStatusCode.OK)
        {
            HttpResponseInTest response = null;

            switch (body)
            {
                case Dictionary<string, string> b:
                    response = HttpUtility.PostAsync(uri, AccessToken, GetFormDataHeaders(), b).Result;
                    break;
                case MultipartFormDataContent b:
                    response = HttpUtility.PostAsync(uri, AccessToken, GetFormDataHeaders(), b).Result;
                    break;
                case StringContent b:
                    response = HttpUtility.PostAsync(uri, AccessToken, GetFormDataHeaders(), b).Result;
                    break;
            }

            if (response == null)
                return Result.Fail<HttpResponseInTest>($"Invalid content");

            if (response.HeaderResponseStatus != expectedStatus)
                return Result.Fail<HttpResponseInTest>($"Status: {response.HeaderResponseStatus}");

            return Result.Ok(response);
        }

    }
}
