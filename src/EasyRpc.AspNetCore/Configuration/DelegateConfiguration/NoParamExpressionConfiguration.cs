using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration.DelegateConfiguration
{
    public class NoParamDelegateExpression<TResult> : BaseDelegateInstanceConfiguration
    {
        private readonly ICurrentApiInformation _currentApi;
        private readonly Expression<Func<TResult>> _expression;

        public NoParamDelegateExpression(ICurrentApiInformation currentApi,  string method, string path, Expression<Func<TResult>> expression)
        {
            _currentApi = currentApi;
            _expression = expression;

            Method = method;
            Path = path;
        }
        
        public override void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            service.ExposeExpression(_currentApi, this, _expression);
        }
    }
}
