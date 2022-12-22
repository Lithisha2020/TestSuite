using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace fathom_end_to_end_testing_suite.Utilities
{
    public class LoggingHandler : DelegatingHandler
    {
        

        public LoggingHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
        {
        }

        public LoggingHandler()
            : base(new HttpClientHandler())
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Request:");
            sb.AppendLine(masks(request.ToString()));
            if (request.Content != null)
            {
                string requestString = await request.Content.ReadAsStringAsync();
                sb.AppendLine(masks(requestString));
            }
            sb.AppendLine();

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            sb.AppendLine("Response:");
            sb.AppendLine(masks(response.ToString()));
            if (response.Content != null)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                sb.AppendLine(masks(responseString));
            }
            sb.AppendLine();
            
            Console.WriteLine(sb.ToString());
            return response;
        }

        private string masks(string requestString)
        {
            return maskJson(maskQuerystringParameter(requestString));
        }

        private string maskQuerystringParameter(string requestString)
        {
            List<string> variablesToBeReplaced = new List<string>();
            variablesToBeReplaced.Add("resource");
            variablesToBeReplaced.Add("client_id");
            variablesToBeReplaced.Add("client_secret");
            variablesToBeReplaced.Add("grant_type");
            variablesToBeReplaced.Add("Password");

            Regex regex = null;

            foreach (var variableToBeReplaced in variablesToBeReplaced)
            {
                string regexStr = string.Format(@"({0})=([^&]*)", variableToBeReplaced);
                regex = new Regex(regexStr);
                requestString = regex.Replace(requestString, "$1*****");
            }

            return requestString;            
        }

        private string maskJson(string requestString)
        {
            List<string> variablesToBeReplaced = new List<string>();
            variablesToBeReplaced.Add("access_token");
            variablesToBeReplaced.Add("Authorization");
            variablesToBeReplaced.Add("Password");

            Regex regex = null;

            foreach (var variableToBeReplaced in variablesToBeReplaced)
            {
                string regexStr = string.Format(@"""({0})"":""([\S\s]*)""", variableToBeReplaced);
                regex = new Regex(regexStr);
                requestString = regex.Replace(requestString, "$1 *****");

                regexStr = string.Format(@"({0}):([\S\s]*)", variableToBeReplaced);
                regex = new Regex(regexStr);
                requestString = regex.Replace(requestString, "$1 *****");
            }

            return requestString;
        }

        private bool isUrlNotTracked(HttpRequestMessage request)
        {
            if (request.RequestUri.AbsoluteUri.Contains("login.microsoftonline.com"))
            {
                return true;
            }
            return false;
        }
    }




    public class HttpEventListener : EventListener
    {
        class HttpEvent
        {
            public Stopwatch Stopwatch { get; set; }

            public string Url { get; set; }

            public DateTimeOffset RequestedAt { get; set; }
        }

        private const int HttpBeginResponse = 140;
        private const int HttpEndResponse = 141;
        private const int HttpBeginGetRequestStream = 142;
        private const int HttpEndGetRequestStream = 143;

        private readonly ConcurrentDictionary<long, HttpEvent> _trackedEvents = new ConcurrentDictionary<long, HttpEvent>();

        public HttpEventListener()
        {
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource != null && eventSource.Name == "System.Diagnostics.Eventing.FrameworkEventSource")
            {
                EnableEvents(eventSource, EventLevel.Informational, (EventKeywords)4);
            }
            base.OnEventSourceCreated(eventSource);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData?.Payload == null)
                return;

            try
            {
                switch (eventData.EventId)
                {
                    case HttpBeginResponse:
                    case HttpBeginGetRequestStream:
                        OnBeginHttpResponse(eventData);
                        break;
                    case HttpEndResponse:
                    case HttpEndGetRequestStream:
                        OnEndHttpResponse(eventData);
                        break;
                }
            }
            catch (Exception)
            {
                // don't let the tracer break due to frailities underneath, you might want to consider unbinding it
            }
        }

        private void OnBeginHttpResponse(EventWrittenEventArgs httpEventData)
        {
            if (httpEventData.Payload.Count < 2)
            {
                return;
            }
#if NET46
            int indexOfId = httpEventData.PayloadNames.IndexOf("id");
            int indexOfUrl = httpEventData.PayloadNames.IndexOf("uri");
#else
            int indexOfId = 0;
            int indexOfUrl = 1;
#endif

            if (indexOfId == -1 || indexOfUrl == -1)
            {
                return;
            }
            long id = Convert.ToInt64(httpEventData.Payload[indexOfId], CultureInfo.InvariantCulture);
            string url = Convert.ToString(httpEventData.Payload[indexOfUrl], CultureInfo.InvariantCulture);
            _trackedEvents[id] = new HttpEvent
            {
                Url = url,
                Stopwatch = new Stopwatch(),
                RequestedAt = DateTimeOffset.UtcNow
            };
            _trackedEvents[id].Stopwatch.Start();
        }

        private void OnEndHttpResponse(EventWrittenEventArgs httpEventData)
        {
            if (httpEventData.Payload.Count < 1)
            {
                return;
            }
#if NET46
            int indexOfId = httpEventData.PayloadNames.IndexOf("id");
            if (indexOfId == -1)
            {
                return;
            }
#else
            int indexOfId = 0;
#endif
            long id = Convert.ToInt64(httpEventData.Payload[indexOfId], CultureInfo.InvariantCulture);
            HttpEvent trackedEvent;
            if (_trackedEvents.TryRemove(id, out trackedEvent))
            {
                trackedEvent.Stopwatch.Stop();
#if NET46
                int indexOfSuccess = httpEventData.PayloadNames.IndexOf("success");
                int indexOfSynchronous = httpEventData.PayloadNames.IndexOf("synchronous");
                int indexOfStatusCode = httpEventData.PayloadNames.IndexOf("statusCode");
#else
                int indexOfSuccess = httpEventData.Payload.Count > 1 ? 1 : -1;
                int indexOfSynchronous = httpEventData.Payload.Count > 2 ? 2 : -1;
                int indexOfStatusCode = httpEventData.Payload.Count > 3 ? 3 : -1;
#endif

                bool? success = indexOfSuccess > -1 ? new bool?(Convert.ToBoolean(httpEventData.Payload[indexOfSuccess])) : null;
                bool? synchronous = indexOfSynchronous > -1 ? new bool?(Convert.ToBoolean(httpEventData.Payload[indexOfSynchronous])) : null;
                int? statusCode = indexOfStatusCode > -1 ? new int?(Convert.ToInt32(httpEventData.Payload[indexOfStatusCode])) : null;

                Console.WriteLine($"Url: {trackedEvent.Url}\r\nElapsed Time: {trackedEvent.Stopwatch.ElapsedMilliseconds}ms\r\nSuccess: {success}\r\nStatus Code: {statusCode}\r\nSynchronus: {synchronous}");
            }
        }
    }
}
