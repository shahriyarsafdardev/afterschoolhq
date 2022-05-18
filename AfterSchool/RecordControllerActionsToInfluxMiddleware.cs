using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using Emmersion.InfluxDB;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace AfterSchool
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RecordControllerActionsToInfluxMiddleware
    {
        private readonly IInfluxRecorder _influxRecorder;
        private readonly AppFunc _next;

        public RecordControllerActionsToInfluxMiddleware(AppFunc next, IInfluxRecorder influxRecorder)
        {
            _next = next;
            _influxRecorder = influxRecorder;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _next(environment);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                SendDataToInflux(environment, elapsedMilliseconds);
            }
        }

        private void SendDataToInflux(IDictionary<string, object> environment, long elapsedMilliseconds)
        {
            var httpContext = (HttpContextBase) environment["System.Web.HttpContextBase"];
            var httpContextResponse = httpContext.Response;
            var httpContextRequest = httpContext.Request;

            var statusCode = httpContextResponse.Status;
            var urlReferrer = httpContextRequest.UrlReferrer?.ToString();
            if (string.IsNullOrEmpty(urlReferrer))
                urlReferrer = "N/A";
            var userAgent = httpContextRequest.UserAgent;

            var callTo = httpContextRequest.Path;

            var routeData = httpContextRequest.RequestContext.RouteData;

            var controller = routeData?.Values["controller"].ToString();
            var controllerAction = controller + "/" + routeData?.Values["action"];

            var points = new[]
            {
                new InfluxPoint(
                    "AfterSchool",
                    new[]
                    {
                        new Field("Duration-Ms", elapsedMilliseconds),
                    },
                    new[]
                    {
                        new Tag("Status-Code", statusCode),
                        new Tag("Call-To", callTo),
                        new Tag("Controller", controller),
                        new Tag("Controller-Action", controllerAction),
                        new Tag("Url-Referrer", urlReferrer),
                        new Tag("User-Agent", userAgent),
                    }
                ),
            };
            _influxRecorder.Record(points);
        }
    }

    public class InfluxLogger : IInfluxLogger
    {
        public void Debug(string message)
        {
            Console.WriteLine(message);
        }

        public void Error(string message, Exception exception)
        {
            Console.WriteLine(message + Environment.NewLine + exception);
        }
    }

    public class InfluxSettings : IInfluxSettings
    {
        /// <summary>
        ///     Constructor to set up values for an InfluxDB 2.* instance.
        /// </summary>
        public InfluxSettings(string hostname, string orgName, string bucketName, string apiToken)
        {
            Hostname = hostname;
            OrgName = orgName;
            BucketName = bucketName;
            ApiToken = apiToken;
        }

        public string Hostname { get; }
        public string OrgName { get; }
        public string BucketName { get; }
        public string ApiToken { get; }
        public int BatchIntervalInSeconds { get; } = 1;
        public int MaxBatchSize { get; } = 10000;
    }
}