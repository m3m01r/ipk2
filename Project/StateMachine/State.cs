namespace IPK
{
    /// <summary>
    /// States from FSM that can be found in IPK Project 2 specification on gitea.
    /// Helps to manage program flow.
    /// </summary>
    public enum State
    {
        Start = 200,
        Auth = 201,
        Open = 202,
        Join = 203,
        End = 204
    }
}