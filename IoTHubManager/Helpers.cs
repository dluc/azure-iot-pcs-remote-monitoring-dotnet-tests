﻿using System;
using System.Security.Cryptography;
using System.Net;
using Helpers.Http;
using Xunit;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Text;

namespace IoTHubManager
{
    class Helpers
    {
        private readonly HttpRequestWrapper Request;

        /*
        Generates random SHA1 hash mimicing the X509 thumb print
         */
        internal static string GenerateNewThumbPrint()
        {

            string input = Guid.NewGuid().ToString();
            SHA1Managed sha = new SHA1Managed();

            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var stringBuilder = new StringBuilder(hash.Length * 2);

            for (int i = 0; i < hash.Length; i++)
            {
                stringBuilder.Append(hash[i].ToString("X2"));
            }

            return stringBuilder.ToString();
        }

        //Helper methods for fetching (and retrying) the current Job Status.
        /**
         * Gets job status using job id.
         */
        private static JObject GetJobStatus(HttpRequestWrapper Request, 
                                            string JobId)
        {
            IHttpResponse jobStatusResponse = Request.Get(JobId, null);
            Assert.Equal(HttpStatusCode.OK, jobStatusResponse.StatusCode);
            return JObject.Parse(jobStatusResponse.Content);
        }

        /**
         * Monitor job status using polling (re-try) mechanism 
         */
        internal static JObject GetJobStatuswithReTry(HttpRequestWrapper Request, 
                                                      string jobId)
        {
            var jobStatus = GetJobStatus(Request, jobId);

            for (int trials = 0; trials < Constants.Jobs.MAX_TRIALS; trials++)
            {
                if (Constants.Jobs.JOB_COMPLETED == jobStatus["Status"].ToObject<int>())
                {
                    break;
                }
                Thread.Sleep(Constants.Jobs.WAIT);
                jobStatus = GetJobStatus(Request, jobId);
            }

            return jobStatus;
        }

        internal static void AssertJobwasCompletedSuccessfully(string content, 
                                                                   int jobType, 
                                                                   HttpRequestWrapper request)
        {
            // Check if job was submitted successfully.
            var job = JObject.Parse(content);
            Assert.Equal<int>(Constants.Jobs.JOB_IN_PROGRESS, job["Status"].ToObject<int>());
            Assert.Equal<int>(jobType, job["Type"].ToObject<int>());

            // Get Job status by polling to verify if job was successful.
            var tagJobStatus = GetJobStatuswithReTry(request, job["JobId"].ToString());
            // Assert to see if Last try yielded correct status.
            Assert.Equal<int>(Constants.Jobs.JOB_COMPLETED, tagJobStatus["Status"].ToObject<int>());
            Assert.Equal<int>(jobType, tagJobStatus["Type"].ToObject<int>());
        }
    }
}
