using System;
using System.Collections;

namespace Coroutine.Internal
{
    internal class CoroutineDispatcher : ICoroutineDispatcher
    {
        ICoroutineWorker coroutineWorker;

        public CoroutineDispatcher(ICoroutineWorker worker)
        {
            coroutineWorker = worker;
        }

        public void Invoke(IEnumerator routine)
        {
            coroutineWorker.StartCoroutine(routine);
        }
    }
}
