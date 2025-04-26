using System.Net.Sockets;

namespace IPK
{
    /// <summary>
    /// This class is used for sending an ERR message if a malformed packet received (also if some received packet can't be received in this state).
    /// It's done like this due to SRP, and also it will be bad to send ERR from where messages are received.
    /// Because we should also get a reply from server, and we can't do it in the same method.
    /// </summary>
    public class ErrorHandler
    {
        /// <summary>
        /// Message that will send this function to a server, default situation, that some malformed packet is received.
        /// </summary>
        public static string ErrorMessage { get; set; } = "Malformed packet received";
        /// <summary>
        /// Tcp variant that sends data ERR message.
        /// </summary>
        /// <param name="stream"> Stream for sending a message. </param>
        /// <param name="clientData"> Client data that are needed for sending a packet. </param>
        /// <param name="error"> By setting this signal in any other part of a program, it indicates that some error occured, and we need to send ERR packet </param>
        /// <exception cref="ErrorException"> If we didn't get an aswer for 5 s period - that means that connection is terminated. </exception>
        /// <exception cref="ReplyException"> Because it's a wrong situation, we need to let the main program know it and end with some error code. </exception>
        public static async Task Error(NetworkStream stream, ClientData clientData,
            AsyncManualResetEvent error)
        {
            await error.WaitAsync();
            error.Reset();
            Task timeoutTask = Task.Delay(5000);
            Task sending = ClientTCP.SendErr(stream, clientData.DisplayName, ErrorMessage);

            Task completedTask = await Task.WhenAny(sending, timeoutTask);
            if (completedTask == timeoutTask)//it's also an error
            {
                throw new ErrorException("Failed to send packet to server");
            }

            throw new ShowedException(ErrorMessage);
        }
        /// <summary>
        /// Udp variant that sends ERR message.
        /// </summary>
        /// <param name="udpClient"> UdpClient that is used for sending data. </param>
        /// <param name="clientData"> Client data that are needed for sending a packet. </param>
        /// <param name="signal"> Signal that are needed to indicate that CONFIRM message is received. </param>
        /// <param name="error"> By setting this signal in any other part of a program, it indicates that some error occured, and we need to send ERR packet </param>
        /// <exception cref="ErrorException"> If we didn't get an answer for 5 s period - that means that connection is terminated. </exception>
        /// <exception cref="ReplyException"> Because it's a wrong situation, we need to let the main program know it and end with some error code. </exception>
        public static async Task Error(UdpClient udpClient, ClientData clientData, AsyncManualResetEvent signal,
            AsyncManualResetEvent error)
        {
            await error.WaitAsync();
            error.Reset();
            int attempt = 0;
            while (attempt <= InputData.Retries)
            {
                Task timeoutTask = Task.Delay(InputData.Timeout);
                Task sending = ClientUDP.SendErr(udpClient, clientData.DisplayName, ErrorMessage, signal);

                Task completedTask = await Task.WhenAny(sending, timeoutTask);
                if (completedTask == sending)
                {
                    break;
                }
                _ = Global.DecrementMessageID;
                attempt++;
            }

            if (attempt - 1 == InputData.Retries)
            {
                throw new ErrorException("Failed to send packet to server");
            }

            throw new ShowedException(ErrorMessage);
        }
    }
}