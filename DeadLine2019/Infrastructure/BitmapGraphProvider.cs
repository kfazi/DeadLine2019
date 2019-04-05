namespace DeadLine2019.Infrastructure
{
    using System.Windows.Media.Imaging;

    using Caliburn.Micro;

    using Microsoft.Msagl.Drawing;
    using Microsoft.Msagl.WpfGraphControl;

    public class BitmapGraphProvider : PropertyChangedBase, IBitmapProvider, IGraphProvider
    {
        private WriteableBitmap _bitmap = BitmapFactory.New(512, 512);

        public WriteableBitmap Bitmap
        {
            get => _bitmap;
            set
            {
                _bitmap = value;
                NotifyOfPropertyChange(nameof(Bitmap));
            }
        }

        public Graph Graph
        {
            get => GraphViewer.Graph;
            set => GraphViewer.Graph = value;
        }

        public bool IsBitmapVisible { get; private set; }

        public bool IsGraphVisible { get; private set; }

        public GraphViewer GraphViewer { get; set; }

        public void ShowBitmap()
        {
            IsBitmapVisible = true;
            NotifyOfPropertyChange(nameof(IsBitmapVisible));
        }

        public void HideBitmap()
        {
            IsBitmapVisible = false;
            NotifyOfPropertyChange(nameof(IsBitmapVisible));
        }

        public void ClearGraph()
        {
            GraphViewer.Graph = new Graph();
        }

        public void UpdateLayout()
        {
            GraphViewer.ProcessGraph();
        }

        public void ShowGraph()
        {
            IsGraphVisible = true;
            NotifyOfPropertyChange(nameof(IsGraphVisible));
        }

        public void HideGraph()
        {
            IsGraphVisible = false;
            NotifyOfPropertyChange(nameof(IsGraphVisible));
        }
    }
}