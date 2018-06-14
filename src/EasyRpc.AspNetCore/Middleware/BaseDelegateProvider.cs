using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Middleware
{
    public abstract class BaseDelegateProvider<TValues,TDelegate>
    {
        public TDelegate CreateDelegate(MethodInfo method)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                typeof(object[]),
                new[] { typeof(TValues), typeof(HttpContext) },
                GetType().GetTypeInfo().Module);

            var ilGenerator = dynamicMethod.GetILGenerator();

            GenerateMethod(method, ilGenerator);

            return (TDelegate)((object)dynamicMethod.CreateDelegate(typeof(TDelegate)));
        }

        private void GenerateMethod(MethodInfo method, ILGenerator ilGenerator)
        {
            var parameters = method.GetParameters();

            ilGenerator.EmitInt(parameters.Length);

            ilGenerator.Emit(OpCodes.Newarr, typeof(object));

            var index = 0;
            var parameterIndex = 0;

            foreach (var parameter in parameters)
            {
                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.EmitInt(index);

                index++;

                var fromServices = parameter.GetCustomAttributes().Any(a => a.GetType().Name == "FromServicesAttribute");

                if (fromServices)
                {
                    GenerateIlForFromServices(parameter, ilGenerator);
                }
                else
                {
                    GenerateIlForParameter(parameter, ilGenerator, parameterIndex);
                    parameterIndex++;
                }

                if (!parameter.ParameterType.IsByRef)
                {
                    ilGenerator.Emit(OpCodes.Box, parameter.ParameterType);
                }

                ilGenerator.Emit(OpCodes.Stelem_Ref);
            }

            ilGenerator.Emit(OpCodes.Ret);
        }

        protected abstract void GenerateIlForParameter(ParameterInfo parameter, ILGenerator ilGenerator, int parameterIndex);

        /// <summary>
        /// Generates IL for From services, it's assumed that HttpContext is the 5 arg to the delegate
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ilGenerator"></param>
        protected void GenerateIlForFromServices(ParameterInfo info, ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_S, 4);

            var openMethod = typeof(BaseDelegateProvider<TValues,TDelegate>).GetRuntimeMethod("GetValueFromServices",
                new[] { typeof(HttpContext) });

            ilGenerator.EmitMethodCall(openMethod.MakeGenericMethod(info.ParameterType));
        }
        
        public static T GetValueFromServices<T>(HttpContext context)
        {
            return context.RequestServices.GetService<T>();
        }
    }
}
