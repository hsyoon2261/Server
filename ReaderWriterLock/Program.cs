using System;
using System.Threading.Tasks;

namespace ReaderWriterLock
{
    class Program
    {
        private static volatile int count = 0;
        private static volatile int count2 = 0;

        private static volatile int count3 = 0;

        private static Lock _lock = new Lock();

        static void Main(string[] args)
        {
            Task t1 = new Task(delegate()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    count++;
                    _lock.WriteUnlock();
                }
            });
            Task t2 = new Task(delegate()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    count--;
                    _lock.WriteUnlock();
                }
            });
            Task t3 = new Task(delegate()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    count2++;
                    _lock.WriteUnlock();
                }
            });
            Task t4 = new Task(delegate()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    count2--;
                    _lock.WriteUnlock();
                }
            });
            Task t5 = new Task(delegate()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.ReadLock();
                    count3++;
                    _lock.ReadUnlock();
                }
            });
            Task t6 = new Task(delegate()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.ReadLock();
                    count3++;
                    _lock.ReadUnlock();
                }
            });
            
            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            t5.Start();
            t6.Start();
            
            Task.WaitAll(t1, t2,t3,t4,t5,t6);
            Console.WriteLine(count);
            Console.WriteLine(count2);
            Console.WriteLine(count3);
        }
    }
}