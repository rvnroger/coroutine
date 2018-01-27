using System;
using System.Collections;

namespace Coroutine
{
    public sealed class CoroutineResult : IYieldInstruction
    {
        public CoroutineResult(object data) { Value = data; }

        public object Value { get; }

        public IEnumerator GetEnumerator() { yield break; }
    }
}
