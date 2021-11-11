using System.Web;

namespace DeveloperTest.Utils
{
    public class HtmlUtils
    {
        /// <summary>
        /// https://stackoverflow.com/questions/3991840/simple-text-to-html-conversion/16722351
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetHtmlFromText(string text)
        {
            text = HttpUtility.HtmlEncode(text);
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\n", "\r");
            text = text.Replace("\r", "<br>\r\n");
            text = text.Replace("  ", " &nbsp;");
            return text;
        }
    }
}