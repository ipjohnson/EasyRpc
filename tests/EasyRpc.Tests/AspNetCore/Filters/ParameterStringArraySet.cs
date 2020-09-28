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
    public class ParameterStringArraySet : BaseRequestTest
    {
        private static readonly int x = 10;
        private static readonly int y = 20;
        private static readonly int z = 30;

        #region Tests

        [Fact]
        public async Task Filters_ParameterStringArrayGet()
        {
            var response = await Post("/Service/AddValues", new { x, y, z });

            var value = await Deserialize<int>(response);

            Assert.Equal(75, value);
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
                    var value = context.Parameters[rpcParameterInfo.Name];

                    if (value is int intValue)
                    {
                        context.Parameters[rpcParameterInfo.Name] = intValue + 5;
                    }
                    else
                    {
                        throw new Exception($"Value should be int but is {value.GetType().FullName}");
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
