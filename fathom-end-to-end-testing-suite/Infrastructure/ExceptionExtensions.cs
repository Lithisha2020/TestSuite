using System;
using System.Collections.Generic;

namespace fathom_end_to_end_testing_suite.Infrastructure
{
    public static class ExceptionExtensions
    {
        public static string GetAllMessages(this Exception e)
        {
            var messages = new List<string>();
            Exception current = e;

            while (current != null)
            {
                messages.Add(current.Message);
                current = current.InnerException;
            }

            return String.Join(". ", messages);
        }
    }
}
