using fathom_end_to_end_testing_suite.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace fathom_end_to_end_testing_suite.Steps
{
    public static class CONTEXT_KEYS
    {
        public static string ACCESS_TOKEN = "ACCESS_TOKEN";
        public static string DATASET_NAME = "DATASET_NAME";
        public static string DATASET_ID = "DATASET_ID";
        public static string DATABASE = "DATABASE";
        public static string TEST_CASE_SCENARIO = "TESTCASE_SCENARIO";
        public static string DATASET_LOADERID = "DATASET_LOADERID";
        public static string DATAFILE = "DATAFILE";
        public static string LOADFILESETTINGS = "LOADFILESETTINGS";
        public static string TEST_CASE_DATAFILE = "FILENAME";
        public static string REQUEST_ID = "REQUESTID";
        public static string DATAMAP_FILE = "DATAMAP_FILE";
        public static string LOADFILE_SETTINGS_UPDATED = "LOADFILE_SETTINGS_UPDATED";
        public static string DATASET_LOADERID_UPDATED = "DATASET_LOADERID_UPDATED";
        public static string REQUEST_ID_VERIFY = "REQUESTID_VERIFY";
    }

    public static class SECRET_KEYS
    {
        public static string FATHOM_URL = "fathom_url";
        public static string RESOURCE = "resource";
        public static string CLIENT_ID = "client_id";
        public static string CLIENT_SECRET = "client_secret";
        public static string GRANT_TYPE = "grant_type";
        public static string AUTH_URL = "auth_url";
    }

    public abstract class Common_Steps
    {
        public static string FATHOM_ENVIRONMENT;
        public readonly ScenarioContext scenarioContext;
        public TestContext testContext;
        public static string EXISTING_DATASET_ID;
        public static string LOADER_ID;
        public static string DATASET_NAME;
        public static int statusCheckInterval = 10000;

        private HttpUtility httpUtility;
        private bool isLocalHost;

        public Common_Steps(ScenarioContext injectedContext)
        {
            scenarioContext = injectedContext;
            testContext = scenarioContext.ScenarioContainer.Resolve<Microsoft.VisualStudio.TestTools.UnitTesting.TestContext>();

            FATHOM_ENVIRONMENT = testContext.Properties[SECRET_KEYS.FATHOM_URL] as string;

            scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN] = string.Empty;

            FathomApiClient.Create(FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString());
        }

        public void GetIdentityToken()
        {
            if (isLocalHost) return;

            httpUtility = new HttpUtility();

            var headers = new Dictionary<string, string>();
            headers.Add("CONTENTTYPE", "multipart/form-data");
            headers.Add("ACCEPT", "application/json");

            var body = new Dictionary<string, string>();

            body.Add("resource", testContext.Properties[SECRET_KEYS.RESOURCE] as string);
            body.Add("client_id", testContext.Properties[SECRET_KEYS.CLIENT_ID] as string);
            body.Add("client_secret", testContext.Properties[SECRET_KEYS.CLIENT_SECRET] as string);
            body.Add("grant_type", testContext.Properties[SECRET_KEYS.GRANT_TYPE] as string);
            string authUrl = testContext.Properties[SECRET_KEYS.AUTH_URL] as string;

            var response = HttpUtility.PostAsync(authUrl, string.Empty, headers, body).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.OK);

            dynamic identityResponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(Convert.ToString(identityResponse.access_token)));
            scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN] = identityResponse.access_token.Value;

            FathomApiClient.Instance.SetAccessToken(identityResponse.access_token.Value);
        }

        public bool isSubsetOf(string supersetJson, string subsetJson)
        {
            var expectedResult = JsonConvert.DeserializeObject<JArray>(subsetJson).ToObject<List<JObject>>();

            var sbMissingKeys = new StringBuilder();

            var actualResult = JsonConvert.DeserializeObject<JArray>(supersetJson).ToObject<List<JObject>>();

            foreach (var element in expectedResult)
            {
                var strName = from p in actualResult
                              where ((string)p["name"]).Equals((string)(element["name"]), StringComparison.InvariantCultureIgnoreCase)
                              select (string)p["name"];

                if (strName.Count() == 0)
                {
                    sbMissingKeys.Append((string)(element["name"]));
                }
            }

            Assert.IsTrue(true,
                sbMissingKeys.Length > 0
                ? string.Format("Missing keys in the expected set: {0}", sbMissingKeys.ToString())
                : string.Empty);

            return sbMissingKeys.Length == 0;
        }

        private bool IsLocalHost() => FATHOM_ENVIRONMENT.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase)
          || FATHOM_ENVIRONMENT.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase);
    }
}
