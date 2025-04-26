namespace IPK
{
    /// <summary>
    /// That class is used as a set of different data that is needed for sending messages all across the program.
    /// </summary>
    public class ClientData
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = "unknown";
        public string Secret { get; set; } = string.Empty;
        public string ChannelID { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
    }
}