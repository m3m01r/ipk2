using System.Text.RegularExpressions;

namespace IPK
{
    /// <summary>
    /// This class is used to check the structure of a message and see if they meet constraints.
    /// </summary>
    public class MessageCheck
    {
        /// <summary>
        /// Here it checks the structure of a message, and if it doesn't meet constraints, it returns error code, otherwise success code.
        /// </summary>
        /// <param name="message"> Message that we are going to check. </param>
        /// <param name="msgID"> It's necessary to understand what type of message we are checking, because we can't get type from its structure. </param>
        /// <returns> ReturnCode represents the success of this check, if it's error => something wrong with input </returns>
        public static int Check(string message, int msgID)
        {
            string charPattern = @"^[a-zA-Z0-9_-]+$";
            string printPattern = @"^[\x21-\x7E]+$";
            string printAndSpacePattern = @"^[\x0A\x20-\x7E]*$";
            if (msgID == MsgIdentifiers.MessageID)
            {
                if (!short.TryParse(message, out _))
                    return ReturnCode.Error;
            }
            else if (msgID == MsgIdentifiers.Username || msgID == MsgIdentifiers.ChannelID)
            {
                if (!(message.Length <= 20 && Regex.IsMatch(message, charPattern)))
                    return ReturnCode.Error;
            }
            else if (msgID == MsgIdentifiers.Secret)
            {
                if (!(message.Length <= 128 && Regex.IsMatch(message, charPattern)))
                    return ReturnCode.Error;
            }
            else if (msgID == MsgIdentifiers.MessageContent)
            {
                if (!(message.Length <= 60000))
                {
                    Console.WriteLine("ERROR: Message is too long, it will be truncated");
                }
                if (!Regex.IsMatch(message, printAndSpacePattern))
                    return ReturnCode.Error;
            }
            else if (msgID == MsgIdentifiers.DisplayName)
            {
                if (!(message.Length <= 20 && Regex.IsMatch(message, printPattern)))
                    return ReturnCode.Error;
            }
            return ReturnCode.Success;
        }
    }
}