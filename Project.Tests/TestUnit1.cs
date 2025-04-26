namespace Project.Tests;

using System.Net.Sockets;
using IPK;
using System.Net;

public class TestUnit1
{
    [Fact]
    public void MessageContentTest1()
    {
        
        ClientData clientData = new();
        bool result = InputCheck.Check( "input", clientData);
        bool result1 = InputCheck.Check( "/join aa", clientData);
        bool result2 = InputCheck.Check( "/auth a a a", clientData);
        bool result4 = InputCheck.Check( "/rename aa", clientData);
        
        Assert.False(result);
        Assert.False(result1);
        Assert.False(result2);
        Assert.True(result4);
        Assert.Equal("input", clientData.MessageContent);
        Assert.Equal("a", clientData.Username);
        Assert.Equal("aa", clientData.ChannelID);
        Assert.Equal("aa", clientData.DisplayName);
    }

    [Fact]
    public void WrongCommandTest1()
    {
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        ClientData clientData = new();

        bool result = InputCheck.Check( "/asd aa", clientData);
        bool result1 = InputCheck.Check( "/ aa", clientData);


        var output = stringWriter.ToString();

        Assert.StartsWith("ERROR: ", output);
        Assert.True(result);
        Assert.True(result1);
    }

    [Fact]
    public void WrongCommandTest2()
    {
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        ClientData clientData = new();

        bool result = InputCheck.Check( "/rename aa a", clientData);
        bool result1 = InputCheck.Check( "/join aa a", clientData);
        bool result2 = InputCheck.Check( "/auth a a a a", clientData);
        bool result3 = InputCheck.Check( "/auth a a", clientData);

        var output = stringWriter.ToString();

        Assert.StartsWith("ERROR: ", output);
        Assert.True(result);
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
    }
    [Fact]
    public void TooLongInputTest1()
    {
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        ClientData clientData = new();

        bool result = InputCheck.Check( "/rename aaaaaaaaaaaaaaaaaaaaa", clientData);
        bool result1 = InputCheck.Check( "/join aaaaaaaaaaaaaaaaaaaaa", clientData);
        bool result2 = InputCheck.Check( "/auth aaaaaaaaaaaaaaaaaaaaa a a", clientData);

        var output = stringWriter.ToString();

        Assert.StartsWith("ERROR: ", output);
        Assert.True(result);
        Assert.True(result1);
        Assert.True(result2);
    }
    [Fact]
    public void MessageContentTest2()
    {
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        ClientData clientData = new();

        string longText = new('A', 60021);// limit on messages is 60000

        bool result = InputCheck.Check( longText, clientData);

        var output = stringWriter.ToString();

        Assert.StartsWith("ERROR: ", output);//It gives an error, because the message is too long
        Assert.False(result);//But the result is false, what means in My program that the message will be sent anyway
        Assert.Equal(60000, clientData.MessageContent.Length);// It should have cutted it.
    }
    [Fact]
    public void ArgumentTest1()
    {
        Exception ex = Record.Exception(() => ArgumentParser.Parse(["-t"]));
        Exception ex1 = Record.Exception(() => ArgumentParser.Parse(["-s"]));
        Exception ex2 = Record.Exception(() => ArgumentParser.Parse(["-p"]));
        Exception ex3 = Record.Exception(() => ArgumentParser.Parse(["-d"]));
        Exception ex4 = Record.Exception(() => ArgumentParser.Parse(["-r"]));
        Exception ex5 = Record.Exception(() => ArgumentParser.Parse(["a"]));

        Assert.NotNull(ex);
        Assert.NotNull(ex1);
        Assert.NotNull(ex2);
        Assert.NotNull(ex3);
        Assert.NotNull(ex4);
        Assert.NotNull(ex5);
    }
    [Fact]
    public void ArgumentTest2()//right argument, but without mandatory 
    {
        Exception ex = Record.Exception(() => ArgumentParser.Parse(["-p", "1234"]));

        Assert.Equal(1234, InputData.ServerPort);
        Assert.NotNull(ex);
    }
    [Fact]
    public void ArgumentTest3()//check if it really changes values.
    {
        Exception ex = Record.Exception(() => ArgumentParser.Parse(["-p", "1234", "-t", "tcp", "-s",  "127.0.0.1", "-r", "5", "-d", "500"]));

        Assert.Equal("127.0.0.1", InputData.Server);    
        Assert.Equal("tcp", InputData.ProtocolType);
        Assert.Equal(1234, InputData.ServerPort);
        Assert.Equal(5, InputData.Retries);
        Assert.Equal(500, InputData.Timeout);
        Assert.Null(ex);
    }
    [Fact]
    public void ArgumentTest4()//giving bad domain name -- because this test uses DNS, it's taking too long, you can delete comments to check it, on my machine it takes 10 seconds to complete.
    {
        Exception ex = Record.Exception(() => ArgumentParser.Parse(["-t", "tcp", "-s",  "aaa"]));

        Assert.NotNull(ex);
    }
    [Fact]
    public async Task ClientUDPtest1(){//check if it's giving exceptions if message is wrong
        UdpClient udpClient = new();//it's empty, but test is not about it
        AsyncManualResetEvent signal = new();
        Exception ex = await Record.ExceptionAsync(() => ClientUDP.SendAuth(udpClient, "''']]", "a", "a", signal));
        Exception ex1 = await Record.ExceptionAsync(() => ClientUDP.SendConfirm(udpClient, [0xFF,0xFF,0xFF]));

        Assert.NotNull(ex);
        Assert.NotNull(ex1);
    }
    [Fact]
    public async Task ClientTCPtest1(){//here we create empty connection just to check if methods send exception.
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();
        var connectTask = client.ConnectAsync(IPAddress.Loopback, port);

        var serverClient = await listener.AcceptTcpClientAsync();
        await connectTask;

        using NetworkStream stream = client.GetStream();

        Exception ex = await Record.ExceptionAsync(() => ClientTCP.SendAuth(stream, "''']]", "a", "a"));
        Exception ex1 = await Record.ExceptionAsync(() => ClientTCP.SendJoin(stream, "''']]", "a"));
        Exception ex2 = await Record.ExceptionAsync(() => ClientTCP.SendBye(stream, "1231111111111111111111"));
        Exception ex3 = await Record.ExceptionAsync(() => ClientTCP.SendErr(stream, "1231111111111111111111", "a"));
        Exception ex4 = await Record.ExceptionAsync(() => ClientTCP.SendMsg(stream, "1231111111111111111111", "a"));
        
        Assert.NotNull(ex);
        Assert.NotNull(ex1);
        Assert.NotNull(ex2);
        Assert.NotNull(ex3);
        Assert.NotNull(ex4);

        listener.Stop();
        serverClient.Close();
        client.Close();
    }
    [Fact]
    public async Task ClientTCPtest2(){//here we create empty connection just to check if methods really send message in normal situation.
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();
        var connectTask = client.ConnectAsync(IPAddress.Loopback, port);

        var serverClient = await listener.AcceptTcpClientAsync();
        await connectTask;

        using NetworkStream stream = client.GetStream();

        Exception ex = await Record.ExceptionAsync(() => ClientTCP.SendAuth(stream, "11111111111111111", "a", "a"));
        Exception ex1 = await Record.ExceptionAsync(() => ClientTCP.SendJoin(stream, "11111111111111111", "a"));
        Exception ex2 = await Record.ExceptionAsync(() => ClientTCP.SendBye(stream, "11111111111111111"));
        Exception ex3 = await Record.ExceptionAsync(() => ClientTCP.SendErr(stream, "11111111111111111", "a"));
        Exception ex4 = await Record.ExceptionAsync(() => ClientTCP.SendMsg(stream, "11111111111111111", "a"));
        
        Assert.Null(ex);
        Assert.Null(ex1);
        Assert.Null(ex2);
        Assert.Null(ex3);
        Assert.Null(ex4);

        listener.Stop();
        serverClient.Close();
        client.Close();
    }
    [Fact]
    public void DataCheck1()//test for TCP variant messages.
    {
        Code result = Code.Auth;//it can never receive auth and process it, it will be just malformed packet
        Code result1 = Code.Auth;//so i check return values using it, so if it return s something, it will change it and assert will work.
        Code result2 = Code.Auth;
        Code result3 = Code.Auth;
        Code result4 = Code.Auth;
        Code result5 = Code.Auth;
        Exception ex = Record.Exception(() =>  result  = Data.Check("REPLY OK IS everything is ok"));
        Exception ex1 = Record.Exception(() => result1 = Data.Check("Malformed message"));//there will be exception and value will be the same.
        Exception ex2 = Record.Exception(() => result2 = Data.Check("REPLY NOK IS something is bad"));
        Exception ex3 = Record.Exception(() => result3 = Data.Check("MSG FROM Server IS Message"));
        Exception ex4 = Record.Exception(() => result4 = Data.Check("BYE FROM Server"));
        Exception ex5 = Record.Exception(() => result5 = Data.Check("ERR FROM Server IS Error occured"));

        Assert.Equal(Code.Reply, result);
        Assert.Equal(Code.Auth, result1);
        Assert.Equal(Code.NotReply, result2);
        Assert.Equal(Code.Msg, result3);
        Assert.Equal(Code.Bye, result4);
        Assert.Equal(Code.Err, result5);
        Assert.Null(ex);
        Assert.NotNull(ex1);
        Assert.Null(ex2);
        Assert.Null(ex3);
        Assert.Null(ex4);
        Assert.Null(ex5);
    }
    [Fact]
    public void DataCheck2()//test for UDP variant messages.
    {
        Code result = Code.Auth;//it can never receive auth and process it, it will be just malformed packet
        Code result1 = Code.Auth;//so i check return values using it, so if it return s something, it will change it and assert will work.
        Code result2 = Code.Auth;
        Code result3 = Code.Auth;
        Code result4 = Code.Auth;
        Code result5 = Code.Auth;
        Exception ex = Record.Exception(() =>  result  = Data.Check([0xFD,0x00,0x00]));
        Exception ex1 = Record.Exception(() => result1 = Data.Check([0x01,0x00,0x00,0x01,0x00,0x00,0x77,0x88,0x00]));
        Exception ex2 = Record.Exception(() => result2 =Data.Check([0x01,0x00,0x00,0x00,0x00,0x00,0x77,0x88,0x00]));
        Exception ex3 = Record.Exception(() => result3 =Data.Check([0x04,0x00,0x00,0x77,0x00,0x77,0x88,0x00]));
        Exception ex4 = Record.Exception(() => result4 =Data.Check([0xFF,0x00,0x00,0x77,0x00]));
        Exception ex5 = Record.Exception(() => result5 =Data.Check([0xFE,0x00,0x00,0x77,0x00,0x77,0x88,0x00]));

        Assert.Equal(Code.Ping, result);
        Assert.Equal(Code.Reply, result1);
        Assert.Equal(Code.NotReply, result2);
        Assert.Equal(Code.Msg, result3);
        Assert.Equal(Code.Bye, result4);
        Assert.Equal(Code.Err, result5);
        Assert.Null(ex);
        Assert.Null(ex1);
        Assert.Null(ex2);
        Assert.Null(ex3);
        Assert.Null(ex4);
        Assert.Null(ex5);
    }
    [Fact]
    public void DataCheck3()//TCP variant check if it checks structure of a packets.
    {
        Exception ex1 = Record.Exception(() =>  Data.Check("REPLY BOK IS everything is ok"));
        Exception ex2 = Record.Exception(() => Data.Check("MSG FROM Server"));
        Exception ex3 = Record.Exception(() => Data.Check("BYE FRM Server"));
        Exception ex4 = Record.Exception(() => Data.Check("ERR FROM Server S Error occured"));

        Assert.NotNull(ex1);
        Assert.NotNull(ex2);
        Assert.NotNull(ex3);
        Assert.NotNull(ex4);
    }
    [Fact]
    public void DataCheck4()//UDP variant check if it checks structure of a packets.
    {
        Exception ex = Record.Exception(() =>  Data.Check([0xFD,0x00]));
        Exception ex1 = Record.Exception(() => Data.Check([0x01,0x00,0x00,0x01,0x00,0x00,0x77,0x88]));
        Exception ex2 = Record.Exception(() => Data.Check([0x01,0x00,0x00,0x00,0x00,0x00,0x77,0x88]));
        Exception ex3 = Record.Exception(() => Data.Check([0x04,0x00,0x00,0x77,0x00,0x77,0x88]));
        Exception ex4 = Record.Exception(() => Data.Check([0xFF,0x00,0x00,0x77]));
        Exception ex5 = Record.Exception(() => Data.Check([0xFE,0x00,0x00,0x77,0x00,0x77,0x88]));

        Assert.NotNull(ex);
        Assert.NotNull(ex1);
        Assert.NotNull(ex2);
        Assert.NotNull(ex3);
        Assert.NotNull(ex4);
        Assert.NotNull(ex5);
    }
    [Fact]
    public void InputFSMTest1()//Giving message in states it can't be sent to get exception
    {
        Exception ex = Record.Exception(() =>  FSM.InputAutomat(Code.Msg));
        Exception ex1 = Record.Exception(() => FSM.InputAutomat(Code.Join));
        CurrentState.SetState(State.Open);
        Exception ex2 = Record.Exception(() => FSM.InputAutomat(Code.Auth));

        Assert.NotNull(ex);
        Assert.NotNull(ex1);
        Assert.NotNull(ex2);
    }
    [Fact]
    public void OutputFSMTest1()//Giving message in states it can't be sent to get exception
    {
        CurrentState.SetState(State.Open);
        int result = FSM.ReadAutomat(Code.Reply);
        int result1 = FSM.ReadAutomat(Code.Bye);
        CurrentState.SetState(State.Open);
        int result2 = FSM.ReadAutomat(Code.NotReply);

        Assert.Equal(ReturnCode.Error, result);
        Assert.Equal(ReturnCode.Error, result1);
        Assert.Equal(ReturnCode.Error, result2);
    }
}