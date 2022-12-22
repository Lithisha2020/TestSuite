using fathom_end_to_end_testing_suite.Models;
using fathom_end_to_end_testing_suite.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using System.Text.RegularExpressions;

namespace fathom_end_to_end_testing_suite.Steps
{
    [Binding]
    public sealed class Fathom_Existing_Dataset_Flow_basics:Common_Steps
    {
        public Fathom_Existing_Dataset_Flow_basics(ScenarioContext injectedContext):base(injectedContext)
        {
        }

        [Given(@"an existing dataset for (.*) with (.*) and (.*)")]
        public void GivenANewDatasetFor_(string test_case_scenario, string fileName, string filesettings)
        {
            var datasetName = string.Format("test_{0}", Guid.NewGuid().ToString("N").ToLower());
            scenarioContext.Add(CONTEXT_KEYS.DATASET_NAME, datasetName);

            var headers = new Dictionary<string, string>();
            headers.Add("CONTENTTYPE", "multipart/form-data");
            headers.Add("ACCEPT", "application/json");
            headers.Add("Authorization", string.Format("bearer {0}", scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN]));


            var body = new Dictionary<string, string>();
            body.Add("name", datasetName);
            body.Add("templateDatasetName", "FATHOMTESTTEMPLATE");

            var response = HttpUtility.PostAsync(string.Format("{0}/{1}", FATHOM_ENVIRONMENT, "/dataset"), string.Empty, headers, body).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.Created);

            dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);
            
            Assert.IsTrue(datasetReponse.datasetId.Value > 0);
            Assert.IsTrue(datasetReponse.name.Value.Equals(scenarioContext[CONTEXT_KEYS.DATASET_NAME]));
            Assert.IsTrue(datasetReponse.database.Value.Equals(scenarioContext[CONTEXT_KEYS.DATASET_NAME]));

            scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO] = test_case_scenario;
            scenarioContext.Add(CONTEXT_KEYS.DATASET_ID, datasetReponse.datasetId.Value);
            scenarioContext.Add(CONTEXT_KEYS.DATAFILE, fileName);
            scenarioContext[CONTEXT_KEYS.LOADFILESETTINGS] = filesettings;
        }
    }
}
