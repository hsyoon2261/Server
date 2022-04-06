using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
            
            //socket setting
            Socket socket = new Socket(endPoint.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
            try
            {
                //connect : blocking 함수 => 못받으면 계속 대기
                socket.Connect(endPoint);
                Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                //send (reverse to server)
                byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World");
                int sendBytes = socket.Send(sendBuff);

                //receive
                byte[] recvBuff = new byte[1024];
                int recvBytes = socket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine($"From Server : {recvData}");

                //exit
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            

        }
    }
}