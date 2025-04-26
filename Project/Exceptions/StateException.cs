namespace IPK
{
    /// <summary>
    /// Thrown if user/server data that are currently processed can't be sent/received in a current state. 
    /// </summary>
    public class StateException : Exception
    {
        public StateException(string message) : base(message)
        {
        }
    }
}