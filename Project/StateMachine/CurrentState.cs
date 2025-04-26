namespace IPK
{
    /// <summary>
    /// This class is used to save and operate with the current state of FSM.
    /// </summary>
    public class CurrentState
    {
        /// <summary>
        /// The Initial state is start, it's a private variable that can be changed only with it's methods.
        /// </summary>
        private static State Current { get; set; } = State.Start;
        /// <summary>
        /// Changes state.
        /// </summary>
        /// <param name="state"> State that will be set. </param>
        public static void SetState(State state)
        {
            Current = state;
        }
        /// <summary>
        /// Gets state.
        /// </summary>
        /// <returns> Returns current state. </returns>
        public static State GetState()
        {
            return Current;
        }
    }
}