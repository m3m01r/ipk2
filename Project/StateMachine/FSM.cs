namespace IPK
{
    /// <summary>
    /// The Main usage of this class is changing states depending on messages that are sent/received.
    /// FSM can be found in IPK Project 2 specification on gitea.
    /// The Main reason for using FSM is to forbid some messages to be sent/received to ensure that communication runs, according to a specification, and that's right.
    /// </summary>
    public class FSM //Finite State Machine
    {
        /// <summary>
        /// This is a variant that checks messages that are sent, and according to their code, it changes state.
        /// </summary>
        /// <param name="code"> Code of message that is sent </param>
        /// <exception cref="StateException"> Thrown if a message that is someone trying to send can't be sent in a current state. </exception>
        public static void InputAutomat(Code code)
        {
            if (code == Code.Auth)
            {
                if (CurrentState.GetState() == State.Start)
                {
                    CurrentState.SetState(State.Auth);
                }
                else if (CurrentState.GetState() == State.Auth)
                {
                    // It means that a user is trying to authenticate again, because their data aren't valid.  
                    // It isn't a wrong situation, so we just stay in this state.
                }
                else
                {
                    throw new StateException("You already authenticated.");
                }
            }
            else if (code == Code.Join)
            {
                if (CurrentState.GetState() == State.Open)
                {
                    CurrentState.SetState(State.Join);
                }
                else
                {
                    throw new StateException("You are not authenticated.");
                }
            }
            else if (code == Code.Msg)
            {
                if (CurrentState.GetState() != State.Open)
                {
                    throw new StateException("You are not authenticated.");
                }
            }
            else if (code == Code.Bye || code == Code.Err)
            {
                CurrentState.SetState(State.End);
            }
        }
        /// <summary>
        /// This is a variant for checking received messages type, and changing states if 
        /// </summary>
        /// <param name="code"> Code(type) of received message. </param>
        /// <returns> This variant uses return code to manage what should happen next, if everything is good, it returns success.
        /// But if something is wrong, it returns error, or if it's bye or err message it also returns error; that indicates that connection will be terminated.</returns>
        public static int ReadAutomat(Code code) //read means messages from server
        {
            if (code == Code.Reply)
            {
                if (CurrentState.GetState() == State.Auth)
                {
                    CurrentState.SetState(State.Open);
                }
                else if (CurrentState.GetState() == State.Open)
                {
                    CurrentState.SetState(State.End);
                    return ReturnCode.Error;
                }
                else if (CurrentState.GetState() == State.Join)
                {
                    CurrentState.SetState(State.Open);
                }
                return ReturnCode.Success;
            }
            if (code == Code.Bye || code == Code.Err)
            {
                CurrentState.SetState(State.End);
                return ReturnCode.Error;
            }
            if (code == Code.Msg)
            {
                if (CurrentState.GetState() == State.Auth)
                {
                    CurrentState.SetState(State.End);
                    return ReturnCode.Error;
                }
                return ReturnCode.Success;
            }
            if (code == Code.NotReply)
            {
                if (CurrentState.GetState() == State.Open)
                {
                    CurrentState.SetState(State.End);
                    return ReturnCode.Error;
                }
                if (CurrentState.GetState() == State.Join)
                {
                    CurrentState.SetState(State.Open);
                }
            }
            return ReturnCode.Success;
        }
    }
}