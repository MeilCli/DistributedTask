using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static DistributedTask.Core.Logger;

namespace DistributedTask.Core
{
    public class DistributedObject : MarshalByRefObject
    {
        public T Exucute<T>(string asyncStateMachineTypeName)
        {
            Log();

            var type = Type.GetType(asyncStateMachineTypeName);

            // 新しいAsyncStateMachineを生成する
            var newStateMachine = Activator.CreateInstance(type) as IAsyncStateMachine;

            // 型パラメーターが一致するDistributedAsyncTaskMethodBuilder.Createメソッドを探す
            Type methodBuilderType = typeof(DistributedAsyncTaskMethodBuilder<>);
            Type genericMethodBuilderType = methodBuilderType.MakeGenericType(
                type.GetRuntimeFields()
                .Where(x => x.FieldType.Name.StartsWith("DistributedAsyncTaskMethodBuilder"))
                .Select(x => x.FieldType.GenericTypeArguments)
                .Where(x => x.Length > 0)
                .Single()
            );
            MethodInfo mi = genericMethodBuilderType.GetMethods().Single(m => m.Name == nameof(DistributedAsyncTaskMethodBuilder<T>.Create));

            // 新しいDistributedAsyncTaskMethodBuilderを生成する
            var newDistributedAsyncTaskMethodBuilder = ((DistributedAsyncTaskMethodBuilder<T>)mi.Invoke(null, null));

            // asyncメソッドはIAsyncStateMachineを実装した何らかのクラスに展開され
            // asyncメソッドの本体に必要なフィールドは参照できないのでリフレクションでセットする
            type.GetField("<>t__builder").SetValue(newStateMachine, newDistributedAsyncTaskMethodBuilder);
            type.GetField("<>1__state").SetValue(newStateMachine, -1);

            // asyncメソッドの本体を動かす
            newDistributedAsyncTaskMethodBuilder.MethodBuilder.Start(ref newStateMachine);

            // 動いている間のコルーチン的コールバックはnewDistributedAsyncTaskMethodBuilder.methodBuilderのを呼び出される

            // asyncメソッドの返り値をTaskの返り値とする
            // Taskなので待機可能で問題ない
            return newDistributedAsyncTaskMethodBuilder.MethodBuilder.Task.Result;
        }
    }
}
