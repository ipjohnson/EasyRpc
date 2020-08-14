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
