using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration.DelegateConfiguration
{
    public class ThreeArgExpressionExpression<TArg1, TArg2, TArg3, TResult> : BaseDelegateInstanceConfiguration, IConfigurationInformationProvider
    {
        private readonly ICurrentApiInformation _currentApi;
        private readonly Expression<Func<TArg1, TArg2, TArg3, TResult>> _expression;

        public ThreeArgExpressionExpression(ICurrentApiInformation currentApi, string method, string path, Expression<Func<TArg1, TArg2, TArg3, TResult>> expression)
        {
            _currentApi = currentApi;
            _expression = expression;

            Method = method;
            Path = path;
        }


        public override void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            service.ExposeExpression(_currentApi,this, _expression);
        }
    }
}
