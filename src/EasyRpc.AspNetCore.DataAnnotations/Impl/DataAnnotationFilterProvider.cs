using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.DataAnnotations.Impl
{
    public class DataAnnotationFilterProvider
    {
        public Func<HttpContext, IEnumerable<ICallFilter>> ProvideFilters(MethodInfo method)
        {
            var filterList = new List<ICallFilter>();

            foreach (var parameter in method.GetParameters())
            {
                var validationAttributes = parameter.GetCustomAttributes<ValidationAttribute>().ToList();

                if (validationAttributes.Count > 0)
                {
                    filterList.Add(new DataAnnotationFilter(validationAttributes, parameter.Position, parameter.Name));
                }

                foreach (var property in parameter.ParameterType.GetTypeInfo().DeclaredProperties)
                {
                    if (property.GetCustomAttributes<ValidationAttribute>().Any())
                    {
                        filterList.Add(new InstanceDataAnnotationFilter(parameter.Position));
                        break;
                    }
                }
            }

            if (filterList.Count > 0)
            {
                return context => filterList;
            }

            return null;
        }
    }
}
