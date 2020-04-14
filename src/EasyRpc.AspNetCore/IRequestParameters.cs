using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// 
    /// </summary>
    public enum EndPointMethodParameterSource
    {
        /// <summary>
        /// Parameter is part of an object in the request body
        /// </summary>
        PostParameter,

        /// <summary>
        /// Parameter is the whole request body
        /// </summary>
        PostBody,

        /// <summary>
        /// Parameter is part of the url
        /// </summary>
        PathParameter,

        /// <summary>
        /// Parameter is part of the query string
        /// </summary>
        QueryStringParameter,
        
        /// <summary>
        /// Parameter should be resolved from the request service scope
        /// </summary>
        RequestServices,

        /// <summary>
        /// Parameter should be RequestExecutionContext
        /// </summary>
        RequestExecutionContext = 100,

        /// <summary>
        /// Parameter is HttpContext
        /// </summary>
        HttpContext = 101,

        /// <summary>
        /// Parameter is HttpRequest
        /// </summary>
        HttpRequest = 102,

        /// <summary>
        /// Parameter is HttpResponse
        /// </summary>
        HttpResponse = 103,

        /// <summary>
        /// Connection abort requested cancellation token
        /// </summary>
        HttpCancellationToken = 104
    }

    /// <summary>
    /// Parameter info for method
    /// </summary>
    public interface IRpcParameterInfo
    {
        /// <summary>
        /// Name of parameter
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Parameter type
        /// </summary>
        Type ParamType { get; }

        /// <summary>
        /// Default value for parameter
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// Has default value
        /// </summary>
        bool HasDefaultValue { get; }

        /// <summary>
        /// Parameter index
        /// </summary>
        int Position { get;  }

        /// <summary>
        /// Source for parameter
        /// </summary>
        EndPointMethodParameterSource ParameterSource { get; }
    }

    public class RpcParameterInfo : IRpcParameterInfo
    {
        public string Name { get; set; }

        [JsonIgnore]
        public Type ParamType { get; set; }

        public string TypeName
        {
            get => ParamType?.FullName;
            set { }
        }

        public object DefaultValue { get; set; }

        public bool HasDefaultValue { get; set; }

        public int Position { get; set; }

        public EndPointMethodParameterSource ParameterSource { get; set; }
    }


    public interface IRequestParameters
    {
        bool TryGetParameter(string parameterName, out object parameterValue);

        bool TrySetParameter(string parameterName, object parameterValue);

        IReadOnlyList<IRpcParameterInfo> ParameterInfos { get; }

        object this[int index] { get; set; }

        int ParameterCount { get; }

        IRequestParameters Clone();

        object this[string parameterName] 
        {
            get
            {
                if (TryGetParameter(parameterName, out var value))
                {
                    return value;
                }

                throw new KeyNotFoundException($"Parameter context does not have parameter named {parameterName}");
            }
            set
            {
                if (!TrySetParameter(parameterName, value))
                {
                    throw new KeyNotFoundException($"Parameter context does not have parameter named {parameterName}");
                }
            }
        }
    }
}
