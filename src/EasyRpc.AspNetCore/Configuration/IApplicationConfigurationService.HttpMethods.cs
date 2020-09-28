using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using EasyRpc.AspNetCore.Configuration.DelegateConfiguration;

namespace EasyRpc.AspNetCore.Configuration
{
    public partial interface IApplicationConfigurationService
    {
        void ExposeExpression<TResult>(ICurrentApiInformation currentApi, BaseDelegateInstanceConfiguration instanceConfiguration,
            Expression<Func<TResult>> expression);

        void ExposeExpression<TArg1, TResult>(ICurrentApiInformation currentApi,  BaseDelegateInstanceConfiguration instanceConfiguration,
            Expression<Func<TArg1, TResult>> expression);
        
        void ExposeExpression<TArg1, TArg2, TResult>(ICurrentApiInformation currentApi, BaseDelegateInstanceConfiguration instanceConfiguration,
            Expression<Func<TArg1, TArg2, TResult>> expression);
        
        void ExposeExpression<TArg1, TArg2, TArg3, TResult>(ICurrentApiInformation currentApi, BaseDelegateInstanceConfiguration instanceConfiguration,
            Expression<Func<TArg1, TArg2, TArg3, TResult>> expression);

        void ExposeDelegate(ICurrentApiInformation currentApi, DelegateInstanceConfiguration delegateInstanceConfiguration, Delegate @delegate);
    }
}
