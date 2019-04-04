namespace DeadLine2019.Infrastructure
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public static class BitmapExtensions
    {
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            using (var argbImage = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(argbImage))
                {
                    graphics.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                }

                var bitmapData = argbImage.LockBits(
                    new Rectangle(0, 0, argbImage.Width, argbImage.Height),
                    ImageLockMode.ReadOnly,
                    argbImage.PixelFormat);

                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width,
                    bitmapData.Height,
                    argbImage.HorizontalResolution,
                    argbImage.VerticalResolution,
                    ConvertPixelFormat(argbImage.PixelFormat),
                    null,
                    bitmapData.Scan0,
                    bitmapData.Stride * bitmapData.Height,
                    bitmapData.Stride);

                argbImage.UnlockBits(bitmapData);
                return bitmapSource;
            }
        }

        public static Bitmap Resize(this Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static void Blit(this WriteableBitmap dest, System.Windows.Point destPosition, WriteableBitmap source)
        {
            Blit(dest, new Rect(destPosition.X, destPosition.Y, source.PixelWidth, source.PixelHeight), source);
        }

        public static void Blit(this WriteableBitmap dest, int destPositionX, int destPositionY, WriteableBitmap source)
        {
            Blit(dest, new Rect(destPositionX, destPositionY, source.PixelWidth, source.PixelHeight), source);
        }

        public static void Blit(this WriteableBitmap dest, Rect destRect, WriteableBitmap source)
        {
            dest.Blit(destRect, source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));
        }

        public static void Blit(this WriteableBitmap dest, System.Windows.Point destPosition, WriteableBitmap source, int rotationDegrees)
        {
            Blit(dest, (int)destPosition.X, (int)destPosition.Y, source, rotationDegrees);
        }

        public static void Blit(this WriteableBitmap dest, int destPositionX, int destPositionY, WriteableBitmap source, int rotationDegrees)
        {
            var diagonalLength = Math.Sqrt(Math.Pow(source.PixelWidth, 2) + Math.Pow(source.PixelHeight, 2));

            var tempBitmap = BitmapFactory.New((int)diagonalLength, (int)diagonalLength);
            var transform = new RotateTransform(rotationDegrees, source.PixelWidth / 2.0, source.PixelHeight / 2.0);

            tempBitmap.BlitRender(source, true, 1.0f, transform);

            dest.Blit(new Rect(destPositionX, destPositionY, diagonalLength, diagonalLength), tempBitmap, new Rect(0, 0, diagonalLength, diagonalLength));
        }

        public static void Blit(this WriteableBitmap dest, Rect destRect, WriteableBitmap source, int rotationDegrees)
        {
            var rotationRadians = rotationDegrees * Math.PI / 180.0;
            var width = (int)(Math.Abs(destRect.Width * Math.Sin(rotationRadians)) + Math.Abs(destRect.Height * Math.Cos(rotationRadians)));
            var height = (int)(Math.Abs(destRect.Width * Math.Cos(rotationRadians)) + Math.Abs(destRect.Height * Math.Sin(rotationRadians)));

            var diagonalLength = Math.Sqrt(Math.Pow(source.PixelWidth, 2) + Math.Pow(source.PixelHeight, 2));

            var tempBitmap = BitmapFactory.New((int)diagonalLength, (int)diagonalLength);
            var transform = new RotateTransform(rotationDegrees, source.PixelWidth / 2.0, source.PixelHeight / 2.0);

            tempBitmap.BlitRender(source, true, 1.0f, transform);

            dest.Blit(new Rect(destRect.X, destRect.Y, width, height), tempBitmap, new Rect(0, 0, diagonalLength, diagonalLength));
        }

        public static void ReplaceColor(this WriteableBitmap bitmap, System.Windows.Media.Color keyColor, System.Windows.Media.Color replacementColor)
        {
            var argbKeyColor = (keyColor.A << 24) | (keyColor.R << 16) | (keyColor.G << 8) | keyColor.B;
            var argbReplacementColor = (replacementColor.A << 24) | (replacementColor.R << 16) | (replacementColor.G << 8) | replacementColor.B;
            var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

            bitmap.Lock();
            unsafe
            {
                var imgData = (byte*)bitmap.BackBuffer;

                var rowIndex = 0;
                for (var row = 0; row < bitmap.PixelHeight; row++)
                {
                    var index = rowIndex;
                    for (var col = 0; col < bitmap.PixelWidth; col++)
                    {
                        var pixelData = imgData + index;
                        var pixel = (int*)pixelData;

                        if (*pixel == argbKeyColor)
                        {
                            *pixel = argbReplacementColor;
                        }

                        index += bytesPerPixel;
                    }

                    rowIndex += bitmap.BackBufferStride;
                }
            }

            bitmap.Unlock();
        }

        public static void Colorize(this WriteableBitmap bitmap, System.Windows.Media.Color color)
        {
            var tempBitmap = BitmapFactory.New(bitmap.PixelWidth, bitmap.PixelHeight);
            tempBitmap.FillRectangle(0, 0, bitmap.PixelWidth, bitmap.PixelHeight, color);

            bitmap.Blit(new System.Windows.Point(0, 0), tempBitmap, new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), Colors.White, WriteableBitmapExtensions.BlendMode.Multiply);
        }

        public static WriteableBitmap GetRotated(this WriteableBitmap bitmap, int rotationDegrees)
        {
            var rotationRadians = rotationDegrees * Math.PI / 180.0;
            var width = (int)(Math.Abs(bitmap.PixelWidth * Math.Sin(rotationRadians)) + Math.Abs(bitmap.PixelHeight * Math.Cos(rotationRadians)));
            var height = (int)(Math.Abs(bitmap.PixelWidth * Math.Cos(rotationRadians)) + Math.Abs(bitmap.PixelHeight * Math.Sin(rotationRadians)));

            var rotatedBitmap = BitmapFactory.New(width, height);
            var transform = new RotateTransform(rotationDegrees, bitmap.PixelWidth / 2.0, bitmap.PixelHeight / 2.0);

            rotatedBitmap.BlitRender(bitmap, true, 1.0f, transform);

            return rotatedBitmap;
        }

        private static System.Windows.Media.PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat)
        {
            switch (sourceFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr32;
            }

            return new System.Windows.Media.PixelFormat();
        }
    }
}