using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices {
    sealed class AsyncMethodBuilderAttribute : Attribute {
        public AsyncMethodBuilderAttribute(Type builderType) {
            BuilderType = builderType;
        }

        public Type BuilderType { get; }
    }
}

namespace DistributedTask.Console.Test {

    [AsyncMethodBuilder(typeof(TestTaskMethodBuilder<>))]
    class TestTask<T> {

        internal readonly Task<T> _task;
        internal readonly T _result;

        public TestTask(T result) {
            _task = null;
            _result = result;
        }

        public TestTask(Task<T> task) {
            _task = task ?? throw new ArgumentNullException(nameof(task));
            _result = default(T);
        }

        public Task<T> AsTask() {
            return _task ?? Task.FromResult(_result);
        }

        public bool IsCompleted { get { return _task == null || _task.IsCompleted; } }

        public bool IsCompletedSuccessfully { get { return _task == null || _task.Status == TaskStatus.RanToCompletion; } }

        public bool IsFaulted { get { return _task != null && _task.IsFaulted; } }

        public bool IsCanceled { get { return _task != null && _task.IsCanceled; } }

        public T Result { get { return _task == null ? _result : _task.GetAwaiter().GetResult(); } }
    }

    class TestTaskMethodBuilder<T> {
        private AsyncTaskMethodBuilder<T> methodBuilder;
        private T result;
        private bool haveResult;
        private bool useBuilder;

        public static TestTaskMethodBuilder<T> Create() {
            System.Console.WriteLine(nameof(Create));
            return new TestTaskMethodBuilder<T>() { methodBuilder = AsyncTaskMethodBuilder<T>.Create() };
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine {
            System.Console.WriteLine(nameof(Start));
            foreach(var t in stateMachine.GetType().GetRuntimeFields()) {
                System.Console.WriteLine($"{t.FieldType.Name}:{t.Name}");
            }
            var fullName = stateMachine.GetType().FullName;
            var type = Type.GetType(fullName);
            var newStateMachine = Activator.CreateInstance(type) as IAsyncStateMachine;

            Type methodBuilderType = typeof(TestTaskMethodBuilder<>);
            Type genericMethodBuilderType = methodBuilderType.MakeGenericType(stateMachine.GetType().GetRuntimeFields().Select(x=>x.FieldType.GenericTypeArguments).Where(x=>x.Length>0).Single());
            System.Console.WriteLine($"{genericMethodBuilderType.GetMethods().Single(x=>x.Name==nameof(Create)).IsGenericMethodDefinition}");
            MethodInfo mi = genericMethodBuilderType.GetMethods().Single(m => m.Name == nameof(Create));
            //MethodInfo genericMi = mi.MakeGenericMethod(type.GenericTypeArguments);
            var testTaskMethodBuilder = ((TestTaskMethodBuilder<T>)mi.Invoke(null, null));
            methodBuilder = testTaskMethodBuilder.methodBuilder;
            type.GetField("<>4__this").SetValue(newStateMachine, new Program());
            type.GetField("<>t__builder").SetValue(newStateMachine, testTaskMethodBuilder);
            type.GetField("<>1__state").SetValue(newStateMachine, -1);
            //newStateMachine.MoveNext();
            methodBuilder.Start(ref newStateMachine);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) {
            System.Console.WriteLine(nameof(SetStateMachine));
            methodBuilder.SetStateMachine(stateMachine);
        }

        public void SetResult(T result) {
            System.Console.WriteLine(nameof(SetResult));
            if (useBuilder) {
                methodBuilder.SetResult(result);
            } else {
                this.result = result;
                haveResult = true;
            }
        }

        public void SetException(Exception exception) {
            System.Console.WriteLine(nameof(SetException));
            methodBuilder.SetException(exception);
        }

        public TestTask<T> Task {
            get {
                System.Console.WriteLine(nameof(Task));
                if (haveResult) {
                    return new TestTask<T>(result);
                } else {
                    useBuilder = true;
                    return new TestTask<T>(methodBuilder.Task);
                }
            }
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine {
            System.Console.WriteLine(nameof(AwaitOnCompleted));
            useBuilder = true;
            methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine {
            System.Console.WriteLine(nameof(AwaitUnsafeOnCompleted));
            useBuilder = true;
            methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }
    }
}
