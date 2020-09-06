using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using Lib.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace Lib.ElasticSearch
{
    public class ElasticProvider : IElasticProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ElasticClient _elasticClient;
        private readonly ElasticSettings _settings;
        private readonly ILogger _logger;

        public ElasticProvider(HttpClient httpClient, IOptions<Settings> options, ILogger logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _settings = options.Value.ElasticSettings;

            if (!_settings.IsValid())
            {
                throw new ArgumentException($"Invalid elastic search settings '{_settings}'");
            }

            if (!IsValidElasticSearchUrl())
            {
                throw new HttpRequestException($"Invalid elastic search url {_settings.Url}");
            }

            var connectionSettings = new ConnectionSettings(new Uri(_settings.Url));
            _elasticClient = new ElasticClient(connectionSettings);
        }

        public async IAsyncEnumerable<T> QueryAsync<T>([EnumeratorCancellation] CancellationToken cancellationToken = default) where T : class
        {
            var cultureInfo = new CultureInfo(_settings.CultureForDate);
            var from = DateTime.ParseExact(_settings.From, _settings.FormatForDate, cultureInfo);
            var to = DateTime.ParseExact(_settings.To, _settings.FormatForDate, cultureInfo);

            var response = await _elasticClient.SearchAsync<T>(s => s
                .Index(_settings.Index)
                .From(0)
                .Query(BuildQuery<T>(_settings.Query, _settings.FieldNameForDate, @from, to))
                .Size(_settings.MaxItems)
                .Sort(x => x.Ascending(_settings.FieldNameForDate))
                .Scroll(_settings.Scroll), cancellationToken);

            while (response.Documents.Any())
            {
                foreach (var document in response.Documents)
                {
                    yield return document;
                }

                response = await _elasticClient.ScrollAsync<T>(_settings.Scroll, response.ScrollId, ct: cancellationToken);
            }
        }

        private bool IsValidElasticSearchUrl()
        {
            try
            {
                var result = _httpClient
                    .GetAsync(_settings.Url, HttpCompletionOption.ResponseHeadersRead)
                    .GetAwaiter()
                    .GetResult();
                return result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to reach elastic search url {url}: {ex}", _settings.Url, ex);
                return false;
            }
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> BuildQuery<T>(string query, string dateField, DateTime @from, DateTime to) where T : class
        {
            return x => x.QueryString(y => y.Query(query)) && x.DateRange(z => z.Field(dateField).GreaterThan(@from).LessThan(to));
        }
    }
}