namespace IPK
{
    /// <summary>
    /// Identifiers that are used for checking them, because without knowing what this message type is this, we can't say if it's used right.
    /// </summary>
    static class MsgIdentifiers
    {
        public static int MessageID => (int)MessageType.MessageID;
        public static int Username => (int)MessageType.Username;
        public static int ChannelID => (int)MessageType.ChannelID;
        public static int Secret => (int)MessageType.Secret;
        public static int DisplayName => (int)MessageType.DisplayName;
        public static int MessageContent => (int)MessageType.MessageContent;

        private enum MessageType
        {
            MessageID = 100,
            Username = 101,
            ChannelID = 102,
            Secret = 103,
            DisplayName = 104,
            MessageContent = 105
        }
    }
}