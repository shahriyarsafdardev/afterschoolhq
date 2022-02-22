using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Emmersion.InfluxDB;
using Newtonsoft.Json;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace AfterSchool
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RecordControllerActionsToInfluxMiddleware
    {
        private readonly IInfluxRecorder _influxRecorder;
        private readonly AppFunc _next;
        private const bool WriteAllSerializedData = false;
        private const bool WriteAllProblemsToFile = true;

        public RecordControllerActionsToInfluxMiddleware(AppFunc next, IInfluxRecorder influxRecorder)
        {
            _next = next;
            _influxRecorder = influxRecorder;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(IDictionary<string, object> environment)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _next(environment);
            }
            finally
            {
                try
                {
                    await SendDataToInflux(environment, stopwatch);
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (WriteAllProblemsToFile) await WriteProblemsToFile(ex.ToString());
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(ex2);
                    }
                }
            }
        }

        private async Task SendDataToInflux(IDictionary<string, object> environment, Stopwatch stopwatch)
        {
            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (WriteAllSerializedData) await SerializeDataAndWriteToFile(environment, elapsedMilliseconds);

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
                        new Field("Duration-Ms", elapsedMilliseconds)
                    },
                    new[]
                    {
                        new Tag("Status-Code", statusCode),
                        new Tag("Call-To", callTo),
                        new Tag("Controller", controller),
                        new Tag("Controller-Action", controllerAction),
                        new Tag("Url-Referrer", urlReferrer),
                        new Tag("User-Agent", userAgent)
                    }
                )
            };
            _influxRecorder.Record(points);
        }

        private static async Task SerializeDataAndWriteToFile(IDictionary<string, object> environment,
            long elapsedMilliseconds)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Error = (_, err) => err.ErrorContext.Handled = true
            };

            environment.Add("Timestamp", DateTimeOffset.UtcNow);
            environment.Add("ElapsedMilliseconds", elapsedMilliseconds);
            var json = JsonConvert.SerializeObject(environment, jsonSerializerSettings);
            Console.WriteLine(json);

            var filePath = $@"C:\code\jsonLogs\allJsonData-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
            using (var streamWriter = File.CreateText(filePath))
            {
                await streamWriter.WriteLineAsync(json);
            }
        }

        private static async Task WriteProblemsToFile(string exceptionString)
        {
            var filePath = $@"C:\code\problems\newProblem-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
            using (var streamWriter = File.CreateText(filePath))
            {
                await streamWriter.WriteLineAsync(exceptionString);
            }
        }
    }

    public class InfluxLogger : IInfluxLogger
    {
        public void Debug(string message)
        {
            WriteProblemsToFile("DEBUG", message);
        }

        public void Error(string message, Exception exception)
        {
            WriteProblemsToFile("ERROR", message + Environment.NewLine + message);
        }

        private static void WriteProblemsToFile(string type, string exceptionString)
        {
            try
            {
                var filePath = $@"C:\code\problems\{type}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
                using (var streamWriter = File.CreateText(filePath))
                {
                    streamWriter.WriteLine(exceptionString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public class InfluxSettings : IInfluxSettings
    {
        public InfluxSettings(string hostname, int port, string dbName)
        {
            Hostname = hostname;
            Port = port;
            DbName = dbName;
        }

        public string Hostname { get; }
        public int Port { get; }
        public string DbName { get; }
        public int BatchIntervalInSeconds { get; } = 1;
        public int MaxBatchSize { get; } = 10000;
    }
}