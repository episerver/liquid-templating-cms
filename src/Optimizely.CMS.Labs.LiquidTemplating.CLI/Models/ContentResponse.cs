namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;
public class ContentLink
{
    public int id { get; set; }
    public int workId { get; set; }
    public string guidValue { get; set; }
    public object providerName { get; set; }
    public string url { get; set; }
    public object expanded { get; set; }
}

public class Language
{
    public string link { get; set; }
    public string displayName { get; set; }
    public string name { get; set; }
}

public class ContentResponse
{
    public ContentLink contentLink { get; set; }
    public string name { get; set; }
    public Language language { get; set; }
    public string template { get; set; }
    public List<string> contentType { get; set; }
}
