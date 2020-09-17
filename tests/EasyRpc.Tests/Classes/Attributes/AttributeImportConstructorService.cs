﻿using Grace.DependencyInjection.Attributes;
using Xunit;

namespace EasyRpc.Tests.Classes.Attributes
{
	public interface IAttributeImportConstructorService
	{
	}

	[Export(typeof(IAttributeImportConstructorService))]
	public class AttributeImportConstructorService : IAttributeImportConstructorService
	{
		public AttributeImportConstructorService(IAttributeBasicService basicService)
		{
			Assert.NotNull(basicService);
		}
	}
}