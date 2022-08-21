using Microsoft.Extensions.FileProviders;
using Optimizely.CMS.Labs.LiquidTemplating.Content;
using System;
using System.IO;

namespace Optimizely.CMS.Labs.LiquidTemplating.FileProviders
{
    /// <summary>
    /// Implementaton of IFileInfo that retrieves file content from CMS Content data
    /// </summary>
    public class ContentRepositoryFileInfo : IFileInfo
    {
        private readonly LiquidTemplateData _content;
        public ContentRepositoryFileInfo(LiquidTemplateData content)
        {
            _content = content;
        }
        public bool Exists => _content != null;

        public bool IsDirectory => false;

        public DateTimeOffset LastModified => _content.Changed;

        public long Length => 0;

        public string Name => _content.Name;

        public string PhysicalPath => _content.BinaryDataContainer.ToString();

        public Stream CreateReadStream()
        {
            return new MemoryStream(_content.TemplateAsBytes);
        }
    }
}
