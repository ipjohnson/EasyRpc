using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Serialization;
using EasyRpc.AspNetCore.CodeGeneration;

namespace EasyRpc.AspNetCore.Serializers
{
    public class XmlContentSerializerTypeAttributor : ISerializationTypeAttributor
    {
        private static readonly ConstructorInfo _xmlTypeAttributeConstructorInfo;

        static XmlContentSerializerTypeAttributor()
        {
            _xmlTypeAttributeConstructorInfo = typeof(XmlTypeAttribute).GetConstructor(new[] {typeof(string)});
        }

        /// <inheritdoc />
        public void AttributeType(TypeBuilder typeBuilder, string classNameHint)
        {
            var attr =
                new CustomAttributeBuilder(_xmlTypeAttributeConstructorInfo,
                    new object[] {classNameHint ?? typeBuilder.Name});

            typeBuilder.SetCustomAttribute(attr);
        }

        /// <inheritdoc />
        public void AttributeProperty(PropertyBuilder propertyBuilder, int index)
        {

        }

        /// <inheritdoc />
        public void AttributeMethodType(TypeBuilder typeBuilder, IEndPointMethodConfigurationReadOnly methodConfiguration)
        {
            typeBuilder.SetCustomAttribute(
                new CustomAttributeBuilder(_xmlTypeAttributeConstructorInfo, new object[] { "args" }));
        }

        /// <inheritdoc />
        public void AttributeMethodProperty(PropertyBuilder propertyBuilder, IEndPointMethodConfigurationReadOnly methodConfiguration,
            RpcParameterInfo parameterInfoParameter)
        {

        }
    }
}
