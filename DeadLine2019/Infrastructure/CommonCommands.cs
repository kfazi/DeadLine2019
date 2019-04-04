namespace DeadLine2019.Infrastructure
{
    using System;
    using System.Text.RegularExpressions;

    public class CommonCommands
    {
        protected readonly NetworkClient NetworkClient;

        public CommonCommands(NetworkClient networkClient)
        {
            NetworkClient = networkClient;
        }

        public void Login(string host, int port, string userName, string password)
        {
            NetworkClient.Connect(host, port);

            var loginText = NetworkClient.ReadLine();
            if (loginText != "LOGIN")
            {
                throw new InvalidOperationException("FatalError - LOGIN token doesn't match");
            }

            NetworkClient.SendLine(userName);
            var passText = NetworkClient.ReadLine();
            if (passText != "PASS")
            {
                throw new InvalidOperationException("FatalError - PASS token doesn't match");
            }

            NetworkClient.SendLine(password);
            CheckResponse(NetworkClient.ReadLine());
        }

        public void Wait()
        {
            NetworkClient.SendLine("WAIT");
            CheckResponse(NetworkClient.ReadLine());

            NetworkClient.ReadLine();

            CheckResponse(NetworkClient.ReadLine());
        }

        protected TokenReader GetTokenReader()
        {
            return new TokenReader(NetworkClient);
        }

        private void CheckResponse(string line)
        {
            if (line == "OK")
            {
                return;
            }

            var regex = new Regex(@"FAILED\s+(\d+)\s+(.*)");
            var match = regex.Match(line);
            if (!match.Success)
            {
                throw new InvalidOperationException($@"Unknown response ""{line}""");
            }

            var code = int.Parse(match.Groups[1].Value);
            var message = match.Groups[2].Value;
            if (code != 6)
            {
                throw new FailedOperationException(code, message);
            }

            HandleForcedWait();
        }

        private void HandleForcedWait()
        {
            var waitingLine = NetworkClient.ReadLine();
            var regex = new Regex(@"WAITING\s+((\d*[.])?\d+)");
            var match = regex.Match(waitingLine);
            if (!match.Success)
            {
                throw new InvalidOperationException($@"Unknown response ""{waitingLine}""");
            }

            var waitTime = (int)(double.Parse(match.Groups[1].Value) * 1000);

            var line = NetworkClient.ReadLine();
            CheckResponse(line);

            throw new ForcedWaitingException(waitTime);
        }
    }
}