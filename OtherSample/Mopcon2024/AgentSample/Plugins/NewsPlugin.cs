using System.ComponentModel;
using System.Xml;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;

public class NewsPlugin
{

    [KernelFunction, Description("Get Today's top news")]
    public string GetTopsNews()
    {
        string url = "http://feeds.bbci.co.uk/news/world/rss.xml";

        try
        {
            var rssString = string.Empty;

            using (var client = new HttpClient())
            {
                rssString = client.GetStringAsync(url).GetAwaiter().GetResult();
            }

            List<NewsItem> newsItems = new List<NewsItem>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(rssString);

            XmlNodeList itemNodes = xmlDoc.SelectNodes("//item");

            if (itemNodes != null)
            {
                foreach (XmlNode item in itemNodes)
                {
                    string title = item.SelectSingleNode("title")?.InnerText ?? "No Title";
                    string description = item.SelectSingleNode("description")?.InnerText ?? "No Description";

                    newsItems.Add(new NewsItem
                    {
                        Title = title,
                        Description = description
                    });
                }
            }
            return JsonConvert.SerializeObject(newsItems, Newtonsoft.Json.Formatting.Indented);
        }
        catch (Exception ex)
        {
            return $"An error occurred: {ex.Message}";
        }
    }

    static async Task<string> DownloadRssFeedAsync(string url)
    {
        using (var client = new HttpClient())
        {
            return await client.GetStringAsync(url);
        }
    }

    static List<NewsItem> ParseRssFeed(string xmlContent)
    {
        List<NewsItem> newsItems = new List<NewsItem>();

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        XmlNodeList itemNodes = xmlDoc.SelectNodes("//item");

        if (itemNodes != null)
        {
            foreach (XmlNode item in itemNodes)
            {
                string title = item.SelectSingleNode("title")?.InnerText ?? "No Title";
                string description = item.SelectSingleNode("description")?.InnerText ?? "No Description";

                newsItems.Add(new NewsItem
                {
                    Title = title,
                    Description = description
                });
            }
        }

        return newsItems;
    }

}
public class NewsItem
{
    public string Title { get; set; }
    public string Description { get; set; }
}