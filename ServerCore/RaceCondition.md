using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ServerCore
{
//경합조건 race condition
class Program
{
//공유변수 하나 두기
static int number = 0;

        static void Thread_1()
        {
            //atomic = 원자성
            //원자성을 보장하기 위해 interlocked를 써보자
            for (int i = 0; i < 100009; i++)
            {
                Interlocked.Increment(ref number);
            }
            //number++; 부분을
            //디 어셈블리로 보면 
            /*실제 과정을 의사코드로 나타내본다
             * int register = number;
             * register += 1;
             * number = register;
             * 이런느낌임. 이 과정속에서
             * 쓰레드간 경합 일어나면 값이 달라지는거임
             *
             */
        }
        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
                Interlocked.Decrement(ref number);
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();
            Task.WaitAll(t1, t2);
            Console.WriteLine(number);

        }
    }
}