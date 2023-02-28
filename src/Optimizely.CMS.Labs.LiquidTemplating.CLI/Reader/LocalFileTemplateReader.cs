using Microsoft.Extensions.FileProviders;
using Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Reader;

internal class LocalFileTemplateReader : ILiquidTemplateReader
{
    private readonly string _path;

    public LocalFileTemplateReader(string path)
    {
        Directory.CreateDirectory(path);
        _path = path;
    }

    public ILiquidTemplateData? Get(string path)
    {
        var filePath = Path.Combine(_path, path);

        if (path.Contains('.'))
        {
            if (File.Exists(filePath))
            {
                var data = new LocalLiquidTemplateData();
                data.Path = filePath;
                data.Name = filePath.Substring(filePath.LastIndexOf('\\') + 1);
                data.ContentId = "0";
                data.LiquidContentType = LiquidContentItem.LiquidTemplateData;
                data.Template = File.ReadAllText(filePath);

                return data;
            } 
        }
        else
        {
            if (Directory.Exists(filePath))
            {
                var data = new LocalLiquidTemplateData();
                data.Path = filePath;
                data.Name = filePath.Substring(filePath.LastIndexOf('\\') + 1);
                data.ContentId = "0";
                data.LiquidContentType = LiquidContentItem.Folder;
        
                return data;
            }

        }

        return null;
    }

    
    public List<ILiquidTemplateData> GetAll()
    {
        //check folder exists
        var liquidFiles = Directory.GetFileSystemEntries(_path, "*.liquid", SearchOption.AllDirectories);
        var directories = Directory.EnumerateDirectories(_path, "*", SearchOption.AllDirectories);

        var results = new List<ILiquidTemplateData>();

        results.AddRange(liquidFiles.Select(s => Get(s)));
        results.AddRange(directories.Select(s => new LocalLiquidTemplateData { Path = s, LiquidContentType = LiquidContentItem.Folder, Name = s.Substring(s.LastIndexOf('\\') + 1) }));

        return results;
    }
}