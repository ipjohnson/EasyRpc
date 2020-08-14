using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using EasyRpc.AspNetCore.Configuration.DelegateConfiguration;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Configuration
{
    public partial class InternalApiConfiguration
    {
        public IExposureExpressionConfiguration GetMethod<TResult>(string path, Expression<Func<TResult>> getExpression)
        {
            var config = new NoParamDelegateConfiguration<TResult>(GetCurrentApiInformation(),  HttpMethods.Get, path, getExpression);

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IExposureExpressionConfiguration GetMethod<TArg1, TResult>(string path, Expression<Func<TArg1, TResult>> getExpression)
        {
            var config = new OneArgDelegateConfiguration<TArg1, TResult>(GetCurrentApiInformation(), HttpMethods.Get, path, getExpression);

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IExposureExpressionConfiguration GetMethod<TArg1, TArg2, TResult>(string path, Expression<Func<TArg1, TArg2, TResult>> getExpression)
        {
            var config = new TwoArgDelegateConfiguration<TArg1,TArg2, TResult>(GetCurrentApiInformation(), HttpMethods.Get, path, getExpression);

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IExposureExpressionConfiguration GetMethod<TArg1, TArg2, TArg3, TResult>(string path, Expression<Func<TArg1, TArg2, TArg3, TResult>> getExpression)
        {
            var config = new ThreeArgDelegateConfiguration<TArg1, TArg2, TArg3, TResult>(GetCurrentApiInformation(), HttpMethods.Get, path, getExpression);

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IExposureExpressionConfiguration PostMethod<TArg1, TResult>(string path, Expression<Func<TArg1, TResult>> postExpression)
        {
            var config = new OneArgDelegateConfiguration<TArg1, TResult>(GetCurrentApiInformation(), HttpMethods.Post, path, postExpression)
            {
                HasBody = true
            };

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IExposureExpressionConfiguration PostMethod<TArg1, TArg2, TResult>(string path, Expression<Func<TArg1, TArg2, TResult>> postExpression)
        {
            var config = new TwoArgDelegateConfiguration<TArg1, TArg2, TResult>(GetCurrentApiInformation(), HttpMethods.Post, path, postExpression)
            {
                HasBody = true
            };

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }
        
        public IExposureExpressionConfiguration PostMethod<TArg1, TArg2, TArg3, TResult>(string path, Expression<Func<TArg1, TArg2, TArg3, TResult>> postExpression)
        {
            var config = new ThreeArgDelegateConfiguration<TArg1, TArg2, TArg3, TResult>(GetCurrentApiInformation(), HttpMethods.Post, path, postExpression)
            {
                HasBody = true
            };

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IExposureExpressionConfiguration HttpMethod<TResult>(string httpMethod, string path, Expression<Func<TResult>> method)
        {
            var config = new NoParamDelegateConfiguration<TResult>(GetCurrentApiInformation(), httpMethod, path, method);

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IExposureExpressionConfiguration HttpMethod<TArg1, TResult>(string httpMethod, string path, Expression<Func<TArg1, TResult>> method)
        {
            var config = new OneArgDelegateConfiguration<TArg1, TResult>(GetCurrentApiInformation(), httpMethod, path, method)
            {
                HasBody = true
            };

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IExposureExpressionConfiguration HttpMethod<TArg1, TArg2, TResult>(string httpMethod, string path, Expression<Func<TArg1, TArg2, TResult>> method)
        {
            var config = new TwoArgDelegateConfiguration<TArg1, TArg2, TResult>(GetCurrentApiInformation(), httpMethod, path, method)
            {
                HasBody = true
            };

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }

        public IExposureExpressionConfiguration HttpMethod<TArg1, TArg2, TArg3, TResult>(string httpMethod, string path, Expression<Func<TArg1, TArg2, TArg3, TResult>> method)
        {
            var config = new ThreeArgDelegateConfiguration<TArg1, TArg2, TArg3, TResult>(GetCurrentApiInformation(), httpMethod, path, method)
            {
                HasBody = true
            };

            _applicationConfigurationService.AddConfigurationObject(config);

            return config;
        }
    }
}
