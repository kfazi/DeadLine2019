namespace DeadLine2019
{
    using System.IO;

    using DeadLine2019.Infrastructure;

    public class LogicState
    {
        public bool IsInitialized { get; set; } = false;

        public int TurnNumber { get; set; }

        public static LogicState Load(string serverName)
        {
            try
            {
                var json = File.ReadAllText($@"logic-{serverName}.json");
                return json.FromJson<LogicState>();
            }
            catch
            {
                return new LogicState();
            }
        }

        public static void Save(LogicState state, string serverName)
        {
            var json = state.ToJson();
            File.WriteAllText($@"logic-{serverName}.json", json);
        }
    }
}