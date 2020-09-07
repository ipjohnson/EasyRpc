using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using EasyRpc.DynamicClient.CodeGeneration;
using MsgPack = MessagePack;

namespace EasyRpc.DynamicClient.MessagePack
{
    public class MessagePackClientSerializationTypeAttributor : IClientSerializationTypeAttributor
    {
        private static readonly ConstructorInfo _messagePackAttrInfo;
        private static readonly ConstructorInfo _messagePackIntKeyAttrInfo;

        private readonly bool _useNameAsKey;

        static MessagePackClientSerializationTypeAttributor()
        {
            _messagePackAttrInfo = typeof(MsgPack.MessagePackObjectAttribute).GetConstructor(new[] { typeof(bool) });
            _messagePackIntKeyAttrInfo = typeof(MsgPack.KeyAttribute).GetConstructor(new[] { typeof(int) });
        }

        public MessagePackClientSerializationTypeAttributor(bool useNameAsKey = false)
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
    }
}
