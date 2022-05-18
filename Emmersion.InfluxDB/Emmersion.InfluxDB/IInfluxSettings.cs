namespace Emmersion.InfluxDB
{
    public interface IInfluxSettings
    {
        /// <summary>
        ///     Typical value: http://localhost:8086
        /// </summary>
        string Hostname { get; }

        string OrgName { get; }

        string BucketName { get; }

        string ApiToken { get; }

        /// <summary>
        ///     Typical value: 1
        /// </summary>
        int BatchIntervalInSeconds { get; }

        /// <summary>
        ///     Typical value: 10000
        /// </summary>
        int MaxBatchSize { get; }
    }
}