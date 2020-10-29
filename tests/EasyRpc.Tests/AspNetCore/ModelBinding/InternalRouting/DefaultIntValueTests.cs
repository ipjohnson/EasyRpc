using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.InternalRouting
{
    public class DefaultIntValueTests : BaseRequestTest
    {
        private const int _defaultValue = 5;

        #region Tests

        [Fact]
        public async Task ModelBinding_InternalRouting_DefaultIntValue_NoValue()
        {
            var response = await Get("/Service/IntValue");

            var value = await Deserialize<int>(response);

            Assert.Equal(_defaultValue, value);
        }

        [Fact]
        public async Task ModelBinding_InternalRouting_DefaultIntValue_TrailingSlash()
        {
            var response = await Get("/Service/IntValue/");

            var value = await Deserialize<int>(response);

            Assert.Equal(_defaultValue, value);
        }
        
        [Fact]
        public async Task ModelBinding_InternalRouting_DefaultIntValue_String()
        {
            var response = await Get("/Service/IntValue/BlahBlah");

            var value = await Deserialize<int>(response);

            Assert.Equal(_defaultValue, value);
        }
        
        [Fact]
        public async Task ModelBinding_InternalRouting_DefaultIntValue_Value()
        {
            var response = await Get("/Service/IntValue/15");

            var value = await Deserialize<int>(response);

            Assert.Equal(15, value);
        }

        #endregion

        #region registration

        protected override bool UseInternalRouting => true;

        protected override void ApiRegistration(IRpcApi api)
        {
            api.DefaultHttpMethod(ExposeDefaultMethod.PostAndGetInt);
            api.Expose<Service>();
        }

        #endregion

        #region Service

        public class Service
        {
            public int IntValue(int value = _defaultValue)
            {
                return value;
            }
        }

        private static int TestValue()
        {
            return _defaultValue;
        }

        #endregion
    }
}
