using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using EasyRpc.AspNetCore.CodeGeneration;

namespace EasyRpc.AspNetCore.Errors
{
    public interface IErrorWrappingService
    {
        object WrapError(RequestExecutionContext context, Exception e);
    }


    public class ErrorWrappingService : IErrorWrappingService
    {
        private IErrorResultTypeCreator _errorResultTypeCreator;
        private Func<IErrorWrapper> _errorWrapperCreator;

        public ErrorWrappingService(IErrorResultTypeCreator errorResultTypeCreator)
        {
            _errorResultTypeCreator = errorResultTypeCreator;
        }

        public object WrapError(RequestExecutionContext context, Exception e)
        {
            if (_errorWrapperCreator == null)
            {
                _errorWrapperCreator = CreateWrapperFunction();
            }

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
