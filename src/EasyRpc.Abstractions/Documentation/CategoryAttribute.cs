using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Documentation
{
    public class CategoryAttribute : Attribute
    {
        public CategoryAttribute(string category)
        {
            Category = category;
        }

        public string Category { get; }
    }
}
