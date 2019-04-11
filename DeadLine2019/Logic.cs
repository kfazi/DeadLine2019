namespace DeadLine2019
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows.Media.Imaging;

    using DeadLine2019.Algorithms;
    using DeadLine2019.Infrastructure;

    public class Logic
    {
        protected readonly WriteableBitmap Bitmap;

        protected readonly IGraphProvider GraphProvider;

        protected readonly Log Logger;

        protected readonly Commands Commands;

        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly WriteableBitmap _groundSprite;

        private readonly WriteableBitmap _waterSprite;

        private readonly WriteableBitmap _treeSprite;

        public Logic(
            IGraphProvider graphProvider,
            IBitmapProvider bitmapProvider,
            Log logger,
            Commands commands,
            ConnectionData connectionData)
        {
            GraphProvider = graphProvider;
            Bitmap = bitmapProvider.Bitmap;
            Logger = logger;
            Commands = commands;

            bitmapProvider.HideBitmap();
            graphProvider.HideGraph();

            _groundSprite = BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Tiles\tile_05.png"), 16, 16);
            _waterSprite = BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Tiles\tile_19.png"), 16, 16);
            _treeSprite = BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Tiles\tile_183.png"), 16, 16);

            Commands.Login(connectionData.Host, connectionData.Port, connectionData.UserName, connectionData.Password);

            Logger.LogCommands = true;
        }

        public LogicState RunLogicStep(LogicState state)
        {
            Logger.Write($"Turn number {state.TurnNumber}");

            if (!state.IsInitialized)
            {
                var random = new Random(123);
                state.Map = new Map2D<int>(500, 500);
                for (var y = 0; y < state.Map.Height; y++)
                {
                    for (var x = 0; x < state.Map.Width; x++)
                    {
                        state.Map[x, y] = random.Next(1, 4);
                    }
                }

                state.IsInitialized = true;
            }

            state.TurnNumber++;

            Commands.Wait();

            return state;
        }

        public void Draw(LogicState state, DrawingWindowState drawingWindowState)
        {
            _stopwatch.Restart();

            using (Bitmap.GetBitmapContext())
            {
                Bitmap.Clear();

                var renderer = new MapRenderer(16, 16);

                renderer.Render(state.Map, Bitmap, ImageProvider, drawingWindowState);
            }

            var elapsed = _stopwatch.ElapsedMilliseconds;
            if (elapsed > 150)
            {
                Logger.Write($"Drawing takes {elapsed}ms! Consider disabling it!");
            }
        }

        private IEnumerable<WriteableBitmap> ImageProvider(int node, int x, int y)
        {
            switch (node)
            {
                case 1:
                    return new[] { _groundSprite };
                case 2:
                    return new[] { _groundSprite, _treeSprite };
                case 3:
                    return new[] { _waterSprite };
                default:
                    return new WriteableBitmap[] { };
            }
        }

        public void UpdateGraph(LogicState state)
        {
        }
    }
}
