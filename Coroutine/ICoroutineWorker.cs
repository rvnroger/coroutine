using System;
using System.Collections;

namespace Coroutine
{
    public interface ICoroutineWorker
    {
        void StartCoroutine(IEnumerator routine);

        void UpdateOne();
        void UpdateAll();

        ICoroutineDispatcher Dispatcher { get; }
    }
}
