using System;
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
    public class StandardFilterTests : BaseRequestTest
    {
        #region Tests

        [Fact]
        public async Task Filters_ApplyFilter()
        {
            var stringA = "Hello";
            var stringB = " World";

            var response = await Get($"/ConcatStrings/{stringA}/{stringB}");

            var value = await Deserialize<string>(response);

            Assert.NotNull(value);
            Assert.Equal(stringA + stringB, value);

            var parameters = Shared.Items["Parameters"] as IRequestParameters;

            Assert.NotNull(parameters);
            Assert.Equal(stringA, parameters[nameof(stringA)]);
            Assert.Equal(stringB, parameters[nameof(stringB)]);

            var filterResult = Shared.Items["Result"] as string;

            Assert.NotNull(filterResult);
            Assert.Equal(stringA + stringB, filterResult);
        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.ApplyFilter<Filter>();
            api.Expose<Service>();
        }

        #endregion

        #region Service

        public class Service
        {
            [GetMethod("/ConcatStrings/{stringA}/{stringB}")]
            public string ConcatStrings(string stringA, string stringB)
            {
                return stringA + stringB;
            }
        }

        #endregion

        #region Filter

        public class Filter : IRequestExecutionFilter
        {
            private readonly ISharedStorage _sharedStorage;

            public Filter(ISharedStorage sharedStorage)
            {
                _sharedStorage = sharedStorage;
            }

            public void BeforeExecute(RequestExecutionContext context)
            {
                _sharedStorage.Items["Parameters"] = context.Parameters;
            }

            public void AfterExecute(RequestExecutionContext context)
            {
                _sharedStorage.Items["Result"] = context.Result;
            }
        }

        #endregion
    }
}
