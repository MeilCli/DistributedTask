using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using static DistributedTask.Core.Logger;

namespace DistributedTask.Core
{

    [AsyncMethodBuilder(typeof(DistributedAsyncTaskMethodBuilder<>))]
    public class DistributedAsyncTask<T>
    {

        private readonly Task<T> _task;
        private readonly TaskAwaiter<T> _taskAwaiter;

        public DistributedAsyncTask(Task<T> task)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
            _taskAwaiter = new TaskAwaiter<T>(_task);
        }

        public Task<T> AsTask()
        {
            return _task;
        }

        public bool IsCompleted => _task.IsCompleted;

        public bool IsCompletedSuccessfully => _task.Status == TaskStatus.RanToCompletion;

        public bool IsFaulted => _task.IsFaulted;

        public bool IsCanceled => _task.IsCanceled;

        public TaskAwaiter<T> GetAwaiter() => _taskAwaiter;

        public T Result => _task.Result;
    }

    public class DistributedAsyncTaskMethodBuilder<T>
    {
        internal AsyncTaskMethodBuilder<T> MethodBuilder;
        private Task<T> distributedHostTask;

        public static DistributedAsyncTaskMethodBuilder<T> Create()
        {
            Log();
            return new DistributedAsyncTaskMethodBuilder<T>() { MethodBuilder = AsyncTaskMethodBuilder<T>.Create() };
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            Log();

            // AsyncStateMachineの名前はあらかじめ必要
            var fullName = stateMachine.GetType().AssemblyQualifiedName;

            // Asyncメソッドの中身を動かす
            // とりあえずのところは非同期スレッドで動かすので仮想マシン扱いしておく
            distributedHostTask = System.Threading.Tasks.Task.Run<T>(() =>
            {
                Log("Run");

                var distributedObject = Activator.GetObject(typeof(DistributedObject), "ipc://DistributedChannel/DistributedObject")
                    as DistributedObject
                    ?? throw new Exception("not found distribute server in ipc");
                return distributedObject.Exucute<T>(fullName);
            });
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            Log();
            MethodBuilder.SetStateMachine(stateMachine);
        }

        public void SetResult(T result)
        {
            Log();
            MethodBuilder.SetResult(result);
        }

        public void SetException(Exception exception)
        {
            Log();
            MethodBuilder.SetException(exception);
        }

        public DistributedAsyncTask<T> Task {
            get {
                Log();

                // distributedHostTaskがこのasyncメソッドの本体
                return new DistributedAsyncTask<T>(distributedHostTask);
            }
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Log();
            MethodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Log();
            MethodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }
    }
}
