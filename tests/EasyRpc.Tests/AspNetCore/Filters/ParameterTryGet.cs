﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Filters
{
    public class ParameterTryGet : BaseRequestTest
    {
        private static readonly int x = 10;
        private static readonly int y = 20;
        private static readonly int z = 30;

        #region Tests

        [Fact]
        public async Task Filters_ParameterTryAccess()
        {
            var response = await Post("/Service/AddValues", new { x, y, z });

            var value = await Deserialize<int>(response);

            Assert.Equal(60, value);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
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
                    if (context.Parameters.TryGetParameter(rpcParameterInfo.Name, out var value))
                    {
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
                    else
                    {
                        throw new Exception($"Could not find parameter {rpcParameterInfo.Name} in parameters");
                    }
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
