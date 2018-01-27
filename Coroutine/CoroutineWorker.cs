#define USE_QUEUE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Coroutine.Internal
{
    internal class CoroutineWorker : ICoroutineWorker
    {
#if USE_QUEUE
        static readonly int coroutineQueueCapacity = 0x10000;

        Queue<CoroutineItem> coroutinesA;
        Queue<CoroutineItem> coroutinesB;
        Queue<CoroutineItem> curCoroutines;
        Queue<CoroutineItem> nextCoroutines;
#else
        LinkedList<CoroutineItem> coroutinesA;
        LinkedList<CoroutineItem> coroutinesB;
        LinkedList<CoroutineItem> curCoroutines;
        LinkedList<CoroutineItem> nextCoroutines;
#endif

        volatile int coroutineCount;
        ReaderWriterLockSlim coroutineLock;
        CoroutineDispatcher coroutineDispatcher;

        Thread workerThread;

        public CoroutineWorker(Thread thread)
        {
#if USE_QUEUE
            coroutinesA = new Queue<CoroutineItem>(coroutineQueueCapacity);
            coroutinesB = new Queue<CoroutineItem>(coroutineQueueCapacity);
#else
            coroutinesA = new LinkedList<CoroutineItem>();
            coroutinesB = new LinkedList<CoroutineItem>();
#endif

            curCoroutines = coroutinesA;
            nextCoroutines = coroutinesB;

            coroutineCount = 0;
            coroutineLock = new ReaderWriterLockSlim();
            coroutineDispatcher = new CoroutineDispatcher(this);

            workerThread = thread;
        }

        public ICoroutineDispatcher Dispatcher
        {
            get { return coroutineDispatcher; }
        }

        public void StartCoroutine(IEnumerator routine)
        {
            var item = new CoroutineItem { Routine = routine, Next = null };
            AddCoroutine(item, (workerThread != Thread.CurrentThread));
        }

        public void UpdateOne()
        {
            if (coroutineCount == 0)
            {
                return;
            }

            var item = NextCoroutine();
            if (item != null)
            {
                UpdateCoroutine(item);
            }

            SwapCoroutines();
        }

        public void UpdateAll()
        {
            if (coroutineCount == 0)
            {
                return;
            }

            var item = NextCoroutine();
            while (item != null)
            {
                UpdateCoroutine(item);
                item = NextCoroutine();
            }

            SwapCoroutines();
        }

        private void UpdateCoroutine(CoroutineItem item)
        {
            if (item.Routine.MoveNext())
            {
                if (item.Routine.Current is IYieldInstruction current)
                {
                    if (!(current is CoroutineResult))
                    {
                        item = new CoroutineItem { Routine = current.GetEnumerator(), Next = item };
                    }
                }
                AddCoroutine(item);
            }
            else
            {
                if (item.Next != null)
                {
                    AddCoroutine(item.Next);
                }
            }
        }

        private void AddCoroutine(CoroutineItem item, bool writeLock = false)
        {
#if USE_QUEUE
            if (writeLock)
            {
                coroutineLock.EnterWriteLock();
                nextCoroutines.Enqueue(item);
                coroutineLock.ExitWriteLock();
            }
            else
            {
                coroutineLock.EnterUpgradeableReadLock();
                nextCoroutines.Enqueue(item);
                coroutineLock.ExitUpgradeableReadLock();
            }
#else
            writeLock = true;
            if (writeLock)
            {
                coroutineLock.EnterWriteLock();
                nextCoroutines.AddLast(item);
                coroutineLock.ExitWriteLock();
            }
            else
            {
                coroutineLock.EnterUpgradeableReadLock();
                nextCoroutines.AddLast(item);
                coroutineLock.ExitUpgradeableReadLock();
            }
#endif

            Interlocked.Increment(ref coroutineCount);
        }

        private CoroutineItem NextCoroutine()
        {
            CoroutineItem item = null;

            if (curCoroutines.Count > 0)
            {
#if USE_QUEUE
                item = curCoroutines.Dequeue();
#else
                item = curCoroutines.First.Value;
                curCoroutines.RemoveFirst();
#endif
            }

            if (item != null)
            {
                Interlocked.Decrement(ref coroutineCount);
            }

            return item;
        }

        private void SwapCoroutines()
        {
            if (curCoroutines.Count == 0)
            {
                coroutineLock.EnterUpgradeableReadLock();
                curCoroutines = Interlocked.Exchange(ref nextCoroutines, curCoroutines);
                coroutineLock.ExitUpgradeableReadLock();
            }
        }

        class CoroutineItem
        {
            public IEnumerator Routine { get; set; }
            public CoroutineItem Next { get; set; }
        }
    }
}
