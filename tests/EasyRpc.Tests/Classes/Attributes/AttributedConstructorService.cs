using System;
using System.Collections.Generic;
using System.Text;
using Grace.DependencyInjection.Attributes;
using EasyRpc.Tests.Classes.Simple;

namespace EasyRpc.Tests.Classes.Attributes
{
    public class AttributedConstructorService
    {
        [Import]
        public AttributedConstructorService(IBasicService basicService)
        {
            BasicService = basicService;
        }

        public AttributedConstructorService(IBasicService basicService, IMultipleService multipleService)
        {
            BasicService = basicService;
            MultipleService = multipleService;
        }

        public IBasicService BasicService { get; }

        public IMultipleService MultipleService { get; }

    }
}
