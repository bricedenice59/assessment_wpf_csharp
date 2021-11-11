using DeveloperTest.ValueObjects;

namespace DeveloperTest.MessageBus
{
    public class LoadHtmlMessage
    {
        public string Html { get; set; }
        public LoadHtmlMessage(string html)
        {
            Html = html;
        }
    }
}
