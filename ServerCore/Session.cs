using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    abstract class Session
    {
        private Socket _socket;
        private int _disconnected = 0;

        private object _lock = new object();
        private Queue<byte[]> _sendQueue = new Queue<byte[]>();

        List<ArraySegment<byte>> _pendinglist = new List<ArraySegment<byte>>();


        //_sendArgs를 멤버변수로 들고있자.. 
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            _socket = socket;
            //완료되었을때 completed 로 이벤트를 완료시켜준다.(Accept, Receive, Send)
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            //usertoken 은 object 타입 아무거나 넣어줘도 됨. 식별자로 구분하고 싶거나 연동하고 싶을때 사용한다.
            //Console.WriteLine(recvArgs.UserToken);
            // recvArgs.UserToken = this;
            // Console.WriteLine(recvArgs.UserToken);
            //listener 에서 AccecpSocket 이라면 Receive 단계에서는 Setbuffer
            //offset 값으로 시작위치, 세션이 buffer를 나눠서 사용할때는 쪼개서 사용하라고.
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);
            //TODO Setbuffer style 3개 찾아보기. 
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            //Onsendcompleted가 registersend를 통해서 호출이 되었으면 Send(lock)->registersend-> 이기 때문에 lock이 필요없겠지만
            //만약 EventHandler를 통해 콜백방식으로 다른 스레드에서 튀어나와서 OnsendCompleted가 실행되면 멀티스레드 동시호출 시 문제가 될수 있으므로 락이 필요하다
            //애초에 lock리 필요한 이유 :OnSendcompleted의 param인 SocketAsyncEventArgs _sendArgs 가 공유자원이기때문.
            
            
            
            RegisterRecv(); //이걸 init에서 시행하지 않고, start라던가 따로 빼서 관리해도 상관없다.
            //byte[] recvBuff = new byte[1024];
            //int recvBytes = clientSocket.Receive(recvBuff);
            // <<vs>> socket.ReceiveAsync or SocketAsyncEventArgs.Completed
            //blocking receive byte 랑 비교.
        }


        public void Send(byte[] sendBuff)
        {
            lock (_lock) //멀티스레드 환경에서 누군가가 동시다발적으로 send를 호출할 수 있으므로 lock이 필요하다. 
            {
                _sendQueue.Enqueue(sendBuff);
                //if (_sendpending == false) // 내가 1빠로 pending을 호출했기 때문에(pending false) 내가 지금 전송까지 할 수 있다고 하면은 RegisterSend를 맞바로 해줄거고, 그게 아니면 Queue에 넣고 끝낸다.
                if(_pendinglist.Count==0) //대기중인애가 한명도 없다면
                    RegisterSend();
            }


            //send가 들어오면 asyncevent연동 해주고
            //걔를 registersend해줄거고, registersend가 완료되면 onsendcompleted가 실행되는 구조가 있는데. 동시 다발적으로(모든 클라이언트들에게)
            //메시지를 보낼려면 방법을 개선해야한다. 따라서 SocketAsyncEventArgs를 위에 멤버변수로 올린다. 
            //매번보내는게 아니라 큐에 쌓아서 한번에 보내도록 하자. 
            //결국 registersend(sendAsync)를 1000개 하는게 아니라, 큐에 1000개를 쌓고 registersend를 한번만 하고 
            // 다시 registersend를 예약하는 그런 방식을 하자는거임. (좀더 이해필요)
            // TODO Session#2 part restudy 
            //_sendArgs.SetBuffer(sendBuff,0,sendBuff.Length); //**code** Queue를 사용하지 않는 구조 (큐 사용 예제랑 비교)
            //_socket.Send(sendBuff); //이것은 동기 방식(비동기 방식과 비교)

            //RegisterSend();
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
            //TODO  _sendpending = true; byte[] buff = _sendQueue.Dequeue(); List 안쓰고 이부분 보고싶으면 세션#2까지 내용으로 하면된다. 나중에 한번 더 정리하자. 일단은 강의대로 따라갑니다._sendArgs.SetBuffer(buff,0,buff.Length);
            // sendAsync가 끝나가지고, OnsendCompleted 가 완료되기 전까지는 보내지 않고, 그냥 큐 에다가만 쌓아뒀다가
            // 작업이 완료가 되었으면 다시 한번 돌아와서 나머지 큐를 비우는 방식으로 진행. .
            
            while (_sendQueue.Count>0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendinglist.Add(new ArraySegment<byte>(buff,0,buff.Length));
            }

            _sendArgs.BufferList = _pendinglist;
            
            bool pending = _socket.SendAsync(_sendArgs); //sendAsync = 보내기 list처리로 sendAsync 한방으로 한번에 보낼수 있게 만들어 본것이다. 
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        //Onsendcompleted 진입했다 => 이벤트처리되었다(완료되었다) 니까 비워줘야지.
                        _sendArgs.BufferList = null; //Bufferlist가 굳이 pendinglist를 가지고 있을 이유가 없으니까.
                        _pendinglist.Clear();

                        Console.WriteLine($"Trans bytes: {_sendArgs.BytesTransferred}");
                        
                        if (_sendQueue.Count > 0)
                            RegisterSend();

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

        }

        #endregion

        #region Receive Network

        void RegisterRecv()
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
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
                    Console.WriteLine($"[From Client] : {recvData}");
                    //했으니 다시 대기하세요. (초기화는 위에서)
                    RegisterRecv();
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

/*
 * this is MSDN SendAsync(SocketAsyncEventArgs)
  https://docs.microsoft.com/ko-kr/dotnet/api/system.net.sockets.socket.sendasync?view=net-6.0#system-net-sockets-socket-sendasync(system-net-sockets-socketasynceventargs)
 * SendAsync(SocketAsyncEventArgs)
*/