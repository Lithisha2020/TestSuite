using System.Collections.Generic;

namespace fathom_end_to_end_testing_suite.Models
{
    class Group
    {
        public string GroupName { get; set; }
        public IEnumerable<GroupVariable> variables { get; set; }
    }
}
