namespace DeadLine2019.Infrastructure
{
    using System.IO;

    public class ConnectionData
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public static ConnectionData Load(string path)
        {
            var content = File.ReadAllText(path);

            return content.FromJson<ConnectionData>();
        }
    }
}