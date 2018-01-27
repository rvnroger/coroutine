using System;
using System.Collections;
using System.Collections.Generic;

namespace Coroutine
{
    public static partial class CoroutineExtensions
    {
        public static CoroutineTask AsCoroutineTask(this IEnumerator enumerator)
        {
            return CoroutineTask.FromRoutine(enumerator);
        }

        public static CoroutineTask<T> AsCoroutineTask<T>(this IEnumerator<T> enumerator)
        {
            return CoroutineTask.FromRoutine(enumerator);
        }

        public static CoroutineTask AsCoroutineTask(this IEnumerable enumerable)
        {
            return CoroutineTask.FromRoutine(enumerable.GetEnumerator());
        }

        public static CoroutineTask<T> AsCoroutineTask<T>(this IEnumerable<T> enumerable)
        {
            return CoroutineTask.FromRoutine(enumerable.GetEnumerator());
        }
    }
}
