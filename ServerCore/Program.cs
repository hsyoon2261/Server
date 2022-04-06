using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ServerCore
{
    

    class Program
    {
        private static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
            

            //address family, socket type, protocol type
            try
            {
                _listener.Init(endPoint);
                while (true)
                {
                    Console.WriteLine("listening...");
                    //accept = blocking함수라서 모든 실행이 여기서 멈추고
                    //client 입장 안하면 아래 단계 가지도 않을거고
                    //client 접속하면 자동으로 완료되면서 다음으로 넘어감.
                    Socket clientSocket = _listener.Accept();

                    //listening from client
                    byte[] recvBuff = new byte[1024];
                    //receive byte (blocking)
                    int recvBytes = clientSocket.Receive(recvBuff);
                    //encoding byte to string
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"From Client = {recvData}");

                    //send
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Server");
                    //blocking
                    clientSocket.Send(sendBuff);
                    //blocking 함수들은 non-blocking(비동기)로 바꿔줘야한다.

                    //kick
                    clientSocket.Shutdown(SocketShutdown.Both);
                    Console.WriteLine("Bye..");
                    clientSocket.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }
    }
}