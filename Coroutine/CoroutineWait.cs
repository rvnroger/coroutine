using System;
using System.Collections;
using System.Diagnostics;

namespace Coroutine
{
    public sealed class WaitForSeconds : IYieldInstruction
    {
        float seconds;

        public WaitForSeconds(float seconds)
        {
            this.seconds = seconds;
        }

        public IEnumerator GetEnumerator()
        {
            return new WaitEnumerator(seconds);
        }

        class WaitEnumerator : IEnumerator
        {
            long expireElapsed;

            internal WaitEnumerator(float seconds)
            {
                expireElapsed = Watcher.ElapsedMilliseconds + (long)(seconds * 1000);
            }

            public object Current { get; }

            public bool MoveNext()
            {
                if (Watcher.ElapsedMilliseconds >= expireElapsed)
                {
                    return false;
                }
                return true;
            }

            public void Reset() { }
        }

        static Stopwatch Watcher = Stopwatch.StartNew();
    }

    public sealed class WaitForEndOfFrame : IYieldInstruction
    {
        public IEnumerator GetEnumerator() { yield break; }
    }
}
