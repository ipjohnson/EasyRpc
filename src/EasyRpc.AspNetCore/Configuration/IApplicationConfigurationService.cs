using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.Configuration
{
    public partial interface IApplicationConfigurationService
    {
        void AddConfigurationObject(object configurationObject);

        void ExposeType(ICurrentApiInformation currentApi, Type type, string name,
            List<IEndPointMethodAuthorization> authorizations, Func<MethodInfo, bool> methodFilter,
            string obsoleteMessage);


        IReadOnlyList<IEndPointMethodHandler> ProvideEndPointHandlers();
    }
}
