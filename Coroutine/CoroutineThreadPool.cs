using System;
using System.Collections.Generic;
using System.Threading;

namespace Coroutine.Internal
{
    internal class CoroutineThreadPool : ICoroutineThreadPool
    {
        List<CoroutineThread> threadList;
        int threadCount = 0;
        int nextDispatcherThread = 0;

        ManualResetEvent waitEvent;
        volatile int stop = 0;
        volatile int threadCounter = 0;

        public CoroutineThreadPool(int threadCount = 0)
        {
            this.threadCount = (threadCount > 0) ? threadCount : Environment.ProcessorCount;

            threadList = new List<CoroutineThread>();
            waitEvent = new ManualResetEvent(false);
        }

        public int ThreadCount
        {
            get { return threadCount; }
        }

        public ICoroutineDispatcher GetDispatcher()
        {
            var thread = threadList[nextDispatcherThread];

            int next = nextDispatcherThread + 1;
            nextDispatcherThread = (next < ThreadCount) ? next : 0;

            return thread.Worker.Dispatcher;
        }

        public void ThreadStart()
        {
            for (int i = 0; i < ThreadCount; ++i)
            {
                var thread = new Thread(new ParameterizedThreadStart(ThreadProc));
                thread.Name = "CoroutineThread_" + (i + 1);

                threadList.Add(new CoroutineThread
                {
                    Thread = thread,
                    Worker = new CoroutineWorker(thread),
                });
            }

            foreach (var thread in threadList)
            {
                thread.Thread.Start(thread);
            }
        }

        public void ThreadStop()
        {
            if (stop == 0)
            {
                stop = 1;
                waitEvent.WaitOne();
            }
        }

        private void ThreadProc(object param)
        {
            if (param is CoroutineThread thread)
            {
                Interlocked.Increment(ref threadCounter);

                while (stop == 0)
                {
                    thread.Worker.UpdateOne();
                }

                thread.Worker.UpdateAll();

                if (Interlocked.Decrement(ref threadCounter) == 0)
                {
                    waitEvent.Set();
                }
            }
        }

        class CoroutineThread
        {
            public Thread Thread { get; set; }
            public ICoroutineWorker Worker { get; set; }
        }
    }
}
