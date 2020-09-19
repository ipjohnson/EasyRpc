using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Documentation;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Documentation
{
    public class KnownOpenApiTypeMapperTests
    {
        [Fact]
        public void Documentation_KnownOpenApiTypeMapper()
        {
            var typeMapper = new KnownOpenApiTypeMapper();

            foreach (var mappedType in typeMapper.MappedTypes)
            {
                var mapping = typeMapper.GetMapping(mappedType);

                Assert.NotNull(mapping);
            }
        }
    }
}
