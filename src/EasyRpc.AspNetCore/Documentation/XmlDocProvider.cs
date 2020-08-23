using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IXmlDocProvider
    {
        /// <summary>
        /// Get XElement that represents the documentation for a given type, can be null
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        XElement GetTypeDocumentation(Type type);

        /// <summary>
        /// Get XElement that represents the documentation for a given member
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        XElement GetMemberDocumentation(MemberInfo memberInfo);
    }

    public class XmlDocProvider : IXmlDocProvider
    {
        /// <inheritdoc />
        public XElement GetTypeDocumentation(Type type)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public XElement GetMemberDocumentation(MemberInfo memberInfo)
        {
            throw new NotImplementedException();
        }
    }
}
