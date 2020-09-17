using System;
using Grace.DependencyInjection.Attributes;

namespace EasyRpc.Tests.Classes.Attributes
{
    [Export]
    [SingletonPerObjectGraph]
    public class AttributedSingletonPerObjectGraphService
    {
        public Guid UniqueId { get; } = Guid.NewGuid();
    }
}
