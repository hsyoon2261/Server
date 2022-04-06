using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;

        public void Init(IPEndPoint endPoint)
        {
            _listenSocket = new Socket(endPoint.AddressFamily,SocketType.Stream,ProtocolType.Tcp);
            _listenSocket.Bind(endPoint);
            //backlog : 최대 대기 수 라이브 시 조절하면 되는 수치
            _listenSocket.Listen(10);
            
            //register
            SocketAsyncEventArgs args = new SocketAsyncEventArgs(); //한번 만들어 주면 계속 재사용이 가능하다.
            //Completed : EventHandler 이벤트 방식 ( 콜백방식으로 OnAccepCompleted를 호출)
            //콜백함수(OnAcceptComplted)를 매개변수로 넣어준다.
            //param in method : object , TEventArgs(SocketAsyncEventArgs)
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); 
            //init register
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            bool pending = _listenSocket.AcceptAsync(args); //비동기 방식 : 예약
            //AcceptAsync return type : boolean
            //AcceptAsync는 당장 완료한다는 보장이 없으니 일단 요청을 하긴 하되(등록) 
            //pending 을 check해서 pending이 false 라면
            // 바로 완료가 되었다는 말임.(펜딩 없이 완료)
            // 따라서 OnAccepCompleted를 바로 호출하면 된다. 
            if(pending==false)
                OnAcceptCompleted(null, args);
        }
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                //TODO
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }
            //처리가 끝났다면 registeraccept를 다시한번 던져주자
            // why 한번 process 끝났으니 다음 작업을 위해 등록을 해주는 것
            RegisterAccept(args);
        }
        public Socket Accept()
        {
            //blcoking 계열 함수 accept를 해결해야한다.
            //SocketAsyncEventArgs을 이용해 비동기로 해결하자.
             //비동기는 값이 없어도 리턴을 때려버릴 수 있기 때문에 주의하자. 
            return _listenSocket.Accept();
        }
    }
}

/*순서를 다시 정리한다
 * 1. 초기화를 하는 시점에서 등록 (Init Register)
 * 2. Client Connect Request
 * 3. Callback 방식으로 OnAcceptCompleted 가 된다.
 * 즉, Init Register => Do RegisterAccept with args(type:SocketAsyncEventArgs) 
 * 4. in RegisterAccept, if pending == false, (pending없이 바로 처리되었다?)
 * 5. then 직접 OnAcceptCompleted를 불러준다.
 * 6. if pending true, 바로호출되진 않음.
 * 7. 이후 args.Completed 가 되면, EventHandler 가 OnAcceptCompleted를 자동으로 호출
 * 
 * 
*/