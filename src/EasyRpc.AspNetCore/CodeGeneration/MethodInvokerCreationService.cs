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
    public interface IMethodInvokerCreationService
    {
        MethodEndPointDelegate BuildMethodInvoker(EndPointMethodConfiguration endPointMethodConfiguration, Type parametersType);
    }

    public class MethodInvokerCreationService : IMethodInvokerCreationService, IApiConfigurationCompleteAware
    {
        private readonly IWrappedResultTypeCreator _wrappedResultTypeCreator;
        private readonly PropertyInfo _contextInstance;
        private readonly PropertyInfo _contextParameters;
        private readonly PropertyInfo _resultProperty;
        private readonly MethodInfo _valueTaskAsTask;
        private readonly MethodInfo _setResultTaskAsync;
        private readonly MethodInfo _setResultValueTaskAsync;
        private readonly MethodInfo _wrapResult;
        private readonly MethodInfo _wrapResultTaskAsync;
        private readonly MethodInfo _wrapResultValueTaskAsync;
        private readonly MethodInfo _applyFiltersAndInvoke;
        private ExposeConfigurations _exposeOptions;

        public MethodInvokerCreationService(IWrappedResultTypeCreator wrappedResultTypeCreator)
        {
            _wrappedResultTypeCreator = wrappedResultTypeCreator;
            _contextInstance = typeof(RequestExecutionContext).GetProperty(nameof(RequestExecutionContext.ServiceInstance));
            _contextParameters = typeof(RequestExecutionContext).GetProperty(nameof(RequestExecutionContext.Parameters));
            _resultProperty = typeof(RequestExecutionContext).GetProperty(nameof(RequestExecutionContext.Result));
            _valueTaskAsTask = typeof(ValueTask).GetMethod("AsTask");
            _setResultTaskAsync = typeof(InvokeHelpers).GetMethod(nameof(InvokeHelpers.SetResultTaskAsync));
            _setResultValueTaskAsync = typeof(InvokeHelpers).GetMethod(nameof(InvokeHelpers.SetResultValueTaskAsync));
            _wrapResult = typeof(InvokeHelpers).GetMethod(nameof(InvokeHelpers.WrapResult));
            _wrapResultTaskAsync = typeof(InvokeHelpers).GetMethod(nameof(InvokeHelpers.WrapResultTaskAsync));
            _wrapResultValueTaskAsync = typeof(InvokeHelpers).GetMethod(nameof(InvokeHelpers.WrapResultValueTaskAsync));
            _applyFiltersAndInvoke = typeof(InvokeHelpers).GetMethod(nameof(InvokeHelpers.ApplyFiltersAndInvoke));
        }

        public MethodEndPointDelegate BuildMethodInvoker(EndPointMethodConfiguration endPointMethodConfiguration, Type parametersType)
        {
            if (endPointMethodConfiguration.InvokeInformation.MethodToInvoke != null)
            {
                return BuildInstanceMethodInvoker(endPointMethodConfiguration, parametersType);
            }
            if (endPointMethodConfiguration.InvokeInformation.DelegateToInvoke != null)
            {
                return BuildDelegateMethodInvoker(endPointMethodConfiguration, parametersType);
            }

            if (endPointMethodConfiguration.InvokeInformation.MethodInvokeDelegate != null)
            {
                return ApplyFiltersToMethodEndPointDelegate(endPointMethodConfiguration, parametersType, endPointMethodConfiguration.InvokeInformation.MethodInvokeDelegate);
            }

            throw new Exception("Invoke information is blank");
        }

        protected virtual MethodEndPointDelegate BuildDelegateMethodInvoker(EndPointMethodConfiguration endPointMethodConfiguration, Type parametersType)
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
                callExpression = Expression.Call(Expression.Constant(invokeDelegate.Target), invokeDelegate.Method, parameterList);
            }

            var methodBodyStatements = new List<Expression> { assignExpression };

            WrapExpression(endPointMethodConfiguration, invokeDelegate.Method, methodBodyStatements, requestParameter, callExpression);

            var compiledInvokeDelegate =
                Expression.Lambda<MethodEndPointDelegate>(Expression.Block(new[] { typedParameterVariable }, methodBodyStatements), requestParameter).Compile();

            return ApplyFiltersToMethodEndPointDelegate(endPointMethodConfiguration, parametersType, compiledInvokeDelegate);
        }

        protected virtual MethodEndPointDelegate BuildInstanceMethodInvoker(EndPointMethodConfiguration endPointMethodConfiguration, Type parametersType)
        {
            var requestParameter = Expression.Parameter(typeof(RequestExecutionContext), "requestExecutionContext");
            var typedParameterVariable = Expression.Variable(parametersType, "localParameters");

            var assignExpression = BuildParameterAssign(parametersType, typedParameterVariable, requestParameter);

            var parameterList = BuildParameterList(endPointMethodConfiguration, parametersType, typedParameterVariable);
            var invokeMethod = endPointMethodConfiguration.InvokeInformation.MethodToInvoke;

            var instanceExpression = Expression.Convert(Expression.Property(requestParameter, _contextInstance), invokeMethod.DeclaringType);

            var invokeExpression = Expression.Call(instanceExpression, invokeMethod, parameterList);

            var methodBodyStatements = new List<Expression> { assignExpression };

            WrapExpression(endPointMethodConfiguration, invokeMethod, methodBodyStatements, requestParameter, invokeExpression);

            var compiledInvokeDelegate =
                Expression.Lambda<MethodEndPointDelegate>(Expression.Block(new[] { typedParameterVariable }, methodBodyStatements), requestParameter).Compile();

            return ApplyFiltersToMethodEndPointDelegate(endPointMethodConfiguration, parametersType, compiledInvokeDelegate);
        }

        protected virtual void WrapExpression(EndPointMethodConfiguration endPointMethodConfiguration,
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

                if (openType == typeof(Task<>))
                {
                    var closeType = invokeMethod.ReturnType.GenericTypeArguments[0];

                    var shouldWrap = endPointMethodConfiguration.RawContentType == null &&
                        _exposeOptions.TypeWrapSelector(closeType);

                    Expression callExpression;

                    if (shouldWrap)
                    {
                        var wrapperType = _wrappedResultTypeCreator.GetTypeWrapper(closeType);

                        var closedMethod = _wrapResultTaskAsync.MakeGenericMethod(wrapperType, closeType);

                        callExpression = Expression.Call(closedMethod, invokeExpression, requestParameter);
                    }
                    else
                    {
                        var setMethod = _setResultTaskAsync.MakeGenericMethod(closeType);

                        callExpression = Expression.Call(setMethod, invokeExpression, requestParameter);
                    }

                    methodBodyStatements.Add(callExpression);
                }
                else if (openType == typeof(ValueTask<>))
                {
                    var closeType = invokeMethod.ReturnType.GenericTypeArguments[0];

                    var shouldWrap = endPointMethodConfiguration.RawContentType == null &&
                                     _exposeOptions.TypeWrapSelector(closeType);

                    Expression callExpression;

                    if (shouldWrap)
                    {
                        var wrapperType = _wrappedResultTypeCreator.GetTypeWrapper(closeType);

                        var closedMethod = _wrapResultValueTaskAsync.MakeGenericMethod(wrapperType, invokeMethod.ReturnType);

                        callExpression = Expression.Call(closedMethod, invokeExpression, requestParameter);
                    }
                    else
                    {
                        var setMethod = _setResultValueTaskAsync.MakeGenericMethod(closeType);

                        callExpression = Expression.Call(setMethod, invokeExpression, requestParameter);
                    }

                    methodBodyStatements.Add(callExpression);
                }
                else
                {
                    var assignValue = Expression.Assign(Expression.Property(requestParameter, _resultProperty),
                        invokeExpression);

                    methodBodyStatements.Add(assignValue);
                    methodBodyStatements.Add(Expression.Constant(Task.CompletedTask));
                }
            }
            else
            {
                var shouldWrap = endPointMethodConfiguration.RawContentType == null &&
                                 _exposeOptions.TypeWrapSelector(invokeMethod.ReturnType);

                if (shouldWrap)
                {
                    var wrapperType = _wrappedResultTypeCreator.GetTypeWrapper(invokeMethod.ReturnType);

                    var closedMethod = _wrapResult.MakeGenericMethod(wrapperType, invokeMethod.ReturnType);

                    var wrapExpression = Expression.Call(closedMethod, invokeExpression, requestParameter);

                    methodBodyStatements.Add(wrapExpression);
                }
                else
                {
                    var assignValue = Expression.Assign(Expression.Property(requestParameter, _resultProperty),
                        invokeExpression);

                    methodBodyStatements.Add(assignValue);
                    methodBodyStatements.Add(Expression.Constant(Task.CompletedTask));
                }
            }
        }

        private Expression BuildParameterAssign(Type parametersType, ParameterExpression typedParameterVariable, ParameterExpression requestParameter)
        {
            return Expression.Assign(typedParameterVariable,
                Expression.Convert(Expression.Property(requestParameter, _contextParameters), parametersType));
        }

        private List<Expression> BuildParameterList(EndPointMethodConfiguration endPointMethodConfiguration,
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

        protected virtual MethodEndPointDelegate ApplyFiltersToMethodEndPointDelegate(EndPointMethodConfiguration endPointMethodConfiguration, Type parametersType, MethodEndPointDelegate endPointDelegate)
        {
            if (endPointMethodConfiguration.Filters == null || endPointMethodConfiguration.Filters.Count == 0)
            {
                return endPointDelegate;
            }

            var requestExecutionContextParameter = Expression.Parameter(typeof(RequestExecutionContext));

            var invokeMethod = Expression.Call(_applyFiltersAndInvoke, requestExecutionContextParameter,
                Expression.Constant(endPointMethodConfiguration.Filters), Expression.Constant(endPointDelegate));

            var lambdaExpression = Expression.Lambda<MethodEndPointDelegate>(invokeMethod, requestExecutionContextParameter);

            return lambdaExpression.Compile();
        }

        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            _exposeOptions = serviceScope.GetRequiredService<IConfigurationManager>().GetConfiguration<ExposeConfigurations>();
        }
    }
}
