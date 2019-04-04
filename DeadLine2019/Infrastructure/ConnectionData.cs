namespace DeadLine2019.Infrastructure
{
    using System.IO;

    public class ConnectionData
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public void Load(string path)
        {
            var content = File.ReadAllText(path);

            var connectionData = content.FromJson<ConnectionData>();

            Host = connectionData.Host;
            Port = connectionData.Port;
            UserName = connectionData.UserName;
            Password = connectionData.Password;
        }
    }
}