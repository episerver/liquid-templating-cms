using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace Optimizely.CMS.Labs.LiquidTemplating.Content
{
    [ContentType(DisplayName = "Liquid Template", GUID = "088d483d-aa5d-408f-8fa8-9d6cda9da03f", AvailableInEditMode = true)]
    [MediaDescriptor(ExtensionString = "liquid")]
    public class LiquidTemplateData : MediaData
    {
        [UIHint(UIHint.Textarea)] 
        public virtual string Template { get; set; }

        public string BinaryDataAsString
        {
            get
            {
                using (var s = BinaryData.OpenRead())
                {
                    StreamReader reader = new StreamReader(s);
                    return reader.ReadToEnd();
                }
            }
        }

        public byte[] TemplateAsBytes
        {
            get
            {
                if (string.IsNullOrEmpty(Template))
                    return null;

                return Encoding.UTF8.GetBytes(Template);
            }
        }

        /// <summary>
        /// A hidden property used through the Content Events system to ensure synchronisation between the blob and the template field
        /// </summary>
        [ScaffoldColumn(false)]
        public virtual Blob ComparisonBinaryData { get; set; }
    }
}
