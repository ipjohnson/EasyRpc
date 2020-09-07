using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using EasyRpc.AspNetCore.CodeGeneration;
using Microsoft.AspNetCore.Mvc.Routing;
using MsgPack = MessagePack;

namespace EasyRpc.AspNetCore.MessagePack
{
    public class MessagePackSerializationTypeAttributor : ISerializationTypeAttributor
    {
        private static readonly ConstructorInfo _messagePackAttrInfo;
        private static readonly ConstructorInfo _messagePackIntKeyAttrInfo;

        private readonly bool _useNameAsKey;

        static MessagePackSerializationTypeAttributor()
        {
            _messagePackAttrInfo = typeof(MsgPack.MessagePackObjectAttribute).GetConstructor(new[] {typeof(bool)});
            _messagePackIntKeyAttrInfo = typeof(MsgPack.KeyAttribute).GetConstructor(new [] {typeof(int)});
        }

        public MessagePackSerializationTypeAttributor(bool useNameAsKey = false)
        {
            _useNameAsKey = useNameAsKey;
        }

        /// <inheritdoc />
        public void AttributeType(TypeBuilder typeBuilder)
        {
            typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(_messagePackAttrInfo, new object[] { _useNameAsKey }));
        }

        /// <inheritdoc />
        public void AttributeProperty(PropertyBuilder propertyBuilder, int index)
        {
            if (!_useNameAsKey)
            {
                propertyBuilder.SetCustomAttribute(new CustomAttributeBuilder(_messagePackIntKeyAttrInfo, new object[] { index }));
            }
        }

        /// <inheritdoc />
        public void AttributeMethodType(TypeBuilder typeBuilder, IEndPointMethodConfigurationReadOnly methodConfiguration)
        {
            typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(_messagePackAttrInfo, new object[]{ _useNameAsKey}));
        }

        /// <inheritdoc />
        public void AttributeMethodProperty(PropertyBuilder propertyBuilder,
            IEndPointMethodConfigurationReadOnly methodConfiguration, 
            RpcParameterInfo parameterInfoParameter)
        {
            if (!_useNameAsKey && 
                parameterInfoParameter.ParameterSource == EndPointMethodParameterSource.PostParameter)
            {
                var position = GetPosition(methodConfiguration, parameterInfoParameter);

                propertyBuilder.SetCustomAttribute(new CustomAttributeBuilder(_messagePackIntKeyAttrInfo, new object[]{ position}));
            }
        }

        private int GetPosition(IEndPointMethodConfigurationReadOnly methodConfiguration, RpcParameterInfo parameterInfoParameter)
        {
            var position = 0;
            
            foreach (var currentParam in methodConfiguration.Parameters)
            {
                if (currentParam == parameterInfoParameter)
                {
                    break;
                }

                if (currentParam.ParameterSource == EndPointMethodParameterSource.PostParameter)
                {
                    position++;
                }
            }

            return position;
        }
    }
}
