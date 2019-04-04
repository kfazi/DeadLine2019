namespace DeadLine2019.Infrastructure
{
    using System.Drawing;
    using System.Windows.Media.Imaging;

    public static class BitmapLoader
    {
        public static WriteableBitmap Load(string fileName)
        {
            using (var bitmap = new Bitmap(fileName))
            {
                var bitmapSource = bitmap.ToBitmapSource();
                return new WriteableBitmap(bitmapSource);
            }
        }

        public static WriteableBitmap Load(string fileName, int width, int height)
        {
            using (var bitmap = new Bitmap(fileName))
            using (var resizedBitmap = bitmap.Resize(width, height))
            { 
                var bitmapSource = resizedBitmap.ToBitmapSource();
                return new WriteableBitmap(bitmapSource);
            }
        }
    }
}