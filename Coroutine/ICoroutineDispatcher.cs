using System;
using System.Collections;

namespace Coroutine
{
    public interface ICoroutineDispatcher
    {
        void Invoke(IEnumerator routine);
    }
}
