using EasyRpc.AspNetCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Binding;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore.Filters;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.Special
{
    public class NewDataTest : BaseRequestTest
    {
        public class SomeClass
        {
            public int TestValue { get; set; }
        }


        #region Service

        public class Service
        {
            [GetMethod("/new-data/{id}")]
            public int NewData([BindNewData] SomeClass someClass, int id)
            {
                return someClass.TestValue + id;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task ModelBinding_Special_NewDataTest()
        {
            var response = await Get("/new-data/10");

            var intValue = await Deserialize<int>(response);

            // 10 + 20 from filter
            Assert.Equal(30, intValue);
        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.ApplyFilter<SomeClassFilter>();
            api.Expose<Service>();
        }

        #endregion

        public class SomeClassFilter : IRequestExecutionFilter
        {
            /// <inheritdoc />
            public void BeforeExecute(RequestExecutionContext context)
            {
                if (context.Parameters.ParameterCount > 0 &&
                    context.Parameters[0] is SomeClass someClass)
                {
                    someClass.TestValue = 20;
                }
            }

            /// <inheritdoc />
            public void AfterExecute(RequestExecutionContext context)
            {
                
            }
        }
    }
}
