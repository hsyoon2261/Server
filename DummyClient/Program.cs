using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
    // public class hssoc : Socket
    // {
    //     public string _userId;
    //     public hssoc(Socket mine) : base(mine.AddressFamily,mine.SocketType,mine.ProtocolType)
    //     {
    //         
    //     }
    // }

    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                string host = Dns.GetHostName();
                IPHostEntry ipHost = Dns.GetHostEntry(host);
                IPAddress ipAddr = ipHost.AddressList[1];
                IPEndPoint endPoint = new IPEndPoint(ipAddr, 7888);

                //socket setting
                // Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // hssoc test = new hssoc(socket);
                // test._userId = "completo";
                //TODO 만들어보기 
                try
                {
                    //connect : blocking 함수 => 못받으면 계속 대기
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                    //send (reverse to server)
                    for (int i = 0; i < 5; i++)
                    {
                        byte[] sendBuff = Encoding.UTF8.GetBytes($"{i} World");
                        int sendBytes = socket.Send(sendBuff);
                        Thread.Sleep(500);
                    }


                    //receive
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"From Server : {recvData}");
                    Thread.Sleep(50000);
                    //exit
                    Console.WriteLine("연결을 종료합니다.");
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(100);
            }
            //DNS
        }
    }
}