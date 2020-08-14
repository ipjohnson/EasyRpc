using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration.DelegateConfiguration
{
    public class ThreeArgDelegateConfiguration<TArg1, TArg2, TArg3, TResult> : ExpressionInstanceConfiguration, IConfigurationInformationProvider
    {
        private ICurrentApiInformation _currentApi;
        private Expression<Func<TArg1, TArg2, TArg3, TResult>> _expression;

        public ThreeArgDelegateConfiguration(ICurrentApiInformation currentApi, string method, string path, Expression<Func<TArg1, TArg2, TArg3, TResult>> expression)
        {
            _currentApi = currentApi;
            _expression = expression;

            Method = method;
            Path = path;
        }


        public void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            service.ExposeExpression(_currentApi,this, _expression);
        }
    }
}
