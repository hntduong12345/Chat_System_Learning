namespace TestChatAPI.Payloads.ChatSessions
{
    public class CreateSessionRequest
    {
        public string SessionName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
