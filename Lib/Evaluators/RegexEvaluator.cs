using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lib.Evaluators
{
    internal static class RegexEvaluator
    {
        private static readonly Lazy<Regex> Regex = BuildProjectionRegex();

        internal static bool IsFieldsProjection(string expression, out string[] fields)
        {
            fields = Array.Empty<string>();

            if (!Regex.Value.IsMatch(expression)) return false;

            fields = System.Text.RegularExpressions.Regex
                .Matches(expression, @"'\w+'")
                .Select(m => m.ToString())
                .ToArray();

            return true;
        }

        private static Lazy<Regex> BuildProjectionRegex()
        {
            return new Lazy<Regex>(() => new Regex(@"\['\w+'(,\s*'\w+')*\]", RegexOptions.Compiled));
        }
    }
}