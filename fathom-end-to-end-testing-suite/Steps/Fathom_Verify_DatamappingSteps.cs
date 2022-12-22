using fathom_end_to_end_testing_suite.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using TechTalk.SpecFlow;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;


namespace fathom_end_to_end_testing_suite.Steps
{
    [Binding]
    public class Fathom_Verify_DatamappingSteps:Common_Steps
    {
        public Fathom_Verify_DatamappingSteps(ScenarioContext injectedContext):base(injectedContext)
        {
        }

        [When(@"I get the variable data in an excel for (.*)")]
        public void WhenIGetTheVariableDataInAnExcel(string responseFileName)
        {
            getVariableDataInExcel(responseFileName, (int)scenarioContext[CONTEXT_KEYS.REQUEST_ID]);
        }

        private void getVariableDataInExcel(string responseFileName, int requestId)
        {
            var responseFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext [CONTEXT_KEYS.TEST_CASE_SCENARIO], responseFileName);

            var response = HttpUtility.GetAsync(string.Format("{0}/dataset/{1}/variable/data/{2}", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_ID], requestId), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.OK);

            var expectedResult = System.IO.File.ReadAllText(responseFilePath);
            var actualResult = response.HttpResponseMessage;


            var dtActual = readCsvc(actualResult);
            var dtExpected = readCsvc(expectedResult);

            bool isDataMatching = isEqual(dtActual, dtExpected);

            //bool isDataMatching = isCompareCsvData(expectedResult, actualResult);

            Assert.IsTrue(isDataMatching);
        }

        private static DataTable readCsvc(string csvText)
        {
            var dt = new DataTable();
            string[] fileContents = csvText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] headers = fileContents[0].Split(',');
            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }

            for (var row = 1; row < fileContents.Length; row++)
            {
                var content = fileContents[row];
                var rowContent = content.Split(',');
                DataRow dr = dt.NewRow();
                for (int i = 0; i < headers.Length; i++)
                {
                    dr[i] = rowContent[i];
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }


        private static bool isEqual(DataTable dtActual, DataTable dtExpected)
        {
            var isequal = true;

            foreach (var row in dtExpected.AsEnumerable())
            {
                var expectedRow = dtActual.AsEnumerable().Where(x => x.Field<string>(dtExpected.Columns[0].ColumnName).Equals(row.Field<string>(dtExpected.Columns[0].ColumnName)));


                for (var columnIndex = 1; columnIndex <= dtExpected.Columns.Count; columnIndex++)
                {
                    expectedRow.Where(x => x.Field<string>(dtExpected.Columns[columnIndex].ColumnName).Equals(row.Field<string>(dtExpected.Columns[columnIndex].ColumnName)));
                }

                if (expectedRow.Count() == 0)
                {
                    isequal = false;
                    break;
                }

            }

            return isequal;
        }

        //private void isCompareCsvData(string expectedResult, string actualResult, bool isCsv)
        //{
        //    var dtExpected = readCsvc(expectedResult);
        //    var dtActual = readCsvc(actualResult);

        //    foreach(var expectedResultRow in dtExpected.Rows)
        //    {
        //        var actualResultRow = dtExpected.Rows.Find()
        //    }
        //}

        //private DataTable readCsvc(string csvText)
        //{
        //    var dt = new DataTable();
        //    string[] fileContents = csvText.Split(new string[] { "\n" }, StringSplitOptions.None);
        //    string[] headers = fileContents[0].Split(',');
        //    foreach (string header in headers)
        //    {
        //        dt.Columns.Add(header);
        //    }

        //    for (var row = 1; row < fileContents.Length; row++)
        //    {
        //        var content = fileContents[row];
        //        var rowContent = content.Split(',');
        //        DataRow dr = dt.NewRow();
        //        for (int i=0; i< headers.Length;i++)
        //        {
        //            dr[i] = rowContent[i];
        //        }
        //        dt.Rows.Add(dr);
        //    }

        //    return dt;
        //}

        private bool isCompareCsvData(string expectedResult, string actualResult)
        {
            var csv = new StringBuilder();
            string[] fileContentsOne = expectedResult.Split(new string[] { "\n" }, StringSplitOptions.None);
            string[] fileContentsTwo = actualResult.Split(new string[] { "\n" }, StringSplitOptions.None);
            if (!fileContentsOne.Length.Equals(fileContentsTwo.Length))
                return false;

            string[] columnshead1 = fileContentsOne[0].Split(new char[] { ',' });
            string[] columnshead2 = fileContentsTwo[0].Split(new char[] { ',' });

            if (columnshead1.Length != columnshead2.Length)
                return false;

            Dictionary<string, string> dict1 = new Dictionary<string, string>();
            Dictionary<string, string> dict2 = new Dictionary<string, string>();

            for (int i = 0; i < fileContentsOne.Length - 1; ++i)
            {
                dict1.Add(fileContentsOne[i], "");
                dict2.Add(fileContentsOne[i], "");
            }

            foreach(var row in dict1)
            {
                if (!dict2.ContainsKey(row.Key))
                    return false;
            }

            return true;
        }
        //private bool isCompareCsvData(string expectedResult, string actualResult)
        //{
        //    DataTable dtExpected = new DataTable();
        //    dtExpected.Columns.AddRange(new DataColumn[6] {
        //    new DataColumn("input_key", typeof(int)),
        //    new DataColumn("input_source", typeof(string)),
        //    new DataColumn("type", typeof(string)),
        //    new DataColumn("from_mapping_file",typeof(string)),
        //    new DataColumn("input_value", typeof(string)),
        //    new DataColumn("mapped_value", typeof(string))
        //    });

        //    DataTable dtActual = new DataTable();
        //    dtExpected.Columns.AddRange(new DataColumn[6] {
        //    new DataColumn("input_key", typeof(int)),
        //    new DataColumn("input_source", typeof(string)),
        //    new DataColumn("type", typeof(string)),
        //    new DataColumn("from_mapping_file",typeof(string)),
        //    new DataColumn("input_value", typeof(string)),
        //    new DataColumn("mapped_value", typeof(string))
        //    });

        //    //Execute a loop over the rows.  
        //    foreach (string row in expectedResult.Split('\n'))
        //    {
        //        if (!string.IsNullOrEmpty(row))
        //        {
        //            dtExpected.Rows.Add();
        //            int i = 0;

        //            //Execute a loop over the columns.  
        //            foreach (string cell in row.Split(','))
        //            {
        //                dtExpected.Rows[dtExpected.Rows.Count - 1][i] = cell;
        //                i++;
        //            }
        //        }
        //    }


        //    //Execute a loop over the rows.  
        //    foreach (string row in actualResult.Split('\n'))
        //    {
        //        if (!string.IsNullOrEmpty(row))
        //        {
        //            dtExpected.Rows.Add();
        //            int i = 0;

        //            //Execute a loop over the columns.  
        //            foreach (string cell in row.Split(','))
        //            {
        //                dtActual.Rows[dtExpected.Rows.Count - 1][i] = cell;
        //                i++;
        //            }
        //        }
        //    }


        //    foreach(var dtRow in dtActual.Rows)
        //    {

        //    }
        //}

        [When(@"upload the data map file with (.*) and (.*)")]
        public void WhenUploadTheDataMapFile(string loadfilesettings, string updatedDataMapFile)
        {
            //Arrange
            var dataFilePath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], updatedDataMapFile);

            var loadfilesettingsPath = string.Format("{0}/Resources/Test-Cases/{1}/{2}", HttpUtility.AssemblyDirectory, scenarioContext[CONTEXT_KEYS.TEST_CASE_SCENARIO], loadfilesettings);

            var headers = new Dictionary<string, string>();
            headers.Add("ACCEPT", "application/json");
            headers.Add("Authorization", string.Format("bearer {0}", scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN]));

            var loadfilesettingsContent = new StringContent(System.IO.File.ReadAllText(loadfilesettingsPath), Encoding.UTF8);

            var querystringParamaters = string.Format(loadfilesettingsContent.ReadAsStringAsync().Result, scenarioContext[CONTEXT_KEYS.DATASET_NAME]);


            using (var content = new MultipartFormDataContent())
            {
                var stream = new StreamContent(File.Open(dataFilePath, FileMode.Open));
                stream.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                stream.Headers.Add("Content-Disposition", "form-data; name=\"import\"; filename=\"" + updatedDataMapFile + "\"");

                content.Add(stream, "file", updatedDataMapFile);

                //Act
                var response = HttpUtility.PostAsync(string.Format("{0}/{1}?{2}", FATHOM_ENVIRONMENT, "load", querystringParamaters), string.Empty, headers, content).Result;

                Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.Created);

                dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);

                //Assert
                Assert.IsTrue(datasetReponse.loaderId.Value > 0);
                scenarioContext.Add(CONTEXT_KEYS.DATASET_LOADERID_UPDATED, datasetReponse.loaderId.Value);
            }
        }
        
        [Then(@"I need to verify the datamap")]
        public void ThenINeedToVerifyTheDatamap()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I update the data mapping")]
        public void WhenIUpdateTheDataMappingWith()
        {   
        }

        [When(@"the file is loaded with loaderId")]
        public void WhenTheFileIsLoaded()
        {
            var response = HttpUtility.GetAsync(string.Format("{0}/load/{1}/status", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_LOADERID_UPDATED]), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

            Assert.IsTrue(response.HeaderResponseStatus == System.Net.HttpStatusCode.OK);
            dynamic datasetReponse = JsonConvert.DeserializeObject<dynamic>(response.HttpResponseMessage);

            bool isSuccessfullyLoaded = false;

            for (var count = 0; count < 100; count++)
            {
                var datasetResponseStatus = datasetReponse.runStatus.Value ?? string.Empty;
                if (string.Equals(datasetResponseStatus, "Error"))
                {
                    Assert.IsFalse(true, "Error loading dataset");
                    break;
                }
                
                if (!string.Equals(datasetResponseStatus, "Complete"))
                {
                    Thread.Sleep(10000 * count);

                    response = HttpUtility.GetAsync(string.Format("{0}/load/{1}/status", FATHOM_ENVIRONMENT, scenarioContext[CONTEXT_KEYS.DATASET_LOADERID_UPDATED]), scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString(), new Dictionary<string, string>(), new Dictionary<string, string>()).Result;

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

            Assert.IsTrue(isSuccessfullyLoaded, "Dataset taking longer than expected to load");

        }

        [Then(@"I should get the variables data for the data mapped variables for (.*)")]
        public void ThenIShouldGetTheVariablesDataForTheDataMappedVariables(string responseFileName)
        {
            getVariableDataInExcel(responseFileName, (int)scenarioContext[CONTEXT_KEYS.REQUEST_ID]);
        }

    }
}
