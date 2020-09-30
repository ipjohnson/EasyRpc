using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Filters;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Base class for rpc module
    /// </summary>
    public abstract class RpcModule : IRpcModule
    {
        void IRpcModule.Configure(IRpcApi api)
        {
            api.ApplyFilter(ApplyFilter);

            Configure(api);

            if (AutoRegister)
            {
                AutoRegisterMethods(api);
            }
        }
        
        /// <summary>
        /// Auto register all public methods
        /// </summary>
        protected bool AutoRegister { get; set; } = true;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="api"></param>
        protected virtual void Configure(IRpcApi api)
        {
            
        }

        /// <summary>
        /// Before execute handler
        /// </summary>
        protected Func<RequestExecutionContext, Task<bool>> BeforeExecute { get; set; }

        /// <summary>
        /// After execute handler
        /// </summary>
        protected Func<RequestExecutionContext, Task> AfterExecute { get; set; }

        /// <summary>
        /// Register public methods
        /// </summary>
        /// <param name="api"></param>
        protected virtual void AutoRegisterMethods(IRpcApi api) => api.Expose(GetType()).Activation(context => this);

        /// <summary>
        /// Apply before and after filter methods
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Func<RequestExecutionContext, IRequestFilter> ApplyFilter(IEndPointMethodConfigurationReadOnly arg)
        {
            if (BeforeExecute != null || AfterExecute != null)
            {
                var filter = new ModuleFilter(BeforeExecute, AfterExecute);

                return context => filter;
            }

            return null;
        }

        /// <summary>
        /// Module filter
        /// </summary>
        private class ModuleFilter : IAsyncRequestExecutionFilter
        {
            private readonly Func<RequestExecutionContext, Task<bool>> _beforeFunc;
            private readonly Func<RequestExecutionContext, Task> _afterFunc;

            public ModuleFilter(Func<RequestExecutionContext, Task<bool>> beforeFunc,
                Func<RequestExecutionContext, Task> afterFunc)
            {
                _beforeFunc = beforeFunc;
                _afterFunc = afterFunc;
            }

            public async Task BeforeExecuteAsync(RequestExecutionContext context)
            {
                if (_beforeFunc != null)
                {
                    context.ContinueRequest = await _beforeFunc(context);
                }
            }

            public async Task AfterExecuteAsync(RequestExecutionContext context)
            {
                if (_afterFunc != null)
                {
                    await _afterFunc(context);
                }
            }
        }
    }
}
