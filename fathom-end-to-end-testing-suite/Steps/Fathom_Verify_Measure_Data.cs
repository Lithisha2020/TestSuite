using fathom_end_to_end_testing_suite.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using TechTalk.SpecFlow;

namespace fathom_end_to_end_testing_suite.Steps
{
    [Binding]
    public class Fathom_Verify_Measure_Data : Common_Steps
    {
        public Fathom_Verify_Measure_Data(ScenarioContext injectedContext) : base(injectedContext)
        {
        }

        [Given(@"a measure data request is created for (.*)")]
        [When(@"a measure data request is created for (.*)")]
        public void ThenAMeasureDataRequestIsCreatedFor(string requestFileName)
        {
            var requestId = createmeasuredatarequest(requestFileName);
            scenarioContext[CONTEXT_KEYS.REQUEST_ID] = requestId;
        }

        [Given(@"measure data request is completed")]
        [When(@"measure data request is completed")]
        public void VariableDataRequestIsCompleted()
        {
            verifythemeasuredatarequestiscompleted();
        }
        public void verifythemeasuredatarequestiscompleted()
        {
            var response = HttpUtility.GetAsync(string.Format("{0}/dataset/{1}/measure/data/{2}/status", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID], scenarioContext[CONTEXT_KEYS.REQUEST_ID]), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.OK);
            dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);

            bool isSuccessfullyLoaded = false;

            for (var count = 0; count < 100; count++)
            {
                var datasetResponseValue = datasetReponse.runStatus.Value ?? string.Empty;

                if (string.Equals(datasetResponseValue, "Error"))
                {
                    Assert.IsFalse(true, "Error loading dataset");
                    break;
                }

                //if (!string.IsNullOrWhiteSpace(datasetReponse.runStatus.Value) && !string.Equals(datasetReponse.runStatus.Value, "Error"))
                //{
                if (!string.Equals(datasetResponseValue, "Complete"))
                {
                    Thread.Sleep(10000 * count);

                    response = HttpUtility.GetAsync(string.Format("{0}/dataset/{1}/measure/data/{2}/status", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID], scenarioContext[CONTEXT_KEYS.REQUEST_ID]), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

                    datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);
                }
                else
                {
                    isSuccessfullyLoaded = true;
                    break;
                }
                //}
                //else
                //{
                //    Assert.IsFalse(true, "Error loading dataset");
                //    break;
                //}
            }

            Assert.IsTrue(isSuccessfullyLoaded, "Dataset variable request failed.");

        }
        [Then(@"I should be able to see the measure data request as (.*)")]
        public void ThenIShouldBeAbleToSeeTheVariableDataRequestAsResponse_Variable_Data_Json(string responseFileName)
        {
            var responseFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], responseFileName);

            var response = HttpUtility.GetAsync(string.Format("{0}/dataset/{1}/measure/data/{2}", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID], scenarioContext[CONTEXT_KEYS.REQUEST_ID]), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.OK);

            var responseFileData = System.IO.File.ReadAllText(responseFilePath);

            responseFileData = responseFileData.Replace("<loader_id>", scenarioContext[CONTEXT_KEYS.DATASET_LOADERID].ToString());

            StringBuilder sbExpectedResult = new StringBuilder();

            var expectedResult = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(responseFileData));

            var actualResult = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage));

            Assert.AreEqual(expectedResult, actualResult);
        }


        private int createmeasuredatarequest(string requestFileName)
        {
            var requestFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], requestFileName);

            var postUrl = string.Format("{0}/dataset/{1}/measure/data", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID]);


            var headers = new Dictionary<string, string>();
            headers.Add("ACCEPT", "application/json");
            headers.Add("Authorization", string.Format("bearer {0}", scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN]));


            var body = new StringContent(System.IO.File.ReadAllText(requestFilePath), Encoding.UTF8, "application/json");


            var response = HttpUtility.PostAsync(postUrl, string.Empty, headers, body).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.Created);

            dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);
            Assert.IsTrue(datasetReponse.requestId.Value > 0);

            return (int)datasetReponse.requestId.Value;
        }
    }
}
