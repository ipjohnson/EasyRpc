using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.Errors;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Errors
{
    public class ErrorResultTypeCreatorTests
    {
        [Fact]
        public void Errors_ResultTypeCreator()
        {
            var errorTypeCreator = new ErrorResultTypeCreator(Array.Empty<ISerializationTypeAttributor>());

            var type = errorTypeCreator.GenerateErrorType();

            var wrapper = Activator.CreateInstance(type) as IErrorWrapper;

            wrapper.Message = "My Message";

            var serialized = JsonSerializer.Serialize(wrapper);
        }
    }
}
