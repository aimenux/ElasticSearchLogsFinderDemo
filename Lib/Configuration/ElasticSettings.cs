using System;
using System.Text;

namespace Lib.Configuration
{
    public class ElasticSettings
    {
        public string Url { get; set; }
        public string Index { get; set; }
        public int MaxItems { get; set; }
        public string Query { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Scroll { get; set; }
        public string JsonPath { get; set; }
        public string FormatForDate { get; set; }
        public string CultureForDate { get; set; }
        public string FieldNameForDate { get; set; }

        public ElasticSettings()
        {
            Query = "*";
            Scroll = "1m";
            MaxItems = 10000;
            CultureForDate = "fr-FR";
            FieldNameForDate = "EventDate";
            FormatForDate = "yyyy-MM-dd HH:mm:ss";
            From = DateTime.Now.AddHours(-1).ToString(FormatForDate);
            To = DateTime.Now.AddHours(1).ToString(FormatForDate);
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Url)
                   && !string.IsNullOrWhiteSpace(Index)
                   && !string.IsNullOrWhiteSpace(Query)
                   && !string.IsNullOrWhiteSpace(From)
                   && !string.IsNullOrWhiteSpace(To)
                   && !string.IsNullOrWhiteSpace(Scroll)
                   && !string.IsNullOrWhiteSpace(FieldNameForDate);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"{nameof(Index)}={Index} {nameof(Query)}={Query} {nameof(MaxItems)}={MaxItems} ");
            builder.Append($"{nameof(FieldNameForDate)}={FieldNameForDate} {nameof(JsonPath)}={JsonPath} ");
            builder.Append($"{nameof(From)}={From} {nameof(To)}={To} {nameof(Scroll)}={Scroll}");
            return builder.ToString();
        }
    }
}