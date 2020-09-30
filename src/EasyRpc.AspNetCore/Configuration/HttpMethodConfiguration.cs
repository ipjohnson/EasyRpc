using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Configuration.DelegateConfiguration;

namespace EasyRpc.AspNetCore.Configuration
{
    public class HttpMethodConfiguration : IHttpMethodConfiguration
    {
        private readonly IInternalApiConfiguration _internalApiConfiguration;

        public HttpMethodConfiguration(IInternalApiConfiguration internalApiConfiguration)
        {
            _internalApiConfiguration = internalApiConfiguration;
        }
        
        /// <inheritdoc />
        public IExposureDelegateConfiguration Handle<TResult>(string method, string path, Func<TResult> func, bool? hasBody = null)
        {
            return RegisterDelegateHandler(method, path, func, hasBody);
        }
        
        /// <inheritdoc />
        public IExposureDelegateConfiguration Handle<TArg, TResult>(string method, string path, Func<TArg, TResult> func, bool? hasBody = null)
        {
            return RegisterDelegateHandler(method, path, func, hasBody);
        }

        /// <inheritdoc />
        public IExposureDelegateConfiguration Handle<TArg1, TArg2, TResult>(string method, string path, Func<TArg1, TArg2, TResult> func, bool? hasBody = null)
        {
            return RegisterDelegateHandler(method, path, func, hasBody);
        }
        
        /// <inheritdoc />
        public IExposureDelegateConfiguration Handle<TArg1, TArg2, TArg3, TResult>(string method, string path, Func<TArg1, TArg2, TArg3, TResult> func, bool? hasBody = null)
        {
            return RegisterDelegateHandler(method, path, func, hasBody);
        }

        /// <inheritdoc />
        public IExposureDelegateConfiguration Handle<TArg1, TArg2, TArg3, TArg4, TResult>(string method, string path, Func<TArg1, TArg2, TArg3, TArg4, TResult> func, bool? hasBody = null)
        {
            return RegisterDelegateHandler(method, path, func, hasBody);
        }

        /// <inheritdoc />
        public IExposureDelegateConfiguration Handle<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string method, string path, Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> func, bool? hasBody = null)
        {
            return RegisterDelegateHandler(method, path, func, hasBody);
        }

        /// <summary>
        /// Register a delegate to handle a specific method/path combination
        /// </summary>
        /// <param name="method"></param>
        /// <param name="path"></param>
        /// <param name="func"></param>
        /// <param name="hasBody"></param>
        /// <returns></returns>
        protected virtual IExposureDelegateConfiguration RegisterDelegateHandler(string method, string path, Delegate func,
            bool? hasBody)
        {
            var delegateConfiguration =
                new DelegateInstanceConfiguration(_internalApiConfiguration.GetCurrentApiInformation(), func)
                {
                    Method = method,
                    Path = path,
                    HasRequestBody = hasBody
                };

            _internalApiConfiguration.ApplicationConfigurationService.AddConfigurationObject(delegateConfiguration);

            return delegateConfiguration;
        }
    }
}
