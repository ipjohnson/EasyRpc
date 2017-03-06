using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EasyRpc.DynamicClient.ProxyGenerator
{
    public interface IJsonMethodObjectWriter
    {
        /// <summary>
        /// Opens the json object and writes everything up to and including params attribute name
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="methodName"></param>
        void OpenMethodObject(JsonTextWriter textWriter, string methodName);

        /// <summary>
        /// writes id property and closes object
        /// </summary>
        /// <param name="textWriter"></param>
        void CloseMethodObject(JsonTextWriter textWriter);
    }

    public class JsonMethodObjectWriter : IJsonMethodObjectWriter
    {
        private int _id;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="methodName"></param>
        public virtual void OpenMethodObject(JsonTextWriter textWriter, string methodName)
        {
            textWriter.WriteStartObject();

            textWriter.WritePropertyName("jsonrpc", false);
            textWriter.WriteValue("2.0");

            textWriter.WritePropertyName("method", false);
            textWriter.WriteValue(methodName);
        }

        /// <summary>
        /// writes id property and closes object
        /// </summary>
        /// <param name="textWriter"></param>
        public virtual void CloseMethodObject(JsonTextWriter textWriter)
        {
            textWriter.WritePropertyName("id", false);

            var value = (uint)Interlocked.Increment(ref _id);

            // don't want to use 0 if for some reason the client send 
            if (value == uint.MinValue)
            {
                value = (uint)Interlocked.Increment(ref _id);
            }

            textWriter.WriteValue(value);

            textWriter.WriteEndObject();
        }
    }
}
