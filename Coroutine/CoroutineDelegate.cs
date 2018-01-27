using System;

namespace Coroutine
{
    public delegate void CoroutineDelegate(Action callback, Action<Exception> exception);
    public delegate void CoroutineDelegate<T>(Action<T> callback, Action<Exception> exception);
}
