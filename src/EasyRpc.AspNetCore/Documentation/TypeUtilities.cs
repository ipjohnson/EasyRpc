using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;

namespace EasyRpc.AspNetCore.Documentation
{
    public static class TypeUtilities
    {
        public static string GetFriendlyTypeName(Type type,out Type currentType, out bool isArray)
        {
            isArray = false;

            currentType = type;

            if (type == typeof(string) ||
                type.GetTypeInfo().IsPrimitive)
            {
                return type.Name;
            }

            foreach (var @interface in type.GetTypeInfo().ImplementedInterfaces)
            {
                if (@interface.IsConstructedGenericType &&
                    @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    isArray = true;

                    currentType = @interface.GetTypeInfo().GetGenericArguments()[0];

                    return GetFriendlyTypeName(currentType, out currentType, out var unused) + "[]";
                }
            }

            if (type.IsConstructedGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(IEnumerable<>))
                {
                    isArray = true;
                    currentType = type.GetTypeInfo().GetGenericArguments()[0];

                    return GetFriendlyTypeName(currentType, out currentType, out var unused) + "[]";
                }

                if (genericTypeDefinition == typeof(ResponseMessage<>))
                {
                    currentType = type.GetTypeInfo().GetGenericArguments()[0];
                    return GetFriendlyTypeName(currentType,out currentType, out isArray);
                }

                if (genericTypeDefinition == typeof(Task<>))
                {
                    currentType = type.GetTypeInfo().GetGenericArguments()[0];
                    return GetFriendlyTypeName(currentType,out currentType, out isArray);
                }

                var name = genericTypeDefinition.Name;
                var index = name.IndexOf('`');
                name = name.Substring(0, index);
                name += '<';

                foreach (var typeArgument in type.GenericTypeArguments)
                {
                    if (name[name.Length - 1] != '<')
                    {
                        name += ',';
                    }

                    name += GetFriendlyTypeName(typeArgument,out var unusedType, out var genericArray) + (genericArray ? "[]" : "");
                }

                return name + ">";
            }

            return type.Name;
        }

        public static TypeRef CreateTypeRef(Type type)
        {
            return new TypeRef
            {
                DisplayName = GetFriendlyTypeName(type,out var currentType, out var isArray),
                FullName = currentType.FullName,
                Array = isArray,
                Type = type
            };
        }
    }
}
