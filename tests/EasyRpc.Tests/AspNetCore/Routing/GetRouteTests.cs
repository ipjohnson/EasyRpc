using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Routing
{
    public class GetRouteTests : BaseRequestTest
    {
        public class PersonnelModel
        {
            public int PersonnelId { get; set; }

            public string Name { get; set; }
        }

        [Fact]
        public async Task Routing_GetRoute()
        {
            var response = await Get("/Personnel");

            var result = await Deserialize<List<PersonnelModel>>(response);

            Assert.NotNull(result);
            Assert.Single(result);

            var instanceResponse = await Get("/Personnel/123456");

            var instanceResult = await Deserialize<PersonnelModel>(instanceResponse);

            Assert.NotNull(instanceResult);
            Assert.Equal(123456, instanceResult.PersonnelId);
        }


        protected override void ApiRegistration(IRpcApi api)
        {
            api.Method.Get("/Personnel",
                () => new List<PersonnelModel> {new PersonnelModel {PersonnelId = 1, Name = "Ted Smith"}});

            api.Method.Get("/Personnel/{personnelId}", (int personnelId) => new PersonnelModel{ PersonnelId = personnelId, Name = "Test"});
        }
    }
}
