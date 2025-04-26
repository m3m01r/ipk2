namespace IPK
{
    /// <summary>
    ///  Thrown if there is a problem with given arguments.
    /// </summary>
    public class ArgumentException : Exception
    {
        public ArgumentException(string message) : base(message)
        {
        }
    }
}