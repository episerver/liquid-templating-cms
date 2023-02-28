using System.Diagnostics;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;

public class LocalLiquidTemplateData : ILiquidTemplateData
{
    public string? Path { get; set; }
    public string? Name { get; set; }
    public string? Template { get; set; }
    public string? ContentId { get; set; }
    public LiquidContentItem LiquidContentType { get; set; }

    public string Key
    {
        get
        {
            if (string.IsNullOrEmpty(Path))
                return string.Empty;

            var liquidkey = "\\views";
            var indexOfRoot = Path.ToLower().IndexOf(liquidkey);

            return Path.Substring(indexOfRoot + 1).Replace("\\", "|");
        }
    }
}
