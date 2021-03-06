﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Utils
{
    public class ImageManipulation
    {
        public int Width { get { return img.Width; } }
        public int Height { get { return img.Height; } }
        Image img;

        private object imgLock = new object();

        public ImageManipulation(Stream input)
        {
            img = Image.FromStream(input);
        }

        public ImageManipulation(ImageManipulation im)
        {
            lock (im.imgLock)
            {
                img = (Image)im.img.Clone();
            }
        }

        public void Crop(int x1, int y1, int x2, int y2)
        {
            // GetComment height and width of current image
            int width = (x2 - x1);
            int height = (y2 - y1);

            // Create "blank" image for drawing new image
            Bitmap outImage = new Bitmap(width, height);
            Graphics outGraphics = Graphics.FromImage(outImage);

            outGraphics.CompositingQuality = CompositingQuality.HighQuality;
            outGraphics.SmoothingMode = SmoothingMode.HighQuality;
            outGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Fill "blank" with new sized image
            outGraphics.DrawImage(img, 0, 0, new Rectangle(x1, y1, width, height), GraphicsUnit.Pixel);

            img.Dispose();
            img = outImage;
        }

        public void Resize(int resizeMaxWidth, int resizeMaxHeight)
        {
            // Declare variable for the conversion
            float ratio;

            // GetComment height and width of current image
            int width = (int)img.Width;
            int height = (int)img.Height;

            // Ratio and conversion for new size
            if (width > resizeMaxWidth)
            {
                ratio = (float)width / (float)resizeMaxWidth;
                width = (int)(width / ratio);
                height = (int)(height / ratio);
            }

            // Ratio and conversion for new size
            if (height > resizeMaxHeight)
            {
                ratio = (float)height / (float)resizeMaxHeight;
                height = (int)(height / ratio);
                width = (int)(width / ratio);
            }

            // Create "blank" image for drawing new image
            Bitmap outImage = new Bitmap(width, height);
            Graphics outGraphics = Graphics.FromImage(outImage);

            outGraphics.CompositingQuality = CompositingQuality.HighQuality;
            outGraphics.SmoothingMode = SmoothingMode.HighQuality;
            outGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Fill "blank" with new sized image
            outGraphics.DrawImage(img, 0, 0, outImage.Width, outImage.Height);

            img.Dispose();
            img = outImage;
        }

        public void FitSize(int newWidth, int newHeight)
        {
            //http://www.dweebd.com/ruby/resizing-and-cropping-images-to-fixed-dimensions/
            // Create "blank" image for drawing new image
            Bitmap outImage = new Bitmap(newWidth, newHeight);
            Graphics outGraphics = Graphics.FromImage(outImage);
            outGraphics.CompositingQuality = CompositingQuality.HighQuality;
            outGraphics.SmoothingMode = SmoothingMode.HighQuality;
            outGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float fW = (float)img.Width;
            float fH = (float)img.Height;
            float fNewW = (float)newWidth;
            float fNewH = (float)newHeight;

            int resultantW, resultantH;

            if ((fW / fNewW) < (fH / fNewH))
            {
                resultantW = newWidth;                 //150
                resultantH = (int)(fH * (fNewW / fW)); //112
                // aplica scale para newWidth x ?
            }
            else
            {
                resultantW = (int)(fW * (fNewH / fH));
                resultantH = newHeight;
                //aplica scale para ? x newHeight
            }

            //crop central
            var posCentral = new Rectangle((newWidth - resultantW)/2, (newHeight - resultantH)/2, resultantW, resultantH);

            outGraphics.DrawImage(img, posCentral, new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);

            img.Dispose();
            img = outImage;
        }

        public Stream SaveToJpeg(long quality = 90L)
        {
            ImageCodecInfo iciJpeg = GetEncoderInfo("image/jpeg");

            Encoder encQuality = Encoder.Quality;
            EncoderParameters encParams = new EncoderParameters(1);
            EncoderParameter paramQuality = new EncoderParameter(encQuality, quality);
            encParams.Param[0] = paramQuality;

            Stream outputStream = new MemoryStream();
            // Save new image as jpg
            img.Save(outputStream, iciJpeg, encParams);

            outputStream.Seek(0, SeekOrigin.Begin);

            return outputStream;
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}