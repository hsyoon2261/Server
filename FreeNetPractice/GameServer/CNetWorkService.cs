using System;
using System.Net.Sockets;

namespace GameServer
{
    public class CNetWorkService
    {
        // 클라이언트의 접속을 받악들이기 위한 객체
        CListener client_listener;

        //메시지 수신, 전송 시 필요한 객체(버퍼 풀)
        // SocketAsyncEventArgsPool는 MSDN 샘플 코드 클래스로 구현함.
        private SocketAsyncEventArgsPool receive_evnet_args_pool;
        private SocketAsyncEventArgsPool send_event_args_pool;

        /// <summary>
        /// 로직 스레드를 사용하려면 use_logicthread를 true로 설정한다.
        ///  -> 하나의 로직 스레드를 생성한다.
        ///  -> 메시지는 큐잉되어 싱글 스레드에서 처리된다.
        /// 
        /// 로직 스레드를 사용하지 않으려면 use_logicthread를 false로 설정한다.
        ///  -> 별도의 로직 스레드는 생성하지 않는다.
        ///  -> IO스레드에서 직접 메시지 처리를 담당하게 된다.
        /// </summary>
        /// <param name="use_logicthread">true=Create single logic thread. false=Not use any logic thread.</param>
        public CNetworkService(bool use_logicthread = false)
        {
            this.session_created_callback = null;
            this.usermanager = new CServerUserManager();

            if (use_logicthread)
            {
                this.logic_entry = new CLogicMessageEntry(this);
                this.logic_entry.start();
            }
        }


        public void initialize(int max_connections, int buffer_size)
        {
            //receive 버퍼만 할당한다.
            //send버퍼는 보낼 때 마다 할당을 하든 풀에서 가져오든 하기 때문에.
            int pre_alloc_count = 1;
            
            
            // 메시지 수신, 전송 시 닷넷 비동기 소켓에서 사용할 버퍼를 관리하는 객체
            BufferManager buffer_manager = new BufferManager(max_connections*buffer_size*pre_alloc_count, buffer_size);
            this.receive_evnet_args_pool = new SocketAsyncEventArgsPool(max_connections);
            this.send_event_args_pool = new SocketAsyncEventArgsPool(max_connections);
            
            
            buffer_manager.InitBuffer();
            
            //preallocate pool of SocketAsyncEventArgs objects 오브젝트 선할당
            SocketAsyncEventArgs args;

            for (int i = 0; i < max_connections; i++)
            {
                // 더이상 UserToken을 미리 생성해 놓지 않는다.
                // 다수의 클라이언트에서 접속 -> 메시지 송수신 -> 접속 해제를 반복할 경우 문제가 생김.
                // 일단 on_new_client에서 그때 그때 생성하도록 하고,
                // 소켓이 종료되면 null로 세팅하여 오류 발생시 확실히 드러날 수 있도록 코드를 변경한다.
                
                //receive pool
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    args = new SocketAsyncEventArgs();
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(receive_completed);
                    args.UserToken = null;
                    
                    this.receive_evnet_args_pool.Push(args);

                }
                
                //send pool
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    args = new SocketAsyncEventArgs();
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(send_completed);
                    args.UserToken = null;
                    
                    // send 버퍼는 보낼 때 설정한다. SetBuffer가 아닌 BufferList를 사용.
                    args.SetBuffer(null,0,0);
                    
                    // add SocketAsyncEvnetArg to the pool
                    this.send_event_args_pool.Push(args);
                }
            }
        }

        // 클라이언트의 접속이 이루어졌을 때 호출되는 델리게이트
        public delegate void SessionHandler(CUserToken token);

        public SessionHandler session_created_callback { get; set; }

        public void listen(string host, int port, int backlog)
        {
            CListener listener = new CListener();
            listener.callback_on_newclient += on_new_client;
            listener.start(host,port,backlog);
        }
        
        


    }
}