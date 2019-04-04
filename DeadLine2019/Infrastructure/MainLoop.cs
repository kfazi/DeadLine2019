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

        private readonly AutoResetEvent _drawFinishedEvent;

        private readonly Stopwatch _iterationStopwatch;

        private readonly Logic _logic;

        private LogicState _logicState;

        private bool _finished;

        public MainLoop(Commands commands, Log log, DrawingWindowState drawingWindowState, IGraphProvider graphProvider, IBitmapProvider bitmapProvider, ConnectionData connectionData)
        {
            _commands = commands;
            _log = log;

            _drawFinishedEvent = new AutoResetEvent(true);
            _iterationStopwatch = new Stopwatch();

            _logicState = new LogicState();
            _logic = new Logic(graphProvider, bitmapProvider, _log, _commands, drawingWindowState, connectionData);
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
                _logic.Draw(_logicState);
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
                    _logic.Draw(_logicState);
                    _logic.UpdateGraph(_logicState);
                    _drawFinishedEvent.Set();
                });

                var elapsedTime = _iterationStopwatch.ElapsedMilliseconds;
                if (elapsedTime < 666)
                {
                    Thread.Sleep((int)(666 - elapsedTime));
                }
            }
        }
    }
}