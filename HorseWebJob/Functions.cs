using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace HorseWebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("imagesqueue")] string message, 
            [Blob("dotnet3/{queueTrigger}.jpg", FileAccess.Read)] Stream input,
            [Blob("dotnet3/{queueTrigger}-thumbnail.jpg")] CloudBlockBlob outputBlob,
            TextWriter log)
        {
            try
            {
                using (Stream output = outputBlob.OpenWrite())
                {
                    if (message.StartsWith("horse"))
                    {
                        ConvertImageToThumbnailJPG(input, output, 80);
                    }
                    else if (message.StartsWith("news"))
                    {
                        ConvertImageToThumbnailJPG(input, output, 800);
                    }
                    else
                    {
                        ConvertImageToThumbnailJPG(input, output, 800);
                    }
                    
                    outputBlob.Properties.ContentType = "image/jpeg";
                }
            }
            catch (Exception ex)
            {
                log.WriteLine(ex.Message);
            }
        }

        public static void ConvertImageToThumbnailJPG(Stream input, Stream output, int thumbnailsize)
        {
            int width;
            int height;
            var originalImage = new Bitmap(input);

            if (originalImage.Width > originalImage.Height)
            {
                width = thumbnailsize;
                height = thumbnailsize * originalImage.Height / originalImage.Width;
            }
            else
            {
                height = thumbnailsize;
                width = thumbnailsize * originalImage.Width / originalImage.Height;
            }

            Bitmap thumbnailImage = null;
            try
            {
                thumbnailImage = new Bitmap(width, height);

                using (Graphics graphics = Graphics.FromImage(thumbnailImage))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(originalImage, 0, 0, width, height);
                }

                thumbnailImage.Save(output, ImageFormat.Jpeg);
            }
            finally
            {
                if (thumbnailImage != null)
                {
                    thumbnailImage.Dispose();
                }
            }
        }
    }
}
