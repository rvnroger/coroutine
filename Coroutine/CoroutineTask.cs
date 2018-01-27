using System;
using System.Collections;
using System.Collections.Generic;

namespace Coroutine
{
    public class CoroutineTask : IYieldInstruction
    {
        protected bool isCompleted;
        protected Exception exception;
        protected object result;

        protected IEnumerator taskEnumerator;
        protected IEnumerator routineTask;
        protected Action asyncTask;

        public CoroutineTask(IEnumerator routineTask) : this()
        {
            taskEnumerator = new RoutineTaskEnumerator(this);
            this.routineTask = routineTask;
        }

        public CoroutineTask(CoroutineDelegate asyncTask) : this()
        {
            taskEnumerator = new AsyncTaskEnumerator(this);
            this.asyncTask = () =>
            {
                asyncTask(
                    () => { TaskEnd(); },
                    (ex) => { exception = ex; TaskEnd(); }
                );
            };
        }

        public bool IsCompleted { get { return isCompleted; } }
        public Exception Exception { get { return exception; } }
        public object Result { get { return result; } }

        public static CoroutineTask FromRoutine(IEnumerator routine)
        {
            return new CoroutineTask(routine);
        }

        public static CoroutineTask<T> FromRoutine<T>(IEnumerator<T> routine)
        {
            return new CoroutineTask<T>(routine);
        }

        public static CoroutineTask FromAsync(Func<AsyncCallback, object, IAsyncResult> begin, Action<IAsyncResult> end, object state = null)
        {
            return new CoroutineTask((callback, exception) =>
            {
                begin((iar) =>
                {
                    try { end(iar); callback(); }
                    catch (Exception ex) { exception(ex); }
                },
                state);
            });
        }

        public static CoroutineTask<T> FromAsync<T>(Func<AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end, object state = null)
        {
            return new CoroutineTask<T>((callback, exception) =>
            {
                begin((iar) =>
                {
                    T result = default(T);
                    try { result = end(iar); callback(result); }
                    catch (Exception ex) { exception(ex); }
                },
                state);
            });
        }

        public IEnumerator GetEnumerator()
        {
            return taskEnumerator;
        }

        protected CoroutineTask()
        {
            isCompleted = false;
        }

        protected void TaskEnd()
        {
            asyncTask = null;
            routineTask = null;

            isCompleted = true;
        }

        void RunAsyncTask()
        {
            try
            {
                asyncTask();
                asyncTask = null;
            }
            catch (Exception ex)
            {
                exception = ex;
                TaskEnd();
            }
        }

        void RunRoutineTask()
        {
            try
            {
                if (!routineTask.MoveNext())
                {
                    result = routineTask.Current as CoroutineResult;
                    TaskEnd();
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                TaskEnd();
            }
        }

        protected class RoutineTaskEnumerator : IEnumerator
        {
            CoroutineTask task;

            internal RoutineTaskEnumerator(CoroutineTask task)
            {
                this.task = task;
            }

            public object Current
            {
                get { return task.routineTask.Current; }
            }

            public bool MoveNext()
            {
                if (task.routineTask != null)
                {
                    task.RunRoutineTask();
                }
                return !task.isCompleted;
            }

            public void Reset() { }
        }

        protected class AsyncTaskEnumerator : IEnumerator
        {
            CoroutineTask task;

            internal AsyncTaskEnumerator(CoroutineTask task)
            {
                this.task = task;
            }

            public object Current
            {
                get { return task.result; }
            }

            public bool MoveNext()
            {
                if (task.routineTask != null)
                {
                    task.RunAsyncTask();
                }
                return !task.isCompleted;
            }

            public void Reset() { }
        }
    }

    public class CoroutineTask<T> : CoroutineTask
    {
        public CoroutineTask(IEnumerator<T> routineTask) : base()
        {
            taskEnumerator = new RoutineTaskEnumerator(this);
            this.routineTask = routineTask;
        }

        public CoroutineTask(CoroutineDelegate<T> asyncTask) : base()
        {
            taskEnumerator = new AsyncTaskEnumerator(this);
            this.asyncTask = () =>
            {
                asyncTask(
                    (res) => { result = res; TaskEnd(); },
                    (ex) => { exception = ex; TaskEnd(); }
                );
            };
        }

        public new T Result { get { return (T)result; } }
    }
}
