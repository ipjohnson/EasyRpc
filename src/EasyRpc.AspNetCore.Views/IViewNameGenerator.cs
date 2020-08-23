using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Configuration;

namespace EasyRpc.AspNetCore.Views
{
    public interface IViewNameGenerator
    {
        /// <summary>
        /// Generate view name based off an endpoint 
        /// </summary>
        /// <param name="currentApi"></param>
        /// <param name="configurationReadOnly"></param>
        /// <returns></returns>
        string GenerateName(ICurrentApiInformation currentApi, IEndPointMethodConfigurationReadOnly configurationReadOnly);
    }
}
