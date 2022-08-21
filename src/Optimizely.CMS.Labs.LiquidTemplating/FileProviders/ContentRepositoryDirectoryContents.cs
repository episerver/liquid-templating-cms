using EPiServer.Core;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Optimizely.CMS.Labs.LiquidTemplating.FileProviders
{
    public class ContentRepositoryDirectoryContents : IDirectoryContents
    {
        private readonly ContentFolder _content;
        public ContentRepositoryDirectoryContents(ContentFolder content)
        {
            _content = content;
        }

        public bool Exists => _content != null;

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
