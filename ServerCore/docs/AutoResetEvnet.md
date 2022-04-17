```c#
using System.Threading;

namespace ServerCore
{
    //AutoResetEvent
    //Manual Reset Event
    class Lock
    {
        //커널단에서 bool 이라고 생각하면 됨. 커널단으로 가서 요청을 한다? 뭔소리지. 
        private AutoResetEvent _available = new AutoResetEvent(true);
        public void Acquire()
        {
            _available.WaitOne(); // 입장시도
            //_available.Reset();// bool=> false로 바꿔준다.
        }

        public void Release()
        {
            _available.Set(); //bool 을 true로 다시 켜준다. 
        }
    }

    class Program1
    {
        private static int _num = 0;
        private static Lock _lock = new Lock();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
            for (int i = 0; i < 10000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }
        }
    }
}
```