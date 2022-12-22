using fathom_end_to_end_testing_suite.Steps;
using TechTalk.SpecFlow;

namespace fathom_end_to_end_testing_suite.Utilities
{
    class EnvironmentUtility
    {
        public static string ResolvePath(string fileName, ScenarioContext context)
            => $"{HttpUtility.AssemblyDirectory}/Resources/Test-Cases/{context[CONTEXT_KEYS.TEST_CASE_SCENARIO]}/{fileName}";
    }
}
