namespace DeadLine2019
{
    using System.Windows;
    using System.Windows.Media.Imaging;

    using DeadLine2019.Infrastructure;

    using Color = System.Windows.Media.Color;
    using Point = System.Drawing.Point;

    public class Logic
    {
        protected readonly WriteableBitmap Bitmap;

        protected readonly IGraphProvider GraphProvider;

        protected readonly Log Logger;

        protected readonly Commands Commands;

        protected readonly DrawingWindowState DrawingWindowState;

        private readonly WriteableBitmap _groundSprite;

        private readonly WriteableBitmap _waterSprite;

        private readonly WriteableBitmap _treeSprite;

        private readonly WriteableBitmap[] _playerSprites;

        public Logic(IGraphProvider graphProvider, IBitmapProvider bitmapProvider, Log logger, Commands commands, DrawingWindowState drawingWindowState, ConnectionData connectionData)
        {
            GraphProvider = graphProvider;
            Bitmap = bitmapProvider.Bitmap;
            Logger = logger;
            Commands = commands;
            DrawingWindowState = drawingWindowState;

            _groundSprite = BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Tiles\tile_05.png"), 16, 16);
            _waterSprite = BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Tiles\tile_19.png"), 16, 16);
            _treeSprite = BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Tiles\tile_183.png"), 16, 16);
            _playerSprites = new[]
            {
                BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Hitman 1\hitman1_silencer.png"), 16, 16),
                BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Hitman 1\hitman1_silencer.png"), 16, 16),
                BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Hitman 1\hitman1_silencer.png"), 16, 16),
                BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Hitman 1\hitman1_silencer.png"), 16, 16),
                BitmapLoader.Load(SolutionDirectory.Get(@"Images\Shooter\Hitman 1\hitman1_silencer.png"), 16, 16),
            };

            _playerSprites[0].Colorize(Color.FromRgb(255, 0, 0));
            _playerSprites[1].Colorize(Color.FromRgb(0, 255, 0));
            _playerSprites[2].Colorize(Color.FromRgb(0, 0, 255));
            _playerSprites[3].Colorize(Color.FromRgb(255, 255, 0));

            Commands.Login(connectionData.Host, connectionData.Port, connectionData.UserName, connectionData.Password);

            Logger.LogCommands = true;
        }

        public LogicState RunLogicStep(LogicState state)
        {
            Logger.Write($"Turn number {state.TurnNumber}");

            state.TurnNumber++;

            Commands.Wait();

            return state;
        }

        public void Draw(LogicState state)
        {
            Bitmap.Clear();

            Bitmap.DrawEllipse(15, 15, 40, 40, Color.FromRgb(200, 30, 100));

            var map = ".~........"
                      + ".~.....*.."
                      + ".~..*....."
                      + ".~~......."
                      + ".~~......."
                      + "......**.."
                      + ".........."
                      + ".....~...."
                      + "*........."
                      + "........~.";

            var players = new[]
            {
                new Point(3, 2),
                new Point(9, 8),
                new Point(0, 6)
            };

            for (var y = 0; y < 10; y++)
            {
                for (var x = 0; x < 10; x++)
                {
                    var tile = map[y * 10 + x];
                    switch (tile)
                    {
                        case '.':
                            Bitmap.Blit(new Rect(x * 16, y * 16, 16, 16), _groundSprite);
                            break;
                        case '~':
                            Bitmap.Blit(new Rect(x * 16, y * 16, 16, 16), _waterSprite);
                            break;
                        case '*':
                            Bitmap.Blit(new Rect(x * 16, y * 16, 16, 16), _groundSprite);
                            Bitmap.Blit(new Rect(x * 16, y * 16, 16, 16), _treeSprite);
                            break;
                    }
                }
            }

            for (var index = 0; index < players.Length; index++)
            {
                var player = players[index];
                Bitmap.Blit(new Rect(player.X * 16, player.Y * 16, 16, 16), _playerSprites[index], 90);
            }
        }

        public void UpdateGraph(LogicState state)
        {
            if (state.TurnNumber % 10 == 0)
            {
                GraphProvider.ClearGraph();
            }

            var graph = GraphProvider.Graph;

            graph.AddEdge("0", $"{state.TurnNumber}");

            GraphProvider.UpdateLayout();
        }
    }
}
