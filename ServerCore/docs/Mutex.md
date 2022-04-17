```c#
    class Program1
    {
        // 잠근 횟수도 카운팅 가능하고, lock을 건 Thread Id 도 보유하고 잇음. 
        private static int _num = 0;
        private static Mutex _lock = new Mutex();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.WaitOne();
                _num++;
                _lock.ReleaseMutex();
            }
            for (int i = 0; i < 10000; i++)
            {
                _lock.WaitOne();
                _num--;
                _lock.ReleaseMutex();
            }
        }
    }
```

- Mutex = 커널 동기화 객체 커널단에서 움직이는 지원해주는 기능이다. 
- SpinLock = 메서드 단위에서 해결(속도면에서 유리)
- 