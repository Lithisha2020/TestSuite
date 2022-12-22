using fathom_end_to_end_testing_suite.Models;
using fathom_end_to_end_testing_suite.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;

namespace fathom_end_to_end_testing_suite.Steps
{
    [Binding]
    public sealed class Fathom_HBG_Steps : Common_Steps
    {
        public Fathom_HBG_Steps(ScenarioContext context) : base(context)
        {
        }


        [Then(@"total blanks in HBG variables should be 0")]
        public void ThenTotalBlanksInHBGVariablesShouldBe()
        {
            var dataSetNameOrId = scenarioContext[CONTEXT_KEYS.DATASET_ID];
            var requestId = scenarioContext[CONTEXT_KEYS.REQUEST_ID];
            var accessToken = scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString();

            var response = HttpUtility.GetAsync($"{FATHOM_ENVIRONMENT}/dataset/{dataSetNameOrId}/variable/data/{requestId}", accessToken, new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.OK);

            var result = JsonConvert.DeserializeObject<DataResult>(response.HttpResponseMessage);
            Assert.IsTrue(result.Data.Any());
            Assert.IsTrue((long)result.Data.First()["total"] == 0);

        }
    }
}
