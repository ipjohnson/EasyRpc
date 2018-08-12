using System.Collections.Generic;
using System.Linq;

namespace EasyRpc.AspNetCore.Content
{
    public interface IContentEncodingProvider
    {
        IContentEncoder GetContentEncoder(string contentString);
    }
    
    public class ContentEncodingProvider : IContentEncodingProvider
    {
        private readonly IContentEncoder _onlyContentEncoder;
        private readonly IContentEncoder[] _contentEncoders;

        public ContentEncodingProvider(IEnumerable<IContentEncoder> contentEncoders)
        {
            _contentEncoders = contentEncoders.Reverse().ToArray();

            if (_contentEncoders.Length == 1)
            {
                _onlyContentEncoder = _contentEncoders[0];
            }
        }

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
