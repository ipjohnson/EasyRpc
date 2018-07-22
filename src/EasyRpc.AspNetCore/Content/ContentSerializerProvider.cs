using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Content
{
    /// <summary>
    /// Provides serializer
    /// </summary>
    public interface IContentSerializerProvider
    {
        IContentSerializer DefaultSerializer { get; }

        /// <summary>
        /// Provide serializer based on http context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IContentSerializer GetSerializer(HttpContext context);

        /// <summary>
        /// Configure serializers
        /// </summary>
        /// <param name="configuration"></param>
        void Configure(EndPointConfiguration configuration);
    }

    public class ContentSerializerProvider : IContentSerializerProvider
    {
        private IContentSerializer _onlySerializer;
        private IContentSerializer[] _serializers;

        public ContentSerializerProvider(IEnumerable<IContentSerializer> serializers)
        {
            // process last serializers first
            _serializers = serializers.Reverse().ToArray();

            if (_serializers.Length == 1)
            {
                _onlySerializer = _serializers[0];
            }
        }

        public IContentSerializer DefaultSerializer => _serializers.First();

        public IContentSerializer GetSerializer(HttpContext context)
        {
            if (_onlySerializer != null)
            {
                return context.Request.ContentType.StartsWith(_onlySerializer.ContentType) ? _onlySerializer : null;
            }

            var contentType = context.Request.ContentType;

            for (var i = 0; i < _serializers.Length; i++)
            {
                if (contentType.StartsWith(_serializers[i].ContentType))
                {
                    return _serializers[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Configure serializers
        /// </summary>
        /// <param name="configuration"></param>
        public void Configure(EndPointConfiguration configuration)
        {
            foreach (var serializer in _serializers)
            {
                serializer.Configure(configuration);
            }
        }
    }
}
