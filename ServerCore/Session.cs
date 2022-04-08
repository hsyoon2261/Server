using System;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public class Session
    {
        private Socket _socket;

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            //완료되었을때 completed 로 이벤트를 완료시켜준다.(Accept, Receive, Send)
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            //usertoken 은 object 타입 아무거나 넣어줘도 됨. 식별자로 구분하고 싶거나 연동하고 싶을때 사용한다.
            //Console.WriteLine(recvArgs.UserToken);
            // recvArgs.UserToken = this;
            // Console.WriteLine(recvArgs.UserToken);
            //listener 에서 AccecpSocket 이라면 Receive 단계에서는 Setbuffer
            //offset 값으로 시작위치, 세션이 buffer를 나눠서 사용할때는 쪼개서 사용하라고.
            recvArgs.SetBuffer(new byte[1024],0,1024);
            //TODO Setbuffer style 3개 찾아보기. 
            
            
            //byte[] recvBuff = new byte[1024];
            //int recvBytes = clientSocket.Receive(recvBuff);
            // <<vs>> socket.ReceiveAsync or SocketAsyncEventArgs.Completed
            //blocking receive byte 랑 비교.
            
            RegisterRecv(recvArgs); //이걸 init에서 시행하지 않고, start라던가 따로 빼서 관리해도 상관없다. 
        }

        public void Send(byte[] sendBuff)
        {
            _socket.Send(sendBuff);
        }

        public void Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            
        }

        #region Receive Network
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (pending==false)
                OnRecvCompleted(null, args); 
            //pending이 false인 경우 OnRecvCompleted 직접 호출
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //Console.WriteLine(args.UserToken);
                    //encoding byte to string
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"{args.UserToken} : {recvData}");
                    //했으니 다시 대기하세요. (초기화는 위에서)
                    RegisterRecv(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed{e.ToString()}");
                }


            }
            else
            {
                //TODO : Disconnect 같은거로 추방
            }
        }
        //OnRecvComplted는 pending 이 false 면(바로 성공했으면) 바로 실행이 되는거고
        //그게 아니면 기다렸다가 이벤트(recvargs.completed) 호출 시 실행이 되는 것.(콜백으로 실행)
        #endregion

    }
}
