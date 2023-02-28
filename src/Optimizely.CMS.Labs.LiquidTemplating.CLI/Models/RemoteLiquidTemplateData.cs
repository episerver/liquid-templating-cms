namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;

public class RemoteLiquidTemplateData : ILiquidTemplateData
{
    public RemoteLiquidTemplateData(ContentResponse content)
    {
        Name = content.name;
        ContentId = content.contentLink.id.ToString();
        LiquidContentType = content.contentType.Contains("LiquidTemplateData") ? LiquidContentItem.LiquidTemplateData : LiquidContentItem.Folder;
        Url = content.contentLink.url;
    }

    public string? Template { get; set; }
    public string? Name { get; set; }
    public string? Hierarchy { get; set; }
    public string? Url { get; set; }
    public string? ContentId { get; set; }
    public LiquidContentItem LiquidContentType { get; set; }

    public string Key
    {
        get
        {
            if (string.IsNullOrEmpty(Hierarchy))
                return string.Empty;

            var liquidkey = "|views";
            var indexOfRoot = Hierarchy.ToLower().IndexOf(liquidkey);

            return Hierarchy.Substring(indexOfRoot + 1).TrimEnd('|');

        }
    }
}
