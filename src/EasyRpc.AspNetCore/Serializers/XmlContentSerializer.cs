using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.Errors;
using Microsoft.AspNetCore.WebUtilities;

namespace EasyRpc.AspNetCore.Serializers
{
    public class XmlContentSerializer : BaseSerializer
    {
        private ImmutableHashTree<Type, XmlSerializer> _serializers = ImmutableHashTree<Type, XmlSerializer>.Empty;
        private readonly string _contentType = "application/xml";
        private readonly int _memoryThreshold = 30 * 1024;

        /// <inheritdoc />
        public XmlContentSerializer(IErrorHandler errorHandler) : base(errorHandler)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<string> SupportedContentTypes => new[] { _contentType };

        /// <inheritdoc />
        public override bool CanDeserialize(RequestExecutionContext context, string contentType)
        {
            return string.Compare(contentType, 0, _contentType, 0, _contentType.Length, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        /// <inheritdoc />
        public override bool CanSerialize(RequestExecutionContext context, string accepts)
        {
            return accepts != null &&
                accepts.IndexOf(_contentType, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        /// <inheritdoc />
        public override async Task SerializeToResponse(RequestExecutionContext context)
        {
            var fileWriteStream = new FileBufferingWriteStream();

            SetStatusAndContent(context, _contentType);

            try
            {
                using var textWriter = new StreamWriter(fileWriteStream);

                using var xmlWriter = new XmlTextWriter(textWriter);

                var serializer = GetSerializer(context.Result.GetType());

                serializer.Serialize(xmlWriter, context.Result);

                xmlWriter.Flush();

                textWriter.Flush();

                await fileWriteStream.DrainBufferAsync(context.HttpContext.Response.Body, context.HttpContext.RequestAborted);
            }
            finally
            {
                await fileWriteStream.DisposeAsync();
            }
        }

        /// <inheritdoc />
        public override async ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context)
        {
            var fileReader = new FileBufferingReadStream(context.HttpContext.Request.Body, _memoryThreshold);

            context.HttpContext.Response.RegisterForDispose(fileReader);

            try
            {
                await fileReader.DrainAsync(context.HttpContext.RequestAborted);

                fileReader.Seek(0L, SeekOrigin.Begin);

                using var textReader = new StreamReader(fileReader);

                using var xmlReader = new XmlTextReader(textReader);

                var serializer = GetSerializer(typeof(T));

                return (T) serializer.Deserialize(xmlReader);
            }
            finally
            {
                await fileReader.DisposeAsync();
            }
        }

        private XmlSerializer GetSerializer(Type type)
        {
            var serializer = _serializers.GetValueOrDefault(type);

            if (serializer != null)
            {
                return serializer;
            }

            var returnSerializer = new XmlSerializer(type);

            ImmutableHashTree.ThreadSafeAdd(ref _serializers, type, returnSerializer,
                (value, newValue) => returnSerializer = value);

            return returnSerializer;
        }
    }
}
