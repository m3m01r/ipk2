using System.Net.Sockets;

namespace IPK
{
    /// <summary>
    /// This class is responsible for checking input given by user, processing it and sending packets with given data/ 
    /// </summary>
    public static class InputCheck
    {
        /// <summary>
        /// This is needed to understand in which state we are - to understand when we need to stop.
        /// </summary>
        public static Code MsgType { get; set; }
        /// <summary>
        /// This async Task works in cycle -> it starts with receiving user input => checks it => check state => send a message => starts again. UDP variant.
        /// </summary>
        /// <param name="udpClient"> UdpClient that's used for sending packets.</param>
        /// <param name="clientData"> Data of a client, which is given here by user input, but used in other parts of code also.</param>
        /// <param name="signal"> This signal is set when CONFIRM is received. We also set timer, and we wait for this signal - if it times-out -> we should do retransmission.
        /// By using it, we ensure that connection stays reliable, if we don't get CONFIRMMed, we think that connection is closed. </param>
        /// <param name="reply"> This signal is set when REPLY message is received.
        /// We also set timer, and we wait for this signal - if it times-out -> there is no answer -> it's an error situation.</param>
        /// <exception cref="FormatingException"> Thrown if given data is wrong.</exception>
        /// <exception cref="ReplyException"> Thrown if there is no *REPLY -> error state.</exception>
        /// <exception cref="ErrorException"> Thrown if there is error occurred with retransmission. </exception>
        public static async Task InputAsync(UdpClient udpClient, ClientData clientData, AsyncManualResetEvent signal, AsyncManualResetEvent reply)
        {
            while (true)
            {
                bool check = true;
                string? input = null;
                while (check)
                {
                    input = await Task.Run(() => Console.ReadLine());
                    check = Check(input, clientData);
                    if (MsgType == Code.Bye)//if it's bye => bye bye.
                    {
                        break;
                    }
                }

                try//if it throws some exception, it can't be a critical issue, so we show it and going to next iteration, to get new input
                {
                    FSM.InputAutomat(MsgType);
                }
                catch (StateException ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    continue;
                }

                if (MsgType == Code.Auth)
                {
                    int attempt = 0;
                    while (attempt <= InputData.Retries)// if we didn't get a CONFIRM message, we should start Udp retransmission.
                    {
                        Task timeoutTask = Task.Delay(InputData.Timeout);
                        Task sending = ClientUDP.SendAuth(udpClient, clientData.Username, clientData.DisplayName,
                            clientData.Secret, signal);
                        Task completedTask = await Task.WhenAny(sending, timeoutTask);
                        if (completedTask == sending)//if completedTask isn't sending, then it's a timer, and we should send a message again.
                        {
                            break;
                        }
                        _ = Global.DecrementMessageID;
                        attempt++;
                    }

                    if (attempt - 1 == InputData.Retries)
                    {
                        throw new FormatingException("Failed to send packet to server");
                    }

                    Task replyTimeout = Task.Delay(5000);// We should get *REPLY message for a request message in 5 seconds, that's an error situation, and we should send an error packet.
                    Task replyWait = reply.WaitAsync();

                    Task completedReply = await Task.WhenAny(replyWait, replyTimeout);
                    if (completedReply == replyWait)
                    {
                        reply.Reset();
                    }
                    else //sending an error packet is the same task as sending any other packet, and probability that we will do retransmission is even higher, because now communication in a bad "state"
                    {
                        Console.WriteLine("ERROR: There is no Reply from server.");
                        attempt = 0;
                        while (attempt <= InputData.Retries)// if we didn't get a CONFIRM message, we should start Udp retransmission.
                        {
                            Task timeoutTask = Task.Delay(InputData.Timeout);
                            Task sending = ClientUDP.SendErr(udpClient, clientData.DisplayName,
                                "There is no *REPLY from server to a request message.", signal);
                            Task completedTask = await Task.WhenAny(sending, timeoutTask);
                            if (completedTask == sending)//if completedTask isn't sending, then it's a timer, and we should send a message again.
                            {
                                break;
                            }
                            _ = Global.DecrementMessageID;
                            attempt++;
                        }

                        throw new ReplyException("There is no Reply from server.");
                    }
                }
                else if (MsgType == Code.Join)
                {
                    int attempt = 0;
                    while (attempt <= InputData.Retries)// if we didn't get a CONFIRM message, we should start Udp retransmission.
                    {
                        Task timeoutTask = Task.Delay(InputData.Timeout);
                        Task sending = ClientUDP.SendJoin(udpClient, clientData.ChannelID, clientData.DisplayName,
                            signal);

                        Task completedTask = await Task.WhenAny(sending, timeoutTask);
                        if (completedTask == sending)//if completedTask isn't sending, then it's a timer, and we should send a message again.
                        {
                            break;
                        }
                        _ = Global.DecrementMessageID;
                        attempt++;
                    }

                    if (attempt - 1 == InputData.Retries)
                    {
                        throw new FormatingException("Failed to send packet to server.");
                    }

                    Task replyTimeout = Task.Delay(5000);//We should get *REPLY message for a request message in 5 seconds, that's an error situation, and we should send an error packet.
                    Task replyWait = reply.WaitAsync();
                    reply.Reset();
                    Task completedReply = await Task.WhenAny(replyWait, replyTimeout);
                    if (completedReply == replyWait)
                    {
                        reply.Reset();
                    }
                    else
                    {
                        Console.WriteLine("ERROR: There is no Reply from server.");
                        attempt = 0;
                        while (attempt <= InputData.Retries)// if we didn't get a CONFIRM message, we should start Udp retransmission.
                        {
                            Task timeoutTask = Task.Delay(InputData.Timeout);
                            Task sending = ClientUDP.SendErr(udpClient, clientData.DisplayName,
                                "There is no *Reply from server to a request message.", signal);

                            Task completedTask = await Task.WhenAny(sending, timeoutTask);
                            if (completedTask == sending)//if completedTask isn't sending, then it's a timer, and we should send a message again.
                            {
                                break;
                            }
                            _ = Global.DecrementMessageID;
                            attempt++;
                        }

                        throw new ReplyException("There is no Reply from server.");
                    }
                }
                else if (MsgType == Code.Msg)
                {
                    int attempt = 0;
                    while (attempt <= InputData.Retries)// if we didn't get a CONFIRM message, we should start Udp retransmission.
                    {
                        Task timeoutTask = Task.Delay(InputData.Timeout);
                        Task sending = ClientUDP.SendMsg(udpClient, clientData.DisplayName, clientData.MessageContent,
                            signal);

                        Task completedTask = await Task.WhenAny(sending, timeoutTask);
                        if (completedTask == sending)//if completedTask isn't sending, then it's a timer, and we should send a message again.
                        {
                            break;
                        }
                        _ = Global.DecrementMessageID;
                        attempt++;
                    }

                    if (attempt - 1 == InputData.Retries)
                    {
                        throw new FormatingException("Failed to send packet to server");
                    }
                }
                else if (MsgType == Code.Bye)
                {
                    int attempt = 0;
                    while (attempt <= InputData.Retries)// if we didn't get a CONFIRM message, we should start Udp retransmission.
                    {
                        Task timeoutTask = Task.Delay(InputData.Timeout);
                        Task sending = ClientUDP.SendBye(udpClient, clientData.DisplayName, signal);

                        Task completedTask = await Task.WhenAny(sending, timeoutTask);
                        if (completedTask == sending)//if completedTask isn't sending, then it's a timer, and we should send a message again.
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

                    break;
                }
            }
        }

        /// <summary>
        /// This async Task works in cycle -> it starts with receiving user input => checks it => check state => send a message => starts again. TCP variant.
        /// </summary>
        /// <param name="stream"> Stream that is used for sending packets. </param>
        /// <param name="clientData"> Data of a client, which is given here by user input, but used in other parts of code also.</param>
        /// <param name="reply"> This signal is set when REPLY message is received.
        /// We also set timer, and we wait for this signal - if it times-out -> there is no answer -> it's an error situation.</param>
        /// <exception cref="ReplyException">If there is no *REPLY for a timeout - it's an error situation.</exception>
        public static async Task InputAsync(NetworkStream stream, ClientData clientData, AsyncManualResetEvent reply)
        {
            while (true)
            {
                bool check = true;
                string? input = null;
                while (check)
                {
                    input = await Task.Run(() => Console.ReadLine());
                    check = Check(input, clientData);
                    if (MsgType == Code.Bye)
                    {
                        break;
                    }
                }

                try
                {
                    FSM.InputAutomat(MsgType);
                }
                catch (StateException ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    continue;
                }

                if (MsgType == Code.Auth)
                {
                    await ClientTCP.SendAuth(stream, clientData.Username, clientData.DisplayName, clientData.Secret);
                    Task replyTimeout = Task.Delay(5000);
                    Task replyWait = reply.WaitAsync();

                    Task completedReply = await Task.WhenAny(replyWait, replyTimeout);
                    if (completedReply == replyWait)
                    {
                        reply.Reset();
                    }
                    else
                    {
                        Console.WriteLine("ERROR: There is no Reply from server");
                        await ClientTCP.SendErr(stream,clientData.DisplayName,"There is no Reply from server");
                        throw new ReplyException("There is no Reply from server");
                    }
                }
                else if (MsgType == Code.Join)
                {
                    await ClientTCP.SendJoin(stream, clientData.ChannelID, clientData.DisplayName);
                    Task replyTimeout = Task.Delay(5000);
                    Task replyWait = reply.WaitAsync();

                    Task completedReply = await Task.WhenAny(replyWait, replyTimeout);
                    if (completedReply == replyWait)
                    {
                        reply.Reset();
                    }
                    else
                    {
                        Console.WriteLine("ERROR: There is no Reply from server");
                        await ClientTCP.SendErr(stream,clientData.DisplayName,"There is no Reply from server");
                        throw new ReplyException("There is no Reply from server");
                    }
                }
                else if (MsgType == Code.Msg)
                {
                    await ClientTCP.SendMsg(stream, clientData.DisplayName, clientData.MessageContent);
                }
                else if (MsgType == Code.Bye)
                {
                    await ClientTCP.SendBye(stream, clientData.DisplayName);
                    break;
                }
            }
        }
        /// <summary>
        /// This method checks input data, saves it in clientData if it's right, otherwise return true (analog of an error).
        /// </summary>
        /// <param name="input"> Data given by user input that we should check. </param>
        /// <param name="clientData"> Data of a client, which is given here by user input, but used in other parts of code also.</param>
        /// <returns>It returns true if input was given wrong or someone did command that shouldn't send something(help or rename).
        /// Otherwise, it returns false -> cycle of receiving should end, and we should send a packet.</returns>
        public static bool Check(string? input, ClientData clientData)
        {
            if (string.IsNullOrEmpty(input))
            {
                MsgType = Code.Bye;
                return true;
            }
            else if (input.StartsWith("/auth"))
            {
                string[] parts = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4)
                {
                    Console.WriteLine("ERROR: Invalid AUTH command. Please provide exactly 3 arguments.");
                    return true;
                }
                else if (MessageCheck.Check(parts[1], MsgIdentifiers.Username) == ReturnCode.Success &&
                         MessageCheck.Check(parts[2], MsgIdentifiers.Secret) == ReturnCode.Success &&
                         MessageCheck.Check(parts[3], MsgIdentifiers.DisplayName) == ReturnCode.Success)
                {
                    clientData.Username = parts[1];
                    clientData.Secret = parts[2];
                    clientData.DisplayName = parts[3];
                    MsgType = Code.Auth;
                    return false;
                }
                else
                {
                    Console.WriteLine("ERROR: Invalid AUTH command. Please provide valid arguments.");
                    return true;
                }
            }
            else if (input.StartsWith("/join"))
            {
                string[] parts = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("ERROR: Invalid JOIN command. Please provide ChannelID.");
                    return true;
                }
                else if (MessageCheck.Check(parts[1], MsgIdentifiers.ChannelID) == ReturnCode.Success)
                {
                    clientData.ChannelID = parts[1];
                    MsgType = Code.Join;
                    return false;
                }
                else
                {
                    Console.WriteLine("ERROR: Invalid JOIN command. Please provide valid arguments.");
                    return true;
                }
            }
            else if (input.StartsWith("/rename"))
            {
                string[] parts = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("ERROR: Invalid RENAME command. Please provide DisplayName.");
                    return true;
                }
                else if (MessageCheck.Check(parts[1], MsgIdentifiers.DisplayName) == ReturnCode.Success)
                {
                    clientData.DisplayName = parts[1];
                    return true;
                }
                else
                {
                    Console.WriteLine("ERROR: Invalid RENAME command. Please provide valid arguments.");
                    return true;
                }
            }
            else if (input.StartsWith("/help"))
            {
                Console.WriteLine("------");
                Console.WriteLine(
                    "Common use: Firstly AUTHentificate, than after confirm - use it by writing commands or just plain text that will be sent to server that you specified\n");
                Console.WriteLine("Available commands: /auth, /join, /rename, /help");
                Console.WriteLine("The notation with braces ({}) is used for required parameters.\n");
                Console.WriteLine("Command: /auth");
                Console.WriteLine("Usage: /auth {Username} {Secret} {DisplayName}");
                Console.WriteLine(
                    "Description: Sends AUTH message with the data provided from the command to the server, locally sets the DisplayName value (same as the /rename command).\n");
                Console.WriteLine("Command: /join");
                Console.WriteLine("Usage: /join {ChannelID}");
                Console.WriteLine("Sends JOIN message with channel name from the command to the server.\n");
                Console.WriteLine("Command: /rename");
                Console.WriteLine("Usage: /rename {DisplayName}");
                Console.WriteLine(
                    "Locally changes the display name of the user to be sent with new messages/selected commands.\n");
                Console.WriteLine("Command: /help");
                Console.WriteLine("Usage: /help");
                Console.WriteLine("Prints out this message");
                Console.WriteLine("------");
                return true;
            }
            else if (input.StartsWith('/'))
            {
                Console.WriteLine("ERROR: Some invalid command. Please try again.");
                return true;
            }
            else
            {
                if (MessageCheck.Check(input, MsgIdentifiers.MessageContent) == ReturnCode.Success)
                {
                    if (input.Length>60000)
                        clientData.MessageContent = input.Substring(0,60000);
                    else 
                        clientData.MessageContent = input;
                    MsgType = Code.Msg;
                    return false;
                }
                else
                {
                    Console.WriteLine("ERROR: Invalid message content. Please provide valid data.");
                    return true;
                }
            }
        }
    }
}