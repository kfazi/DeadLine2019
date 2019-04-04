namespace DeadLine2019.Infrastructure
{
    using System;
    using System.Globalization;
    using System.Net.Sockets;
    using System.Text;

    public class NetworkClient : IDisposable
    {
        public const int DefaultTimeout = 10000;

        private readonly Log _log;

        private readonly ASCIIEncoding _asciiEncoding;

        private TcpClient _tcpClient;

        private const int BufferSize = 256;

        private const char EndLineChar = '\n';

        private NetworkStream _stream;

        private string _lastBuffer = string.Empty;

        public NetworkClient(Log log)
        {
            _log = log;

            _asciiEncoding = new ASCIIEncoding();
        }

        ~NetworkClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Connect(string address, int port)
        {
            _tcpClient?.Close();
            _tcpClient = new TcpClient { NoDelay = true };
            _tcpClient.Connect(address, port);
            _stream = _tcpClient.GetStream();
        }

        public void Send(string text)
        {
            _log.WriteTo(text);

            var data = _asciiEncoding.GetBytes(text);
            _stream.WriteTimeout = DefaultTimeout;
            _stream.Write(data, 0, data.Length);
        }

        public void SendLine()
        {
            SendLine(string.Empty);
        }

        public void SendLine(string text)
        {
            Send(text);
            Send(EndLineChar.ToString(CultureInfo.InvariantCulture));
        }

        public string ReadLine(int timeout)
        {
            var text = string.Empty;
            var hasNewLine = false;
            while (!hasNewLine)
            {
                var buffer = new byte[BufferSize];

                if (_lastBuffer.IndexOf(EndLineChar) == -1)
                {
                    _stream.ReadTimeout = timeout;
                    var bytesRead = _stream.Read(buffer, 0, BufferSize);
                    _lastBuffer += _asciiEncoding.GetString(buffer, 0, bytesRead);
                }

                var newLineIndex = _lastBuffer.IndexOf(EndLineChar);
                if (newLineIndex != -1)
                {
                    hasNewLine = true;
                    text = _lastBuffer.Substring(0, newLineIndex);
                    _lastBuffer = _lastBuffer.Substring(newLineIndex + 1);
                }
            }

            _log.WriteLineFrom(text);
            return text;
        }

        public string ReadLine()
        {
            return ReadLine(DefaultTimeout);
        }

        private void Dispose(bool isDisposing)
        {
            if (!isDisposing) return;

            _tcpClient?.Close();
        }
    }
}