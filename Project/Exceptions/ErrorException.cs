namespace IPK
{
    /// <summary>
    /// Thrown if there is a malformed/out-of-order packet.
    /// </summary>
    public class ErrorException : Exception
    {
        public ErrorException(string message) : base(message)
        {
        }
    }
}