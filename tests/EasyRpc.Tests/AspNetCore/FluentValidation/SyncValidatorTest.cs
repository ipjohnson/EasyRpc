using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.FluentValidation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.FluentValidation
{
    public class SyncValidatorTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            public Model CheckModel(Model model)
            {
                Assert.True(model.IntValue > 0);

                return model;
            }
        }

        public class Model
        {
            public int IntValue { get; set; }
        }

        public class ModelValidator : AbstractValidator<Model>
        {
            public ModelValidator()
            {
                RuleFor(m => m.IntValue).Must( (model, token) => model.IntValue > 0);
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task FluentValidation_SyncValidator()
        {
            var model = new Model();

            var response = await Post("/service/checkmodel", model);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            model.IntValue = 1;

            response = await Post("/service/checkmodel", model);

            var result = await Deserialize<Model>(response);

            Assert.NotNull(result);
            Assert.Equal(model.IntValue, result.IntValue);
        }

        #endregion

        #region Registration

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddSingleton<IValidator<Model>, ModelValidator>();
        }

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.ApplyFluentValidation();
            api.Expose<Service>();
        }

        #endregion
    }
}
