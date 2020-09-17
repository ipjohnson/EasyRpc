using Grace.DependencyInjection.Attributes;

namespace EasyRpc.Tests.Classes.Attributes
{
	public interface IAttributedOpenGenericTransient<T>
	{
	}

	[Export(typeof(IAttributedOpenGenericTransient<>))]
	public class AttributedOpenGenericTransient<T> : IAttributedOpenGenericTransient<T>
	{
	}
}