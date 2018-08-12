using System;

namespace EasyRpc.AspNetCore.Middleware
{
    public abstract class BaseExposedMethodInformation
    {
        private object[] _data = Array.Empty<object>();

        public object GetSerializerData(int serializerId)
        {
            if (_data.Length < serializerId)
            {
                return null;
            }

            return _data[serializerId - 1];
        }

        public void SetSerializerData(int serializerId, object serializerData)
        {
            if (_data.Length < serializerId)
            {
                var newArray = new object[serializerId];

                _data.CopyTo(newArray, 0);

                _data = newArray;
            }

            _data[serializerId - 1] = serializerData;
        }
    }
}
