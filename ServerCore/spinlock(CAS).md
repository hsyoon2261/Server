using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ServerCore
{
//spinlock lock 이 풀릴 때 까지 기다림.
class SpinLock
{
//volatile = 가시성 보장
volatile int _locked = 0;

        public void Acquire()
        {
            while (true)
            {
                //int original = Interlocked.Exchange(ref _locked, 1);
                //original은 스택에 있는, 따라서 경합하지 않는 하나의 스레드에서만
                //사용하는 값이라서 그냥 읽어도 문제가 없다. (original은 int형 이므로)
                //if (original == 0)
                //break;
                // CAS = Compare and Swap
                int expected = 0;
                int desired = 1;
                if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                    break;
            }
        }

        public void Release()
        {
            _locked = 0;
        }
    }

    class Program
    {
        private static int _num = 0;
        private static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();
            Task.WaitAll(t1, t2);

            Console.WriteLine("Task complete " + _num);
        }
    }
}