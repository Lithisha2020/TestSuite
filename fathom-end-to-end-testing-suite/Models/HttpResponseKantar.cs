using System.Collections.Generic;
using System.Net;

namespace fathom_end_to_end_testing_suite.Models
{
    public class HttpResponseInTest
    {
        public HttpStatusCode HeaderResponseStatus { get; set; }
        public string HttpResponseMessage { get; set; }
    }

    public class DataResult
    {
        public string ResultType { get; set; }
        public string RunStatus { get; set; }
        public IEnumerable<Dictionary<string, object>> MetaData { get; set; }
        public IEnumerable<Dictionary<string, object>> Data { get; set; }

    }
}
