namespace IPK
{
    /// <summary>
    /// Thrown when the data being sent does not meet the constraints.
    /// </summary>
    public class FormatingException : Exception
    {
        public FormatingException(string message) : base(message)
        {
        }
    }
}