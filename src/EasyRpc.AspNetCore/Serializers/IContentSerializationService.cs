using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Serializers
{
    public interface IContentSerializationService
    {
        IEnumerable<string> SupportedContentTypes { get; }

        Task SerializeToResponse(RequestExecutionContext context);

        ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context);
    }
}
