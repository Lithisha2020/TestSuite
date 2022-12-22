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
using TechTalk.SpecFlow;

namespace fathom_end_to_end_testing_suite.Steps
{
    [Binding]
    public sealed class Fathom_New_Dataset_Flow_basic : Common_Steps
    {
        public Fathom_New_Dataset_Flow_basic(ScenarioContext injectedContext) : base(injectedContext)
        {
        }

        [Given(@"an authentication token is available")]
        public void GivenAnAuthenticationTokenIsAvailable()
        {
            GetIdentityToken();
        }

        [Given(@"a new dataset for (.*) with (.*) for (.*) and (.*)")]
        public void GivenANewDatasetFor_(string test_case_scenario, string template, string datafiles, string filesettings)
        {
            var datasetName = $"test_ftm_{DateTime.Now.Ticks}_{Guid.NewGuid().ToString("N").ToLower().Substring(0, 8)}";
            scenarioContext.Add(CONTEXT_KEYS.DATASET_NAME, datasetName);

            var headers = new Dictionary<string, string>();
            headers.Add("CONTENTTYPE", "multipart/form-data");
            headers.Add("ACCEPT", "application/json");
            headers.Add("Authorization", string.Format("bearer {0}", scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN]));


            var body = new Dictionary<string, string>();
            body.Add("name", datasetName);
            body.Add("templateDatasetName", template);

            var response = HttpUtility.PostAsync(string.Format("{0}/{1}", FATHOM_ENVIRONMENT, "/dataset"), string.Empty, headers, body).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.Created);

            dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);

            Assert.IsTrue(datasetReponse.datasetId.Value > 0);
            Assert.IsTrue(datasetReponse.name.Value.Equals(scenarioContext[CONTEXT_KEYS.DATASET_NAME]));
            Assert.IsTrue(datasetReponse.database.Value.Equals(scenarioContext[CONTEXT_KEYS.DATASET_NAME]));

            scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO] = test_case_scenario;
            scenarioContext.Add(CONTEXT_KEYS.DATASET_ID, datasetReponse.datasetId.Value);
            scenarioContext.Add(CONTEXT_KEYS.DATAFILE, datafiles);
            scenarioContext.Add(CONTEXT_KEYS.LOADFILESETTINGS, filesettings);
        }

        //[Given(@"a new dataset for (.*) with (.*)")]
        //public void GivenANewDatasetForWith(string test_case_scenario, string template)
        //{
        //    var datasetName = string.Format("test_{0}", Guid.NewGuid().ToString("N").ToLower());
        //    scenarioContext.Add(CONTEXT_KEYS.DATASET_NAME, datasetName);

        //    var headers = new Dictionary<string, string>();
        //    headers.Add("CONTENTTYPE", "multipart/form-data");
        //    headers.Add("ACCEPT", "application/json");
        //    headers.Add("Authorization", string.Format("bearer {0}", scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN]));


        //    var body = new Dictionary<string, string>();
        //    body.Add("name", datasetName);
        //    body.Add("templateDatasetName", template);

        //    var response = HttpUtility.PostAsync(string.Format("{0}/{1}", FATHOM_ENVIRONMENT, "/dataset"), string.Empty, headers, body).Result;

        //    Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.Created);

        //    dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);

        //    Assert.IsTrue(datasetReponse.datasetId.Value > 0);
        //    Assert.IsTrue(datasetReponse.name.Value.Equals(scenarioContext[CONTEXT_KEYS.DATASET_NAME]));
        //    Assert.IsTrue(datasetReponse.database.Value.Equals(scenarioContext[CONTEXT_KEYS.DATASET_NAME]));

        //    scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO] = test_case_scenario;
        //    scenarioContext.Add(CONTEXT_KEYS.DATASET_ID, datasetReponse.datasetId.Value);
        //}

        [Given(@"I load data file and they are loaded")]
        public void GivenILoadDataFileWithAndTheyAreLoaded()
        {
            string filesettings = scenarioContext[CONTEXT_KEYS.LOADFILESETTINGS] as string;
            string datafilelist = scenarioContext[CONTEXT_KEYS.DATAFILE] as string;

            var filesettingList = filesettings.Split(',');
            var datafiles = datafilelist.Split(',');

            for (var cntDataFile = 0; cntDataFile < datafiles.Length; cntDataFile++)
            {
                var datafile = datafiles[cntDataFile];
                var loadsettingfile = filesettingList[cntDataFile];

                var dataFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], datafile);

                var dataFileName = System.IO.Path.GetFileName(datafile);

                var loadfilesettingsPath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], loadsettingfile);

                var loadfilesettingsContent = new StringContent(System.IO.File.ReadAllText(loadfilesettingsPath), Encoding.UTF8);

                var headers = new Dictionary<string, string>();
                headers.Add("ACCEPT", "application/json");
                headers.Add("Authorization", string.Format("bearer {0}", scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN]));

                var querystringParamaters = string.Format(loadfilesettingsContent.ReadAsStringAsync().Result, scenarioContext[CONTEXT_KEYS.DATASET_NAME]);

                using (var content = new MultipartFormDataContent())
                {
                    var stream = new StreamContent(File.Open(dataFilePath, FileMode.Open));
                    stream.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    stream.Headers.Add("Content-Disposition", string.Format("form-data; name=\"import\"; filename=\"{0}\"", dataFileName));

                    content.Add(stream, "file", dataFileName);

                    var response = HttpUtility.PostAsync(string.Format("{0}/{1}?{2}", FATHOM_ENVIRONMENT, "load", querystringParamaters), string.Empty, headers, content).Result;

                    Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.Created);

                    dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);
                    Assert.IsTrue(datasetReponse.loaderId.Value > 0);

                    isDataFileLoaded(datasetReponse.loaderId.Value.ToString());
                }
            }
        }

        private void isDataFileLoaded(string loaderId)
        {
            var response = HttpUtility.GetAsync(string.Format("{0}/load/{1}/status", FATHOM_ENVIRONMENT, loaderId), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.OK);
            dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);

            bool isSuccessfullyLoaded = false;

            for (var count = 0; count < 100; count++)
            {
                if (!string.IsNullOrWhiteSpace(datasetReponse.runStatus.Value) && !string.Equals(datasetReponse.runStatus.Value, "Error"))
                {
                    if (!string.Equals(datasetReponse.runStatus.Value, "Complete"))
                    {
                        Thread.Sleep(10000 * count);

                        GetIdentityToken();
                        response = HttpUtility.GetAsync(string.Format("{0}/load/{1}/status", FATHOM_ENVIRONMENT, loaderId), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

                        datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);
                    }
                    else
                    {
                        isSuccessfullyLoaded = true;
                        break;
                    }
                }
                else
                {
                    Assert.IsFalse(true, "Error loading dataset");
                    break;
                }
            }

            Assert.IsTrue(isSuccessfullyLoaded, "Dataset taking longer than expected to load");

        }

        [When(@"I load data file")]
        public void WhenILoadDataFile()
        {
            string filesettings = scenarioContext[CONTEXT_KEYS.LOADFILESETTINGS] as string;
            wheniloaddatafile(filesettings);
        }

        [Given(@"I load data file")]
        public void GivenILoadDataFile()
        {
            string filesettings = scenarioContext[CONTEXT_KEYS.LOADFILESETTINGS] as string;
            wheniloaddatafile(filesettings);
        }

        private void wheniloaddatafile(string filesettings)
        {
            var loaderIds = new List<string>();
            var datafiles = (scenarioContext[CONTEXT_KEYS.DATAFILE] as string).Split(',');

            var filesettingList = filesettings.Split(',');

            for (var cntDataFile = 0; cntDataFile < datafiles.Length; cntDataFile++)
            //foreach (var datafile in datafiles)
            {
                var datafile = datafiles[cntDataFile];
                var loadsettingfile = filesettingList[cntDataFile];

                var dataFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], datafile);

                var dataFileName = System.IO.Path.GetFileName(datafile);

                var loadfilesettingsPath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], loadsettingfile);

                var loadfilesettingsContent = new StringContent(System.IO.File.ReadAllText(loadfilesettingsPath), Encoding.UTF8);

                var headers = new Dictionary<string, string>();
                headers.Add("ACCEPT", "application/json");
                headers.Add("Authorization", string.Format("bearer {0}", scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN]));

                var querystringParamaters = string.Format(loadfilesettingsContent.ReadAsStringAsync().Result, scenarioContext[CONTEXT_KEYS.DATASET_NAME]);

                using (var content = new MultipartFormDataContent())
                {
                    var stream = new StreamContent(File.Open(dataFilePath, FileMode.Open));
                    stream.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    stream.Headers.Add("Content-Disposition", string.Format("form-data; name=\"import\"; filename=\"{0}\"", dataFileName));

                    content.Add(stream, "file", dataFileName);

                    var response = HttpUtility.PostAsync(string.Format("{0}/{1}?{2}", FATHOM_ENVIRONMENT, "load", querystringParamaters), string.Empty, headers, content).Result;

                    Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.Created);

                    dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);
                    Assert.IsTrue(datasetReponse.loaderId.Value > 0);

                    loaderIds.Add(datasetReponse.loaderId.Value.ToString());

                    if (cntDataFile == 0)
                    {

                        EXISTING_DATASET_ID = scenarioContext[CONTEXT_KEYS.DATASET_ID].ToString();
                        LOADER_ID = datasetReponse.loaderId.Value.ToString();
                        DATASET_NAME = scenarioContext[CONTEXT_KEYS.DATASET_NAME].ToString();
                    }
                }
            }

            scenarioContext.Add(CONTEXT_KEYS.DATASET_LOADERID, string.Join(",", loaderIds.Select(x => x)));
        }

        [When(@"the file is loaded")]
        public void WhenTheFileIsLoaded()
        {
            theFileIsLoaded();
        }

        [Given(@"the file is loaded")]
        public void GivenTheFileIsLoaded()
        {
            theFileIsLoaded();
        }

        private void theFileIsLoaded()
        {
            var loaderIds = (scenarioContext[CONTEXT_KEYS.DATASET_LOADERID] as string).Split(',');

            foreach (var loaderId in loaderIds)
            {
                var response = HttpUtility.GetAsync(string.Format("{0}/load/{1}/status", FATHOM_ENVIRONMENT, loaderId), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

                Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.OK);
                dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);

                bool isSuccessfullyLoaded = false;

                for (var count = 0; count < 100; count++)
                {
                    if (!string.IsNullOrWhiteSpace(datasetReponse.runStatus.Value) && !string.Equals(datasetReponse.runStatus.Value, "Error"))
                    {
                        if (!string.Equals(datasetReponse.runStatus.Value, "Complete"))
                        {
                            Thread.Sleep(statusCheckInterval * count);

                            response = HttpUtility.GetAsync(string.Format("{0}/load/{1}/status", FATHOM_ENVIRONMENT, loaderId), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

                            datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);
                        }
                        else
                        {
                            isSuccessfullyLoaded = true;
                            break;
                        }
                    }
                    else
                    {
                        Assert.IsFalse(true, "Error loading dataset");
                        break;
                    }
                }

                Assert.IsTrue(isSuccessfullyLoaded, "Dataset taking longer than expected to load");

            }
        }

        [Then(@"should be able to measure data for variables")]
        public void ThenShouldBeAbleToMeasureDataForVariables()
        {
            var requestFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], "request-variable-data.json");

            var headers = new Dictionary<string, string>();
            headers.Add("CONTENTTYPE", "multipart/form-data");
            headers.Add("ACCEPT", "application/json");
            headers.Add("Authorization", string.Format("bearer {0}", scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN]));


            var body = new StringContent(System.IO.File.ReadAllText(requestFilePath), Encoding.UTF8, "application/json");

            var response = HttpUtility.PostAsync(string.Format("{0}/dataset/{1}/variable/data", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID]), string.Empty, headers, body).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.Created);
        }

        [Given(@"existing dataset for (.*)")]
        public void GivenExistingDataset(string testCaseScenario)
        {
            scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO] = testCaseScenario;
            scenarioContext[CONTEXT_KEYS.DATASET_ID] = EXISTING_DATASET_ID;
            scenarioContext[CONTEXT_KEYS.DATASET_LOADERID] = LOADER_ID;
            scenarioContext[CONTEXT_KEYS.DATASET_NAME] = DATASET_NAME;


            //scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO] = testCaseScenario;
            //scenarioContext[CONTEXT_KEYS.DATASET_ID] = 314;
            //scenarioContext[CONTEXT_KEYS.DATASET_LOADERID] = 305;
            //scenarioContext[CONTEXT_KEYS.DATASET_NAME] = "test_6d3cc290d20e42f29ec86b2d811dabf6";
        }


        [Then(@"I should see all the variables1  from (.*)")]
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


        [When(@"I create vdr request for(.*)")]
        public void WhenICreateVdrRequestFor(string vdrrequestFileName)
        {
            var requestId = createvdrrequest(vdrrequestFileName);
            scenarioContext[CONTEXT_KEYS.REQUEST_ID] = requestId;
        }

        private int createvdrrequest(string vdrrequestFileName)
        {
            var requestFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], vdrrequestFileName);

            var postUrl = string.Format("{0}/dataset/{1}/variable/data", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID]);


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
        [Then(@"verify vdr should be success")]
        public void ThenVerifyVdrShouldBeSuccess()
        {
            verifyVdrShouldBeSuccess();
        }

        public void verifyVdrShouldBeSuccess()
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
        [Then(@"Vdr response should be stored in (.*)")]
        public void ThenVdrResponseShouldBeStoredIn(string responseFileName)
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
    }
}
