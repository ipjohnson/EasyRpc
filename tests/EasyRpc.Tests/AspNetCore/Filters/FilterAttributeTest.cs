using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Filters
{
    public class FilterAttributeTest : BaseRequestTest
    {
        #region Tests

        [Fact]
        public async Task Filters_FilterAttribute()
        {
            var a = 5;
            var b = 15;

            var response = await Post("/Service/Add", new {a, b});

            var value = await Deserialize<int>(response);

            Assert.NotNull(value);
            Assert.Equal((a + 10) + (b + 10), value);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<Service>();
        }

        #endregion

        #region Service

        public class Service
        {
            [AddTen]
            public int Add(int a, int b)
            {
                return a + b;
            }
        }

        #endregion

        #region Filter

        public class AddTenAttribute : InstanceFilterAttribute
        {
            public AddTenAttribute() : base(typeof(AddTenFilter))
            {
            }
        }

        public class AddTenFilter : IRequestExecutionFilter
        {
            public void BeforeExecute(RequestExecutionContext context)
            {
                foreach (var parameterInfo in context.Parameters.ParameterInfos)
                {
                    if (parameterInfo.ParamType == typeof(int))
                    {
                        var intValue = (int) context.Parameters[parameterInfo.Position];

                        context.Parameters[parameterInfo.Position] = intValue + 10;
                    }
                }
            }

            public void AfterExecute(RequestExecutionContext context)
            {
                
            }
        }

        #endregion
    }
}
