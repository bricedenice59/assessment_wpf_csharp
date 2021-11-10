namespace DeveloperTest.ValueObjects
{
    public class EmailObject
    {
        public object Uid { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Date { get; set; }

        public string Body { get; set; }
    }
}