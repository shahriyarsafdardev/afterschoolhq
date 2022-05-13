using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Emmersion.InfluxDB
{
    internal interface ISender
    {
        void SendPayload(string payload);
    }

    internal class Sender : ISender
    {
        private readonly IHttpClient httpClient;
        private readonly IInfluxSettings settings;

        public Sender(IHttpClient httpClient, IInfluxSettings settings)
        {
            this.settings = settings;
            this.httpClient = httpClient;
        }

        public void SendPayload(string payload)
        {
            var content = new StringContent(payload, Encoding.UTF8,"text/plain");
            var message = new HttpRequestMessage(HttpMethod.Post,
                    $"{settings.Hostname}:{settings.Port}/api/v2/write?org={settings.OrgName}&bucket={settings.BucketName}&precision=ns")
                {Content = content};
            message.Headers.Authorization = new AuthenticationHeaderValue("Token", settings.ApiToken);
            message.Headers.Add("Accept", "application/json");
          

            (int statusCode, string responseBody) response;
            try
            {
                response = httpClient.Execute(message);
            }
            catch (Exception e)
            {
                throw new AggregateException("Could not send points", e);
            }

            if (response.statusCode != 204)
                throw new Exception(
                    $"Failed to write metrics (influxUrl: {message.RequestUri}, response: {response.responseBody})");
        }
    }
}