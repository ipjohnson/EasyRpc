using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;

namespace EasyRpc.AspNetCore.Middleware
{
    public class FactoryMethodConfiguration : IFactoryMethodConfiguration
    {
        private Action<string, Delegate, InvokeMethodWithArray, IExposedMethodParameter[]> _addMethod;

        public FactoryMethodConfiguration(Action<string, Delegate, InvokeMethodWithArray, IExposedMethodParameter[]> addMethod)
        {
            _addMethod = addMethod;
        }

        public IFactoryMethodConfiguration Action(string methodName, Action method)
        {
            _addMethod(methodName, 
                method,
                (instance, values, version, id) =>
            {
                method();

                return ReturnNoResult();
            }, null);

            return this;
        }

        public IFactoryMethodConfiguration Action<TIn>(string methodName, Action<TIn> method, params IExposedMethodParameter[] methodParameters)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) =>
            {
                method((TIn)values[0]);

                return ReturnNoResult();
            }, methodParameters);

            return this;
        }

        public IFactoryMethodConfiguration Action<TIn1, TIn2>(string methodName, Action<TIn1, TIn2> method, params IExposedMethodParameter[] methodParameters)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) =>
            {
                method((TIn1)values[0], (TIn2)values[1]);

                return ReturnNoResult();
            }, methodParameters);

            return this;
        }

        public IFactoryMethodConfiguration Action<TIn1, TIn2, TIn3>(string methodName, Action<TIn1, TIn2, TIn3> method, params IExposedMethodParameter[] methodParameters)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) =>
            {
                method((TIn1)values[0], (TIn2)values[1], (TIn3)values[2]);

                return ReturnNoResult();
            }, methodParameters);

            return this;
        }

        public IFactoryMethodConfiguration Action<TIn1, TIn2, TIn3, TIn4>(string methodName, Action<TIn1, TIn2, TIn3, TIn4> method, params IExposedMethodParameter[] methodParameters)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) =>
            {
                method((TIn1)values[0], (TIn2)values[1], (TIn3)values[2], (TIn4)values[3]);

                return ReturnNoResult();
            }, methodParameters);


            return this;
        }

        public IFactoryMethodConfiguration Func<TResult>(string methodName, Func<TResult> method)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) => ReturnResult(method()), null);

            return this;
        }

        public IFactoryMethodConfiguration Func<TIn, TResult>(string methodName, Func<TIn, TResult> method, params IExposedMethodParameter[] methodParameters)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) => ReturnResult(method((TIn)values[0])), 
                methodParameters);

            return this;
        }

        public IFactoryMethodConfiguration Func<TIn1, TIn2, TResult>(string methodName, Func<TIn1, TIn2, TResult> method, params IExposedMethodParameter[] methodParameters)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) => ReturnResult(method((TIn1)values[0], (TIn2)values[1])), 
                methodParameters);

            return this;
        }

        public IFactoryMethodConfiguration Func<TIn1, TIn2, TIn3, TResult>(string methodName, Func<TIn1, TIn2, TIn3, TResult> method, params IExposedMethodParameter[] methodParameters)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) => ReturnResult(method((TIn1)values[0], (TIn2)values[1], (TIn3)values[2])), 
                methodParameters);

            return this;
        }

        public IFactoryMethodConfiguration Func<TIn1, TIn2, TIn3, TIn4, TResult>(string methodName, Func<TIn1, TIn2, TIn3, TIn4, TResult> method, params IExposedMethodParameter[] methodParameters)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) => ReturnResult(method((TIn1)values[0], (TIn2)values[1], (TIn3)values[2], (TIn4)values[3])), 
                methodParameters);

            return this;
        }

        private Task<ResponseMessage> ReturnNoResult()
        {
            return Task.FromResult(new ResponseMessage());
        }

        private Task<ResponseMessage> ReturnResult<TResult>(TResult result)
        {
            return Task.FromResult<ResponseMessage>(new ResponseMessage<TResult>(result));
        }
    }
}
