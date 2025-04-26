namespace IPK
{
    /// <summary>
    /// Return codes, for success and failure.
    /// </summary>
    public static class ReturnCode
    {
        public static int Success => (int)Code.Success;
        public static int Error => (int)Code.Error;

        private enum Code
        {
            Success = -1,
            Error = -2
        }
    }
}