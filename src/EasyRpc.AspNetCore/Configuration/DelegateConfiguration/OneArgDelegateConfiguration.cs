﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration.DelegateConfiguration
{
    public class OneArgDelegateConfiguration<TArg1, TResult> : ExpressionInstanceConfiguration, IConfigurationInformationProvider
    {
        private readonly ICurrentApiInformation _currentApi;
        private readonly Expression<Func<TArg1, TResult>> _expression;

        public OneArgDelegateConfiguration(ICurrentApiInformation currentApi, string method, string path, Expression<Func<TArg1, TResult>> expression)
        {
            _currentApi = currentApi;
            _expression = expression;

            Method = method;
            Path = path;
        }

        public void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            service.ExposeExpression(_currentApi, this, _expression);
        }
    }
}