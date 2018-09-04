using System.Collections.Generic;
using System.Linq;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Content
{
    /// <summary>
    /// Provides serializer
    /// </summary>
    public interface IContentSerializerProvider
    {
        /// <summary>
        /// Default serializer
        /// </summary>
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
        void Configure(IExposeMethodInformationCacheManager cacheManager);
    }

    /// <summary>
    /// Provides content serializers
    /// </summary>
    public class ContentSerializerProvider : IContentSerializerProvider
    {
        private readonly IContentSerializer _onlySerializer;
        private readonly IContentSerializer[] _serializers;

        /// <summary>
        /// DEfault constructor
        /// </summary>
        /// <param name="serializers"></param>
        public ContentSerializerProvider(IEnumerable<IContentSerializer> serializers)
        {
            // process last serializers first
            _serializers = serializers.Reverse().ToArray();
            
            if (_serializers.Length == 1)
            {
                _onlySerializer = _serializers[0];
            }
        }

        /// <summary>
        /// Gets the default serializer
        /// </summary>
        public IContentSerializer DefaultSerializer => _serializers.First();

        /// <summary>
        /// Get serializer based on context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IContentSerializer GetSerializer(HttpContext context)
        {
            if (_onlySerializer != null)
            {
                return _onlySerializer.CanSerialize(context) ? _onlySerializer : null;
            }

            for (var i = 0; i < _serializers.Length; i++)
            {
                if (_serializers[i].CanSerialize(context))
                {
                    return _serializers[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Configure serializers
        /// </summary>
        public void Configure(IExposeMethodInformationCacheManager cacheManager)
        {
            var id = 1;

            foreach (var serializer in _serializers)
            {
                serializer.SerializerId = id++;

                serializer.Configure(cacheManager);
            }
        }
    }
}
