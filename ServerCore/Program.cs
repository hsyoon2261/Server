using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ServerCore
{
    class GameSeesion : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
        }
        public override void OnRecv(ArraySegment<byte> buffer)
        {
        }
        public override void OnSend(int numOfBytes)
        {
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
        }
    }
    class Program
    {
        private static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {

                GameSeesion session = new GameSeesion();
                session.Start(clientSocket);
                byte[] sendBuff = Encoding.UTF8.GetBytes("Hello first chat server !");
                session.Send(sendBuff);
                
                Thread.Sleep(60000);
                session.Disconnect();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            //DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            Console.WriteLine(ipAddr.ToString());
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7888);


            //address family, socket type, protocol type
            //start listening from client
            _listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("listening...");
            
            while (true)
            {
                //accept = blocking함수라서 모든 실행이 여기서 멈추고(따라서 사용안함)
                //client 입장 안하면 아래 단계 가지도 않을거고
                //client 접속하면 자동으로 완료되면서 다음으로 넘어감.
                // Socket clientSocket = _listener.Accept();
            }
        }
    }
}
