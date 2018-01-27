using System;

namespace Coroutine
{
    public interface ICoroutineThreadPool
    {
        ICoroutineDispatcher GetDispatcher();

        void ThreadStart();
        void ThreadStop();

        int ThreadCount { get; }
    }
}
