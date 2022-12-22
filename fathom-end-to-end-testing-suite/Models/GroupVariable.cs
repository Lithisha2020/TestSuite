namespace fathom_end_to_end_testing_suite.Models
{
    class GroupVariable
    {
        public string VariableName { get; set; }
        public string QueryType { get; set; }
        public string TransformType { get; set; }
        public bool IsPattern { get; set; }
        public string Expression { get; set; }
    }
}
