namespace Core
{
    public class ElasticSettings
    {
        public string Url { get; set; }
        public string Index { get; set; }
        public int MaxItems { get; set; }
        public string Query { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public int Scroll { get; set; }

        public ElasticSettings()
        {
            Query = "*";
            Scroll = 30;
            MaxItems = 10_000;
        }
    }
}