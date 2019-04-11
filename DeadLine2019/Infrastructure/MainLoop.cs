namespace DeadLine2019.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;

    using Caliburn.Micro;

    public class MainLoop
    {
        private readonly Commands _commands;

        private readonly Log _log;

        private readonly DrawingWindowState _drawingWindowState;

        private readonly BitmapGraphProvider _bitmapGraphProvider;

        private readonly AutoResetEvent _drawFinishedEvent;

        private readonly Stopwatch _iterationStopwatch;

        private readonly Logic _logic;

        private LogicState _logicState;

        private bool _finished;

        public MainLoop(Commands commands, Log log, DrawingWindowState drawingWindowState, BitmapGraphProvider bitmapGraphProvider, ConnectionData connectionData)
        {
            _commands = commands;
            _log = log;
            _drawingWindowState = drawingWindowState;
            _bitmapGraphProvider = bitmapGraphProvider;

            _drawFinishedEvent = new AutoResetEvent(true);
            _iterationStopwatch = new Stopwatch();

            _logicState = new LogicState();
            _logic = new Logic(bitmapGraphProvider, bitmapGraphProvider, _log, _commands, connectionData);
        }

        public void ProcessCommand(string command)
        {
            try
            {
                var commandArgs = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var methodInfo = typeof(Commands).GetMethod(commandArgs[0]);
                if (methodInfo == null)
                {
                    _log.Write("Method not found");
                    return;
                }

                var parameters = methodInfo.GetParameters();
                var realParameters = new List<object>();
                for (var i = 1; i < commandArgs.Length; i++)
                {
                    var parameter = parameters[i - 1];
                    var type = parameter.ParameterType;
                    realParameters.Add(Convert.ChangeType(commandArgs[i], type));
                }

                methodInfo.Invoke(_commands, realParameters.ToArray());
            }
            catch (TargetInvocationException exception)
            {
                _log.Write(exception.InnerException == null ? exception.Message : exception.InnerException.Message);
            }
            catch (Exception exception)
            {
                _log.Write(exception.Message);
            }
        }

        public void Finish()
        {
            _finished = true;
        }

        public void Draw()
        {
            Execute.OnUIThread(() =>
            {
                _logic.Draw(_logicState, _drawingWindowState);
                _drawFinishedEvent.Set();
            });
        }

        public void Run()
        {
            while (!_finished)
            {
                _iterationStopwatch.Restart();

                _drawFinishedEvent.WaitOne();

                _logicState = _logic.RunLogicStep(_logicState);

                Execute.OnUIThread(() =>
                {
                    if (_bitmapGraphProvider.IsBitmapVisible)
                    {
                        _logic.Draw(_logicState, _drawingWindowState);
                    }

                    if (_bitmapGraphProvider.IsGraphVisible)
                    {
                        _logic.UpdateGraph(_logicState);
                    }

                    _drawFinishedEvent.Set();
                });

                var elapsedTime = _iterationStopwatch.ElapsedMilliseconds;
                if (elapsedTime < 100)
                {
                    Thread.Sleep((int)(100 - elapsedTime));
                }
            }
        }
    }
}