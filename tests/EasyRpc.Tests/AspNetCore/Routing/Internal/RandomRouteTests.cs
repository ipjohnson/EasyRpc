using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Routing.Internal
{
    public class RandomRouteTests : BaseRequestTest
    {
        private readonly Random _random = new Random();
        private List<string> _routes;

        #region Tests

        [Fact]
        public async Task Routing_Internal_RandomRoutes()
        {
            _routes = GenerateRouteList();

            foreach (var route in _routes)
            {
                var response = await Get(route);

                var stringValue = await Deserialize<string>(response);

                Assert.Equal(route, stringValue);
            }
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            foreach (var route in _routes)
            {
                api.GetMethod(route, () => route);
            }
        }

        private List<string> GenerateRouteList()
        {
            var routes = new List<string>();

            for (var i = 0; i < 3000; i++)
            {
                var path = GeneratePathString();

                routes.Add(path);
            }

            return routes;
        }

        private string GeneratePathString()
        {
            var builder = new StringBuilder("/");

            var count = _random.Next(10, 20);

            for (var i = 0; i < count; i++)
            {
                var value = (char) _random.Next(65, 90);

                builder.Append(value);
            }

            return builder.ToString();
        }

        #endregion
    }
}
