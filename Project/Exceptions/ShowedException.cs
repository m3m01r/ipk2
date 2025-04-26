namespace IPK
{
    /// <summary>
    /// Thrown if an exception message was already shown, so program just needed to end with some error code.
    /// </summary>
    public class ShowedException : Exception
    {
        public ShowedException(string message) : base(message)
        {
        }
    }
}