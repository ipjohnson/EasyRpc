using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasyRpc.AspNetCore.EndPoints
{
    /// <summary>
    /// Implementation for Clone on all generate types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequestParametersImplementations<T> : IRequestParameters where T : IRequestParameters, new()
    {
        IRequestParameters IRequestParameters.Clone()
        {
            var newT = new T();

            for (var i = 0; i < ParameterCount; i++)
            {
                newT[i] = this[i];
            }

            return newT;
        }
    }
}
