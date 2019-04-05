namespace DeadLine2019.Infrastructure
{
    using System.Windows.Media.Imaging;

    public interface IBitmapProvider
    {
        WriteableBitmap Bitmap { get; set; }

        void ShowBitmap();

        void HideBitmap();
    }
}