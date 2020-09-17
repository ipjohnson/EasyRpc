﻿using Grace.DependencyInjection.Attributes;
using EasyRpc.Tests.Classes.Simple;

namespace EasyRpc.Tests.Classes.Attributes
{
	[SomeTest(TestValue = 5)]
	[Export(typeof(IAttributedSimpleObject))]
	public class AttributedSimpleObjectA : IAttributedSimpleObject
	{
	}

	[SomeTest]
	[Export(typeof(IAttributedSimpleObject))]
	public class AttributedSimpleObjectB : IAttributedSimpleObject
	{
	}

	[SomeTest]
	[Export(typeof(IAttributedSimpleObject))]
	public class AttributedSimpleObjectC : IAttributedSimpleObject
	{
	}

	[Export(typeof(IAttributedSimpleObject))]
	public class AttributedSimpleObjectD : IAttributedSimpleObject
	{
	}

	[Export(typeof(IAttributedSimpleObject))]
	public class AttributedSimpleObjectE : IAttributedSimpleObject
	{
	}
}