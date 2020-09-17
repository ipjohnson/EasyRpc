using System;

namespace EasyRpc.Tests.Classes.Simple
{
    public interface IUniqueInstanceService
    {
        Guid UniqueId { get; }
    }

    public class UniqueInstanceService : IUniqueInstanceService
    {
        public Guid UniqueId { get; } = Guid.NewGuid();
    }
}
