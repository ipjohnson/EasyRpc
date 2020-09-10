using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.CodeGeneration
{
    /// <summary>
    /// Interface for building MethodEndPointDelegate
    /// </summary>
    public interface IMethodInvokerCreationService
    {
        /// <summary>
        /// Build method invoker delegate given an endpoint configuration and parameter type
        /// </summary>
        /// <param name="endPointMethodConfiguration"></param>
        /// <param name="parametersType"></param>
        /// <returns></returns>
        InvokeMethodDelegate<T> BuildMethodInvoker<T>(IEndPointMethodConfigurationReadOnly endPointMethodConfiguration, Type parametersType);
    }

    /// <inheritdoc cref="IMethodInvokeInformation"/>
    public class MethodInvokerCreationService : IMethodInvokerCreationService, IApiConfigurationCompleteAware
    {
        private readonly PropertyInfo _contextInstance;
        private readonly PropertyInfo _contextParameters;
        private readonly PropertyInfo _resultProperty;
        private readonly MethodInfo _valueTaskAsTask;
        private readonly MethodInfo _setResultValueTaskAsync;
        private readonly MethodInfo _taskFromResult;
        private readonly MethodInfo _wrapResult;
        private readonly MethodInfo _wrapResultTaskAsync;
        private readonly MethodInfo _wrapResultValueTaskAsync;
        private ExposeConfigurations _exposeOptions;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MethodInvokerCreationService()
        {
            _contextInstance = typeof(RequestExecutionContext).GetProperty(nameof(RequestExecutionContext.ServiceInstance));
            _contextParameters = typeof(RequestExecutionContext).GetProperty(nameof(RequestExecutionContext.Parameters));
            _resultProperty = typeof(RequestExecutionContext).GetProperty(nameof(RequestExecutionContext.Result));
            _valueTaskAsTask = typeof(ValueTask).GetMethod("AsTask");
            _setResultValueTaskAsync = typeof(ValueTask<>).GetMethod("AsTask");
            _taskFromResult = typeof(Task).GetMethod("FromResult");
            _wrapResult = typeof(InvokeHelpers).GetMethod(nameof(InvokeHelpers.WrapResult));
            _wrapResultTaskAsync = typeof(InvokeHelpers).GetMethod(nameof(InvokeHelpers.WrapResultTaskAsync));
            _wrapResultValueTaskAsync = typeof(InvokeHelpers).GetMethod(nameof(InvokeHelpers.WrapResultValueTaskAsync));
        }

        /// <inheritdoc />
        public InvokeMethodDelegate<T> BuildMethodInvoker<T>(IEndPointMethodConfigurationReadOnly endPointMethodConfiguration, Type parametersType)
        {
            if (endPointMethodConfiguration.InvokeInformation.MethodToInvoke != null)
            {
                return BuildInstanceMethodInvoker<T>(endPointMethodConfiguration, parametersType);
            }
            if (endPointMethodConfiguration.InvokeInformation.DelegateToInvoke != null)
            {
                return BuildDelegateMethodInvoker<T>(endPointMethodConfiguration, parametersType);
            }
            
            throw new Exception("Invoke information is blank");
        }

        /// <summary>
        /// build delegate method invoker
        /// </summary>
        /// <param name="endPointMethodConfiguration"></param>
        /// <param name="parametersType"></param>
        /// <returns></returns>
        protected virtual InvokeMethodDelegate<T> BuildDelegateMethodInvoker<T>(IEndPointMethodConfigurationReadOnly endPointMethodConfiguration, Type parametersType)
        {
            var requestParameter = Expression.Parameter(typeof(RequestExecutionContext), "requestExecutionContext");
            var typedParameterVariable = Expression.Variable(parametersType, "localParameters");

            var assignExpression = BuildParameterAssign(parametersType, typedParameterVariable, requestParameter);

            var parameterList = BuildParameterList(endPointMethodConfiguration, parametersType, typedParameterVariable);

            var invokeDelegate = endPointMethodConfiguration.InvokeInformation.DelegateToInvoke;

            Expression callExpression;

            if (invokeDelegate.Method.IsStatic)
            {
                if ((invokeDelegate.Method.GetParameters().Length - parameterList.Count) == 1)
                {
                    callExpression = Expression.Invoke(Expression.Constant(invokeDelegate), parameterList);
                }
                else
                {
                    callExpression = Expression.Call(invokeDelegate.Method, parameterList);
                }
            }
            else
            {
                callExpression = Expression.Invoke(Expression.Constant(invokeDelegate), parameterList);
            }

            var methodBodyStatements = new List<Expression> { assignExpression };

            WrapExpression<T>(endPointMethodConfiguration, invokeDelegate.Method, methodBodyStatements, requestParameter, callExpression);

            var compiledInvokeDelegate =
                Expression.Lambda<InvokeMethodDelegate<T>>(Expression.Block(new[] { typedParameterVariable }, methodBodyStatements), requestParameter).Compile();

            return compiledInvokeDelegate;
        }

        /// <summary>
        /// build instance method invoker
        /// </summary>
        /// <param name="endPointMethodConfiguration"></param>
        /// <param name="parametersType"></param>
        /// <returns></returns>
        protected virtual InvokeMethodDelegate<T> BuildInstanceMethodInvoker<T>(IEndPointMethodConfigurationReadOnly endPointMethodConfiguration, Type parametersType)
        {
            var requestParameter = Expression.Parameter(typeof(RequestExecutionContext), "requestExecutionContext");
            var typedParameterVariable = Expression.Variable(parametersType, "localParameters");

            var assignExpression = BuildParameterAssign(parametersType, typedParameterVariable, requestParameter);

            var parameterList = BuildParameterList(endPointMethodConfiguration, parametersType, typedParameterVariable);
            var invokeMethod = endPointMethodConfiguration.InvokeInformation.MethodToInvoke;

            var instanceExpression = Expression.Convert(Expression.Property(requestParameter, _contextInstance), invokeMethod.DeclaringType);

            var invokeExpression = Expression.Call(instanceExpression, invokeMethod, parameterList);

            var methodBodyStatements = new List<Expression> { assignExpression };

            WrapExpression<T>(endPointMethodConfiguration, invokeMethod, methodBodyStatements, requestParameter, invokeExpression);

            var compiledInvokeDelegate =
                Expression.Lambda<InvokeMethodDelegate<T>>(Expression.Block(new[] { typedParameterVariable }, methodBodyStatements), requestParameter).Compile();

            return compiledInvokeDelegate;
        }

        /// <summary>
        /// Wrap result expression
        /// </summary>
        /// <param name="endPointMethodConfiguration"></param>
        /// <param name="invokeMethod"></param>
        /// <param name="methodBodyStatements"></param>
        /// <param name="requestParameter"></param>
        /// <param name="invokeExpression"></param>
        protected virtual void WrapExpression<T>(IEndPointMethodConfigurationReadOnly endPointMethodConfiguration,
            MethodInfo invokeMethod,
            List<Expression> methodBodyStatements,
            ParameterExpression requestParameter,
            Expression invokeExpression)
        {
            if (invokeMethod.ReturnType == typeof(void))
            {
                methodBodyStatements.Add(invokeExpression);
                methodBodyStatements.Add(Expression.Constant(Task.CompletedTask));
            }
            else if (invokeMethod.ReturnType == typeof(Task))
            {
                methodBodyStatements.Add(invokeExpression);
            }
            else if (invokeMethod.ReturnType == typeof(ValueTask))
            {
                var valueTaskExpression = Expression.Call(invokeExpression, _valueTaskAsTask);

                methodBodyStatements.Add(valueTaskExpression);
            }
            else if (invokeMethod.ReturnType.IsConstructedGenericType)
            {
                var openType = invokeMethod.ReturnType.GetGenericTypeDefinition();

                if (openType == typeof(ValueTask<>))
                {
                    if (endPointMethodConfiguration.WrappedType != null)
                    {
                        var closedMethod = _wrapResultValueTaskAsync.MakeGenericMethod(endPointMethodConfiguration.WrappedType, invokeMethod.ReturnType.GenericTypeArguments[0]);

                        var callExpression = Expression.Call(closedMethod, invokeExpression, requestParameter);

                        methodBodyStatements.Add(callExpression);
                    }
                    else
                    {
                        var setMethod = invokeMethod.ReturnType.GetMethod("AsTask");

                        var callExpression = Expression.Call(invokeExpression, setMethod);

                        methodBodyStatements.Add(callExpression);
                    }
                }
                else if (openType == typeof(Task<>))
                {
                    if (endPointMethodConfiguration.WrappedType != null)
                    {
                        var closedMethod = _wrapResultTaskAsync.MakeGenericMethod(endPointMethodConfiguration.WrappedType, invokeMethod.ReturnType.GenericTypeArguments[0]);

                        var callExpression = Expression.Call(closedMethod, invokeExpression, requestParameter);

                        methodBodyStatements.Add(callExpression);
                    }
                    else
                    {
                        methodBodyStatements.Add(invokeExpression);
                    }
                }
                else
                {
                    var closedMethod = _taskFromResult.MakeGenericMethod(typeof(T));

                    var callFromResult = Expression.Call(closedMethod, invokeExpression);

                    methodBodyStatements.Add(callFromResult);
                }
            }
            else
            {
                //var shouldWrap = endPointMethodConfiguration.RawContentType == null &&
                //                 _exposeOptions.TypeWrapSelector(invokeMethod.ReturnType);

                //if (shouldWrap)
                //{
                //    var wrapperType = _wrappedResultTypeCreator.GetTypeWrapper(invokeMethod.ReturnType);

                //    var closedMethod = _wrapResult.MakeGenericMethod(wrapperType, invokeMethod.ReturnType);

                //    var wrapExpression = Expression.Call(closedMethod, invokeExpression, requestParameter);

                //    methodBodyStatements.Add(wrapExpression);
                //}
                //else
                //{
                    //if (invokeExpression.Type.IsValueType)
                    //{
                    //    invokeExpression = Expression.Convert(invokeExpression, typeof(object));
                    //}

                    var closedMethod = _taskFromResult.MakeGenericMethod(typeof(T));

                    var callFromResult = Expression.Call(closedMethod, invokeExpression);

                    methodBodyStatements.Add(callFromResult);
                //}
            }
        }

        /// <summary>
        /// Build parameter assignment statement
        /// </summary>
        /// <param name="parametersType"></param>
        /// <param name="typedParameterVariable"></param>
        /// <param name="requestParameter"></param>
        /// <returns></returns>
        protected virtual Expression BuildParameterAssign(Type parametersType, ParameterExpression typedParameterVariable, ParameterExpression requestParameter)
        {
            return Expression.Assign(typedParameterVariable,
                Expression.Convert(Expression.Property(requestParameter, _contextParameters), parametersType));
        }

        /// <summary>
        /// Build parameter list expressions 
        /// </summary>
        /// <param name="endPointMethodConfiguration"></param>
        /// <param name="parametersType"></param>
        /// <param name="typedParameterVariable"></param>
        /// <returns></returns>
        protected virtual List<Expression> BuildParameterList(IEndPointMethodConfigurationReadOnly endPointMethodConfiguration,
            Type parametersType, ParameterExpression typedParameterVariable)
        {
            var parameterList = new List<Expression>();

            foreach (var rpcParameterInfo in endPointMethodConfiguration.Parameters)
            {
                var property = parametersType.GetProperty(rpcParameterInfo.Name);

                parameterList.Add(Expression.Property(typedParameterVariable, property));
            }

            return parameterList;
        }
        
        /// <inheritdoc />
        public virtual void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            _exposeOptions = serviceScope.GetRequiredService<IConfigurationManager>().GetConfiguration<ExposeConfigurations>();
        }
    }
}
