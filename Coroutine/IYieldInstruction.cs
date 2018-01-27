using System;
using System.Collections;

namespace Coroutine
{
    public interface IYieldInstruction
    {
        IEnumerator GetEnumerator();
    }
}
