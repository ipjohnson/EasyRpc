using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using EasyRpc.DynamicClient;
using EasyRpc.Sample.Interfaces;

namespace EasyRpc.Client.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var pg = new Castle.DynamicProxy.ProxyGenerator();

            var mathService = (IIntMathService)pg.CreateInterfaceProxyWithoutTarget(typeof(IIntMathService), CreateInterceptor());

            var value = mathService.Add(10, 20);

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            for(int i = 0; i < 10000; i++)
                value = mathService.Add(50, 10);

            stopwatch.Stop();

            File.AppendAllText(@"c:\temp\output.txt",stopwatch.ElapsedMilliseconds.ToString() + Environment.NewLine);
        }
        

        private static IInterceptor CreateInterceptor()
        {
            return new DynamicClientInterceptor(new RpcClientProvider (), new DefaultNamingConventionService());
        }
    }
}
