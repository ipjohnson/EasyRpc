using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Documentation
{
    public class TypeDefinition
    {
        public string Comment { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        /// <summary>
        /// System Type
        /// </summary>
        [JsonIgnore]
        public Type Type { get; set; }

        public List<PropertyDefinition> Properties { get; set; }

        public List<EnumValueDefinition> EnumValues { get; set; }
    }

    public class PropertyDefinition
    {
        public string Name { get; set; }

        public string Comment { get; set; }
        
        public TypeRef PropertyType { get; set; }

        public bool Required { get; set; }

    }

    public class EnumValueDefinition
    {
        public string Name { get; set; }

        public object Value { get; set; }
    }
}
