namespace DeadLine2019.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    using Caliburn.Micro;

    using Microsoft.Msagl.Drawing;

    public class MainViewModel : Screen
    {
        private readonly List<string> _history = new List<string> { string.Empty };

        private readonly DrawingWindowState _drawingWindowState;

        private readonly MainLoop _mainLoop;

        private readonly BitmapGraphProvider _bitmapGraphProvider;

        private bool _processingCommand;

        private int _historyIndex;

        private Point? _draggingStart;

        public MainViewModel(MainLoop mainLoop, BitmapGraphProvider bitmapGraphProvider, Log log, DrawingWindowState drawingWindowState, ConnectionData connectionData)
        {
            _mainLoop = mainLoop;
            _bitmapGraphProvider = bitmapGraphProvider;
            _drawingWindowState = drawingWindowState;
            Log = log;
            Log.PropertyChanged += OnTextChanged;
            _bitmapGraphProvider.PropertyChanged += OnBitmapGraphChanged;
            DisplayName = $"{connectionData.Host}:{connectionData.Port}";
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _bitmapGraphProvider.GraphViewer = ((MainView)GetView()).GraphViewer;
        }

        public Log Log { get; }

        public WriteableBitmap Bitmap => _bitmapGraphProvider.Bitmap;

        public Graph Graph => _bitmapGraphProvider.Graph;

        public bool IsBitmapVisible => _bitmapGraphProvider.IsBitmapVisible;

        public bool IsGraphVisible => _bitmapGraphProvider.IsGraphVisible;

        public async Task MouseMoveCommandAsync(ActionExecutionContext context)
        {
            if (!(context.EventArgs is MouseEventArgs mouseEventArgs))
            {
                return;
            }

            if (_draggingStart == null)
            {
                return;
            }

            var currentPosition = mouseEventArgs.GetPosition(context.Source);
            var distance = currentPosition - _draggingStart;
            _draggingStart = currentPosition;

            _drawingWindowState.X += (int)distance.Value.X;
            _drawingWindowState.Y += (int)distance.Value.Y;

            _mainLoop.Draw();
        }

        public async Task MouseDownCommandAsync(ActionExecutionContext context)
        {
            if (!(context.EventArgs is MouseEventArgs mouseEventArgs))
            {
                return;
            }

            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                _draggingStart = mouseEventArgs.GetPosition(context.Source);
            }
        }

        public async Task MouseUpCommandAsync(ActionExecutionContext context)
        {
            if (!(context.EventArgs is MouseEventArgs mouseEventArgs))
            {
                return;
            }

            if (mouseEventArgs.LeftButton == MouseButtonState.Released)
            {
                _draggingStart = null;
            }
        }

        public async Task MouseWheelCommandAsync(ActionExecutionContext context)
        {
            if (!(context.EventArgs is MouseWheelEventArgs mouseWheelEventArgs))
            {
                return;
            }

            _drawingWindowState.Z += mouseWheelEventArgs.Delta;
            _mainLoop.Draw();
        }

        public async Task CommandInputConfirmed(ActionExecutionContext context)
        {
            if (!(context.EventArgs is KeyEventArgs keyEventArgs) || !(context.Source is TextBox inputTextBox))
            {
                return;
            }

            _history[_historyIndex] = inputTextBox.Text;
            switch (keyEventArgs.Key)
            {
                case Key.Down:
                {
                    _historyIndex++;
                    if (_historyIndex >= _history.Count)
                    {
                        _historyIndex = _history.Count - 1;
                    }

                    var index = inputTextBox.CaretIndex;
                    inputTextBox.Text = _history[_historyIndex];
                    inputTextBox.CaretIndex = index;
                    break;
                }
                case Key.Up:
                {
                    _historyIndex--;
                    if (_historyIndex < 0)
                    {
                        _historyIndex = 0;
                    }

                    var index = inputTextBox.CaretIndex;
                    inputTextBox.Text = _history[_historyIndex];
                    inputTextBox.CaretIndex = index;
                    break;
                }
                case Key.Enter when !_processingCommand:
                {
                    if (string.IsNullOrWhiteSpace(inputTextBox.Text))
                    {
                        break;
                    }

                    _processingCommand = true;

                    if (_history.Count > 1 && _history[_history.Count - 1] == _history[_history.Count - 2])
                    {
                        _history[_history.Count - 1] = string.Empty;
                    }
                    else if (_history.Last() != inputTextBox.Text)
                    {
                        _history.Add(inputTextBox.Text);
                    }

                    if (_history.Last() != string.Empty)
                    {
                        _history.Add(string.Empty);
                    }

                    _historyIndex = _history.Count - 1;

                    var command = inputTextBox.Text;
                    inputTextBox.Clear();
                    await Task.Run(() => _mainLoop.ProcessCommand(command));

                    _processingCommand = false;
                    break;
                }
            }
        }

        private void OnTextChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName != "Text")
            {
                return;
            }

            var view = (FrameworkElement)GetView();

            var logTextBox = (TextBox)view?.FindName("LogTextBox");
            if (logTextBox == null)
            {
                return;
            }

            var scrollToEnd = logTextBox.CaretIndex == logTextBox.Text.Length;
            if (scrollToEnd)
            {
                logTextBox.ScrollToEnd();
            }
        }

        private void OnBitmapGraphChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(e.PropertyName);
        }
    }
}