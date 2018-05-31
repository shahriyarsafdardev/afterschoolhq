using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;

namespace AfterSchool.Models
{
    public class RebornImage
    {
        public string FileLocation { get; set; }

        public RebornImage()
        {

        }

        public static SystemMessages CreateThumbnailMaxDimension(int maxDim, string fileLocation, string newLocation, System.Drawing.Imaging.ImageFormat imgFormat)
        {
            Image i = Image.FromFile(fileLocation);
            int newWidth = 0;
            int newHeight = 0;

            if (i.Width < i.Height)
            {
                newHeight = maxDim;
                newWidth = (i.Width * newHeight) / i.Height;
            }
            else if (i.Height < i.Width)
            {
                newWidth = maxDim;
                newHeight = (i.Height * newWidth) / i.Width;
            }
            else
            {
                newWidth = maxDim;
                newHeight = maxDim;
            }

            if (i.Width < newWidth && i.Height < newHeight)
            {
                SystemMessages msgs = new SystemMessages();
                try
                {
                    System.IO.File.Copy(fileLocation, newLocation);
                }
                catch (Exception ex)
                {
                    msgs.Add(ex.Message, MessageType.Error);
                }
                return msgs;
            }
            else
            {
                return CreateThumbnail(i, newWidth, newHeight, newLocation, imgFormat);
            }
        }
        public static SystemMessages CreateThumbnailMinDimension(int minDim, string fileLocation, string newLocation, System.Drawing.Imaging.ImageFormat imgFormat)
        {
            Image i = Image.FromFile(fileLocation);
            int newWidth = 0;
            int newHeight = 0;

            if (i.Width > i.Height)
            {
                newHeight = minDim;
                newWidth = (i.Width * newHeight) / i.Height;
            }
            else if (i.Height > i.Width)
            {
                newWidth = minDim;
                newHeight = (i.Height * newWidth) / i.Width;
            }
            else
            {
                newWidth = minDim;
                newHeight = minDim;
            }

            if (i.Width < newWidth || i.Height < newHeight)
            {
                SystemMessages msgs = new SystemMessages();
                try
                {
                    System.IO.File.Copy(fileLocation, newLocation);
                }
                catch (Exception ex)
                {
                    msgs.Add(ex.Message, MessageType.Error);
                }
                return msgs;
            }
            else
            {
                return CreateThumbnail(i, newWidth, newHeight, newLocation, imgFormat);
            }
        }

        public static SystemMessages CreateThumbnail(int newWidth, string fileLocation, string newLocation, System.Drawing.Imaging.ImageFormat imgFormat)
        {
            Image i = Image.FromFile(fileLocation);
            int newHeight = (i.Height * newWidth) / i.Width;

            if (i.Width < newWidth || i.Height < newHeight)
            {
                SystemMessages msgs = new SystemMessages();
                try
                {
                    System.IO.File.Copy(fileLocation, newLocation);
                }
                catch (Exception ex)
                {
                    msgs.Add(ex.Message, MessageType.Error);
                }
                return msgs;
            }
            else
            {
                return CreateThumbnail(i, newWidth, newHeight, newLocation, imgFormat);
            }

        }
        public static SystemMessages CreateThumbnail(Image image, int newWidth, int newHeight, string fileLocation, string newLocation, System.Drawing.Imaging.ImageFormat imgFormat)
        {
            Image i = Image.FromFile(fileLocation);
            if (i.Width < newWidth || i.Height < newHeight)
            {
                SystemMessages msgs = new SystemMessages();
                try
                {
                    System.IO.File.Copy(fileLocation, newLocation);
                }
                catch (Exception ex)
                {
                    msgs.Add(ex.Message, MessageType.Error);
                }
                return msgs;
            }
            else
            {
                return CreateThumbnail(i, newWidth, newHeight, newLocation, imgFormat);
            }
        }
        public static SystemMessages CreateThumbnail(Image image, int newWidth, int newHeight, string newLocation, System.Drawing.Imaging.ImageFormat imgFormat)
        {
            SystemMessages msgs = new SystemMessages();
            try
            {
                Bitmap thumbnailBitmap = new Bitmap(newWidth, newHeight);

                var thumbnailGraph = Graphics.FromImage(thumbnailBitmap);
                thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbnailGraph.DrawImage(image, imageRectangle);

                thumbnailBitmap.Save(newLocation, imgFormat);

                thumbnailGraph.Dispose();
                thumbnailBitmap.Dispose();
                image.Dispose();
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error);
            }
            return msgs;
        }

        public static SystemMessages CreateThumbnail(int newWidth, int newHeight, Stream fromStream, Stream toStream, System.Drawing.Imaging.ImageFormat imgFormat)
        {
            SystemMessages msgs = new SystemMessages();

            try
            {
                Image image = Image.FromStream(fromStream);
                Bitmap thumbnailBitmap = new Bitmap(newWidth, newHeight);

                var thumbnailGraph = Graphics.FromImage(thumbnailBitmap);
                thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbnailGraph.DrawImage(image, imageRectangle);

                thumbnailBitmap.Save(toStream, imgFormat);

                thumbnailGraph.Dispose();
                thumbnailBitmap.Dispose();
                image.Dispose();
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error);
            }
            return msgs;
        }
        public static SystemMessages CreateThumbnail(double scaleFactor, Stream fromStream, Stream toStream, System.Drawing.Imaging.ImageFormat imgFormat)
        {
            SystemMessages msgs = new SystemMessages();
            Image image = Image.FromStream(fromStream);
            int newWidth = (int)(image.Width * scaleFactor);
            int newHeight = (int)(image.Height * scaleFactor);

            return CreateThumbnail(newWidth, newHeight, fromStream, toStream, imgFormat);
        }
    }
}