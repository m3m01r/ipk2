namespace IPK
{
    /// <summary>
    /// All possible codes for all types of messages that can be sent/received in this program.
    /// The only code that's different from specification is NotReply, that's used for recognition if this is a successful action or not, needed for correct work of FSM.
    /// </summary>
    public enum Code
    {
        Confirm = 0x00,
        Reply = 0x01,
        Auth = 0x02,
        Join = 0x03,
        Msg = 0x04,
        NotReply = 0x05,
        Ping = 0xFD,
        Err = 0xFE,
        Bye = 0xFF
    }
}