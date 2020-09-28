using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Routing
{

    public class TestAttribute : Attribute
    {

    }
    public class PostRouteTests
    {
        public void Testing<TIn,TResult>(Expression<Func<TIn,TResult>> expression)
        {
            
        }

        public void Testing<TIn, TResult>(Expression<Func<TIn, Task<TResult>>> expression)
        {

        }

        public void Testing<TIn, TResult>(Expression<Func<TIn, ValueTask<TResult>>> expression)
        {

        }


        public void Testing<TIn>(Expression<Func<TIn, Task>> expression)
        {
            Add(api => api.Get("/test", Test));
        }

        public class TestApi
        {
            public void Get<T>(string path, Func<T> action)
            {

            }
        }

        public delegate void RegisterApi(TestApi api);

        public void Add(Expression<RegisterApi> _)
        {

        }

        public int Test()
        {
            return 0;
        }

        

        [Fact]
        public void Testing2()
        {
            Testing((int x) => x + 1);
            Testing((int x) => Task.FromResult(x));
            Testing((int x) => new ValueTask<int>(x));
        }

        public void Test3(int value)
        {

        }
    }
}
