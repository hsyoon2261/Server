using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public class Session
    {
        private Socket _socket;
        private int _disconnected = 0;
        //_sendArgs를 멤버변수로 들고있자.. 
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();


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
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            
            
            //byte[] recvBuff = new byte[1024];
            //int recvBytes = clientSocket.Receive(recvBuff);
            // <<vs>> socket.ReceiveAsync or SocketAsyncEventArgs.Completed
            //blocking receive byte 랑 비교.
            
            RegisterRecv(recvArgs); //이걸 init에서 시행하지 않고, start라던가 따로 빼서 관리해도 상관없다. 
        }

        

        public void Send(byte[] sendBuff)
        {
            //send가 들어오면 asyncevent연동 해주고
            //걔를 registersend해줄거고, registersend가 완료되면 onsendcompleted가 실행되는 구조가 있는데. 동시 다발적으로(모든 클라이언트들에게)
            //메시지를 보낼려면 방법을 개선해야한다. 따라서 SocketAsyncEventArgs를 위에 멤버변수로 올린다. 
            // TODO Session#2 part restudy 
            _sendArgs.SetBuffer(sendBuff,0,sendBuff.Length);
            //_socket.Send(sendBuff); //이것은 동기 방식(비동기 방식과 비교)
            
            RegisterSend();
        }

        public void Disconnect()
        {
            //multithead 환경을 위해 Interlocked를 사용하자.
            // 이걸로 채팅방, 로그인등 상태관리를 하자. 
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            
        }
        #region Seding Network
        //send 의 경우 리시브처럼 하는 시점이 정해져 있지 않다. 
        //receive 같은 경우 우리가 초반에 Start를 할때 한번만 RegisterRecv을 예약을 해둔 다음에 (버퍼도 설정해둠)
        // 실제로 클라이언트쪽에서 메시지를 전송을 하면 그제서야 RegisterRecv가 완료가 된 다음에
        // OnRecvCompleted가 자동으로 호출이 되고, 그러면 우리는 다시 예약을 하기 위해 try문 마지막에 RegisterRecv를 다시한번 호출하는 그런 구조인데
        // 따라서 start 시점이 아닌, send 시점에다가 registersend를 하게끔 만들어 줘야한다. .

        void RegisterSend()
        {
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 &&)
            {
                try
                {
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnSendCompleted Failed{e.ToString()}");
                }

            }
            else
            {
                Disconnect();
            }
        }
        
        #endregion

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
                Disconnect();
            }
        }
        //OnRecvComplted는 pending 이 false 면(바로 성공했으면) 바로 실행이 되는거고
        //그게 아니면 기다렸다가 이벤트(recvargs.completed) 호출 시 실행이 되는 것.(콜백으로 실행)
        #endregion

    }
}
