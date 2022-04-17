using System.ComponentModel.DataAnnotations;
using System.Threading;

//락-프리 프로그래밍
namespace ReaderWriterLock
{
    // 재귀적 락을 허용할지(No)
    // writelock acquire 한 상태에서 또다시 재귀적으로 같은 thread에서 acquire 할때 그것을 허용해 줄 것인지.
    // 스핀락 정책(5000번->yeild)
    public class Lock
    {
        private const int EMPTY_FLAG = 0x00000000;
        //음수부분

        private const int WRITE_MASK = 0x7FFF0000;
        private const int READ_MASK = 0x0000FFFF;

        private const int MAX_SPIN_COUNT = 5000;
        //int 는 32비트로 구성되어있다. 계산기 -프로그래머 항목 참고

        // [Unused(1)][WriteThreadId(15)][ReadCount(16)]
        private int _flag = EMPTY_FLAG;

        public void WriteLock()
        {
            while (true)
            {
                //아무도 writelock or readlock 을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다.
                int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    //락프리 프로그래밍 패턴
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                        return;

                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            for (int i = 0; i < MAX_SPIN_COUNT; i++)
            {
                int expected = (_flag & READ_MASK); // flag 에 readmask 연산 = writemask empty인지 체크하는거다 사실상
                //readmask 가 32중에 0~15 부분 읽는것 이므로 그냥 +1 해도 되는거임. 
                //아무도 WriteLock을 획득하고 잇지 않으면(expected와 _flag가 같다면 = _flag를 writelock이 안건드렸다면) ReadCount를 1 늘린다.
                if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                {
                    //Interlocked 로 read 행위 경합// 락프리 프로그래밍 패턴 
                    return;
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}