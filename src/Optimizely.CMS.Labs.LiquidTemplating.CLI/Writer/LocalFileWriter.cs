using Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;
using System.Text;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Writer
{
    public class LocalFileWriter : ILiquidTemplateWriter
    {
        private readonly string _path;

        public LocalFileWriter(string presentWorkingDirectory)
        {
            _path = presentWorkingDirectory;
        }

        public void Create(IEnumerable<ILiquidTemplateData> items)
        {
            foreach (ILiquidTemplateData item in items)
            {
                Create(item);
            }
        }

        public void Create(ILiquidTemplateData item)
        {
            var keyAsLocalPath = item.Key.Replace('|', '\\');
            var itemToCreate = Path.Combine(_path, keyAsLocalPath);

            if (item.LiquidContentType == LiquidContentItem.Folder)
            {
                if (!Directory.Exists(itemToCreate))
                {
                    Directory.CreateDirectory(itemToCreate);
                }
            }
            else
            {
                if (!File.Exists(itemToCreate))
                {
                    using (FileStream fs = File.Create(itemToCreate))
                    {
                        var data = string.IsNullOrEmpty(item.Template) ? string.Empty : item.Template;
                        Byte[] template = new UTF8Encoding(true).GetBytes(item.Template);
                        fs.Write(template);
                    }
                }
            }
        }

        public void Update(IEnumerable<ILiquidTemplateData> items)
        {
            foreach (ILiquidTemplateData item in items)
            {
                Update(item);
            }
        }

        public void Update(ILiquidTemplateData item)
        {
            var keyAsLocalPath = item.Key.Replace('|', '\\');
            var itemToUpdate = Path.Combine(_path, keyAsLocalPath);

            File.WriteAllText(itemToUpdate, item.Template);
        }


        public void Delete(IEnumerable<ILiquidTemplateData> items)
        {
            foreach (ILiquidTemplateData item in items)
            {
                Delete(item);
            }
        }

        public void Delete(ILiquidTemplateData item)
        {
            var keyAsLocalPath = item.Key.Replace('|', '\\');
            var itemToDelete = Path.Combine(_path, keyAsLocalPath);

            if (item.LiquidContentType == LiquidContentItem.Folder)
            {
                if (Directory.Exists(itemToDelete))
                {
                    Directory.Delete(itemToDelete);
                }
            }
            else
            {
                if (File.Exists(itemToDelete))
                {
                    File.Delete(itemToDelete);
                }
            }
        }
    }
}
