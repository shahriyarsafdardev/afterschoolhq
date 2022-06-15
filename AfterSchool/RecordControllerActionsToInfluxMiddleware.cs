using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BasicLibrary;
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
            const string notApplicable = "N/A";
            var httpContext = (HttpContextBase) environment["System.Web.HttpContextBase"];
            var response = httpContext.Response;
            var request = httpContext.Request;
            var routeData = request.RequestContext.RouteData;

            var ipAddress = string.IsNullOrEmpty(request.ServerVariables["HTTP_X_FORWARDED_FOR"])
                ? request.ServerVariables["REMOTE_ADDR"]
                : request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',').FirstOrDefault()?.Trim();

            if (ipAddress.IsEmpty()) ipAddress = notApplicable;

            var accountId = httpContext.Items["AccountId"]?.ToString();
            if (accountId.IsEmpty()) accountId = notApplicable;

            var webUserId = httpContext.Items["WebUserId"]?.ToString();
            if (webUserId.IsEmpty()) webUserId = notApplicable;

            var statusCode = response.Status;
            var callTo = request.Path;
            var controller = routeData?.Values["controller"]?.ToString() ?? notApplicable;
            var controllerAction = controller == notApplicable ? notApplicable : controller + "/" + routeData?.Values["action"];

            var urlReferrer = request.UrlReferrer?.ToString();
            if (string.IsNullOrEmpty(urlReferrer))
                urlReferrer = notApplicable;

            var userAgent = request.UserAgent;

            var points = new[]
            {
                new InfluxPoint(
                    "AfterSchool",
                    new[]
                    {
                        new Field("Duration-Ms", elapsedMilliseconds),
                        new Field("Ip-Address", ipAddress),
                        new Field("Account-Id", accountId),
                        new Field("Web-User-Id", webUserId),
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