using System;
using System.Collections.Generic;
using Lib.Configuration;
using Lib.Evaluators;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Lib.Serializers
{
    public class CustomJsonSerializer : ICustomJsonSerializer
    {
        private readonly ElasticSettings _settings;

        public CustomJsonSerializer(IOptions<Settings> options)
        {
            _settings = options.Value.ElasticSettings;
        }

        public string Serialize<T>(T obj)
        {
            var jsonPath = _settings?.JsonPath;
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            
            if (!JsonPathEvaluator.IsJsonPathValid(jsonPath, json, out var tokens))
            {
                return json;
            }

            if (!RegexEvaluator.IsFieldsProjection(jsonPath, out var fields))
            {
                return JsonConvert.SerializeObject(tokens, Formatting.Indented);
            }

            var subObj = new Dictionary<string, object>();
            var length = Math.Min(tokens.Length, fields.Length);

            for (var index = 0; index < length; index++)
            {
                subObj.TryAdd(fields[index], tokens[index]);
            }

            return JsonConvert.SerializeObject(subObj, Formatting.Indented);
        }
    }
}
