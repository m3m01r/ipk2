namespace IPK
{
    /// <summary>
    /// Thrown if there is no answer for a request message.
    /// </summary>
    public class ReplyException : Exception
    {
        public ReplyException(string message) : base(message)
        {
        }
    }
}