using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    public class ExcludingConfigurator
    {
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludingProperties = new HashSet<PropertyInfo>();

        public void ExcludeType<TPropType>()
        {
            ExcludeType(typeof(TPropType));
        }

        public void ExcludeType(Type type)
        {
            excludingTypes.Add(type);
        }

        public void ExcludeProperty(PropertyInfo propertyInfo)
        {
            excludingProperties.Add(propertyInfo);
        }

        public IEnumerable<PropertyInfo> Filter(IEnumerable<PropertyInfo> properties)
        {
            return properties.Except(excludingProperties)
                .Where(prop => !excludingTypes.Contains(prop.PropertyType));
        }
    }
}