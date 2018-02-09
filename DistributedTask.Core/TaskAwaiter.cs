using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;

namespace DistributedTask.Core
{
    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion
    {
        private readonly Task<TResult> _task;

        internal TaskAwaiter(Task<TResult> task)
        {
            _task = task;
        }

        public bool IsCompleted {
            get { return _task.IsCompleted; }
        }

        [SecuritySafeCritical]
        public void OnCompleted(Action continuation)
        {
            continuation();
        }

        [SecurityCritical]
        public void UnsafeOnCompleted(Action continuation)
        {
            continuation();
        }

        public TResult GetResult()
        {
            return _task.Result;
        }
    }
}
