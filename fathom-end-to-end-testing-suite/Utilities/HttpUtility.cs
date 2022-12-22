using fathom_end_to_end_testing_suite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;
using System.IO;

namespace fathom_end_to_end_testing_suite.Utilities
{
    public class HttpUtility
    {
        private static readonly HttpClient HttpClient;

        static HttpUtility()
        {
            HttpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));
            HttpClient.Timeout = new TimeSpan(1, 0, 0);
        }

        public static async Task<HttpResponseInTest> GetAsync(string url, 
            string authorizationToken, 
            Dictionary<string, string> headers, 
            Dictionary<string, string> querystringParameters)
        {
            HttpClient.DefaultRequestHeaders.Clear();

            if (!string.IsNullOrWhiteSpace(authorizationToken))
            {
                HttpClient.DefaultRequestHeaders.Add("Authorization", string.Format("{0} {1}", "bearer", authorizationToken));
            }

            foreach (var headerKey in headers.Keys)
            {
                HttpClient.DefaultRequestHeaders.Add(headerKey, headers[headerKey]);
            }

            StringBuilder urlSb = new StringBuilder(url);
            if (querystringParameters.Count > 0)
            {
                urlSb.Append("?");
            }
            
            foreach (var querystringParamter in querystringParameters)
            {
                urlSb.Append(string.Format("{0}={1}&", querystringParamter.Key, querystringParamter.Value));
            }

            url = urlSb.ToString();

            HttpResponseMessage response = await HttpClient.GetAsync(url);
            var responseMessaage = await response.Content.ReadAsStringAsync();


            return new HttpResponseInTest { HttpResponseMessage = responseMessaage, HeaderResponseStatus = response.StatusCode };
        }


        public static async Task<HttpResponseInTest> PostAsync(string url,
           string authorizationToken,
           Dictionary<string, string> headers,
           Dictionary<string, string> body)
            {
            HttpClient.DefaultRequestHeaders.Clear();

            if (!string.IsNullOrWhiteSpace(authorizationToken))
            {
                HttpClient.DefaultRequestHeaders.Add("Authorization", string.Format("{0} {1}", "bearer", authorizationToken));
            }

            foreach (var headerKey in headers.Keys)
            {
                if(!HttpClient.DefaultRequestHeaders.Contains(headerKey))
                    HttpClient.DefaultRequestHeaders.Add(headerKey, headers[headerKey]);
            }

            var dataContent = new FormUrlEncodedContent(body.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)));
            
            HttpResponseMessage response = await HttpClient.PostAsync(url, dataContent);
            var responseMessaage = await response.Content.ReadAsStringAsync();


            return new HttpResponseInTest { HttpResponseMessage = responseMessaage, HeaderResponseStatus = response.StatusCode };
        }

        public static async Task<HttpResponseInTest> PostAsync(string url,
           string authorizationToken,
           Dictionary<string, string> headers,
           MultipartFormDataContent body)
        {
            HttpClient.DefaultRequestHeaders.Clear();

            if (!string.IsNullOrWhiteSpace(authorizationToken))
            {
                HttpClient.DefaultRequestHeaders.Add("Authorization", string.Format("{0} {1}", "bearer", authorizationToken));
            }

            foreach (var headerKey in headers.Keys)
            {
                if (!HttpClient.DefaultRequestHeaders.Contains(headerKey))
                    HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(headerKey, headers[headerKey]);
            }


            HttpResponseMessage response = await HttpClient.PostAsync(url, body);
            var responseMessaage = await response.Content.ReadAsStringAsync();


            return new HttpResponseInTest { HttpResponseMessage = responseMessaage, HeaderResponseStatus = response.StatusCode };
        }

        public static async Task<HttpResponseInTest> PostAsync(string url,
           string authorizationToken,
           Dictionary<string, string> headers,
           StringContent body)
        {
            HttpClient.DefaultRequestHeaders.Clear();

            if (!string.IsNullOrWhiteSpace(authorizationToken))
            {
                HttpClient.DefaultRequestHeaders.Add("Authorization", string.Format("{0} {1}", "bearer", authorizationToken));
            }

            foreach (var headerKey in headers.Keys)
            {
                if (!HttpClient.DefaultRequestHeaders.Contains(headerKey))
                    HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(headerKey, headers[headerKey]);
            }


            HttpResponseMessage response = await HttpClient.PostAsync(url, body);
            var responseMessaage = await response.Content.ReadAsStringAsync();


            return new HttpResponseInTest { HttpResponseMessage = responseMessaage, HeaderResponseStatus = response.StatusCode };
        }

        public static string AssemblyDirectory
        {
            get
            {
                //string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                //UriBuilder uri = new UriBuilder(codeBase);
                //string path = Uri.UnescapeDataString(uri.Path);
                //return Path.GetDirectoryName(path);

                var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();

                return path.Replace("obj", "bin");
            }
        }
    }
}
