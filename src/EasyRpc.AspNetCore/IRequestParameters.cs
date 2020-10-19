using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// 
    /// </summary>
    public enum EndPointBindingType
    {
        InvokeParameter,

        InstanceProperty,

        Other
    }

    /// <summary>
    /// parameter sources
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
        /// Parameter is 
        /// </summary>
        HeaderParameter,

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
        int Position { get; }

        /// <summary>
        /// Binding type (parameter, property, other)
        /// </summary>
        EndPointBindingType BindingType { get; }

        /// <summary>
        /// Source for parameter
        /// </summary>
        EndPointMethodParameterSource ParameterSource { get; }
    }

    /// <summary>
    /// Rpc parameter object
    /// </summary>
    public class RpcParameterInfo : IRpcParameterInfo
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public Type ParamType { get; set; }

        /// <summary>
        /// Type name of property
        /// </summary>
        public string TypeName
        {
            get => ParamType?.FullName;
            set { }
        }

        /// <inheritdoc />
        public object DefaultValue { get; set; }

        /// <inheritdoc />
        public bool HasDefaultValue { get; set; }

        /// <inheritdoc />
        public int Position { get; set; }

        /// <inheritdoc />
        public EndPointBindingType BindingType { get; set; } = EndPointBindingType.InvokeParameter;

        /// <inheritdoc />
        public EndPointMethodParameterSource ParameterSource { get; set; }
    }

    /// <summary>
    /// parameters for request 
    /// </summary>
    public interface IRequestParameters
    {
        /// <summary>
        /// Try getting parameter by name
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        bool TryGetParameter(string parameterName, out object parameterValue);

        /// <summary>
        /// Try setting parameter value by name
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        bool TrySetParameter(string parameterName, object parameterValue);

        /// <summary>
        /// List of parameter info object for the call
        /// </summary>
        IReadOnlyList<IRpcParameterInfo> ParameterInfos { get; }

        /// <summary>
        /// Access parameters based on index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object this[int index] { get; set; }

        /// <summary>
        /// Count of parameters
        /// </summary>
        int ParameterCount { get; }

        /// <summary>
        /// Clone parameters
        /// </summary>
        /// <returns></returns>
        IRequestParameters Clone();

        /// <summary>
        /// Access parameters by name
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
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
