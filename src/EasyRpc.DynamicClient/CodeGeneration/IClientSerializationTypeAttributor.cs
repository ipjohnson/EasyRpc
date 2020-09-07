using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace EasyRpc.DynamicClient.CodeGeneration
{
    public interface IClientSerializationTypeAttributor
    {
        void AttributeType(TypeBuilder typeBuilder);

        void AttributeProperty(PropertyBuilder propertyBuilder, int index);
    }
}
