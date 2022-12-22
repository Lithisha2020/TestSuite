using fathom_end_to_end_testing_suite.Infrastructure;
using fathom_end_to_end_testing_suite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace fathom_end_to_end_testing_suite.Utilities
{
    class FathomApiClient : FathomApiClientBase
    {
        private static Lazy<FathomApiClient> Singleton = null;

        private FathomApiClient(string environment, string accessToken) : base(environment, accessToken)
        {
        }

        public static FathomApiClient Instance { get => Singleton?.Value; }

        public static FathomApiClient Create(string environment, string accessToken)
        {
            if (Singleton == null)
                Singleton = new Lazy<FathomApiClient>(() => new FathomApiClient(environment, accessToken));

            return Instance;
        }

        public Result Import(long loaderId, string fathomNamespace)
        {
            var uri = GetEndpointUri(FATHOM_ENDPOINTS.IMPORT);

            var body = new Dictionary<string, string>();
            body.Add("loaderId", loaderId.ToString());
            body.Add("namespace", fathomNamespace);

            return Post(uri, body);
        }

        public Result CreateGroup(long datasetId, string groupName, IEnumerable<GroupVariable> variables)
        {
            var uri = GetEndPointUrilWithDataset(datasetId.ToString(), FATHOM_ENDPOINTS.VARIABLE_GROUP);

            var group = new Group()
            {
                GroupName = groupName,
                variables = variables
            };

            var body = new StringContent(JsonConvert.SerializeObject(group), Encoding.UTF8, "application/json");

            return Post(uri, body, HttpStatusCode.Created);
        }

        public Result CreateSnapshot(long datasetId, string snapshotName, string groupName, int maxStatusCheckAttempts, int statusCheckInterval)
        {
            var uri = GetEndPointUrilWithDataset(datasetId.ToString(), $"{FATHOM_ENDPOINTS.VARIABLE_GROUP}/{groupName}/snapshot");

            var body = new Dictionary<string, string>();
            body.Add("name", snapshotName);
            body.Add("label", snapshotName);

            var result = Post(uri, body, HttpStatusCode.Created);

            if (result.IsFailure)
                return Result.Fail(result.Error);

            dynamic reponse = JsonConvert.DeserializeObject<dynamic>(result.Value.HttpResponseMessage);

            if (reponse.requestId.Value == 0)
                return Result.Fail("Request id was not generated");

            uri = GetEndPointUrilWithDataset(datasetId.ToString(), $"{FATHOM_ENDPOINTS.VARIABLE_GROUP}/{reponse.requestId.Value}/status");

            return CheckStatus(uri, maxStatusCheckAttempts, statusCheckInterval);
        }

        public Result CheckStatus(string uri, int maxAttempts, int interval)
        {
            var count = 0;

            do
            {
                var result = Get(uri);

                dynamic response = JsonConvert.DeserializeObject<dynamic>(result.Value.HttpResponseMessage);

                if (response.runStatus.Value != "Complete")
                {
                    Thread.Sleep(interval * count);
                    count++;
                }
                else
                {
                    if (response.runStatus.Value == "Fail")
                        return Result.Fail("Fail status");

                    return Result.Ok();
                }


            } while (count < maxAttempts);

            return Result.Fail("Status check timed out");
        }

    }

    public static class FATHOM_ENDPOINTS
    {
        public static string IMPORT = "import";
        public static string VARIABLE_GROUP = "group/variable";
    }
}
