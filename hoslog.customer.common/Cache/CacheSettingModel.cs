namespace hoslog.customer.common.Cache
{
    public class RedisCacheSetting
    {
        public bool isEnabled { get; set; }
        public string? host { get; set; }
        public int port { get; set; }
        public bool useSsl { get; set; }
        public string? sslHost { get; set; }
        public int connectTimeout { get; set; }
        public int syncTimeout { get; set; }
        public string? instanceName { get; set; }
        public string? password { get; set; }
        public string? channel { get; set; }
        public int minimumIntervalInMinutes { get; set; }
        public int mediumIntervalInMinutes { get; set; }
        public int maximumIntervalInMinutes { get; set; }
        public int minimumIntervalInSeconds { get; set; }
        public int mediumIntervalInSeconds { get; set; }
        public int maximumIntervalInSeconds { get; set; }
    }
}
