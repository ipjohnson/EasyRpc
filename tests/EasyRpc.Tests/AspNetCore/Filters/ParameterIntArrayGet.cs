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
    public class ParameterIntArrayGet : BaseRequestTest
    {
        private static readonly int x = 10;
        private static readonly int y = 20;
        private static readonly int z = 30;

        #region Tests

        [Fact]
        public async Task Filters_ParameterIntArrayGet()
        {
            var response = await Post("/Service/AddValues", new {x, y, z});

            var value = await Deserialize<GenericResult<int>>(response);

            Assert.NotNull(value);
            Assert.Equal(60, value.Result);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.DefaultHttpMethod(ExposeDefaultMethod.PostOnly);
            api.ApplyFilter<Filter>();
            api.Expose<Service>();
        }

        #endregion

        #region Service

        public class Service
        {
            public int AddValues(int x, int y, int z)
            {
                return x + y + z;
            }
        }

        #endregion

        #region Filter

        public class Filter : IRequestExecutionFilter
        {
            public void BeforeExecute(RequestExecutionContext context)
            {
                Assert.Equal(3, context.Parameters.ParameterCount);
                Assert.Equal(3, context.Parameters.ParameterInfos.Count);

                var count = 0;

                foreach (var rpcParameterInfo in context.Parameters.ParameterInfos)
                {
                    var value = context.Parameters[rpcParameterInfo.Position];

                    switch (rpcParameterInfo.Name)
                    {
                        case nameof(x):
                            Assert.Equal(x, value);
                            break;
                        case nameof(y):
                            Assert.Equal(y, value);
                            break;
                        case nameof(z):
                            Assert.Equal(z, value);
                            break;
                        default:
                            throw new Exception($"Unknown parameter {rpcParameterInfo.Name}");
                    }

                    count++;
                }

                Assert.Equal(3, count);
            }

            public void AfterExecute(RequestExecutionContext context)
            {
                
            }
        }


        #endregion

    }
}
