using System.Net;
using System.Net.Sockets;

namespace IPK
{
    /// <summary>
    /// Entry point, main.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main logic of program which starts all tasks and processes.
        /// </summary>
        /// <param name="args"> Arguments of a command line. </param>
        /// <exception cref="Exception"> Resending all exceptions from tasks, because they suppress them. </exception>
        static async Task Main(string[] args)
        {
            try
            {
                ArgumentParser.Parse(args);

                AsyncManualResetEvent reply = new();
                AsyncManualResetEvent error = new();
                AsyncManualResetEvent
                    signal = new(); //this signal is for UDP, it will block writing new input until CONFIRM is received - otherwise error
                ClientData clientData = new();

                await Task.Delay(1); // ensures the method remains asynchronous

                Task ioTask;
                Task receiveTask;
                Task errorTask;

                if (InputData.ProtocolType == "udp")
                {
                    using UdpClient udpClient = new(new IPEndPoint(IPAddress.Any, 0));
                    Console.CancelKeyPress += async (_, e) => //we 
                    {
                        e.Cancel = true;
                        try
                        {
                            await ExitCc.SendBye(udpClient, clientData, signal);
                        }
                        catch (ErrorException ex)
                        {
                            Console.WriteLine($"ERROR: {ex.Message}");
                        }

                        Environment.Exit(0);
                    };
                    ClientUDP.serverPort = InputData.ServerPort;
                    ClientUDP.serverIp = InputData.Server;
                    ioTask = InputCheck.InputAsync(udpClient, clientData, signal, reply);
                    receiveTask = Reader.Read(udpClient, signal, reply, error);
                    errorTask = ErrorHandler.Error(udpClient, clientData, signal, error);
                    Task.WaitAny(ioTask, receiveTask, errorTask);
                }
                else
                {
                    using TcpClient client = new TcpClient(InputData.Server, InputData.ServerPort);
                    using NetworkStream stream = client.GetStream();
                    Console.CancelKeyPress += async (_, e) =>
                    {
                        e.Cancel = true; // Prevent the process from terminating
                        try
                        {
                            await ExitCc.SendBye(stream, clientData);
                        }
                        catch (ErrorException ex)
                        {
                            Console.WriteLine($"ERROR: {ex.Message}");
                        }

                        Environment.Exit(0); // Exit the program
                    };
                    ioTask = InputCheck.InputAsync(stream, clientData, reply);
                    receiveTask = Reader.Read(stream, reply, error);
                    errorTask = ErrorHandler.Error(stream, clientData, error);
                    Task.WaitAny(ioTask, receiveTask, errorTask);
                }

                if (ioTask.IsFaulted) //Because async tasks suppress their exceptions, we should resend them
                {
                    throw ioTask.Exception.Flatten().InnerExceptions.First();
                }

                if (receiveTask.IsFaulted)
                {
                    throw receiveTask.Exception.Flatten().InnerExceptions.First();
                }

                if (errorTask.IsFaulted)
                {
                    throw errorTask.Exception.Flatten().InnerExceptions.First();
                }

            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Environment.Exit(1);
            }
            catch (FormatingException ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Environment.Exit(2);
            }
            catch (StateException ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Environment.Exit(3);
            }
            catch (ErrorException ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Environment.Exit(4);
            }
            catch (ReplyException) //"ERROR:" message had already shown
            {
                Environment.Exit(5);
            }
            catch (ShowedException)
            {
                Environment.Exit(6);
            }
            catch (Exception ex) //Any other exception
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Environment.Exit(7);
            }
        }
    }
}