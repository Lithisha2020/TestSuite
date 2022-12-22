using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace fathom_end_to_end_testing_suite.Steps
{
    [Binding]
    public class TestRunHooks
    {
        public static FeatureContext featureContext;

        public TestRunHooks(FeatureContext injectedContext)
        {          
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext injectFeatureContext)
        {
            featureContext = injectFeatureContext;
        }

        [AfterFeature]
        public static void AfterFeature(FeatureContext injectFeatureContext)
        {
            featureContext = null;
        }

        [BeforeScenario]
        public static void BeforeScenario()
        {
        }

        [BeforeScenarioBlock]
        public static void BeforeScenarioBlock()
        {
        }

        [AfterScenarioBlock]
        public static void AfterScenarioBlock()
        {
        }

        [BeforeStep]
        public static void BeforeScenarioStep()
        {
        }

        [AfterStep]
        public static void AfterScenarioStep()
        {
        }
    }
}
