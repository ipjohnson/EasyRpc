using Grace.DependencyInjection.Attributes;

namespace EasyRpc.Tests.Classes.Attributes
{
    public interface IAttributedSingletonPerScopeService
    {
        
    }

    [ExportByInterfaces]
    [SingletonPerScope]
    public class AttributedSingletonPerScopeService : IAttributedSingletonPerScopeService
    {
    }
}
