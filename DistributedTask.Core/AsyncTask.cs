using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using static DistributedTask.Core.Logger;

namespace DistributedTask.Core
{
    [AsyncMethodBuilder(typeof(SAsyncTaskMethodBuilder<>))]
    public class AsyncTask<T>
    {

        private readonly Task<T> _task;
        private readonly TaskAwaiter<T> _taskAwaiter;

        public AsyncTask(Task<T> task)
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

    public class SAsyncTaskMethodBuilder<T>
    {
        private AsyncTaskMethodBuilder<T> methodBuilder;
        private Task<T> distributedHostTask;

        public static SAsyncTaskMethodBuilder<T> Create()
        {
            Log();
            return new SAsyncTaskMethodBuilder<T>() { methodBuilder = AsyncTaskMethodBuilder<T>.Create() };
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            Log();

            // AsyncStateMachineの名前はあらかじめ必要
            var fullName = stateMachine.GetType().AssemblyQualifiedName; ;

            // Asyncメソッドの中身を動かす
            // とりあえずのところは非同期スレッドで動かすので仮想マシン扱いしておく
            distributedHostTask = System.Threading.Tasks.Task.Run<T>(() =>
            {
                Log("Run");

                var type = Type.GetType(fullName);

                // 新しいAsyncStateMachineを生成する
                var newStateMachine = Activator.CreateInstance(type) as IAsyncStateMachine;

                // 新しいSAsyncTaskMethodBuilderを生成する
                var newSAsyncTaskMethodBuilder = Create();

                // asyncメソッドはIAsyncStateMachineを実装した何らかのクラスに展開され
                // asyncメソッドの本体に必要なフィールドは参照できないのでリフレクションでセットする
                type.GetField("<>t__builder").SetValue(newStateMachine, newSAsyncTaskMethodBuilder);
                type.GetField("<>1__state").SetValue(newStateMachine, -1);

                // asyncメソッドの本体を動かす
                newSAsyncTaskMethodBuilder.methodBuilder.Start(ref newStateMachine);

                // 動いている間のコルーチン的コールバックはnewSAsyncTaskMethodBuilder.methodBuilderのを呼び出される

                // asyncメソッドの返り値をTaskの返り値とする
                // Taskなので待機可能で問題ない
                return newSAsyncTaskMethodBuilder.methodBuilder.Task.Result;
            });
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            Log();
            methodBuilder.SetStateMachine(stateMachine);
        }

        public void SetResult(T result)
        {
            Log();
            methodBuilder.SetResult(result);
        }

        public void SetException(Exception exception)
        {
            Log();
            methodBuilder.SetException(exception);
        }

        public AsyncTask<T> Task {
            get {
                Log();

                // distributedHostTaskがこのasyncメソッドの本体
                return new AsyncTask<T>(distributedHostTask);
            }
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Log();
            methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Log();
            methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }
    }
}
