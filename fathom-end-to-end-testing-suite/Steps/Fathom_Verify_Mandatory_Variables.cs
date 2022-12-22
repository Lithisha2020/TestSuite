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
    public class Fathom_Verify_Mandatory_Variables : Common_Steps
    {
        public Fathom_Verify_Mandatory_Variables(ScenarioContext injectedContext) : base(injectedContext)
        {
        }

        [Then(@"I should see all the variables from (.*)")]
        public void ThenIShouldSeeAllTheVariablesFrom_(string responseAllVariablesFileName)
        {
            //Arrange
            var responseAllVariablesFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], responseAllVariablesFileName);

            string url = string.Format("{0}/dataset/{1}/variable", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID]);


            //Act
            var response = HttpUtility.GetAsync(url, scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;


            //Assert
            var expectedResult = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(responseAllVariablesFilePath)));

            var actualResult = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage));

            Assert.IsTrue(isSubsetOf(actualResult, expectedResult), "Get All variables missing variables");
        }


        [Given(@"a variable data request is created for (.*)")]
        [When(@"a variable data request is created for (.*)")]
        public void GivenAVariableDataRequestIsCreated(string requestFileName)
        {
            var requestId = createvariabledatarequest(requestFileName);

            scenarioContext.Add(CONTEXT_KEYS.REQUEST_ID, requestId);
        }

        [Then(@"a variable data request is created for (.*)")]
        public void ThenAVariableDataRequestIsCreatedFor(string requestFileName)
        {
            var requestId = createvariabledatarequest(requestFileName);
            scenarioContext[CONTEXT_KEYS.REQUEST_ID] = requestId;
        }


        private int createvariabledatarequest(string requestFileName)
        {
            var requestFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], requestFileName);

            var postUrl = string.Format("{0}/dataset/{1}/variable/data", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID]);

            GetIdentityToken();

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

        [Then(@"variable data request is completed")]
        public void ThenVariableDataRequestIsCompleted()
        {
            variableDataRequestIsCompleted();
        }

        [Given(@"variable data request is completed")]
        [When(@"variable data request is completed")]
        public void VariableDataRequestIsCompleted()
        {
            variableDataRequestIsCompleted();
        }

        public void variableDataRequestIsCompleted()
        {
            var response = HttpUtility.GetAsync(string.Format("{0}/dataset/{1}/variable/data/{2}/status", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID], scenarioContext[CONTEXT_KEYS.REQUEST_ID]), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

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

                    response = HttpUtility.GetAsync(string.Format("{0}/dataset/{1}/variable/data/{2}/status", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID], scenarioContext[CONTEXT_KEYS.REQUEST_ID]), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

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

        [Then(@"I should be able to see the variable data request as (.*)")]
        public void ThenIShouldBeAbleToSeeTheVariableDataRequestAsResponse_Variable_Data_Json(string responseFileName)
        {
            var responseFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], responseFileName);

            var response = HttpUtility.GetAsync(string.Format("{0}/dataset/{1}/variable/data/{2}", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID], scenarioContext[CONTEXT_KEYS.REQUEST_ID]), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.OK);

            var responseFileData = System.IO.File.ReadAllText(responseFilePath);

            responseFileData = responseFileData.Replace("<loader_id>", scenarioContext[CONTEXT_KEYS.DATASET_LOADERID].ToString());

            StringBuilder sbExpectedResult = new StringBuilder();

            var expectedResult = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(responseFileData));

            var actualResult = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage));

            Assert.AreEqual(expectedResult, actualResult);
        }






        [Then(@"I should get the variables data for the final namespace for (.*)")]
        public void ThenIShouldGetTheVariablesDataForTheFinalNamespaceFor(string p0)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
