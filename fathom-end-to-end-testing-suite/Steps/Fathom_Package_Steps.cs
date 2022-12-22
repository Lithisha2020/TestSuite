using fathom_end_to_end_testing_suite.Infrastructure;
using fathom_end_to_end_testing_suite.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace fathom_end_to_end_testing_suite.Steps
{
    [Binding]
    public sealed class Fathom_Package_Steps : Common_Steps
    {
        readonly FathomPackageUtility fathomPackageUtility;

        public Fathom_Package_Steps(ScenarioContext context) : base(context)
        {
            fathomPackageUtility = new FathomPackageUtility(FATHOM_ENVIRONMENT, context);
        }

        [Given(@"an existing minimum package version (.*)")]
        public void GivenAnExistingMinimumPackageVersion(string packageVersion)
        {
            var util = fathomPackageUtility;

            var name = util.GetPackageName(packageVersion);

            var result = util.GetLatestVersion(name).AndThen((latestVersion) => util.IsGreaterOrEqual(latestVersion, packageVersion));

            Assert.IsTrue(result.IsSuccess, result.Error);
            Assert.IsTrue(result.Value, "Package version does not meet minimum requirements for this test");
        }

        [Given(@"a test package (.*) for (.*) has been applied")]
        public void GivenATestPackageForHasBeenApplied(string packageDefinitionFile, string scenario)
        {
            var packageDefinitionFilePath = $"{HttpUtility.AssemblyDirectory}/Resources/Test-Cases/{scenario}/{packageDefinitionFile}";
            var datasetName = scenarioContext[CONTEXT_KEYS.DATASET_NAME].ToString();

            var result = fathomPackageUtility.ApplyPackageFromFile(datasetName, packageDefinitionFilePath);
            Assert.IsTrue(result.IsSuccess, result.Error);
        }
    }
}
