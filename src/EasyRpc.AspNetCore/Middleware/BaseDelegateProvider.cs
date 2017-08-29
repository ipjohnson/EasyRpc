using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Middleware
{
    public abstract class BaseDelegateProvider
    {
        /// <summary>
        /// Generates IL for From services, it's assumed that HttpContext is the 5 arg to the delegate
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ilGenerator"></param>
        protected void GenerateIlForFromServices(ParameterInfo info, ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_S, 4);

            var openMethod = typeof(BaseDelegateProvider).GetRuntimeMethod("GetValueFromServices",
                new[] { typeof(HttpContext) });

            ilGenerator.EmitMethodCall(openMethod.MakeGenericMethod(info.ParameterType));
        }
        
        public static T GetValueFromServices<T>(HttpContext context)
        {
            return context.RequestServices.GetService<T>();
        }
    }
}
