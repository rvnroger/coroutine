using System;
using Coroutine.Internal;

namespace Coroutine
{
    public class CoroutineService
    {
        ICoroutineThreadPool threadPool;

        public CoroutineService(int threadCount = 0)
        {
            threadPool = new CoroutineThreadPool(threadCount);
        }

        public void Start()
        {
            threadPool.ThreadStart();
        }

        public void Stop()
        {
            threadPool.ThreadStop();
        }

        public ICoroutineDispatcher GetDispatcher()
        {
            return threadPool.GetDispatcher();
        }
    }
}
