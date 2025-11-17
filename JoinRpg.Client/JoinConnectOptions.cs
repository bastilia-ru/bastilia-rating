namespace JoinRpg.Client
{
    public class JoinConnectOptions
    {
        public required string Host { get; set; } = "https://joinrpg.ru";
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
