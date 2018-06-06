using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Documentation
{
    public class TypeDefinition
    {
        public string Comment { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public List<PropertyDefinition> Properties { get; set; }
    }

    public class PropertyDefinition
    {
        public string Name { get; set; }

        public string Comment { get; set; }
        
        public string PropertyType { get; set; }

        public bool Required { get; set; }

    }
}
