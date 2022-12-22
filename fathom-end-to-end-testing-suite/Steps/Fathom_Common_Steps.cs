using fathom_end_to_end_testing_suite.Models;
using fathom_end_to_end_testing_suite.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow;

namespace fathom_end_to_end_testing_suite.Steps
{
    [Binding]
    public class Fathom_Common_Steps : Common_Steps
    {
        public Fathom_Common_Steps(ScenarioContext context) : base(context)
        {
        }

        [When(@"a group (.*) is created with (.*)")]
        public void WhenAGroupIsCreatedWith(string groupName, string groupVariableFile)
        {
            var datasetId = long.Parse(scenarioContext[CONTEXT_KEYS.DATASET_ID].ToString());
            var variables = File.ReadAllText(EnvironmentUtility.ResolvePath(groupVariableFile, scenarioContext))
                .Split(',')
                .Select(v => new GroupVariable() { VariableName = v, QueryType = "primary", TransformType = "normal" });

            var api = FathomApiClient.Instance;

            var result = api.CreateGroup(datasetId, groupName, variables);

            Assert.IsTrue(result.IsSuccess, result.Error);
        }

        [When(@"a snapshot (.*) of the group (.*) is created")]
        public void WhenASnapshotIsCreatedWith(string snapshotName, string groupName)
        {
            var datasetId = long.Parse(scenarioContext[CONTEXT_KEYS.DATASET_ID].ToString());
            var api = FathomApiClient.Instance;

            var result = api.CreateSnapshot(datasetId, snapshotName, groupName, 100, statusCheckInterval);

            Assert.IsTrue(result.IsSuccess, result.Error);
        }

        [When(@"the data is imported in namespace (.*)")]
        public void WhenDataIsImported(string fathomNamespace)
        {
            var loaderId = long.Parse(scenarioContext[CONTEXT_KEYS.DATASET_LOADERID].ToString());
            var api = FathomApiClient.Instance;

            var result = api.Import(loaderId, fathomNamespace);

            Assert.IsTrue(result.IsSuccess, result.Error);
        }
    }
}
