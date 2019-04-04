namespace DeadLine2019
{
    using DeadLine2019.Infrastructure;

    public class Commands : CommonCommands
    {
        public Commands(NetworkClient networkClient)
            : base(networkClient)
        {
        }
    }
}