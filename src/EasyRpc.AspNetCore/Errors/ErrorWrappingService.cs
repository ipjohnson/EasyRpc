using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using EasyRpc.AspNetCore.CodeGeneration;

namespace EasyRpc.AspNetCore.Errors
{
    /// <summary>
    /// Service that returns an error object given an exception
    /// </summary>
    public interface IErrorWrappingService
    {
        IErrorWrapper GetErrorWrapper();

        object WrapError(RequestExecutionContext context, Exception e);
    }

    /// <summary>
    /// Wraps exception with error class
    /// </summary>
    public class ErrorWrappingService : IErrorWrappingService
    {
        private readonly IErrorResultTypeCreator _errorResultTypeCreator;
        private Func<IErrorWrapper> _errorWrapperCreator;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="errorResultTypeCreator"></param>
        public ErrorWrappingService(IErrorResultTypeCreator errorResultTypeCreator)
        {
            _errorResultTypeCreator = errorResultTypeCreator;
        }

        public IErrorWrapper GetErrorWrapper()
        {
            _errorWrapperCreator ??= CreateWrapperFunction();

            return _errorWrapperCreator();
        }

        /// <inheritdoc />
        public object WrapError(RequestExecutionContext context, Exception e)
        {
            _errorWrapperCreator ??= CreateWrapperFunction();

            var wrapper = _errorWrapperCreator();

            wrapper.Message = e.Message;

            return wrapper;
        }

        private Func<IErrorWrapper> CreateWrapperFunction()
        {
            var wrapperType = _errorResultTypeCreator.GenerateErrorType();

            var newExpression = Expression.New(wrapperType);

            var lambda = Expression.Lambda<Func<IErrorWrapper>>(newExpression);

            return lambda.Compile();
        }
    }
}
