using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;

namespace EasyRpc.AspNetCore.Middleware
{
    public class FactoryMethodConfiguration : IFactoryMethodConfiguration
    {
        private Action<string, Delegate, InvokeMethodWithArray> _addMethod;

        public FactoryMethodConfiguration(Action<string, Delegate, InvokeMethodWithArray> addMethod)
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
            });

            return this;
        }

        public IFactoryMethodConfiguration Action<TIn>(string methodName, Action<TIn> method)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) =>
            {
                method((TIn)values[0]);

                return ReturnNoResult();
            });

            return this;
        }

        public IFactoryMethodConfiguration Action<TIn1, TIn2>(string methodName, Action<TIn1, TIn2> method)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) =>
            {
                method((TIn1)values[0], (TIn2)values[1]);

                return ReturnNoResult();
            });

            return this;
        }

        public IFactoryMethodConfiguration Action<TIn1, TIn2, TIn3>(string methodName, Action<TIn1, TIn2, TIn3> method)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) =>
            {
                method((TIn1)values[0], (TIn2)values[1], (TIn3)values[2]);

                return ReturnNoResult();
            });

            return this;
        }

        public IFactoryMethodConfiguration Action<TIn1, TIn2, TIn3, TIn4>(string methodName,
            Action<TIn1, TIn2, TIn3, TIn4> method)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) =>
            {
                method((TIn1)values[0], (TIn2)values[1], (TIn3)values[2], (TIn4)values[3]);

                return ReturnNoResult();
            });


            return this;
        }

        public IFactoryMethodConfiguration Func<TResult>(string methodName, Func<TResult> method)
        {
            _addMethod(methodName, method, (instance, values, version, id) => ReturnResult(method()));

            return this;
        }

        public IFactoryMethodConfiguration Func<TIn, TResult>(string methodName, Func<TIn, TResult> method)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) => ReturnResult(method((TIn)values[0])));

            return this;
        }

        public IFactoryMethodConfiguration Func<TIn1, TIn2, TResult>(string methodName, Func<TIn1, TIn2, TResult> method)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) => ReturnResult(method((TIn1)values[0], (TIn2)values[1])));

            return this;
        }

        public IFactoryMethodConfiguration Func<TIn1, TIn2, TIn3, TResult>(string methodName, Func<TIn1, TIn2, TIn3, TResult> method)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) => ReturnResult(method((TIn1)values[0], (TIn2)values[1], (TIn3)values[2])));

            return this;
        }

        public IFactoryMethodConfiguration Func<TIn1, TIn2, TIn3, TIn4, TResult>(string methodName, Func<TIn1, TIn2, TIn3, TIn4, TResult> method)
        {
            _addMethod(methodName,
                method,
                (instance, values, version, id) => ReturnResult(method((TIn1)values[0], (TIn2)values[1], (TIn3)values[2], (TIn4)values[3])));

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
