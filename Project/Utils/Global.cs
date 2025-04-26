namespace IPK
{
    /// <summary>
    /// Global id of a message that's increments for each packet sent. 
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// This is a private variable that contains the value of a message that was sent last.
        /// </summary>
        private static ushort _messageID = 0;
        /// <summary>
        /// To make it easier, just increment this variable each time it's used. 
        /// </summary>
        public static ushort MessageID
        {
            get { return _messageID++; }
        }
        /// <summary>
        /// In some situations, like udp retransmission, this property that it increments after each use isn't fit right,
        /// it's the same packet that we are retransmitting.
        /// But adding incrementing was leading to major checks and changes to a code, and also it's not quite comfortable.
        /// So just added decrementing for some parts where it's necessary.
        /// </summary>
        public static ushort DecrementMessageID
        {
            get { return --_messageID; }
        }
    }
}