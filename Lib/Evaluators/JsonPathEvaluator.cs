using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Lib.Evaluators
{
    internal static class JsonPathEvaluator
    {
        internal static bool IsJsonPathValid(string jsonPath, string json, out JToken[] tokens)
        {
            tokens = Array.Empty<JToken>();

            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                return false;
            }

            try
            {
                tokens = JObject.Parse(json)
                    .SelectTokens(jsonPath)
                    .ToArray();

                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }
    }
}