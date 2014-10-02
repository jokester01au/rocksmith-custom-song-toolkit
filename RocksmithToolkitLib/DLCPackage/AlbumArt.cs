using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using DevIL;

namespace RocksmithToolkitLib.DLCPackage
{
    public class AlbumArt
    {
        private Stream Stream;

        public AlbumArt(Stream stream)
        {
            this.Stream = stream;
        }

        public System.Drawing.Image Image { get 
        {
            ImageImporter imageImporter = new ImageImporter();
            ImageExporter imageExporter = new ImageExporter();
            using (var imageStream = new MemoryStream())
            {
                DevIL.Image image = imageImporter.LoadImageFromStream(Stream);
                imageExporter.SaveImageToStream(image, ImageType.Bmp, imageStream);
                imageStream.Position = 0L;
                return new Bitmap(imageStream);
            }
        }
            set
            {
                ImageImporter imageImporter = new ImageImporter();
                ImageExporter imageExporter = new ImageExporter();
                using (var imageStream = new MemoryStream())
                {
                    value.Save(imageStream, ImageFormat.Bmp);
                    imageStream.Position = 0L;
                    DevIL.Image image = imageImporter.LoadImageFromStream(ImageType.Bmp, imageStream);
                    this.Stream.Position = 0L;
                    imageExporter.SaveImageToStream(image, ImageType.Dds, this.Stream);
                }
            }
        }

    }
}
