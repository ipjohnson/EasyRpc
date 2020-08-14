using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.SimpleServices;

namespace EasyRpc.Tests.AspNetCore.Errors
{
    public class ContentTypeErrorTests : BaseRequestTest
    {
        

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<AttributedIntMathService>();
        }
    }
}
