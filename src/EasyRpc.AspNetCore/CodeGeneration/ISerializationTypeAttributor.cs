using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace EasyRpc.AspNetCore.CodeGeneration
{
    /// <summary>
    /// Used by 3rd party serializer to attribute type for serialization
    /// </summary>
    public interface ISerializationTypeAttributor
    {
        void AttributeType(TypeBuilder typeBuilder);

        void AttributeProperty(PropertyBuilder propertyBuilder, int index);

        void AttributeMethodType(TypeBuilder typeBuilder, IEndPointMethodConfigurationReadOnly methodConfiguration);

        void AttributeMethodProperty(PropertyBuilder propertyBuilder,
            IEndPointMethodConfigurationReadOnly methodConfiguration, 
            RpcParameterInfo parameterInfoParameter);
    }
}
