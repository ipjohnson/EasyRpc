using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyRpc.Abstractions.Encoding;

namespace EasyRpc.AspNetCore.ContentEncoding
{
    public class DefaultCompressionPredicateProvider : ICompressionPredicateProvider
    {
        public Action<RequestExecutionContext> ProvideCompressionPredicate(IEndPointMethodConfigurationReadOnly configuration)
        {
            if (configuration.ReturnType == typeof(string))
            {
                return StringCompressionCheck;
            }

            if (configuration.ReturnType.IsAssignableFrom(typeof(IList)))
            {
                var compressAttribute = (CompressAttribute)
                    configuration.InvokeInformation.MethodToInvoke?.GetCustomAttributes()
                        .FirstOrDefault(a => a is CompressAttribute);

                var min = compressAttribute?.Min ?? 1;

                return new ListCompressionCheck(min).CompressionCheck;
            }

            return NullCompressionCheck;
        }

        public static void StringCompressionCheck(RequestExecutionContext context)
        {
            if (context.Result is string stringResult &&
                stringResult.Length > 1500)
            {
                context.CanCompress = true;
            }
        }

        public static void NullCompressionCheck(RequestExecutionContext context)
        {
            context.CanCompress = context.Result != null;
        }

        public class ListCompressionCheck
        {
            private int _min;

            public ListCompressionCheck(int min)
            {
                _min = min > 0 ? min : 1;
            }

            public void CompressionCheck(RequestExecutionContext context)
            {
                if (context.Result is IList list)
                {
                    context.CanCompress = list.Count > _min;
                }
            }
        }
    }
}
