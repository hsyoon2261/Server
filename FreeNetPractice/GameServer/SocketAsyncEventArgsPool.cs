using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace GameServer
{
    public class SocketAsyncEventArgsPool
    {
        private Stack<SocketAsyncEventArgs> m_pool;
        //Initializes the object pool to specified size
        //The "capacity" parameter is the maximum number of
        //SocketAsyncEvnetArgs objects the pool can hold

        public SocketAsyncEventArgsPool(int capacity)
        {
            m_pool = new Stack<SocketAsyncEventArgs>(capacity);
        }
        
        //Add a SocketAsyncEventArg instance to the pool
        //item parameter is the SocketAsyncEventArgs instance in pool
        //adding item in pool..
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentException("Items added to SocketAsyncEventArgsPool cannot be null");
            }

            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }
        
        //pop and return object
        public SocketAsyncEventArgs Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }

        public int Count
        {
            get { return m_pool.Count; }
        }
    }
}