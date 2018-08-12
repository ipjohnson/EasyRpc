using System.Collections.Generic;
using System.Linq;

namespace EasyRpc.AspNetCore.Content
{
    /// <summary>
    /// Provides content encoder based on content string
    /// </summary>
    public interface IContentEncodingProvider
    {
        /// <summary>
        /// Get content encoder based on content encoding string
        /// </summary>
        /// <param name="contentString"></param>
        /// <returns></returns>
        IContentEncoder GetContentEncoder(string contentString);
    }
    
    /// <summary>
    /// Provides content encoders
    /// </summary>
    public class ContentEncodingProvider : IContentEncodingProvider
    {
        private readonly IContentEncoder _onlyContentEncoder;
        private readonly IContentEncoder[] _contentEncoders;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="contentEncoders"></param>
        public ContentEncodingProvider(IEnumerable<IContentEncoder> contentEncoders)
        {
            _contentEncoders = contentEncoders.Reverse().ToArray();

            if (_contentEncoders.Length == 1)
            {
                _onlyContentEncoder = _contentEncoders[0];
            }
        }

        /// <summary>
        /// Get content encoder based on content encoding string
        /// </summary>
        /// <param name="contentString"></param>
        /// <returns></returns>
        public IContentEncoder GetContentEncoder(string contentString)
        {
            if (_onlyContentEncoder != null)
            {
                return contentString.Contains(_onlyContentEncoder.ContentEncoding) ? _onlyContentEncoder : null;
            }

            for (var i = 0; i < _contentEncoders.Length; i++)
            {
                if (contentString.Contains(_contentEncoders[i].ContentEncoding))
                {
                    return _contentEncoders[i];
                }
            }

            return null;
        }
    }
}
