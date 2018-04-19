using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt.Server.Internal
{
    public static class MethodInvokerBuilder
    {
        public static Func<object, object[], object> Build(Type contract, MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            // lambda parameters
            ParameterExpression instanceParam = Expression.Parameter(typeof(object), "instance");
            ParameterExpression parametersArrayParam = Expression.Parameter(typeof(object[]), "parameters");

            // we convert the type of object to specific contract
            UnaryExpression instanceAccessExpression = Expression.Convert(instanceParam, contract);

            // execute the call
            MethodCallExpression body = Expression.Call(instanceAccessExpression, method, BuildParameters(parameters, parametersArrayParam));

            if (method.ReturnType == typeof(void))
            {
                // void method, compile the lambda that returns no value
                Action<object, object[]> lambda = Expression.Lambda<Action<object, object[]>>(body, instanceParam, parametersArrayParam).Compile();

                return (p1, p2) =>
                {
                    lambda(p1, p2);
                    return null;
                };
            }

            // compile lambda
            return
                Expression.Lambda<Func<object, object[], object>>(
                    Expression.Convert(body, typeof(object)),
                    instanceParam,
                    parametersArrayParam)
                    .Compile();
        }

        public static Func<Task, object> BuildTaskResultProvider(Type resultType)
        {
            // lambda parameters
            ParameterExpression taskParam = Expression.Parameter(typeof(Task), "completedTask");

            // make task generic
            Type taskType = typeof(Task<>);
            taskType = taskType.MakeGenericType(resultType);
            PropertyInfo resultProperty = taskType.GetRuntimeProperty(nameof(Task<object>.Result));

            // execute the call
            MethodCallExpression body = Expression.Call(Expression.Convert(taskParam, taskType), resultProperty.GetGetMethod());

            return Expression.Lambda<Func<Task, object>>(Expression.Convert(body, typeof(object)), taskParam).Compile();
        }

        private static Expression[] BuildParameters(ParameterInfo[] parameters, ParameterExpression parametersArrayParam)
        {
            if (parameters.Length == 0)
            {
                return Array.Empty<Expression>();
            }

            Expression[] result = new Expression[parameters.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Expression.Convert(Expression.ArrayAccess(parametersArrayParam, Expression.Constant(i)), parameters[i].ParameterType);
            }

            return result;
        }
    }
}
