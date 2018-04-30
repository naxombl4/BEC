using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BEC
{
    class TelnetEngine
    {
        public string login, password;
        enum Verbs
        {
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            SNEG = 250,
            ENDSNEG = 240,
            IAC = 255
        }

        enum SubCommand
        {
            TermType = 24,
            TermSpeed = 32,
            XDisplay = 35,
            EnvirOption = 39,
            WinSize = 31,
            Echo = 1,
            SGA = 3,
            RFC = 33,
            Status = 5
        }

        TcpClient telnet;

        public string ReadStream()
        {
            if (!telnet.Connected) return null;
            StringBuilder sb = new StringBuilder();
            do
            {
                ParseTelnet(sb);
                Thread.Sleep(100);
            }
            while (telnet.Available > 0);
            return sb.ToString();
        }

        void ParseTelnet(StringBuilder sb)
        {
            while (telnet.Available > 0)
            {
                int input = telnet.GetStream().ReadByte();
                System.Collections.ArrayList buffer = new System.Collections.ArrayList();
                byte[] bytestosend;
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC: //interpret as command
                        int inputverb = telnet.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                                int inputoption = telnet.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                switch (inputoption)
                                {
                                    case (int)SubCommand.TermType:
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.WONT);
                                        buffer.Add((byte)SubCommand.TermType);
                                        bytestosend = buffer.OfType<byte>().ToArray();
                                        telnet.GetStream().Write(bytestosend, 0, bytestosend.Length);
                                        buffer.Clear();
                                        break;
                                    case (int)SubCommand.TermSpeed:
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.WONT);
                                        buffer.Add((byte)SubCommand.TermSpeed);
                                        bytestosend = buffer.OfType<byte>().ToArray();
                                        telnet.GetStream().Write(bytestosend, 0, bytestosend.Length);
                                        buffer.Clear();
                                        break;
                                    case (int)SubCommand.XDisplay:
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.WONT);
                                        buffer.Add((byte)SubCommand.XDisplay);
                                        bytestosend = buffer.OfType<byte>().ToArray();
                                        telnet.GetStream().Write(bytestosend, 0, bytestosend.Length);
                                        buffer.Clear();
                                        break;
                                    case (int)SubCommand.EnvirOption:
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.WONT);
                                        buffer.Add((byte)SubCommand.EnvirOption);
                                        bytestosend = buffer.OfType<byte>().ToArray();
                                        telnet.GetStream().Write(bytestosend, 0, bytestosend.Length);
                                        buffer.Clear();
                                        break;
                                    case (int)SubCommand.Echo:
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.WONT);
                                        buffer.Add((byte)SubCommand.Echo);
                                        bytestosend = buffer.OfType<byte>().ToArray();
                                        telnet.GetStream().Write(bytestosend, 0, bytestosend.Length);
                                        buffer.Clear();
                                        break;
                                    case (int)SubCommand.WinSize:
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.WILL);
                                        buffer.Add((byte)SubCommand.WinSize);
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.SNEG);
                                        buffer.Add((byte)SubCommand.WinSize);
                                        buffer.Add((byte)0);
                                        buffer.Add((byte)179);
                                        buffer.Add((byte)0);
                                        buffer.Add((byte)54);
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.ENDSNEG);
                                        bytestosend = buffer.OfType<byte>().ToArray();
                                        telnet.GetStream().Write(bytestosend, 0, bytestosend.Length);
                                        buffer.Clear();
                                        break;
                                    case (int)SubCommand.RFC:
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.WILL);
                                        buffer.Add((byte)SubCommand.RFC);
                                        bytestosend = buffer.OfType<byte>().ToArray();
                                        telnet.GetStream().Write(bytestosend, 0, bytestosend.Length);
                                        buffer.Clear();
                                        break;
                                }
                                break;
                            case (int)Verbs.WILL:
                                inputoption = telnet.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                switch (inputoption)
                                {
                                    case (int)SubCommand.SGA:
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.DO);
                                        buffer.Add((byte)SubCommand.SGA);
                                        bytestosend = buffer.OfType<byte>().ToArray();
                                        telnet.GetStream().Write(bytestosend, 0, bytestosend.Length);
                                        buffer.Clear();
                                        break;
                                    case (int)SubCommand.Status:
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.DONT);
                                        buffer.Add((byte)SubCommand.Status);
                                        bytestosend = buffer.OfType<byte>().ToArray();
                                        telnet.GetStream().Write(bytestosend, 0, bytestosend.Length);
                                        buffer.Clear();
                                        break;
                                    case (int)SubCommand.Echo:
                                        buffer.Add((byte)Verbs.IAC);
                                        buffer.Add((byte)Verbs.DO);
                                        buffer.Add((byte)SubCommand.Echo);
                                        bytestosend = buffer.OfType<byte>().ToArray();
                                        telnet.GetStream().Write(bytestosend, 0, bytestosend.Length);
                                        buffer.Clear();
                                        break;
                                }
                                break;
                            case (int)Verbs.SNEG:
                                inputoption = telnet.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                if (inputoption == (int)SubCommand.RFC)
                                {
                                    int status = telnet.GetStream().ReadByte();
                                    if (status == 1)
                                    {
                                        if (telnet.Available > 0) break;
                                        do
                                        {
                                            Thread.Sleep(100);
                                        }
                                        while (telnet.Available <= 0);
                                    }
                                }

                                break;
                            case (int)Verbs.ENDSNEG:
                                inputoption = telnet.GetStream().ReadByte();
                                break;

                        }
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }

        public void SendCMD(string cmd)
        {
            //cmd += "\n";
            NetworkStream netStream = telnet.GetStream();
            if (netStream.CanWrite)
            {
                Byte[] sendBytes = Encoding.ASCII.GetBytes(cmd + "\r");
                netStream.Write(sendBytes, 0, sendBytes.Length);
            }
            Thread.Sleep(1000);
        }

        public void SendAbort()
        {
            NetworkStream netStream = telnet.GetStream();
            if (netStream.CanWrite)
                netStream.WriteByte(byte.Parse("3"));
            Thread.Sleep(1000);
        }

        public string CreateConnection(string ip)
        {
            try
            {
                string temp;
                string state = "";
                telnet = new TcpClient();
                telnet.Connect(ip, 23);
                telnet.ReceiveBufferSize = 65536;
                temp = ReadStream();
                while (temp.IndexOf("ame:") == -1 && temp.IndexOf("ogin:") == -1)
                {
                    Thread.Sleep(100);
                    temp = ReadStream();
                }
                SendCMD(login);
                temp = ReadStream();
                while (temp.IndexOf("ord:") == -1)
                {
                    Thread.Sleep(100);
                    temp = ReadStream();
                }
                SendCMD(password);
                temp = "";

                //Читаем до получения приглашающего символа, либо повторного запроса на авторизацию
                do
                {
                    temp = ReadStream();
                    Thread.Sleep(100);
                }
                while (temp != "");

                if (temp.IndexOf("ame:") != -1 || temp.IndexOf("ogin:") != -1)
                {
                    state = "Login incorrect";
                    CloseConnection();
                    return state;
                }

                if (temp.IndexOf('#') == -1 || temp.IndexOf('>') == -1)
                {
                    state = "Success";
                    return state;
                }

                return state;
            }

            catch (SocketException)
            {
                return "Timeout";
            }
        }

        public void CloseConnection()
        {
            telnet.Close();
        }
    }
}
