namespace DeadLine2019.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TokenReader
    {
        private static readonly char[] SplitChars = { ' ', '\t', '\n', '\r' };

        private readonly NetworkClient _client;

        private readonly Queue<string> _tokens;

        public TokenReader(NetworkClient client)
        {
            _client = client;

            _tokens = new Queue<string>();
        }

        public void Skip()
        {
            ReadNextToken();
        }

        public string ReadString()
        {
            return ReadNextToken();
        }

        public T ReadEnum<T>()
        {
            return (T)Enum.Parse(typeof(T), ReadNextToken(), true);
        }

        public int ReadInt()
        {
            return int.Parse(ReadNextToken());
        }

        public uint ReadUInt()
        {
            return uint.Parse(ReadNextToken());
        }

        public double ReadDouble()
        {
            return double.Parse(ReadNextToken());
        }

        private string ReadNextToken()
        {
            if (_tokens.Any())
            {
                return _tokens.Dequeue();
            }

            var line = _client.ReadLine();
            var tokens = line.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                _tokens.Enqueue(token);
            }

            return _tokens.Dequeue();
        }
    }
}