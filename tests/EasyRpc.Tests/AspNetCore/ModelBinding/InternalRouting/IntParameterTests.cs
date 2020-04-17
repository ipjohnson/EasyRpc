using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.InternalRouting
{
    public class IntParameterTests : BaseRequestTest
    {
        #region Service

        public class Service
        {
            public int OneParam(int id)
            {
                return id;
            }

            public int TwoParam(int id1, int id2)
            {
                return id1 + id2;
            }


            public int ThreeParam(int id1, int id2, int id3)
            {
                return id1 + id2 + id3;
            }
        }
        #endregion

        #region Tests

        [Fact]
        public async Task ModelBinding_InternalRouting_OneIntParam()
        {
            var arg1 = 5;

            var response = await Get($"/{nameof(Service)}/{nameof(Service.OneParam)}/{arg1}");

            var value = await Deserialize<GenericResult<int>>(response);

            Assert.NotNull(value);
            Assert.Equal(arg1, value.Result);
        }
        
        [Fact]
        public async Task ModelBinding_InternalRouting_TwoIntParam()
        {
            var arg1 = 5;
            var arg2 = 10;

            var response = await Get($"/{nameof(Service)}/{nameof(Service.TwoParam)}/{arg1}/{arg2}");

            var value = await Deserialize<GenericResult<int>>(response);

            Assert.NotNull(value);
            Assert.Equal(arg1 + arg2, value.Result);
        }
        
        [Fact]
        public async Task ModelBinding_InternalRouting_ThreeIntParam()
        {
            var arg1 = 5;
            var arg2 = 10;
            var arg3 = 15;

            var response = await Get($"/{nameof(Service)}/{nameof(Service.ThreeParam)}/{arg1}/{arg2}/{arg3}");

            var value = await Deserialize<GenericResult<int>>(response);

            Assert.NotNull(value);
            Assert.Equal(arg1 + arg2 + arg3, value.Result);
        }

        #endregion


        #region Registration
        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.DefaultHttpMethod(ExposeDefaultMethod.PostAndGetInt);
            api.Expose<Service>();
        }
        #endregion
    }
}
