using System.Threading;
using System.Threading.Tasks;
using Lib.Configuration;
using Lib.ElasticSearch;
using Lib.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.HostedServices
{
    public class ElasticHostedService : IHostedService
    {
        private readonly IElasticProvider _provider;
        private readonly ICustomJsonSerializer _serializer;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public ElasticHostedService(
            IElasticProvider elasticProvider,
            ICustomJsonSerializer serializer,
            IConfiguration configuration,
            ILogger logger)
        {
            _provider = elasticProvider;
            _serializer = serializer;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            LogCurrentSessionConfiguration();

            _logger.LogInformation("Starting streaming logs from elastic search");

            await foreach (var payload in _provider.QueryAsync<ElasticPayload>(cancellationToken))
            {
                var json = _serializer.Serialize(payload);
                _logger.LogInformation("Found matching logs: {json}", json);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping streaming logs from elastic search");
            return Task.CompletedTask;
        }

        private void LogCurrentSessionConfiguration()
        {
            var settings = _configuration.GetSection("Settings:ElasticSettings").Get<ElasticSettings>();
            var maxLogFileSizeInBytes = _configuration.GetValue<int>("Logging:FileSizeLimitBytes");
            var maxLogFileSizeInMegaBytes = maxLogFileSizeInBytes / 1000000;
            var session = $"{settings} MaxLogFileSizeInMegaBytes={maxLogFileSizeInMegaBytes}MB";
            _logger.LogInformation("Current session configuration: {session}", session);
        }
    }
}
