﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Lib.Configuration;
using Microsoft.Extensions.Options;
using Nest;

namespace Lib.ElasticSearch
{
    public class ElasticProvider : IElasticProvider
    {
        private readonly ElasticClient _client;
        private readonly ElasticSettings _settings;

        public ElasticProvider(IOptions<Settings> options)
        {
            _settings = options.Value.ElasticSettings;
            if (!_settings.IsValid())
            {
                throw new ArgumentException($"Invalid elastic search settings '{_settings}'");
            }

            var connectionSettings = new ConnectionSettings(new Uri(_settings.Url));
            _client = new ElasticClient(connectionSettings);
        }

        public async IAsyncEnumerable<T> QueryAsync<T>([EnumeratorCancellation] CancellationToken cancellationToken = default) where T : class
        {
            var cultureInfo = new CultureInfo(_settings.CultureForDate);
            var from = DateTime.ParseExact(_settings.From, _settings.FormatForDate, cultureInfo);
            var to = DateTime.ParseExact(_settings.To, _settings.FormatForDate, cultureInfo);

            var response = await _client.SearchAsync<T>(s => s
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

                response = await _client.ScrollAsync<T>(_settings.Scroll, response.ScrollId, ct: cancellationToken);
            }
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> BuildQuery<T>(string query, string dateField, DateTime @from, DateTime to) where T : class
        {
            return x => x.QueryString(y => y.Query(query)) && x.DateRange(z => z.Field(dateField).GreaterThan(@from).LessThan(to));
        }
    }
}